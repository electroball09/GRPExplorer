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

        private Stopwatch stopwatch = new Stopwatch();
        private LogProxy log = new LogProxy("PackedBigFile");

        public PackedBigFile(FileInfo _fileInfo) : base(_fileInfo.FullName)
        {
            if (!_fileInfo.Exists)
                throw new Exception("_fileInfo doesn't exist!");

            fileInfo = _fileInfo;
            fileIO = new BigFileIO(fileInfo);
            fileUtil = new BigFileUtil();
        }

        public override void LoadFromDisk()
        {
            stopwatch.Reset();
            stopwatch.Start();

            log.Info("Loading big file into memory: " + fileInfo.FullName);

            FileHeader = fileIO.ReadHeader();
            CountInfo = fileIO.ReadFileCountInfo(ref FileHeader);

            log.Info("Header and count info read");

            BigFileFolderInfo[] folders = fileIO.ReadFolderInfos(ref FileHeader, ref CountInfo);
            BigFileFileInfo[] files = fileIO.ReadFileInfos(ref FileHeader, ref CountInfo);

            log.Info("Creating folder tree and file mappings...");

            rootFolder = fileUtil.CreateRootFolderTree(folders);
            mappingData = fileUtil.CreateFileMappings(rootFolder, files);
            fileUtil.MapFilesToFolders(rootFolder, mappingData);

            log.Info("Bigfile loaded!");

            fileUtil.diagData.DebugLog(log);

            log.Info("Time taken: " + stopwatch.ElapsedMilliseconds + "ms");

            stopwatch.Stop();
        }
    }
}
