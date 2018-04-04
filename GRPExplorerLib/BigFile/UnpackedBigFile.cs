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
        private Stopwatch stopwatch = new Stopwatch();

        private UnpackedRenamedFileMapping renamedMapping;
        public UnpackedRenamedFileMapping RenamedMapping { get { return renamedMapping; } }

        private IBigFileVersion version;

        public UnpackedBigFile(DirectoryInfo dir) : base(dir.FullName + "\\")
        {
            directory = dir;
            log.Info("Creating unpacked bigfile, directory: " + dir.FullName);
        }

        public override void LoadFromDisk()
        {
            stopwatch.Reset();
            stopwatch.Start();
            log.Info("Loading unpacked bigfile from disk...");

            UnpackedFileKeyMappingFile mappingFile = new UnpackedFileKeyMappingFile(new DirectoryInfo(fileOrDirectory));
            renamedMapping = mappingFile.LoadMappingData();

            FileHeader = yetiHeaderFile.ReadHeader();
            CountInfo = yetiHeaderFile.ReadFileCountInfo(ref FileHeader);

            log.Info(string.Format("Version: {0:X4}", CountInfo.BigFileVersion));

            version = VersionRegistry.GetVersion(CountInfo.BigFileVersion);

            fileUtil.BigFileVersion = version;

            UnpackedFolderMapAndFilesList folderAndFiles = fileUtil.CreateFolderTreeAndFilesListFromDirectory(new DirectoryInfo(directory.FullName + "\\" + BigFileConst.UNPACK_DIR), renamedMapping);
            rootFolder = folderAndFiles.folderMap[0];

            mappingData = fileUtil.CreateFileMappingData(folderAndFiles.folderMap[0], folderAndFiles.filesList);

            fileUtil.MapFilesToFolders(rootFolder, mappingData);

            log.Info("Unpacked bigfile loaded!");
            log.Info("  Time taken: " + stopwatch.ElapsedMilliseconds + "ms");
            stopwatch.Stop();
        }
    }
}
