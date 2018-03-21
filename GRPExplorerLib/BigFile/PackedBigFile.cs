using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.Diagnostics;

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
        
        private LogProxy log = new LogProxy("PackedBigFile");

        public PackedBigFile(FileInfo _fileInfo) : base(_fileInfo.FullName)
        {
            log.Info("Creating packed bigfile, file: " + _fileInfo.FullName);

            fileReader = new PackedBigFileFileReader(this);

            if (!_fileInfo.Exists)
                throw new Exception("_fileInfo doesn't exist!");
        }

        public override void LoadFromDisk()
        {
            BigFileLoadOperationStatus status = new BigFileLoadOperationStatus();
            status.stopwatch.Start();

            log.Info("Loading big file into memory: " + fileInfo.FullName);

            FileHeader = yetiHeaderFile.ReadHeader();
            CountInfo = yetiHeaderFile.ReadFileCountInfo(ref FileHeader);

            log.Info("Header and count info read");

            BigFileFolderInfo[] folders = yetiHeaderFile.ReadFolderInfos(ref FileHeader, ref CountInfo);
            BigFileFileInfo[] files = yetiHeaderFile.ReadFileInfos(ref FileHeader, ref CountInfo);

            log.Info("Creating folder tree and file mappings...");

            rootFolder = fileUtil.CreateRootFolderTree(folders);
            mappingData = fileUtil.CreateFileMappingData(rootFolder, files);
            fileUtil.MapFilesToFolders(rootFolder, mappingData);

            log.Info("Bigfile loaded!");

            fileUtil.diagData.DebugLog(log);

            status.stopwatch.Stop();

            log.Info("Time taken: " + status.TimeTaken + "ms");
        }
    }
}
