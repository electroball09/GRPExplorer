using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace GRPExplorerLib.BigFile
{
    public class BigFileConst
    {
        public const string BIGFILE_EXTENSION = ".big";
        public const string GRP_EXPLORER_EXTENSION = ".gex";
        public const string METADATA_FILE_NAME = "metadata" + GRP_EXPLORER_EXTENSION;
        public const string UNPACK_DIR = "Unpack\\";
        public const string UNPACK_FILE_MAPPING_NAME = "filemapping" + GRP_EXPLORER_EXTENSION;
        public const string PACK_STAGING_DIR = @"\Temp\";
        public const string EXTENSIONS_LIST_FILE_NAME = "FileExtensionsList";
    }
}
