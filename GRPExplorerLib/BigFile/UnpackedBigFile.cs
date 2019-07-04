using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Diagnostics;
using GRPExplorerLib.Logging;
using GRPExplorerLib.BigFile.Versions;
using GRPExplorerLib.BigFile.Files;

namespace GRPExplorerLib.BigFile
{
    public class UnpackedBigFile : BigFile
    {
        private FileInfo metadataFileInfo;
        public override FileInfo MetadataFileInfo
        {
            get
            {
                if (metadataFileInfo == null)
                    metadataFileInfo = new FileInfo(fileOrDirectory + BigFileConst.METADATA_FILE_NAME);

                return metadataFileInfo;
            }
        }

        private UnpackedBigFileFileReader fileReader;
        public override BigFileFileReader FileReader
        {
            get
            {
                return fileReader;
            }
        }

        private DirectoryInfo directory;
        public DirectoryInfo Directory { get { return directory; } }

        private ILogProxy log = LogManager.GetLogProxy("UnpackedBigFile");

        private UnpackedRenamedFileMapping renamedMapping;
        public UnpackedRenamedFileMapping RenamedMapping { get { return renamedMapping; } }

        private IBigFileVersion version;
        public override IBigFileVersion Version
        {
            get
            {
                return version;
            }
        }

        public UnpackedBigFile(DirectoryInfo dir) : base(dir.FullName + "\\")
        {
            directory = dir;
            log.Info("Creating unpacked bigfile, directory: " + dir.FullName);
            fileReader = new UnpackedBigFileFileReader(this);
        }

        public override void LoadFromDisk()
        {
            status.stopwatch.Reset();
            status.stopwatch.Start();
            status.UpdateProgress(0f);
            log.Info("Loading unpacked bigfile from disk...");

            UnpackedFileKeyMappingFile mappingFile = new UnpackedFileKeyMappingFile(new DirectoryInfo(fileOrDirectory));
            renamedMapping = mappingFile.LoadMappingData();

            status.UpdateProgress(0.2f);

            SegmentHeader = segment.ReadSegmentHeader();
            FileHeader = header.ReadHeader(ref SegmentHeader);
            
            log.Info(string.Format("Count info offset: {0:X8}", SegmentHeader.InfoOffset));
            version = BigFileVersions.GetVersion(FileHeader.BigFileVersion);
            fileUtil.BigFileVersion = version;
            filesAndFolders.Version = version;
            log.Info(string.Format("Version: {0:X4}", FileHeader.BigFileVersion));

            rawFolderInfos = filesAndFolders.ReadFolderInfos(ref SegmentHeader, ref FileHeader);
            rawFileInfos = filesAndFolders.ReadFileInfos(ref SegmentHeader, ref FileHeader);

            fileUtil.BigFileVersion = version;
            filesAndFolders.Version = version;

            status.UpdateProgress(0.4f);

            rootFolder = fileUtil.CreateRootFolderTree(rawFolderInfos);
            fileMap = fileUtil.CreateFileMappingData(rootFolder, rawFileInfos);
            fileUtil.MapFilesToFolders(rootFolder, fileMap);

            UnpackedFolderMapAndFilesList folderAndFiles = fileUtil.CreateFolderTreeAndFilesListFromDirectory(new DirectoryInfo(directory.FullName + "\\" + BigFileConst.UNPACK_DIR), renamedMapping, fileMap);
            rootFolder = folderAndFiles.folderMap[0];

            rawFileInfos = folderAndFiles.filesList;
            rawFolderInfos = folderAndFiles.foldersList;

            status.UpdateProgress(0.6f);

            fileMap = fileUtil.CreateFileMappingData(folderAndFiles.folderMap[0], folderAndFiles.filesList);

            status.UpdateProgress(0.8f);

            fileUtil.MapFilesToFolders(rootFolder, fileMap);

            status.UpdateProgress(1.0f);

            status.stopwatch.Stop();
            log.Info("Unpacked bigfile loaded!");
            log.Info("  Time taken: " + status.TimeTaken + "ms");
        }

        public override void LoadExtraData(BigFileOperationStatus statusToUse)
        {
            BigFileFile[] files = fileMap.KeyMapping.Values.ToArray();

            int count = 0;
            foreach (int[] header in fileReader.ReadAllHeaders(files, fileUtil.IOBuffers, fileReader.DefaultFlags))
            {
                statusToUse.UpdateProgress((float)files.Length / (float)count);
                fileUtil.AddFileReferencesToFile(files[count], header);
                count++;
            }
        }
    }
}
