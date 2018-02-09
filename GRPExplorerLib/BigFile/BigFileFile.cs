using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using GRPExplorerLib.Util;

namespace GRPExplorerLib.BigFile
{
    public class BigFileFile
    {
        private string name;
        public string Name { get { return name; } }

        private BigFileFileInfo fileInfo;
        public BigFileFileInfo FileInfo { get { return fileInfo; } }

        private BigFileFolder parentFolder;
        public BigFileFolder ParentFolder { get { return parentFolder; } }

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
                return fullName;
            }
        }

        public BigFileFile(BigFileFileInfo _fileInfo, BigFileFolder _parentFolder)
        {
            fileInfo = _fileInfo;
            parentFolder = _parentFolder;
            name = fileInfo.Name.EncodeToGoodString();
        }
    }
}
