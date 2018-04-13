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

namespace GRPExplorerLib.BigFile
{
    public struct BigFileUnpackOptions
    {
        public DirectoryInfo Directory;
        public BigFileFlags Flags;
        public int Threads;

        internal void Log(ILogProxy log)
        {
            log.Info(">BigFileUnpackOptions:");
            log.Info("    Directory: " + Directory?.FullName);
            log.Info("    Flags: " + Flags);
            log.Info("    Threads: " + Threads.ToString());
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

            public int progress;
        }

        public const int NUM_THREADED_TASKS = 16;

        private ILogProxy log = LogManager.GetLogProxy("BigFileUnpacker");

        private PackedBigFile bigFile;

        private Stopwatch stopwatch = new Stopwatch();

        public DiagData diagData;

        private readonly string formatted_diag_msg = string.Format("     {0,6}   {1,6}   {2,6}   {3,6}", "Thread", "Time", "Index", "Count");

        private UnpackThreadInfo[] unpackThreads = new UnpackThreadInfo[NUM_THREADED_TASKS];
        public bool IsUnpacking
        {
            get
            {
                for (int i = 0; i < NUM_THREADED_TASKS; i++)
                {
                    if (unpackThreads[i] != null && unpackThreads[i].isUnpacking)
                        return true;
                }

                return false;
            }
        }


        public BigFileUnpacker(PackedBigFile _bigFile)
        {
            bigFile = _bigFile;
        }

        //public void UnpackBigfile(DirectoryInfo dir, BigFileFlags flags)
        public BigFileUnpackOperationStatus UnpackBigfile(BigFileUnpackOptions options)
        {
            if (options.Threads > NUM_THREADED_TASKS)
            {
                log.Error(string.Format("Can't have more threads than the max! ({0} > {1})", options.Threads, NUM_THREADED_TASKS));
                log.Error("    Threads will be clamped to the max!");
                options.Threads = NUM_THREADED_TASKS;
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

            log.Info("Creating renamed mapping file...");
            UnpackedRenamedFileMapping renamedMapping = CreateRenamedFileMapping(bigFile.RootFolder);
            UnpackedFileKeyMappingFile mappingFile = new UnpackedFileKeyMappingFile(options.Directory);
            mappingFile.SaveMappingData(renamedMapping);
            log.Info("Mapping file saved!");

            stopwatch.Reset();
            stopwatch.Start();

            log.Info("Beginning extract...");

            verifyAndResetThreads(options.Threads);

            if ((options.Flags & BigFileFlags.UseThreading) != 0)
            {
                int dividedCount = bigFile.MappingData.FilesList.Length / NUM_THREADED_TASKS;
                int dividedRemainder = bigFile.MappingData.FilesList.Length % NUM_THREADED_TASKS;
                log.Info("Divided files into " + NUM_THREADED_TASKS + " pools of " + dividedCount + " with " + dividedRemainder + " left over (to be tacked onto the last!)");

                List<UnpackThreadInfo> usingThreads = new List<UnpackThreadInfo>();
                for (int i = 0; i < NUM_THREADED_TASKS; i++)
                {
                    unpackThreads[i].options = options;
                    unpackThreads[i].bigFile = bigFile;
                    unpackThreads[i].fileMapping = renamedMapping;
                    unpackThreads[i].startIndex = i * dividedCount;
                    unpackThreads[i].count = dividedCount;
                    unpackThreads[i].threadID = i;
                    unpackThreads[i].OnWorkDoneCallback = internal_OnThreadFinished;
                }
                unpackThreads[NUM_THREADED_TASKS - 1].count += dividedRemainder; //add the remainder onto the last info

                for (int i = 0; i < NUM_THREADED_TASKS; i++)
                {
                    ThreadPool.QueueUserWorkItem(internal_UnpackFiles, unpackThreads[i]);
                }

                return new BigFileUnpackOperationStatus(usingThreads);
            }
            else //use the function for threads even without one
            {
                unpackThreads[0].options = options;
                unpackThreads[0].bigFile = bigFile;
                unpackThreads[0].fileMapping = renamedMapping;
                unpackThreads[0].startIndex = 0;
                unpackThreads[0].count = bigFile.MappingData.FilesList.Length;
                unpackThreads[0].threadID = 0;
                unpackThreads[0].OnWorkDoneCallback = internal_OnThreadFinished;

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
            for (int i = 0; i < NUM_THREADED_TASKS; i++)
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

                unpackThreads[i].options = new BigFileUnpackOptions();
                unpackThreads[i].bigFile = null;
                unpackThreads[i].fileMapping = null;
                unpackThreads[i].startIndex = 0;
                unpackThreads[i].count = 0;
                unpackThreads[i].threadID = i;
                unpackThreads[i].progress = 0;
                unpackThreads[i].OnWorkDoneCallback = internal_OnThreadFinished;
            }
        }

        private void internal_UnpackFiles(object state)
        {
            UnpackThreadInfo info = state as UnpackThreadInfo;
            info.isUnpacking = true;
            info.stopwatch.Reset();
            info.stopwatch.Start();
            
            int dataOffset = info.bigFile.YetiHeaderFile.CalculateDataOffset(ref info.bigFile.FileHeader, ref info.bigFile.CountInfo);
            byte[] buffer = info.buffers[4];
            using (FileStream fs = File.OpenRead(info.bigFile.MetadataFileInfo.FullName))
            {
                BigFileFile currFile = null;
                for (int i = info.startIndex; i < info.startIndex + info.count; i++)
                {
                    currFile = bigFile.MappingData.FilesList[i];
                    if (string.IsNullOrEmpty(currFile.Name))
                    {
                        log.Error(string.Format("File (key:{0:X8}) does not have a file name!", currFile.FileInfo.Key));
                        continue;
                    }

                    log.Info("Unpacking file " + currFile.Name);
                    //info.fileMapping[currFile.FileInfo.Key].DebugLog(log);

                    int fileSize = info.bigFile.FileReader.ReadFile(fs, currFile, info.buffers, info.options.Flags);

                    string fileName = info.options.Directory.FullName + info.fileMapping[currFile.FileInfo.Key].FileName;

                    //write the read data to the unpacked file
                    try
                    {
                        using (FileStream newFs = File.Create(fileName))
                        {
                            if (fileSize != -1) //if the offset is bad, only create the file, don't write anything
                            {
                                buffer = info.buffers[fileSize]; //fuck me
                                newFs.Write(buffer, 0, fileSize);
                            }
                        }
                    }
                    catch (Exception ex)
                    {
                        log.Error("God Damnit.\n\n" + ex.Message);
                    }
                }
            }

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
                for (int i = 0; i < NUM_THREADED_TASKS; i++)
                {
                    string str = string.Format("  {0,6}        {1,4}s  {2,6}   {3,6}", i, unpackThreads[i].stopwatch.ElapsedMilliseconds / 1000, unpackThreads[i].startIndex, unpackThreads[i].count);
                    log.Info(str);
                }
            }
        }

        public void GenerateYetiMetadataFile(DirectoryInfo dir, PackedBigFile bigfile)
        {
            stopwatch.Reset();
            stopwatch.Start();
            FileInfo metadataFileInfo = new FileInfo(dir.FullName + "\\" + BigFileConst.METADATA_FILE_NAME);
            using (FileStream fs = File.Create(metadataFileInfo.FullName))
            {
                FileBuffer bytesToWrite = bigfile.YetiHeaderFile.ReadFileAndFolderMetadataRaw(ref bigFile.FileHeader, ref bigFile.CountInfo);
                fs.Write(bytesToWrite.bytes, 0, bytesToWrite.size);
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

        public UnpackedRenamedFileMapping CreateRenamedFileMapping(BigFileFolder folder, UnpackedRenamedFileMapping mapping = null, Dictionary<string, int> fileRenameCounts = null)
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
                CreateRenamedFileMapping(childFolder, mapping, fileRenameCounts);
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
