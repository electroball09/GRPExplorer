using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using GRPExplorerLib.Util;
using System.Diagnostics;
using System.IO.Compression;
using System.Threading;
using GRPExplorerLib.Logging;
using GRPExplorerLib.BigFile.Extra;

namespace GRPExplorerLib.BigFile
{
    public struct BigFileUnpackOptions
    {
        public DirectoryInfo Directory;
        public BigFileFlags Flags;
        public int Threads;
        public string LoadExtensionsFile;

        internal void Log(ILogProxy log)
        {
            log.Info(">BigFileUnpackOptions:");
            log.Info("    Directory: " + Directory?.FullName);
            log.Info("    Flags: " + Flags);
            log.Info("    Threads: " + Threads.ToString());
            log.Info("    LoadExtensionsFile: " + LoadExtensionsFile.ToString());
        }
    }

    public class BigFileUnpackOperationStatus : BigFileOperationStatus
    {
        private List<BigFileUnpacker.UnpackThreadInfo> threads;

        internal BigFileUnpackOperationStatus(List<BigFileUnpacker.UnpackThreadInfo> infos)
        {
            threads = infos;
        }

        public override string OperationName
        {
            get
            {
                return "Unpack";
            }
        }

        public override float Progress
        {
            get
            {
                float val = 0f;
                foreach (BigFileUnpacker.UnpackThreadInfo info in threads)
                {
                    float threadProgress = info.count / ((float)info.progress);
                    val += threadProgress * (1 / threads.Count);
                }
                return val;
            }
        }
    }

    public class BigFileUnpacker
    {
        public struct DiagData
        {
            public float GenerateYetiMetadataFile;
            public float CreateDirectories;
            public float CreateRenamedFileMapping;
            public float WriteUnpackedFiles;

            internal void DebugLog(ILogProxy log)
            {
                log.Debug(" > BigFileUnpacker.DiagData Dump: ");
                log.Debug("    GenerateYetiMetadataFile: " + GenerateYetiMetadataFile + "ms");
                log.Debug("           CreateDirectories: " + CreateDirectories + "ms");
                log.Debug("    CreateRenamedFileMapping: " + CreateRenamedFileMapping + "ms");
            }
        }

        internal class UnpackThreadInfo
        {
            public BigFileUnpackOptions options;
            public BigFile bigFile;
            public UnpackedRenamedFileMapping fileMapping;
            public int startIndex;
            public int count;
            public int threadID;
            public Action<UnpackThreadInfo> OnWorkDoneCallback;
            public IOBuffers buffers = new IOBuffers();
            public bool isUnpacking = false;
            public Stopwatch stopwatch = new Stopwatch();
            public Dictionary<short, string> fileExtensionsList;

            public int progress;
        }

        public const int MAX_UNPACK_THREADS = 16;

        private ILogProxy log = LogManager.GetLogProxy("BigFileUnpacker");

        private BigFile bigFile;

        private BigFileFileExtensionsList extGen;

        private Stopwatch stopwatch = new Stopwatch();

        public DiagData diagData;

        private readonly string formatted_diag_msg = string.Format("     {0,6}   {1,6}   {2,6}   {3,6}", "Thread", "Time", "Index", "Count");

        private UnpackThreadInfo[] unpackThreads = new UnpackThreadInfo[MAX_UNPACK_THREADS];
        public bool IsUnpacking
        {
            get
            {
                for (int i = 0; i < MAX_UNPACK_THREADS; i++)
                {
                    if (unpackThreads[i] != null && unpackThreads[i].isUnpacking)
                        return true;
                }

                return false;
            }
        }

        public BigFileUnpacker(BigFile _bigFile)
        {
            bigFile = _bigFile;
            extGen = new BigFileFileExtensionsList(_bigFile);
        }
        
        public BigFileUnpackOperationStatus UnpackBigfile(BigFileUnpackOptions options)
        {
            if (options.Threads > MAX_UNPACK_THREADS)
            {
                log.Error(string.Format("Can't have more threads than the max! ({0} > {1})", options.Threads, MAX_UNPACK_THREADS));
                log.Error("    Threads will be clamped to the max!");
                options.Threads = MAX_UNPACK_THREADS;
            }
            
            log.Info("Unpacking a bigfile to directory: \"" + options.Directory.FullName + "\"");

            options.Log(log);

            if (!options.Directory.Exists)
            {
                log.Info("Directory does not exist, creating it...");
                Directory.CreateDirectory(options.Directory.FullName);
            }

            GenerateYetiMetadataFile(options.Directory, bigFile);

            DirectoryInfo unpackDir = new DirectoryInfo(options.Directory.FullName + "\\" + BigFileConst.UNPACK_DIR);
            log.Info("Creating unpack dir: " + unpackDir.FullName);
            Directory.CreateDirectory(unpackDir.FullName);

            log.Info("Creating unpacked directories...");
            CreateDirectoriesFromTree(unpackDir, bigFile.RootFolder);

            Dictionary<short, string> fileExtensionsList = null;
            if (!string.IsNullOrEmpty(options.LoadExtensionsFile))
            {
                log.Info("Loading extensions list from file {0}", options.LoadExtensionsFile);
                fileExtensionsList = extGen.LoadFileExtensionsList(new FileInfo(options.LoadExtensionsFile));
            }

            log.Info("Creating renamed mapping file...");
            UnpackedRenamedFileMapping renamedMapping = CreateRenamedFileMapping(bigFile.RootFolder, fileExtensionsList);
            UnpackedFileKeyMappingFile mappingFile = new UnpackedFileKeyMappingFile(options.Directory);
            mappingFile.SaveMappingData(renamedMapping);
            log.Info("Mapping file saved!");

            stopwatch.Reset();
            stopwatch.Start();

            log.Info("Beginning extract...");

            verifyAndResetThreads(options.Threads);

            if ((options.Flags & BigFileFlags.UseThreading) != 0)
            {
                int dividedCount = bigFile.FileMap.FilesList.Length / options.Threads;
                int dividedRemainder = bigFile.FileMap.FilesList.Length % options.Threads;
                log.Info("Divided files into " + options.Threads + " pools of " + dividedCount + " with " + dividedRemainder + " left over (to be tacked onto the last!)");

                List<UnpackThreadInfo> usingThreads = new List<UnpackThreadInfo>();
                for (int i = 0; i < options.Threads; i++)
                {
                    unpackThreads[i].options = options;
                    unpackThreads[i].bigFile = bigFile;
                    unpackThreads[i].fileMapping = renamedMapping;
                    unpackThreads[i].startIndex = i * dividedCount;
                    unpackThreads[i].count = dividedCount;
                    unpackThreads[i].threadID = i;
                    unpackThreads[i].OnWorkDoneCallback = internal_OnThreadFinished;
                    unpackThreads[i].fileExtensionsList = fileExtensionsList;
                }
                unpackThreads[options.Threads - 1].count += dividedRemainder; //add the remainder onto the last info

                for (int i = 0; i < options.Threads; i++)
                {
                    ThreadPool.QueueUserWorkItem(internal_UnpackFiles, unpackThreads[i]);
                    usingThreads.Add(unpackThreads[i]);
                }

                return new BigFileUnpackOperationStatus(usingThreads);
            }
            else //use the function for threads even without one
            {
                unpackThreads[0].options = options;
                unpackThreads[0].bigFile = bigFile;
                unpackThreads[0].fileMapping = renamedMapping;
                unpackThreads[0].startIndex = 0;
                unpackThreads[0].count = bigFile.FileMap.FilesList.Length;
                unpackThreads[0].threadID = 0;
                unpackThreads[0].OnWorkDoneCallback = internal_OnThreadFinished;
                unpackThreads[0].fileExtensionsList = fileExtensionsList;

                internal_UnpackFiles(unpackThreads[0]); //teehee

                diagData.WriteUnpackedFiles = stopwatch.ElapsedMilliseconds;
                stopwatch.Stop();

                log.Info("Extract complete!");

                diagData.DebugLog(log);

                log.Info("Unpack complete!");

                return null;
            }
        }

        private void verifyAndResetThreads(int threads)
        {
            for (int i = 0; i < MAX_UNPACK_THREADS; i++)
            {
                if (unpackThreads[i] == null)
                {
                    if (i <= threads)
                    {
                        unpackThreads[i] = new UnpackThreadInfo();
                    }
                    else
                    {
                        continue;
                    }
                }

                unpackThreads[i].options = default(BigFileUnpackOptions);
                unpackThreads[i].bigFile = null;
                unpackThreads[i].fileMapping = null;
                unpackThreads[i].startIndex = 0;
                unpackThreads[i].count = 0;
                unpackThreads[i].threadID = i;
                unpackThreads[i].progress = 0;
                unpackThreads[i].OnWorkDoneCallback = internal_OnThreadFinished;
                unpackThreads[i].fileExtensionsList = null;
            }
        }

        private void internal_UnpackFiles(object state)
        {
            UnpackThreadInfo info = state as UnpackThreadInfo;
            info.isUnpacking = true;
            info.stopwatch.Reset();
            info.stopwatch.Start();
            
            //int segmentDataOffset = info.bigFile.FileUtil.CalculateDataOffset(ref info.bigFile.SegmentHeader, ref info.bigFile.FileHeader);
            //byte[] buffer = info.buffers[4];
            BigFileFile[] files = new BigFileFile[info.count];
            Array.Copy(info.bigFile.FileMap.FilesList, info.startIndex, files, 0, info.count);

            IEnumerator<int[]> headers = info.bigFile.FileReader.ReadAllHeaders(files, info.buffers, info.options.Flags).GetEnumerator();
            IEnumerator<int> data = info.bigFile.FileReader.ReadAllData(files, info.buffers, info.options.Flags).GetEnumerator();

            for (int i = 0; i < files.Length; i++)
            {
                info.progress = i;

                headers.MoveNext();
                data.MoveNext();

                log.Info("Unpacking file {0}", files[i].Name);

                //********************************************//
                //DON'T FORGET THE ******* UNPACK SUBDIRECTORY//
                //********************************************//
                string dataFileName = info.options.Directory.FullName + "\\"
                                    + BigFileConst.UNPACK_DIR + "\\"
                                    + info.fileMapping[files[i].FileInfo.Key].FileName;

                string headerFileName = dataFileName + BigFileConst.UNPACKED_HEADER_FILE_EXTENSION;

                using (FileStream dataFS = File.Create(dataFileName))
                using (FileStream headerFS = File.Create(headerFileName))
                {
                    int size = data.Current;
                    int[] header = headers.Current;
                    if (size != -1)
                    {
                        int headerCount = header.Length;
                        dataFS.Write(info.buffers[size], 0, size);

                        headerFS.Write(headerCount.ToByteArray(info.buffers[4]), 0, 4);
                        for (int j = 0; j < headerCount; j++)
                        {
                            headerFS.Write(header[j].ToByteArray(info.buffers[4]), 0, 4);
                            if (header[j] == 0)
                                log.Error("WTF");
                        }
                    }
                    else
                    {
                        log.Error("Can't unpack file {0} because size is -1", files[i].Name);
                    }
                }
            }

            //int index = -1;
            //foreach (int size in info.bigFile.FileReader.ReadAllRaw(files, info.buffers, info.options.Flags))
            //{
            //    index++;

            //    info.progress = index;

            //    log.Info("Unpacking file {0}", files[index].Name);

            //    //********************************************//
            //    //DON'T FORGET THE ******* UNPACK SUBDIRECTORY//
            //    //********************************************//
            //    string dataFileName = info.options.Directory.FullName + "\\" 
            //                        + BigFileConst.UNPACK_DIR + "\\" 
            //                        + info.fileMapping[files[index].FileInfo.Key].FileName;

            //    string headerFileName = dataFileName + BigFileConst.UNPACKED_HEADER_FILE_EXTENSION;

            //    IEnumerable<int> p = info.bigFile.FileReader.ReadAllData(files, info.buffers, info.options.Flags);

            //    using (FileStream dataFS = File.Create(dataFileName))
            //    using (FileStream headerFS = File.Create(headerFileName))
            //    {
            //        if (size != -1)
            //        {
            //            int headerCount = BitConverter.ToInt32(info.buffers[size], 0);
            //            int dataOffset = headerCount * 4 + 4;

            //            if (dataOffset > size)
            //            {
            //                log.Error("Hmmm... something's wrong here");
            //                headerFS.Write(info.buffers[size], 0, size);
            //            }
            //            else
            //            {
            //                dataFS.Write(info.buffers[size], dataOffset, size - dataOffset);
            //                headerFS.Write(info.buffers[size], 0, dataOffset);
            //            }
            //        }
            //        else
            //        {
            //            log.Error("Can't unpack file {0} because size is -1", files[index].Name);
            //        }
            //    }
            //}

            log.Info("Unpack thread (ID:" + info.threadID + ") finished work!");
            info.isUnpacking = false;
            info.stopwatch.Stop();
            
            info.OnWorkDoneCallback.Invoke(info);
        }

        private void internal_OnThreadFinished(UnpackThreadInfo info)
        {
            if (!IsUnpacking)
            {
                log.Info("All unpacking threads finished their work!");
                log.Info(" > Time taken: ");
                log.Info(formatted_diag_msg);
                for (int i = 0; i < MAX_UNPACK_THREADS; i++)
                {
                    if (unpackThreads[i] != null)
                    {
                        string str = string.Format("  {0,6}        {1,4}s  {2,6}   {3,6}", i, unpackThreads[i].stopwatch.ElapsedMilliseconds / 1000, unpackThreads[i].startIndex, unpackThreads[i].count);
                        log.Info(str);
                    }
                }
            }
        }

        public void GenerateYetiMetadataFile(DirectoryInfo dir, BigFile bigfile)
        {
            stopwatch.Reset();
            stopwatch.Start();
            FileInfo metadataFileInfo = new FileInfo(dir.FullName + "\\" + BigFileConst.METADATA_FILE_NAME);
            using (FileStream fs = File.Create(metadataFileInfo.FullName))
            {
                //FileBuffer bytesToWrite = bigfile.Segment.ReadFileAndFolderMetadataRaw(ref bigFile.SegmentHeader, ref bigFile.FileHeader);
                bigFile.Segment.WriteSegmentHeader(fs, ref bigFile.SegmentHeader);
                bigFile.Header.WriteHeader(fs, ref bigFile.FileHeader);
                bigFile.FilesAndFolders.WriteFileInfos(fs, bigFile.RawFileInfos);
                bigFile.FilesAndFolders.WriteFolderInfos(fs, bigFile.RawFolderInfos);
                //fs.Write(bytesToWrite.bytes, 0, bytesToWrite.size);
            }
            stopwatch.Stop();
            diagData.GenerateYetiMetadataFile = stopwatch.ElapsedMilliseconds;
        }

        public void CreateDirectoriesFromTree(DirectoryInfo dir, BigFileFolder rootFolder)
        {
            stopwatch.Reset();
            stopwatch.Start();
            string str = "";
            foreach (KeyValuePair<short, BigFileFolder> kvp in bigFile.RootFolder.FolderMap)
            {
                str = dir + "\\" + kvp.Value.FullDirectoryName;
                if (!Directory.Exists(str))
                {
                    log.Debug("   Creating directory: " + str);
                    Directory.CreateDirectory(str);
                }
            }
            stopwatch.Stop();
            diagData.CreateDirectories = stopwatch.ElapsedMilliseconds;
        }

        public UnpackedRenamedFileMapping CreateRenamedFileMapping(BigFileFolder folder, Dictionary<short, string> extensionsList, UnpackedRenamedFileMapping mapping = null, Dictionary<string, int> fileRenameCounts = null)
        {
            bool isFirst = mapping == null;
            if (isFirst)
            {
                mapping = new UnpackedRenamedFileMapping();
                fileRenameCounts = new Dictionary<string, int>();
                stopwatch.Reset();
                stopwatch.Start();
            }

            foreach (BigFileFile file in folder.Files)
            {
                string fullName = file.FullFolderPath + file.Name;

                if (extensionsList != null && extensionsList.ContainsKey(file.FileInfo.FileType))
                {
                    fullName += "." + extensionsList[file.FileInfo.FileType];
                }
                else
                {
                    fullName += string.Format(".{0:X4}", file.FileInfo.FileType);
                }

                string fullNameLower = fullName.ToLowerInvariant();

                if (fileRenameCounts.ContainsKey(fullNameLower))
                {
                    fileRenameCounts[fullNameLower]++;
                    fullName += "_" + fileRenameCounts[fullNameLower];
                }
                else
                {
                    fileRenameCounts.Add(fullNameLower, 1);
                }

                log.Debug(file.Name + " remapped to " + fullName);

                UnpackedRenamedFileMapping.RenamedFileMappingData data = new UnpackedRenamedFileMapping.RenamedFileMappingData()
                {
                    Key = file.FileInfo.Key,
                    OriginalName = file.Name,
                    FileName = fullName
                };

                mapping[file.FileInfo.Key] = data;
            }

            foreach (BigFileFolder childFolder in folder.SubFolders)
            {
                CreateRenamedFileMapping(childFolder, extensionsList, mapping, fileRenameCounts);
            }

            if (isFirst)
            {
                stopwatch.Stop();
                diagData.CreateRenamedFileMapping = stopwatch.ElapsedMilliseconds;
            }

            return mapping;
        }
    }
}
