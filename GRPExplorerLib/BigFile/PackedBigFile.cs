using System;
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

            FileHeader = yetiHeaderFile.ReadHeader();
            CountInfo = yetiHeaderFile.ReadFileCountInfo(ref FileHeader);

            log.Info("Header and count info read");

            log.Info(string.Format("Version: {0:X4}", CountInfo.BigFileVersion));

            version = VersionRegistry.GetVersion(CountInfo.BigFileVersion);

            status.UpdateProgress(0.2f);

            yetiHeaderFile.BigFileVersion = version;
            fileUtil.BigFileVersion = version;

            IBigFileFolderInfo[] folders = yetiHeaderFile.ReadFolderInfos(ref FileHeader, ref CountInfo);
            IBigFileFileInfo[] files = yetiHeaderFile.ReadFileInfos(ref FileHeader, ref CountInfo);

            log.Info("Creating folder tree and file mappings...");

            status.UpdateProgress(0.4f);

            rootFolder = fileUtil.CreateRootFolderTree(folders);
            status.UpdateProgress(0.6f);
            mappingData = fileUtil.CreateFileMappingData(rootFolder, files);
            status.UpdateProgress(0.8f);
            fileUtil.MapFilesToFolders(rootFolder, mappingData);
            status.UpdateProgress(1f);
            status.stopwatch.Stop();

            log.Info("Bigfile loaded!");

            fileUtil.diagData.DebugLog(log);
            
            log.Info("Time taken: " + status.TimeTaken + "ms");
        }
    }
}
