using System;
using System.Collections.Generic;
using System.Collections;
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
    public class BigFileFilesAndFolders
    {
        ILogProxy log = LogManager.GetLogProxy("BigFileFilesAndFolders");
        IOBuffers buffers = new IOBuffers();
        FileInfo fileInfo;
        IBigFileVersion version;
        public IBigFileVersion Version { get { return version; } set { version = value; } }
        DiagTools diag = new DiagTools();

        public BigFileFilesAndFolders(FileInfo _fileInfo)
        {
            fileInfo = _fileInfo;
        }

        public IBigFileFolderInfo[] ReadFolderInfos(ref BigFileSegmentHeader segmentHeader, ref BigFileHeaderStruct header)
        {
            if (fileInfo == null || !fileInfo.Exists)
                throw new Exception("File info cannot be null!");

            log.Debug("Reading folder infos from file {0}", fileInfo.FullName);

            IBigFileFolderInfo[] infos;
            using (FileStream fs = File.OpenRead(fileInfo.FullName))
            {
                infos = ReadFolderInfos(fs, ref segmentHeader, ref header);
            }
            return infos;
        }

        public IBigFileFolderInfo[] ReadFolderInfos(Stream stream, ref BigFileSegmentHeader segmentHeader, ref BigFileHeaderStruct header)
        {
            if (version == null)
                throw new NullReferenceException("Version cannot be null!");

            log.Info("Reading big file folders, count: {0}", header.Folders);
            BigFileVersions.DebugLogVersion(version, log);

            diag.StartStopwatch();

            IBigFileFolderInfo[] infos = new IBigFileFolderInfo[header.Folders];
            IBigFileFolderInfo tmpInfo = version.CreateFolderInfo();

            int folderOffset = BigFileUtil.CalculateFolderOffset(version, ref segmentHeader, ref header);
            byte[] buffer = buffers[tmpInfo.StructSize];
            stream.Seek(folderOffset, SeekOrigin.Begin);
            for (short i = 0; i < header.Folders; i++)
            {
                stream.Read(buffer, 0, tmpInfo.StructSize);
                infos[i] = tmpInfo.FromBytes(buffer);
                infos[i].DebugLog(log);
            }

            log.Info("Folder infos read!  Time taken: {0}ms", diag.StopwatchTime);

            return infos;
        }

        public IBigFileFileInfo[] ReadFileInfos(ref BigFileSegmentHeader segmentHeader, ref BigFileHeaderStruct header)
        {
            if (fileInfo == null || !fileInfo.Exists)
                throw new Exception("File info cannot be null!");

            log.Debug("Reading file infos from file {0}", fileInfo.FullName);

            IBigFileFileInfo[] infos;
            using (FileStream fs = File.OpenRead(fileInfo.FullName))
            {
                infos = ReadFileInfos(fs, ref segmentHeader, ref header);
            }
            return infos;
        }

        public IBigFileFileInfo[] ReadFileInfos(Stream stream, ref BigFileSegmentHeader segmentHeader, ref BigFileHeaderStruct header)
        {
            if (version == null)
                throw new NullReferenceException("Version cannot be null!");

            log.Info("Reading big file file infos, count: {0}", header.Files);
            BigFileVersions.DebugLogVersion(version, log);

            diag.StartStopwatch();

            IBigFileFileInfo[] infos = new IBigFileFileInfo[header.Files];
            IBigFileFileInfo tmpInfo = version.CreateFileInfo();

            int fileOffset = segmentHeader.InfoOffset + header.StructSize;
            log.Debug("File info offset: {0:X8}", fileOffset);
            byte[] buffer = buffers[tmpInfo.StructSize];
            stream.Seek(fileOffset, SeekOrigin.Begin);
            for (int i = 0; i < header.Files; i++)
            {
                stream.Read(buffer, 0, tmpInfo.StructSize);
                infos[i] = tmpInfo.FromBytes(buffer);
                infos[i].DebugLog(log);
            }

            log.Info("File infos read!  Time taken: {0}ms", diag.StopwatchTime);
            log.Info("File count: {0}", header.Files);

            return infos;
        }

        public void WriteFileInfos(ref BigFileSegmentHeader segmentHeader, ref BigFileHeaderStruct header, IBigFileFileInfo[] infos)
        {
            if (fileInfo == null || !fileInfo.Exists)
                throw new Exception("File info cannot be null!");

            log.Debug("Writing file infos to file {0}", fileInfo.FullName);

            using (FileStream fs = File.OpenWrite(fileInfo.FullName))
            {
                int fileOffset = segmentHeader.InfoOffset + header.StructSize;
                fs.Seek(fileOffset, SeekOrigin.Begin);
                WriteFileInfos(fs, infos);
            }
        }

        public void WriteFileInfos(Stream stream, IBigFileFileInfo[] infos)
        {
            log.Info("Writing file infos to a stream, count: {0}", infos.Length);

            diag.StartStopwatch();

            int totalSize = infos[0].StructSize * infos.Length;
            int remainder = (((totalSize - 1) / 8 + 1) * 8) - totalSize; //align to 8 bytes
            log.Debug("Total length: {0:X8}", totalSize);
            log.Debug("Remainder: {0}", remainder);

            byte[] buffer = buffers[infos[0].StructSize];
            for (int i = 0; i < infos.Length; i++)
            {
                int size = infos[i].ToBytes(buffer);
                stream.Write(buffer, 0, size);
            }

            for (int i = 0; i < remainder; i++)
            {
                //fill 8 byte aligned remainder with nothing
                stream.WriteByte(0x00);
            }

            log.Info("File infos written!  Time taken: {0}ms", diag.StopwatchTime);
        }

        public void WriteFolderInfos(ref BigFileSegmentHeader segmentHeader, ref BigFileHeaderStruct header, IBigFileFolderInfo[] infos)
        {
            if (fileInfo == null || !fileInfo.Exists)
                throw new Exception("File info cannot be null!");

            log.Debug("Writing folder infos to file {0}", fileInfo.FullName);

            using (FileStream fs = File.OpenWrite(fileInfo.FullName))
            {
                int folderOffset = BigFileUtil.CalculateFolderOffset(version, ref segmentHeader, ref header);
                fs.Seek(folderOffset, SeekOrigin.Begin);
                WriteFolderInfos(fs, infos);
            }
        }

        public void WriteFolderInfos(Stream stream, IBigFileFolderInfo[] infos)
        {
            log.Info("Writing folder infos to a stream, count: {0}", infos.Length);

            diag.StartStopwatch();
            
            int totalSize = infos[0].StructSize * infos.Length;
            int remainder = (((totalSize - 1) / 8 + 1) * 8) - totalSize; //align to 8 bytes
            log.Debug("Total length: {0:X8}", totalSize);
            log.Debug("Remainder: {0}", remainder);

            byte[] buffer = buffers[infos[0].StructSize];
            for (int i = 0; i < infos.Length; i++)
            {
                int size = infos[i].ToBytes(buffer);
                stream.Write(buffer, 0, size);
            }

            for (int i = 0; i < remainder; i++)
            {
                //fill 8 byte aligned remainder with nothing
                stream.WriteByte(0x00);
            }

            log.Info("Folder infos written!  Time taken: {0}ms", diag.StopwatchTime);
        }
    }
}
