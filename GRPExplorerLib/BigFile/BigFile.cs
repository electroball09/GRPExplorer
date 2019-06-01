using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using GRPExplorerLib.Util;
using GRPExplorerLib.BigFile.Versions;
using GRPExplorerLib.BigFile.Files;

namespace GRPExplorerLib.BigFile
{
    public abstract class BigFile
    {
        public static BigFile OpenBigFile(string filePath)
        {
            if (filePath.EndsWith(".big"))
                return new PackedBigFile(new FileInfo(filePath));
            else
                return new UnpackedBigFile(new DirectoryInfo(filePath));
        }

        protected string fileOrDirectory;

        public abstract FileInfo MetadataFileInfo { get; }

        protected BigFileSegment segment;
        public BigFileSegment Segment { get { return segment; } }
        protected BigFileHeader header;
        public BigFileHeader Header { get { return header; } }
        protected BigFileFilesAndFolders filesAndFolders;
        public BigFileFilesAndFolders FilesAndFolders { get { return filesAndFolders; } }
        protected BigFileUtil fileUtil;
        public BigFileUtil FileUtil { get { return fileUtil; } }
        
        public abstract BigFileFileReader FileReader { get; }

        protected BigFileFolder rootFolder;
        public BigFileFolder RootFolder { get { return rootFolder; } }
        protected FileMappingData fileMap;
        public FileMappingData FileMap { get { return fileMap; } }

        protected BigFileFileLoader fileLoader;
        public BigFileFileLoader FileLoader { get { return fileLoader; } }

        public bool IsLoaded { get { return rootFolder != null; } }

        protected bool isExtraDataLoaded = false;
        public bool IsExtraDataLoaded { get { return isExtraDataLoaded; } }

        protected BigFileLoadOperationStatus status = new BigFileLoadOperationStatus();
        public BigFileOperationStatus LoadStatus { get { return status; } }

        public abstract IBigFileVersion Version { get; }

        protected IBigFileFileInfo[] rawFileInfos;
        public IBigFileFileInfo[] RawFileInfos { get { return rawFileInfos; } }
        protected IBigFileFolderInfo[] rawFolderInfos;
        public IBigFileFolderInfo[] RawFolderInfos { get { return rawFolderInfos; } }
        
        public BigFileSegmentHeader SegmentHeader;
        public BigFileHeaderStruct FileHeader;

        internal BigFile(string _fileOrDirectory)
        {
            fileOrDirectory = _fileOrDirectory;

            segment = new BigFileSegment(MetadataFileInfo);
            header = new BigFileHeader(MetadataFileInfo);
            filesAndFolders = new BigFileFilesAndFolders(MetadataFileInfo);
            fileUtil = new BigFileUtil();
            fileLoader = new BigFileFileLoader(this);
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
