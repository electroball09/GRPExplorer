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
using GRPExplorerLib.BigFile.Versions;
using Ionic.Zlib;

namespace GRPExplorerLib.BigFile
{
    public struct BigFilePackOptions
    {
        public DirectoryInfo Directory;
        public string BigFileName;
        public BigFileFlags Flags;
        public int Threads;

        internal void Log(ILogProxy log)
        {
            log.Info(">BigFilePackOptions:");
            log.Info("   >Directory: " + Directory.FullName);
            log.Info("   >BigFileName: " + BigFileName);
            log.Info("   >Flags: " + Flags.ToString());
            log.Info("   >Threads: " + Threads.ToString());
        }
    }

    public class BigFilePackOperationStatus : BigFileOperationStatus
    {
        private List<BigFilePacker.BigFilePackInfo> packInfos;

        internal BigFilePackOperationStatus(List<BigFilePacker.BigFilePackInfo> _packInfos)
        {
            packInfos = _packInfos;
        }

        public override string OperationName
        {
            get
            {
                return "BigFilePack";
            }
        }

        public override float Progress
        {
            get
            {
                float val = 0f;
                foreach (BigFilePacker.BigFilePackInfo info in packInfos)
                {
                    float progress = info.count / ((float)info.progress);
                    val += progress * (1 / packInfos.Count);
                }
                return val;
            }
        }
    }

    public class BigFilePacker
    {
        internal class BigFilePackInfo
        {
            public BigFilePackOptions Options;
            public int ThreadID;
            public int startIndex;
            public int count;
            public IOBuffers IOBuffers = new IOBuffers();
            public bool isPacking = false;
            public Stopwatch stopwatch = new Stopwatch();
            public BigFile bigFile;
            public Action<BigFilePackInfo> OnCompleted;

            public int progress;
        }

        public const int MAX_PACK_THREADS = 16;

        private ILogProxy log = LogManager.GetLogProxy("BigFilePacker");
        private BigFile bigFile;

        private BigFilePackInfo[] packInfos = new BigFilePackInfo[MAX_PACK_THREADS];
        public bool IsPacking
        {
            get
            {
                for (int i = 0; i < MAX_PACK_THREADS; i++)
                {
                    if (packInfos[i] != null && packInfos[i].isPacking)
                        return true;
                }

                return false;
            }
        }

        public BigFilePacker(BigFile _bigFile)
        {
            bigFile = _bigFile;
        }

        public BigFilePackOperationStatus PackBigFile(BigFilePackOptions options)
        {
            if (options.Threads > MAX_PACK_THREADS)
            {
                log.Error(string.Format("Can't have more threads than the max! ({0} > {1})", options.Threads, MAX_PACK_THREADS));
                log.Error("    Threads will be clamped to the max!");
                options.Threads = MAX_PACK_THREADS;
            }
            else if (options.Threads <= 0)
            {
                log.Error("What in the name of all that is good and holy are you trying to do?");
                log.Error("    (BigFilePackOptions threads <= 0)");
                return null;
            }

            log.Info("Packing a big file to directory: " + options.Directory.FullName);

            options.Log(log);

            if (!options.Directory.Exists)
            {
                log.Info("Creating directory...");
                Directory.CreateDirectory(options.Directory.FullName);
            }

            VerifyAndResetPackInfos(options.Threads);

            if ((options.Flags & BigFileFlags.UseThreading) != 0)
            {
                return internal_ThreadedPack(options);
            }
            else
            {
                packInfos[0].Options = options;
                packInfos[0].startIndex = 0;
                packInfos[0].count = bigFile.MappingData.FilesList.Length;
                packInfos[0].bigFile = bigFile;
                packInfos[0].OnCompleted = internal_OnPackFinished;

                internal_GenBigFileChunk(packInfos[0]);

                log.Info("Finished pack!");

                return null;
            }
        }

        private BigFilePackOperationStatus internal_ThreadedPack(BigFilePackOptions options)
        {
            //set up threads
            int dividedCount = bigFile.MappingData.FilesList.Length / options.Threads;
            int dividedRemainder = bigFile.MappingData.FilesList.Length % options.Threads;
            log.Info("Divided files into " + options.Threads + " pools of " + dividedCount + " with " + dividedRemainder + " left over (to be tacked onto the last!)");

            List<BigFilePackInfo> infos = new List<BigFilePackInfo>();
            for (int i = 0; i < options.Threads; i++)
            {
                packInfos[i].Options = options;
                packInfos[i].startIndex = i * dividedCount;
                packInfos[i].count = dividedCount;
                packInfos[i].bigFile = bigFile;
                packInfos[i].OnCompleted = internal_OnPackFinished;
            }
            packInfos[options.Threads - 1].count += dividedRemainder;

            for (int i = 0; i < options.Threads; i++)
            {
                ThreadPool.QueueUserWorkItem(internal_GenBigFileChunk, packInfos[i]);
                infos.Add(packInfos[i]);
            }

            return new BigFilePackOperationStatus(infos);
        }

        private void internal_GenBigFileChunk(object state)
        {
            BigFilePackInfo info = state as BigFilePackInfo;
            info.isPacking = true;


            string targetFileName = info.Options.Directory.FullName + @"\" + info.Options.BigFileName + ".chunk" + info.ThreadID.ToString();
            string metadataFilename = info.Options.Directory.FullName + @"\" + info.Options.BigFileName + ".meta" + info.ThreadID.ToString();

            log.Info("Generating bigfile chunk: " + targetFileName);

            using (FileStream chunkFS = new FileStream(targetFileName, FileMode.Create, FileAccess.Write))
            using (FileStream metaFS = new FileStream(metadataFilename, FileMode.Create, FileAccess.Write))
            {
                //prefix metadata file with number of files
                byte[] countBytes = BitConverter.GetBytes(info.count);
                metaFS.Write(countBytes, 0, 4);

                BigFileFile[] filesToWrite = new BigFileFile[info.count];
                Array.Copy(bigFile.MappingData.FilesList, info.startIndex, filesToWrite, 0, info.count);
                
                BigFileFile currFile = null;
                IBigFileFileInfo fileInfo;
                byte[] currentBuffer;

                int index = info.startIndex;
                foreach (int size in bigFile.FileReader.ReadAll(filesToWrite, info.IOBuffers, info.Options.Flags))
                {
                    currFile = info.bigFile.MappingData.FilesList[index];
                    if (size == -1)
                    {
                        log.Error("Couldn't pack file " + currFile.Name + " because size was -1!");
                        continue;
                    }

                    log.Info("Packing file " + currFile.Name);

                    currentBuffer = info.IOBuffers[size];

                    fileInfo = bigFile.Version.CreateFileInfo();
                    fileInfo.Key = currFile.FileInfo.Key;
                    fileInfo.Name = currFile.FileInfo.Name;
                    fileInfo.FileNumber = index;
                    fileInfo.FileType = currFile.FileInfo.FileType;
                    fileInfo.Flags = currFile.FileInfo.Flags;
                    fileInfo.Folder = currFile.FileInfo.Folder;
                    fileInfo.Unknown_01 = currFile.FileInfo.Unknown_01;
                    fileInfo.Unknown_03 = currFile.FileInfo.Unknown_03;
                    fileInfo.TimeStamp = currFile.FileInfo.TimeStamp;
                    fileInfo.ZIP = currFile.FileInfo.ZIP;
                    fileInfo.Offset = (int)chunkFS.Position;

                    if (fileInfo.ZIP == 1 && (info.Options.Flags & BigFileFlags.Compress) != 0)
                    {
                        int sizePos = (int)chunkFS.Position;
                        chunkFS.Write(info.IOBuffers[8], 0, 8); //write 8 bytes of garbage to fill the space for decompressed and compressed size
                        using (ZlibStream zs = new ZlibStream(chunkFS, Ionic.Zlib.CompressionMode.Compress, true))
                        {
                            zs.Write(info.IOBuffers[size], 0, size);
                        }
                        int newPos = (int)chunkFS.Position;
                        int compressedSize = newPos - sizePos - 8;

                        //go back to the file offset and write the compressed and decompressed sizes
                        chunkFS.Seek(sizePos, SeekOrigin.Begin);
                        chunkFS.Write(BitConverter.GetBytes(compressedSize), 0, 4);
                        chunkFS.Write(BitConverter.GetBytes(size), 0, 4);
                        chunkFS.Seek(newPos, SeekOrigin.Begin);
                    }
                    else
                    {
                        chunkFS.Write(BitConverter.GetBytes(size), 0, 4);
                        chunkFS.Write(info.IOBuffers[size], 0, size);
                    }

                    byte[] infoBuffer = info.IOBuffers[fileInfo.StructSize];
                    fileInfo.ToBytes(infoBuffer);

                    metaFS.Write(infoBuffer, 0, fileInfo.StructSize);

                    index++;
                }
            }
        }

        private void internal_OnPackFinished(BigFilePackInfo info)
        {
            log.Info("Thread finished: " + info.ThreadID);
        }

        private void VerifyAndResetPackInfos(int threads)
        {
            for (int i = 0; i < MAX_PACK_THREADS; i++)
            {
                if (packInfos[i] == null)
                {
                    if (i < threads)
                        packInfos[i] = new BigFilePackInfo();
                    else
                        continue;
                }

                packInfos[i].count = 0;
                packInfos[i].startIndex = 0;
                packInfos[i].ThreadID = i;
                packInfos[i].Options = default(BigFilePackOptions);
                packInfos[i].bigFile = null;
                packInfos[i].OnCompleted = null;
            }
        }
    }
}
