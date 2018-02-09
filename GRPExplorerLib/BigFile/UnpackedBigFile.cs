using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Diagnostics;

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

        private DirectoryInfo directory;
        public DirectoryInfo Directory { get { return directory; } }

        private LogProxy log = new LogProxy("UnpackedBigFile");
        private Stopwatch stopwatch = new Stopwatch();

        private UnpackedRenamedFileMapping renamedMapping;

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

            FileHeader = fileIO.ReadHeader();
            CountInfo = fileIO.ReadFileCountInfo(ref FileHeader);

            UnpackedFolderMapAndFilesList folderAndFiles = fileUtil.CreateFolderTreeAndFilesListFromDirectory(new DirectoryInfo(directory.FullName + "\\" + BigFileConst.UNPACK_DIR), renamedMapping);
            rootFolder = folderAndFiles.folderMap[0];

            mappingData = fileUtil.CreateFileMappingData(folderAndFiles.folderMap[0], folderAndFiles.filesList);

            fileUtil.MapFilesToFolders(rootFolder, mappingData);
        }
    }
}
