using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.Runtime.InteropServices;
using GRPExplorerLib.Util;
using System.Diagnostics;

namespace GRPExplorerLib.BigFile
{
    public class BigFileIO
    {
        private LogProxy log;

        private FileInfo fileInfo;

        private Stopwatch stopwatch = new Stopwatch();

        private IOBuffers buffers = new IOBuffers();

        public BigFileIO(FileInfo _fileInfo)
        {
            if (!_fileInfo.Exists)
                throw new Exception("_fileInfo doesn't exist!");

            fileInfo = _fileInfo;

            log = new LogProxy("BigFileIO");

            log.Info("Create BigFileIO, file: " + fileInfo.FullName);
        }

        public int CalculateFolderOffset(ref BigFileHeader header, ref BigFileFileCountInfo countInfo)
        {
            BigFileFileInfo tmpFileInfo = new BigFileFileInfo();
            int baseSize =(header.InfoOffset + countInfo.StructSize) + (countInfo.Files * tmpFileInfo.StructSize);
            baseSize += baseSize % 8; // align to 8 bytes
            return baseSize;
        }

        public int CalculateDataOffset(ref BigFileHeader header, ref BigFileFileCountInfo countInfo)
        {
            BigFileFolderInfo tmpFolderInfo = new BigFileFolderInfo();
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

        public BigFileFolderInfo[] ReadFolderInfos(ref BigFileHeader header, ref BigFileFileCountInfo countInfo)
        {
            BigFileFolderInfo tmpFolderInfo = new BigFileFolderInfo();

            log.Info("Reading folder infos...  Count: " + countInfo.Folders);
            log.Debug("FolderInfo struct size: " + tmpFolderInfo.StructSize);

            stopwatch.Reset();
            stopwatch.Start();

            BigFileFolderInfo[] folderInfos = new BigFileFolderInfo[countInfo.Folders];

            int folderOffset = CalculateFolderOffset(ref header, ref countInfo);
            byte[] bytes = buffers[tmpFolderInfo.StructSize];
            using (FileStream fs = File.OpenRead(fileInfo.FullName))
            {
                fs.Seek(folderOffset, SeekOrigin.Begin);
                for (short i = 0; i < countInfo.Folders; i++)
                {
                    fs.Read(bytes, 0, tmpFolderInfo.StructSize);

                    folderInfos[i] = MarshalUtil.BytesToStruct<BigFileFolderInfo>(bytes);

                    folderInfos[i].DebugLog(log);
                }
            }

            stopwatch.Stop();
            float timeTaken = stopwatch.ElapsedMilliseconds;

            log.Info("Time taken: " + timeTaken + "ms");

            return folderInfos;
        }

        public BigFileFileInfo[] ReadFileInfos(ref BigFileHeader header, ref BigFileFileCountInfo countInfo)
        {
            BigFileFileInfo tmpFileInfo = new BigFileFileInfo();

            log.Info("Reading file infos...  Count: " + countInfo.Files);
            log.Debug("FileInfo struct size: " + tmpFileInfo.StructSize);

            stopwatch.Reset();
            stopwatch.Start();

            BigFileFileInfo[] fileInfos = new BigFileFileInfo[countInfo.Files];

            int baseOffset = header.InfoOffset + countInfo.StructSize;
            byte[] bytes = buffers[tmpFileInfo.StructSize];
            using (FileStream fs = File.OpenRead(fileInfo.FullName))
            {
                fs.Seek(baseOffset, SeekOrigin.Begin);
                for (int i = 0; i < countInfo.Files; i++)
                {
                    fs.Read(bytes, 0, tmpFileInfo.StructSize);
                    fileInfos[i] = MarshalUtil.BytesToStruct<BigFileFileInfo>(bytes);

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

        public FileBuffer ReadFileRaw(BigFileFileInfo file, ref BigFileHeader header, ref BigFileFileCountInfo countInfo)
        {
            int dataOffset = CalculateDataOffset(ref header, ref countInfo);

            byte[] bytes = buffers[4];
            int size = 0;
            using (FileStream fs = File.OpenRead(fileInfo.FullName))
            {
                uint offset = (uint)dataOffset + ((uint)file.Offset * 8);
                fs.Seek(offset, SeekOrigin.Begin);
                fs.Read(bytes, 0, 4);
                size = BitConverter.ToInt32(bytes, 0);
                bytes = buffers[size];
                fs.Read(bytes, 0, size);
            }

            return new FileBuffer(bytes, size);
        }
    }


}
