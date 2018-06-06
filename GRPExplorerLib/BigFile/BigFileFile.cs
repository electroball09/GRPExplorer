using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using GRPExplorerLib.Util;
using GRPExplorerLib.BigFile.Versions;

namespace GRPExplorerLib.BigFile
{
    public class BigFileFile
    {
        private string name;
        public string Name
        {
            get
            {
                return string.IsNullOrEmpty(name) ?
                    "NULL_FILE_NAME" :
                    name;
            }
        }

        private IBigFileFileInfo fileInfo;
        public IBigFileFileInfo FileInfo { get { return fileInfo; } set { fileInfo = value; } }

        private BigFileFolder parentFolder;
        public BigFileFolder ParentFolder { get { return parentFolder; } }

        private BigFileFile[] fileReferences;
        public BigFileFile[] FileReferences { get { return fileReferences; } set { fileReferences = value; } }

        private FileMappingData mappingData;
        public FileMappingData MappingData { get { return mappingData; } set { mappingData = value; } }

        public string FullFolderPath
        {
            get
            {
                BigFileFolder folder = parentFolder;
                string fullName = "";
                while (folder != null && folder.ParentFolder != null)
                {
                    fullName = folder.Name + "/" + fullName;
                    folder = folder.ParentFolder;
                }
                fullName += "/";
                return fullName;
            }
        }

        public BigFileFile(IBigFileFileInfo _fileInfo, BigFileFolder _parentFolder)
        {
            fileInfo = _fileInfo;
            parentFolder = _parentFolder;
            name = fileInfo.Name.EncodeToGoodString();
        }
    }
}
