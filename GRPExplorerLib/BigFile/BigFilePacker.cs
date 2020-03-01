﻿using System;
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
using GRPExplorerLib.BigFile.Files;
using Ionic.Zlib;

namespace GRPExplorerLib.BigFile
{
    public struct BigFilePackOptions
    {
        public DirectoryInfo Directory;
        public string BigFileName;
        public BigFileFlags Flags;
        public int Threads;
        public bool DeleteChunks;

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
            public DiagTools diag = new DiagTools();
            public BigFile bigFile;
            public YetiObject[] filesList;
            public Action<BigFilePackInfo> OnCompleted;
            public int filesChunked = 0;

            public int progress = 0;
        }

        private struct ChunkedFileMetadata
        {
            public int Number;
            public int Key;
            public int Offset;
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

            //MAKE SURE DECOMPRESS FLAG IS SET
            // otherwise we run the risk of not decompressing files when we need to
            options.Flags |= BigFileFlags.Decompress; 

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
                log.Error("Single threaded pack not supported!");

                //packInfos[0].Options = options;
                //packInfos[0].startIndex = 0;
                //packInfos[0].count = bigFile.FileMap.FilesList.Length;
                //packInfos[0].bigFile = bigFile;
                //packInfos[0].OnCompleted = internal_OnPackFinished;

                //internal_GenBigFileChunk(packInfos[0]);

                //log.Info("Finished pack!");

                return null;
            }
        }

        private BigFilePackOperationStatus internal_ThreadedPack(BigFilePackOptions options)
        {
            //set up threads
            int dividedCount = bigFile.FileMap.FilesList.Length / options.Threads;
            int dividedRemainder = bigFile.FileMap.FilesList.Length % options.Threads;
            log.Info("Divided files into " + options.Threads + " pools of " + dividedCount + " with " + dividedRemainder + " left over (to be tacked onto the last!)");

            YetiObject[] files = new YetiObject[bigFile.FileMap.FilesList.Length];
            Array.Copy(bigFile.FileMap.FilesList, files, files.Length);
            Array.Sort(files,
                (fileA, fileB) =>
                {
                    return fileA.FileInfo.Offset.CompareTo(fileB.FileInfo.Offset);
                });

            List<BigFilePackInfo> infos = new List<BigFilePackInfo>();
            for (int i = 0; i < options.Threads; i++)
            {
                packInfos[i].Options = options;
                packInfos[i].startIndex = i * dividedCount;
                packInfos[i].count = dividedCount;
                packInfos[i].bigFile = bigFile;
                packInfos[i].filesList = files;
            }
            packInfos[options.Threads - 1].count += dividedRemainder;
            packInfos[options.Threads - 1].OnCompleted = internal_OnPackFinished; //the last thread gets the job of stitching together the chunks

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
            info.diag.StartStopwatch();

            string tempDir = Environment.CurrentDirectory + BigFileConst.PACK_STAGING_DIR;
            Directory.CreateDirectory(tempDir);
            string chunkFileName = tempDir + info.Options.BigFileName + ".chunk" + info.ThreadID.ToString();
            string metadataFilename = tempDir + info.Options.BigFileName + ".meta" + info.ThreadID.ToString();

            log.Info("Generating bigfile chunk: " + chunkFileName);
            
            using (FileStream chunkFS = new FileStream(chunkFileName, FileMode.Create, FileAccess.Write))
            using (FileStream metaFS = new FileStream(metadataFilename, FileMode.Create, FileAccess.Write))
            using (BinaryWriter chunkBW = new BinaryWriter(chunkFS))
            using (BinaryWriter metaBW = new BinaryWriter(metaFS))
            {
                //fill 8 bytes to be filled with number of files chunked and final size later
                metaBW.Write(-1);
                metaBW.Write(-1L);

                YetiObject[] filesToWrite = new YetiObject[info.count];
                Array.Copy(info.filesList, info.startIndex, filesToWrite, 0, info.count);

                log.Info("Thread ID {0} - First file is {1}", info.ThreadID, filesToWrite[0].Name);
                log.Info("Thread ID {0} - Last file is {1}", info.ThreadID, filesToWrite[filesToWrite.Length - 1].Name);
                
                //foreach (int size in bigFile.FileReader.ReadAllRaw(filesToWrite, info.IOBuffers, info.Options.Flags))
                foreach (BigFileFileRead read in bigFile.FileReader.ReadAllFiles(filesToWrite.ToList(), info.IOBuffers, info.Options.Flags))
                {
                    //if (currFile.FileInfo.FileType == 0x3c)
                    //{
                    //    log.Error("Skipped file {0}", currFile.Name);
                    //    index++;
                    //    continue;
                    //}

                    if (read.IsError())
                    {
                        metaBW.Write("ERR0");
                        metaBW.Write(read.file.FileInfo.FileNumber);
                        metaBW.Write(read.file.FileInfo.Key);
                        metaBW.Write(-1L);
                    }
                    else
                    {
                        log.Debug("Packing file {0}, size: {1}, ZIP: {2}", read.file.Name, read.dataSize, read.file.FileInfo.ZIP);

                        //write the file number, key, and offset to metadata file
                        metaBW.Write("FILE");
                        metaBW.Write(read.file.FileInfo.FileNumber);
                        metaBW.Write(read.file.FileInfo.Key);
                        metaBW.Write(chunkFS.Position);

                        int totalSize = read.dataSize + (read.header.Length * 4 + 4);

                        if (read.file.FileInfo.FileType == YetiObjects.YetiObjectType.ini)
                        {
                            for (int i = 0; i < read.header.Length; i++)
                            {
                                log.Info("Universe.ini ref {0:X8}", read.header[i]);
                            }
                        }

                        if (read.file.FileInfo.ZIP == 1 && (info.Options.Flags & BigFileFlags.Compress) != 0)
                        {
                            int sizePos = (int)chunkFS.Position;
                            chunkFS.Write(info.IOBuffers[8], 0, 8); //write 8 bytes of garbage to fill the space for decompressed and compressed size
                            using (ZlibStream zs = new ZlibStream(chunkFS, Ionic.Zlib.CompressionMode.Compress, true))
                            using (BinaryWriter bw = new BinaryWriter(zs))
                            {
                                bw.Write(read.header.Length);
                                for (int i = 0; i < read.header.Length; i++)
                                    bw.Write(read.header[i]);
                                zs.Write(info.IOBuffers[read.dataSize], 0, read.dataSize);
                            }

                            int newPos = (int)chunkFS.Position;
                            int remainder = ((((newPos - sizePos) - 1) / 8 + 1) * 8) - (newPos - sizePos);

                            for (int i = 0; i < remainder; i++)
                                chunkFS.WriteByte(0x00);

                            newPos = (int)chunkFS.Position;

                            int compressedSize = newPos - sizePos - 4;
                            
                            //go back to the file offset and write the compressed and decompressed sizes
                            chunkFS.Seek(sizePos, SeekOrigin.Begin);
                            chunkBW.Write(compressedSize);
                            chunkBW.Write(totalSize);
                            chunkFS.Seek(newPos, SeekOrigin.Begin);
                        }
                        else
                        {
                            int sizePos = (int)chunkFS.Position;
                            chunkBW.Write(totalSize);
                            chunkBW.Write(read.header.Length);
                            for (int i = 0; i < read.header.Length; i++)
                                chunkBW.Write(read.header[i]);
                            chunkFS.Write(info.IOBuffers[read.dataSize], 0, read.dataSize);
                            int remainder = (((((int)chunkFS.Position - sizePos) - 1) / 8 + 1) * 8) - ((int)chunkFS.Position - sizePos);
                            for (int i = 0; i < remainder; i++)
                                chunkFS.WriteByte(0x00);
                        }
                    }

                    info.filesChunked++;
                }

                //write number of files chunked and final file size
                metaFS.Seek(0, SeekOrigin.Begin);
                metaBW.Write(info.filesChunked);
                metaBW.Write(chunkFS.Length);

                if (chunkFS.Length % 8 != 0)
                    throw new Exception("oh shit what happened");
            }

            info.isPacking = false;
            info.diag.StopStopwatch();

            log.Info("Thread (ID: {0}) finished chunking work, time: {1,5}s", info.ThreadID, info.diag.StopwatchTime / 1000);

            if (info.OnCompleted != null)
                info.OnCompleted.Invoke(info);
        }

        private void internal_OnPackFinished(BigFilePackInfo info)
        {
            while (IsPacking)
                Thread.Sleep(500); //wait for all threads to finish

            log.Info("All chunking threads finished their work!");
            log.Info(" >Chunking result:");
            log.Info("   {0,6}   {1,6}   {2,6}   {3,6}", "Thread", "Time", "Start", "Count");
            for (int i = 0; i < info.Options.Threads; i++)
                log.Info("   {0,6}        {1,4}s  {2,6}   {3,6}", packInfos[i].ThreadID, packInfos[i].diag.StopwatchTime / 1000, packInfos[i].startIndex, packInfos[i].count);

            log.Info("Starting packaging");

            string targetFileName = info.Options.Directory.FullName + @"\" + info.Options.BigFileName + BigFileConst.BIGFILE_EXTENSION;
            FileInfo targetFileInfo = new FileInfo(targetFileName);
            if (targetFileInfo.Exists)
            {
                WinMessageBoxResult overwriteResult = WinMessageBox.Show("The file\n" + targetFileName + "\n already exists.\n\nOverwrite?", "File already exists", WinMessageBoxFlags.btnYesNo | WinMessageBoxFlags.iconQuestion);
                if (overwriteResult != WinMessageBoxResult.Yes)
                {
                    log.Error("Target file already exists and the user chose not to overwrite!");
                    if (info.Options.DeleteChunks)
                    {
                        log.Info("Deleting generated chunks!");

                        for (int threadID = 0; threadID < info.Options.Threads; threadID++)
                        {
                            string metadataFilename = Environment.CurrentDirectory + BigFileConst.PACK_STAGING_DIR + info.Options.BigFileName + ".meta" + packInfos[threadID].ThreadID.ToString();
                            string chunkFileName = Environment.CurrentDirectory + BigFileConst.PACK_STAGING_DIR + info.Options.BigFileName + ".chunk" + packInfos[threadID].ThreadID.ToString();
                            File.Delete(metadataFilename);
                            File.Delete(chunkFileName);
                            log.Info("Deleted metadata file {0}", metadataFilename);
                            log.Info("Deleted chunk file {0}", chunkFileName);
                        }
                    }

                    return;
                }
            }

            info.diag.StartStopwatch();
            
            using (FileStream targetFS = new FileStream(targetFileName, FileMode.Create, FileAccess.Write))
            {
                //Dictionary<int, ChunkedFileMetadata> metadataMap = new Dictionary<int, ChunkedFileMetadata>();
                List<ChunkedFileMetadata> metadataList = new List<ChunkedFileMetadata>();
                int chunkedFileOffsetInTargetFile = 0;
                for (int threadID = 0; threadID < info.Options.Threads; threadID++)
                {
                    string metadataFilename = Environment.CurrentDirectory + BigFileConst.PACK_STAGING_DIR + info.Options.BigFileName + ".meta" + packInfos[threadID].ThreadID.ToString();

                    log.Info("Collating metadata from file " + metadataFilename);

                    //extract the metadata from the metadata files
                    using (FileStream metaFS = new FileStream(metadataFilename, FileMode.Open, FileAccess.Read))
                    using (BinaryReader metaBR = new BinaryReader(metaFS))
                    {
                        int fileCount = metaBR.ReadInt32();
                        long chunkFileSize = metaBR.ReadInt64();
                        
                        for (int j = 0; j < fileCount; j++)
                        {
                            string type = metaBR.ReadString();
                            if (type != "ERR0" && type != "FILE")
                                throw new Exception("omg wtf");

                            ChunkedFileMetadata mdata = new ChunkedFileMetadata()
                            {
                                Number = metaBR.ReadInt32(),
                                Key = metaBR.ReadInt32(),
                            };

                            long offset = metaBR.ReadInt64();
                            if (offset != -1)
                            {
                                offset = offset / 8;
                                offset += chunkedFileOffsetInTargetFile;
                            }

                            mdata.Offset = (int)offset;

                            metadataList.Add(mdata);
                        }

                        chunkedFileOffsetInTargetFile += (int)(chunkFileSize / 8);
                    }

                    if (info.Options.DeleteChunks)
                    {
                        log.Info("Deleting metadata file...");
                        File.Delete(metadataFilename);
                    }
                }

                log.Info("Metadata collation took {0,4}s", info.diag.StopwatchTime / 1000);
                info.diag.StartStopwatch();
                
                //write the segment header to the target bigfile
                log.Info("Writing segment header to new bigfile...");
                info.bigFile.Segment.WriteSegmentHeader(targetFS, ref info.bigFile.SegmentHeader);
                log.Info("Segment header written!");

                //create a new header with the number of files we're packing
                log.Info("Writing file header to new bigfile...");
                BigFileHeaderStruct header = new BigFileHeaderStruct()
                {
                    Files = metadataList.Count,
                    Folders = (short)info.bigFile.RawFolderInfos.Length, //oh boy
                    BigFileVersion = info.bigFile.Version.Identifier,
                    Unknown_02 = info.bigFile.FileHeader.Unknown_02,
                };
                header.DebugLog(log);
                info.bigFile.Header.WriteHeader(targetFS, ref header);
                log.Info("File header written!");

                //create a list of file infos to write, copying all but the offset and file number from the original file info
                log.Info("Creating new file info list...");
                IBigFileFileInfo[] newFileInfos = new IBigFileFileInfo[metadataList.Count];
                for (int i = 0; i < metadataList.Count; i++)
                {
                    newFileInfos[i] = info.bigFile.Version.CreateFileInfo();

                    info.bigFile.FileMap[metadataList[i].Key].FileInfo.Copy(newFileInfos[i]);

                    if (metadataList[i].Offset == -1)
                    {
                        newFileInfos[i].Offset = -1;
                        log.Error("METATADA FILE OFFSET IS -1");
                    }
                    else
                    {
                        //if (metadataList[i].Offset % 8 != 0)
                        //    log.Error("WAIT WHAT: {0} {1:X4}", metadataList[i].Offset, metadataList[i].Key);
                        newFileInfos[i].Offset = metadataList[i].Offset;
                    }
                    newFileInfos[i].FileNumber = metadataList[i].Number;
                    newFileInfos[i].ZIP = (info.Options.Flags & BigFileFlags.Compress) != 0 ? newFileInfos[i].ZIP : 0;
                }
                log.Info("New file info list created!");

                log.Info("Writing file and folder infos to new bigfile...");
                //write file infos to file
                info.bigFile.FilesAndFolders.WriteFileInfos(targetFS, newFileInfos);
                //write folder infos to file
                info.bigFile.FilesAndFolders.WriteFolderInfos(targetFS, info.bigFile.RawFolderInfos);
                log.Info("File and folder infos written!");

                log.Info("File metadata generation took {0,4}s", info.diag.StopwatchTime / 1000);
                info.diag.StartStopwatch();

                //copy chunk file data to target bigfile
                for (int threadID = 0; threadID < info.Options.Threads; threadID++)
                {
                    string chunkFileName = Environment.CurrentDirectory + BigFileConst.PACK_STAGING_DIR + info.Options.BigFileName + ".chunk" + packInfos[threadID].ThreadID.ToString();

                    log.Info("Copying chunk data from chunk {0}", chunkFileName);
                    log.Info(" Current offset: {0:X8}", targetFS.Position);

                    byte[] buffer = info.IOBuffers[IOBuffers.MB * 36];
                    using (FileStream chunkFS = new FileStream(chunkFileName, FileMode.Open, FileAccess.Read))
                    {
                        int readSize = -1;
                        while ((readSize = chunkFS.Read(buffer, 0, IOBuffers.MB * 36)) != 0)
                        {
                            
                             targetFS.Write(buffer, 0, readSize);
                        }
                    }

                    log.Info("Chunk data copied!  Current offset: {0:X8}", targetFS.Position);

                    if (info.Options.DeleteChunks)
                    {
                        log.Info("Deleting chunk...");
                        File.Delete(chunkFileName);
                    }
                }

                log.Info("All chunk data written!");
                log.Info("Chunk data copying time taken: {0,4}s", info.diag.StopwatchTime / 1000);
                log.Info("Bigfile packing finished!");
            }
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
                packInfos[i].filesChunked = 0;
            }
        }
    }
}
