﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.Diagnostics;
using GRPExplorerLib.Logging;
using GRPExplorerLib.BigFile.Versions;

namespace GRPExplorerLib.BigFile
{
    public class PackedBigFile : BigFile
    {
        private PackedBigFileFileReader fileReader;
        public override BigFileFileReader FileReader
        {
            get
            {
                return fileReader;
            }
        }

        private FileInfo fileInfo;
        public override FileInfo MetadataFileInfo
        {
            get
            {
                if (fileInfo == null)
                    fileInfo = new FileInfo(fileOrDirectory);

                return fileInfo;
            }
        }
        
        private ILogProxy log = LogManager.GetLogProxy("PackedBigFile");

        private IBigFileVersion version;
        public override IBigFileVersion Version
        {
            get
            {
                return version;
            }
        }

        public PackedBigFile(FileInfo _fileInfo) : base(_fileInfo.FullName)
        {
            log.Info("Creating packed bigfile, file: " + _fileInfo.FullName);

            fileReader = new PackedBigFileFileReader(this);

            if (!_fileInfo.Exists)
                throw new Exception("_fileInfo doesn't exist!");
        }

        public override void LoadFromDisk()
        {
            status.stopwatch.Reset();
            status.stopwatch.Start();
            status.UpdateProgress(0f);

            log.Info("Loading big file into memory: " + fileInfo.FullName);

            SegmentHeader = segment.ReadSegmentHeader();
            FileHeader = header.ReadHeader(ref SegmentHeader);

            log.Info("Header and count info read");

            log.Info(string.Format("Version: {0:X4}", FileHeader.BigFileVersion));

            version = BigFileVersions.GetVersion(FileHeader.BigFileVersion);

            status.UpdateProgress(0.2f);
            
            fileUtil.BigFileVersion = version;
            filesAndFolders.Version = version;

            rawFolderInfos = filesAndFolders.ReadFolderInfos(ref SegmentHeader, ref FileHeader);
            rawFileInfos = filesAndFolders.ReadFileInfos(ref SegmentHeader, ref FileHeader);

            log.Info("Creating folder tree and file mappings...");

            status.UpdateProgress(0.4f);

            rootFolder = fileUtil.CreateRootFolderTree(rawFolderInfos);
            status.UpdateProgress(0.6f);
            mappingData = fileUtil.CreateFileMappingData(rootFolder, rawFileInfos);
            status.UpdateProgress(0.8f);
            fileUtil.MapFilesToFolders(rootFolder, mappingData);
            status.UpdateProgress(1f);
            status.stopwatch.Stop();

            log.Info("Bigfile loaded!");

            fileUtil.diagData.DebugLog(log);
            
            log.Info("Time taken: " + status.TimeTaken + "ms");
        }

        public override void LoadExtraData(BigFileOperationStatus statusToUse)
        {
            BigFileFile[] files = mappingData.KeyMapping.Values.ToArray();

            int count = 0;
            foreach (int size in fileReader.ReadAll(files, fileUtil.IOBuffers))
            {
                statusToUse.UpdateProgress((float)files.Length / (float)count);
                fileUtil.AddFileReferencesToFile(files[count], fileUtil.IOBuffers, size);
                count++;
            }
        }
    }
}
