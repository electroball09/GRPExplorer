using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.Runtime.InteropServices;
using GRPExplorerLib.Util;
using System.Diagnostics;
using GRPExplorerLib.Logging;
using GRPExplorerLib.BigFile.Versions;

namespace GRPExplorerLib.BigFile
{
    public class YetiHeaderFile
    {
        private ILogProxy log = LogManager.GetLogProxy("YetiHeaderFile");

        private FileInfo fileInfo;

        private Stopwatch stopwatch = new Stopwatch();

        private IOBuffers buffers = new IOBuffers();

        private IBigFileVersion version;
        public IBigFileVersion BigFileVersion { get { return version; } set { version = value; } }

        public YetiHeaderFile(FileInfo _fileInfo)
        {
            if (!_fileInfo.Exists)
                log.Error("_fileInfo doesn't exist!");

            fileInfo = _fileInfo;

            log.Info("Create YetiHeaderFile, file: " + fileInfo.FullName);
        }

        public int CalculateFolderOffset(ref BigFileHeader header, ref BigFileFileCountInfo countInfo)
        {
            if (version == null)
                throw new NullReferenceException("There's no version!  Can't calculate folder offset!");

            IBigFileFileInfo tmpFileInfo = version.CreateFileInfo();
            int baseSize = (header.InfoOffset + countInfo.StructSize) + (countInfo.Files * tmpFileInfo.StructSize);
            baseSize += baseSize % 8; // align to 8 bytes
            return baseSize;
        }

        public int CalculateDataOffset(ref BigFileHeader header, ref BigFileFileCountInfo countInfo)
        {
            if (version == null)
                throw new NullReferenceException("There's no version!  Can't calculate data offset!");

            IBigFileFolderInfo tmpFolderInfo = version.CreateFolderInfo();
            int folderOffset = CalculateFolderOffset(ref header, ref countInfo);
            int dataOffset = folderOffset + (countInfo.Folders * tmpFolderInfo.StructSize);
            dataOffset += dataOffset % 8; // align to 8 bytes;
            return dataOffset;
        }

        public BigFileHeader ReadHeader()
        {
            BigFileHeader header = new BigFileHeader();

            log.Info("Reading header...");
            log.Debug("Header struct size: " + header.StructSize);

            stopwatch.Reset();
            stopwatch.Start();

            byte[] bytes = buffers[header.StructSize];
            using (FileStream fs = File.OpenRead(fileInfo.FullName))
            {
                fs.Read(bytes, 0, header.StructSize);
            }

            header = MarshalUtil.BytesToStruct<BigFileHeader>(bytes);

            float readTime = stopwatch.ElapsedMilliseconds;

            header.DebugLog(log);

            stopwatch.Stop();

            float logTime = stopwatch.ElapsedMilliseconds - readTime;

            log.Info("Read time was " + readTime + "ms, log time was " + logTime + "ms");

            return header;
        }

        public BigFileFileCountInfo ReadFileCountInfo(ref BigFileHeader header)
        {
            BigFileFileCountInfo countInfo = new BigFileFileCountInfo();

            log.Info("Reading CountInfo...");
            log.Debug("CountInfo struct size: " + countInfo.StructSize);

            stopwatch.Reset();
            stopwatch.Start();

            byte[] bytes = buffers[countInfo.StructSize];
            using (FileStream fs = File.OpenRead(fileInfo.FullName))
            {
                fs.Seek(header.InfoOffset, SeekOrigin.Begin);
                fs.Read(bytes, 0, countInfo.StructSize);
            }

            countInfo = MarshalUtil.BytesToStruct<BigFileFileCountInfo>(bytes);

            float readTime = stopwatch.ElapsedMilliseconds;

            countInfo.DebugLog(log);

            stopwatch.Stop();

            float logTime = stopwatch.ElapsedMilliseconds - readTime;

            log.Info("Read time was " + readTime + "ms, log time was " + logTime + "ms");

            return countInfo;
        }

        public IBigFileFolderInfo[] ReadFolderInfos(ref BigFileHeader header, ref BigFileFileCountInfo countInfo)
        {
            IBigFileFolderInfo tmpFolderInfo = version.CreateFolderInfo();

            log.Info("Reading folder infos...  Count: " + countInfo.Folders);
            log.Debug("FolderInfo struct size: " + tmpFolderInfo.StructSize);

            stopwatch.Reset();
            stopwatch.Start();

            IBigFileFolderInfo[] folderInfos = new IBigFileFolderInfo[countInfo.Folders];

            int folderOffset = CalculateFolderOffset(ref header, ref countInfo);
            byte[] bytes = buffers[tmpFolderInfo.StructSize];
            using (FileStream fs = File.OpenRead(fileInfo.FullName))
            {
                fs.Seek(folderOffset, SeekOrigin.Begin);
                for (short i = 0; i < countInfo.Folders; i++)
                {
                    fs.Read(bytes, 0, tmpFolderInfo.StructSize);

                    folderInfos[i] = tmpFolderInfo.FromBytes(bytes);

                    folderInfos[i].DebugLog(log);
                }
            }

            stopwatch.Stop();
            float timeTaken = stopwatch.ElapsedMilliseconds;

            log.Info("Time taken: " + timeTaken + "ms");

            return folderInfos;
        }

        public IBigFileFileInfo[] ReadFileInfos(ref BigFileHeader header, ref BigFileFileCountInfo countInfo)
        {
            IBigFileFileInfo tmpFileInfo = version.CreateFileInfo();

            log.Info("Reading file infos...  Count: " + countInfo.Files);
            log.Debug("FileInfo struct size: " + tmpFileInfo.StructSize);

            stopwatch.Reset();
            stopwatch.Start();

            IBigFileFileInfo[] fileInfos = new IBigFileFileInfo[countInfo.Files];

            int baseOffset = header.InfoOffset + countInfo.StructSize;
            log.Debug("baseOffset: " + baseOffset);
            byte[] bytes = buffers[tmpFileInfo.StructSize];
            using (FileStream fs = File.OpenRead(fileInfo.FullName))
            {
                fs.Seek(baseOffset, SeekOrigin.Begin);
                for (int i = 0; i < countInfo.Files; i++)
                {
                    fs.Read(bytes, 0, tmpFileInfo.StructSize);

                    fileInfos[i] = tmpFileInfo.FromBytes(bytes);

                    fileInfos[i].DebugLog(log);
                }
            }

            stopwatch.Stop();
            float timeTaken = stopwatch.ElapsedMilliseconds;

            log.Info("Time taken: " + timeTaken + "ms");

            return fileInfos;
        }

        public FileBuffer ReadFileAndFolderMetadataRaw(ref BigFileHeader header, ref BigFileFileCountInfo countInfo)
        {
            int dataOffset = CalculateDataOffset(ref header, ref countInfo);

            byte[] bytes = buffers[dataOffset];
            using (FileStream fs = File.OpenRead(fileInfo.FullName))
            {
                fs.Read(bytes, 0, dataOffset);
            }

            return new FileBuffer(bytes, dataOffset);
        }

        public void WriteYetiHeader(BigFile bigFile)
        {
            log.Info("Writing Yeti header to file " + fileInfo.FullName);

            using (FileStream fs = File.OpenWrite(fileInfo.FullName))
            {
                byte[] buffer = buffers[bigFile.FileHeader.StructSize];
                MarshalUtil.StructToBytes<BigFileHeader>(bigFile.FileHeader, buffer);
                fs.Write(buffer, 0, bigFile.FileHeader.StructSize);

                int blankBytesToWrite = bigFile.FileHeader.InfoOffset - bigFile.FileHeader.StructSize;

                for (int i = 0; i < blankBytesToWrite; i++)
                {
                    fs.WriteByte(0x00);
                }

                BigFileFileCountInfo countInfo = new BigFileFileCountInfo()
                {
                    Files = bigFile.MappingData.FilesList.Length,
                    Folders = (short)bigFile.RootFolder.FolderMap.Count,
                    BigFileVersion = bigFile.CountInfo.BigFileVersion,
                    Unknown_02 = bigFile.CountInfo.Unknown_02
                };

                buffer = buffers[countInfo.StructSize];
                MarshalUtil.StructToBytes(countInfo, buffer);
                fs.Write(buffer, 0, countInfo.StructSize);
            }

            log.Info("Header written!");
        }

        public void WriteYetiFileAndFolderInfo(BigFile bigFile)
        {
            log.Info("Writing bigfile file and folder infos to file " + fileInfo.FullName);

            using (FileStream fs = File.OpenWrite(fileInfo.FullName))
            {
                fs.Seek(bigFile.FileHeader.InfoOffset + bigFile.CountInfo.StructSize, SeekOrigin.Begin);

                int fileStructSize = bigFile.MappingData.FilesList[0].FileInfo.StructSize;
                int remainder = (bigFile.MappingData.FilesList.Length * fileStructSize) % 8;
                log.Info("File struct remainder: " + remainder);
                byte[] buffer = buffers[fileStructSize];
                for (int i = 0; i < bigFile.MappingData.FilesList.Length; i++)
                {
                    bigFile.MappingData.FilesList[i].FileInfo.ToBytes(buffer);
                    fs.Write(buffer, 0, fileStructSize);
                }

                for (int i = 0; i < remainder; i++)
                    fs.WriteByte(0x00);

                int folderStructSize = bigFile.RootFolder.InfoStruct.StructSize;
                remainder = (bigFile.RootFolder.FolderMap.Count * folderStructSize) % 8;
                log.Info("Folder struct remainder: " + remainder);
                buffer = buffers[folderStructSize];
                foreach (KeyValuePair<short, BigFileFolder> kvp in bigFile.RootFolder.FolderMap)
                {
                    kvp.Value.InfoStruct.ToBytes(buffer);
                    fs.Write(buffer, 0, folderStructSize);
                }

                for (int i = 0; i < remainder; i++)
                    fs.WriteByte(0x00);
            }

            log.Info("Infos written!");
        }
    }
}
