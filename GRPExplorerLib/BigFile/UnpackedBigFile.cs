using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Diagnostics;
using GRPExplorerLib.Logging;
using GRPExplorerLib.BigFile.Versions;

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

        public override BigFileFileReader FileReader
        {
            get
            {
                throw new NotImplementedException("aaahhhhhhhhhhhhhhhhhhhhhhhhhh");
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

            FileHeader = yetiHeaderFile.ReadHeader();
            CountInfo = yetiHeaderFile.ReadFileCountInfo(ref FileHeader);

            log.Info(string.Format("Version: {0:X4}", CountInfo.BigFileVersion));

            log.Info(string.Format("Count info offset: {0:X8}", FileHeader.InfoOffset));
            version = VersionRegistry.GetVersion(CountInfo.BigFileVersion);

            fileUtil.BigFileVersion = version;

            status.UpdateProgress(0.4f);

            UnpackedFolderMapAndFilesList folderAndFiles = fileUtil.CreateFolderTreeAndFilesListFromDirectory(new DirectoryInfo(directory.FullName + "\\" + BigFileConst.UNPACK_DIR), renamedMapping);
            rootFolder = folderAndFiles.folderMap[0];

            status.UpdateProgress(0.6f);

            mappingData = fileUtil.CreateFileMappingData(folderAndFiles.folderMap[0], folderAndFiles.filesList);

            status.UpdateProgress(0.8f);

            fileUtil.MapFilesToFolders(rootFolder, mappingData);

            status.UpdateProgress(1.0f);

            status.stopwatch.Stop();
            log.Info("Unpacked bigfile loaded!");
            log.Info("  Time taken: " + status.TimeTaken + "ms");
        }

        public override void LoadExtraData(BigFileOperationStatus statusToUse)
        {
            throw new NotImplementedException();
        }
    }
}
