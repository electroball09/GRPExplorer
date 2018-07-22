#pragma warning disable CS0649
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using GRPExplorerLib.Util;
using GRPExplorerLib.Logging;
using System.Threading;
using System.IO;
using System.IO.Compression;
using System.Diagnostics;
using GRPExplorerLib.BigFile.Versions;

namespace GRPExplorerLib.BigFile
{
    public class _BigFilePacker
    {
        public const int NUM_THREADED_TASKS = 4;

        private class PackThreadInfo
        {
            public int threadID;
            public int start;
            public int count;
            public FileOffsetThreadedRegister register;
            public IOBuffers buffers = new IOBuffers();
            public UnpackedBigFile bigFile;
            public bool isPacking = false;
            public FileInfo targetFile;
            public BigFileFlags flags = BigFileFlags.Compress;
            public DirectoryInfo sourceDir;
        }

        private class FileOffsetThreadedRegister
        {
            private long offset = 0;

            public void Reset()
            {
                lock(this)
                {
                    offset = 0;
                }
            }

            public long Register(long size)
            {
                lock(this)
                {
                    long old = offset;
                    offset += size;
                    return old;
                }
            }

            public long Get()
            {
                lock(this)
                {
                    return offset;
                }
            }
        }

        private UnpackedBigFile bigFile;
        private ILogProxy log = LogManager.GetLogProxy("BigFilePacker");
        private IOBuffers buffers = new IOBuffers();
        private Stopwatch stopwatch = new Stopwatch();
        private FileOffsetThreadedRegister register = new FileOffsetThreadedRegister();

        private PackThreadInfo[] packThreads = new PackThreadInfo[NUM_THREADED_TASKS];
        public bool IsPacking
        {
            get
            {
                for (int i = 0; i < NUM_THREADED_TASKS; i++)
                {
                    if (packThreads[i].isPacking)
                        return true;
                }

                return false;
            }
        }

        public _BigFilePacker(UnpackedBigFile _bigFile)
        {
            bigFile = _bigFile;

            log.Info("Creating BigFilePacker...");
        }

        public void PackBigFile(DirectoryInfo dir, string fileName = "Yeti.big", BigFileFlags flags = BigFileFlags.Compress)
        {
//            FileInfo outputFile = new FileInfo(dir.FullName + "\\" + fileName);
//            log.Info("Beginning pack for a bigfile to file " + outputFile.FullName);
//            log.Info("Flags: " + flags.ToString());

//            if (outputFile.Exists)
//            {
//                log.Error("Cannot continue, file already exists!");
//                return;
//            }

//            if (!dir.Exists)
//            {
//                log.Info("Creating directory " + dir.FullName);
//                Directory.CreateDirectory(dir.FullName);
//            }

//            YetiHeaderFile headerFile = new YetiHeaderFile(outputFile);
//            headerFile.WriteYetiHeader(bigFile);
//            headerFile.WriteYetiFileAndFolderInfo(bigFile);

//            if ((flags & BigFileFlags.UseThreading) != 0)
//            {
//                throw new Exception("Packing does not support threading yet!");

//#pragma warning disable CS0162 //SHUT UP
//                long requiredSpace = CalculateRequiredSpace(flags);
//#pragma warning restore CS0162

//                PreallocateSpace(outputFile, requiredSpace, bigFile.YetiHeaderFile.CalculateDataOffset(ref bigFile.FileHeader, ref bigFile.CountInfo));

//                log.Info("Calculating each threads' file count...");
//                int dividedCount = bigFile.MappingData.FilesList.Length / NUM_THREADED_TASKS;
//                int dividedRemainder = bigFile.MappingData.FilesList.Length % NUM_THREADED_TASKS;
//                log.Info("Divided files into " + NUM_THREADED_TASKS + " pools of " + dividedCount + " with " + dividedRemainder + " left over (to be tacked onto the last!)");
//                for (int i = 0; i < NUM_THREADED_TASKS; i++)
//                {
//                    if (packThreads[i] == null)
//                        packThreads[i] = new PackThreadInfo();

//                    packThreads[i].threadID = i;
//                    packThreads[i].start = i * dividedCount;
//                    packThreads[i].count = dividedCount;
//                    packThreads[i].register = register;
//                    packThreads[i].isPacking = false;
//                    packThreads[i].bigFile = bigFile;
//                    packThreads[i].targetFile = outputFile;
//                    packThreads[i].sourceDir = new DirectoryInfo(bigFile.Directory.FullName + "\\" + BigFileConst.UNPACK_DIR);
//                    packThreads[i].flags = flags;
//                }
//                packThreads[NUM_THREADED_TASKS - 1].count += dividedRemainder;

//                register.Reset();

//                for (int i = 0; i < NUM_THREADED_TASKS; i++)
//                {
//                    ThreadPool.QueueUserWorkItem(internal_PackFiles, packThreads[i]);
//                }
//            }
//            else
//            {
//                using (FileStream fs = File.OpenWrite(outputFile.FullName))
//                {
//                    byte[] buffer = buffers[4];
//                    int dataOffset = bigFile.YetiHeaderFile.CalculateDataOffset(ref bigFile.FileHeader, ref bigFile.CountInfo);
//                    fs.Seek(dataOffset, SeekOrigin.Begin);
//                    BigFileFile currFile = null;
//                    for (int i = 0; i < bigFile.MappingData.FilesList.Length; i++)
//                    {
//                        currFile = bigFile.MappingData.FilesList[i];
//                        if (currFile.FileInfo.Key == 0)
//                            continue;

//                        log.Info("Packing file " + currFile.Name);

//                        if ((flags & BigFileFlags.Compress) != 0)
//                        {
//                            using (FileStream srcStream = File.OpenRead(bigFile.Directory + "\\" + BigFileConst.UNPACK_DIR + bigFile.RenamedMapping[currFile.FileInfo.Key].FileName))
//                            {
//                                buffer = buffers[srcStream.Length];
//                                srcStream.Read(buffer, 0, (int)srcStream.Length);
//                                currFile.FileInfo = new BigFileFileInfo()
//                                {
//                                    Offset = (int)fs.Position - dataOffset,
//                                    Key = currFile.FileInfo.Key,
//                                    Name = currFile.FileInfo.Name,
//                                    Folder = currFile.FileInfo.Folder,
//                                    FileNumber = currFile.FileInfo.FileNumber,
//                                    CRC32 = currFile.FileInfo.CRC32,
//                                    ZIP = 1,
//                                };
//                                long compressedSizeOffset = fs.Position;
//                                fs.Write((0x00000000).ToByteArray(buffers[4]), 0, 4);
//                                fs.Write(((int)srcStream.Length).ToByteArray(buffers[4]), 0, 4);
//                                fs.WriteByte(0x78);
//                                fs.WriteByte(0x9C);
//                                using (DeflateStream ds = new DeflateStream(fs, CompressionMode.Compress, true))
//                                {
//                                    ds.Write(buffer, 0, (int)srcStream.Length);
//                                }
//                                long currOffset = fs.Position;
//                                fs.Seek(compressedSizeOffset, SeekOrigin.Begin);
//                                fs.Write(((int)(currOffset - compressedSizeOffset - 8)).ToByteArray(buffers[4]), 0, 0);
//                                fs.Seek(currOffset, SeekOrigin.Begin);
//                            }
//                        }
//                        else
//                        {
//                            using (FileStream srcStream = File.OpenRead(bigFile.Directory + "\\" + BigFileConst.UNPACK_DIR + bigFile.RenamedMapping[currFile.FileInfo.Key].FileName))
//                            {
//                                buffer = buffers[srcStream.Length];
//                                srcStream.Read(buffer, 0, (int)srcStream.Length);
//                                currFile.FileInfo = new BigFileFileInfo()
//                                {
//                                    Offset = (int)fs.Position - dataOffset,
//                                    Key = currFile.FileInfo.Key,
//                                    Name = currFile.FileInfo.Name,
//                                    Folder = currFile.FileInfo.Folder,
//                                    FileNumber = currFile.FileInfo.FileNumber,
//                                    CRC32 = currFile.FileInfo.CRC32,
//                                    ZIP = 0,
//                                };
//                                ((int)srcStream.Length).ToByteArray(buffers[4], 0);
//                                fs.Write(((int)srcStream.Length).ToByteArray(buffers[4]), 0, 4);
//                                log.Debug("Wrote file " + currFile.FileInfo.Name + ", length of: " + srcStream.Length + "   wrote length: " + BitConverter.ToInt32(buffers[4], 0));
//                                fs.Write(buffer, 0, (int)srcStream.Length);
//                            }
//                        }
//                    }
//                }

//                log.Info("Files packed!");
//            }

//            headerFile.WriteYetiHeader(bigFile);
//            headerFile.WriteYetiFileAndFolderInfo(bigFile);

//            //if ((flags & BigFilePackFlags.WaitOnThreads) != 0)
//            //{
//            //    while (IsPacking)
//            //    {
//            //        Thread.Sleep(10);
//            //    }
//            //}
        }

        private void internal_PackFiles(object obj)
        {
            PackThreadInfo info = obj as PackThreadInfo;
            info.isPacking = true;

            int dataOffset = info.bigFile.FileUtil.CalculateDataOffset(ref info.bigFile.SegmentHeader, ref info.bigFile.FileHeader);
            using (FileStream fs = File.OpenWrite(info.targetFile.FullName))
            {
                BigFileFile currFile = null;
                for (int i = info.start; i < info.start + info.count; i++)
                {
                    currFile = info.bigFile.FileMap.FilesList[i];
                    FileInfo srcFile = new FileInfo(info.sourceDir.FullName + info.bigFile.RenamedMapping[currFile.FileInfo.Key].FileName);

                    log.Info("Packing file " + currFile.Name);

                    using (FileStream srcStream = File.OpenRead(srcFile.FullName))
                    {
                        byte[] buffer = buffers[srcStream.Length];
                        srcStream.Read(buffer, 0, (int)srcStream.Length);

                        long fileSize = 4; //4 for the size info
                        
                        if ((info.flags & BigFileFlags.Compress) != 0)
                        {
                            fileSize += 4; //4 for compressed size info
                            fileSize += 2; //4 for zlib header info
                            using (MemoryStream ms = new MemoryStream())
                            using (DeflateStream ds = new DeflateStream(ms, CompressionMode.Compress))
                            {
                                ds.Write(buffer, 0, (int)fs.Length);
                                fileSize += ms.Length;

                                buffer = info.buffers[ms.Length + 10];

                                long offset = info.register.Register(fileSize);

                                ByteUtil.ToByteArray((int)srcStream.Length, buffer);
                                ByteUtil.ToByteArray((int)ms.Length, buffer, 4);
                                buffer[8] = 0x78; //ZLIB
                                buffer[9] = 0x9C; //HEADER

                                ms.Read(buffer, 10, (int)ms.Length);

                                fs.Seek(dataOffset + offset, SeekOrigin.Begin);

                                fs.Write(buffer, 0, (int)ms.Length + 10);
                            }
                        }
                        else
                        {
                            fileSize += srcStream.Length;

                            long offset = info.register.Register(fileSize);

                            fs.Seek(dataOffset + offset, SeekOrigin.Begin);

                            fs.Write(((int)srcStream.Length).ToByteArray(info.buffers[4]), 0, 4);

                            fs.Write(buffer, 0, (int)srcStream.Length);
                        }
                    }
                }
            }

            info.isPacking = false;
            
            log.Info("Unpack thread (ID:" + info.threadID + ") finished work!");

            if (!IsPacking)
            {
                log.Info("All packing threads finished their work!");
            }
        }

        public void PreallocateSpace(FileInfo file, long space, int offset)
        {
            log.Info("Preallocating " + (space / 1024 / 1024) + "MB in file " + file.FullName);
            stopwatch.Reset();
            stopwatch.Start();
            using (FileStream fs = File.OpenWrite(file.FullName))
            {
                fs.Seek(offset, SeekOrigin.Begin);

                for (long i = 0; i < space; i++)
                {
                    fs.WriteByte(0x00);
                }
            }
            log.Info("Preallocating done!  Time taken: " + stopwatch.ElapsedMilliseconds + "ms");
            stopwatch.Stop();
        }

        public long CalculateRequiredSpace(BigFileFlags flags)
        {
            log.Info("Calculating required space for directory " + bigFile.Directory);
            log.Info("Flags: " + flags);

            long space = 0;
            int fileCount = 0;

            Action<DirectoryInfo> recursion = null;
            recursion = (DirectoryInfo subDir) =>
            {
                foreach (FileInfo file in subDir.GetFiles())
                {
                    int size = 4; //4 for the size info
                    using (FileStream fs = File.OpenRead(file.FullName))
                    {
                        if ((flags & BigFileFlags.Compress) != 0)
                        {
                            size += 4; //4 for compressed size info
                            size += 2; //4 for zlib header info
                            using (MemoryStream ms = new MemoryStream((int)fs.Length))
                            using (DeflateStream ds = new DeflateStream(ms, CompressionMode.Compress, true))
                            {
                                byte[] buffer = buffers[fs.Length];
                                fs.Read(buffer, 0, (int)fs.Length);
                                ds.Write(buffer, 0, (int)fs.Length);

                                ms.Seek(0, SeekOrigin.Begin);

                                while (ms.ReadByte() != -1)
                                {
                                    size++;
                                }
                            }
                        }
                        else
                        {
                            while (fs.ReadByte() != -1)
                            {
                                size++;
                            }
                        }
                    }
                    space += size;
                    fileCount++;
                    if (fileCount % 500 == 0)
                        log.Info("Progress: " + fileCount + "/" + bigFile.FileMap.FilesList.Length);
                }

                foreach (DirectoryInfo dirInfo in subDir.GetDirectories())
                {
                    recursion.Invoke(dirInfo);
                }
            };

            log.Info(fileCount.ToString());

            recursion.Invoke(bigFile.Directory);

            log.Info("Space required: " + (space / 1024 / 1024) + "MB");

            return space;
        }
    }
}
