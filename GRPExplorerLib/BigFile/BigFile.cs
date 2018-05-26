using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using GRPExplorerLib.Util;
using GRPExplorerLib.BigFile.Versions;

namespace GRPExplorerLib.BigFile
{
    public abstract class BigFile
    {
        protected string fileOrDirectory;

        public abstract FileInfo MetadataFileInfo { get; }

        protected YetiHeaderFile yetiHeaderFile;
        public YetiHeaderFile YetiHeaderFile { get { return yetiHeaderFile; } }
        protected BigFileUtil fileUtil;
        public BigFileUtil FileUtil { get { return fileUtil; } }
        
        public abstract BigFileFileReader FileReader { get; }

        protected BigFileFolder rootFolder;
        public BigFileFolder RootFolder { get { return rootFolder; } }
        protected FileMappingData mappingData;
        public FileMappingData MappingData { get { return mappingData; } }

        public bool IsLoaded { get { return rootFolder != null; } }

        protected bool isExtraDataLoaded = false;
        public bool IsExtraDataLoaded { get { return isExtraDataLoaded; } }

        protected BigFileLoadOperationStatus status = new BigFileLoadOperationStatus();
        public BigFileOperationStatus LoadStatus { get { return status; } }

        public abstract IBigFileVersion Version { get; }
        
        public BigFileHeader FileHeader;
        public BigFileFileCountInfo CountInfo;

        internal BigFile(string _fileOrDirectory)
        {
            fileOrDirectory = _fileOrDirectory;

            yetiHeaderFile = new YetiHeaderFile(MetadataFileInfo);
            fileUtil = new BigFileUtil();
        }

        public abstract void LoadFromDisk();
        public abstract void LoadExtraData(BigFileOperationStatus statusToUse);
    }

    public sealed class BigFileLoadOperationStatus : BigFileOperationStatus
    {
        public override string OperationName
        {
            get
            {
                return "Load";
            }
        }
    }

    public sealed class BigFileExtraDataLoadOperationStatus : BigFileOperationStatus
    {
        public override string OperationName
        {
            get
            {
                return "Extra Data Load";
            }
        }
    }
}
