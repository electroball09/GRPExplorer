using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using GRPExplorerLib.Util;
using GRPExplorerLib.BigFile.Versions;

namespace GRPExplorerLib.BigFile
{
    public class BigFileFolder
    {
        private short folderIndex;
        public short FolderIndex { get { return folderIndex; } }

        private IBigFileFolderInfo infoStruct;
        public IBigFileFolderInfo InfoStruct { get { return infoStruct; } }

        private List<BigFileFolder> subFolders = new List<BigFileFolder>();
        public List<BigFileFolder> SubFolders { get { return subFolders; } }

        private Dictionary<short, BigFileFolder> folderMap;
        public Dictionary<short, BigFileFolder> FolderMap { get { return folderMap; } }

        private List<BigFileFile> files = new List<BigFileFile>();
        public List<BigFileFile> Files { get { return files; } }

        private string name;
        public string Name { get { return name.Trim(); } }

        public BigFileFolder ParentFolder
        {
            get
            {
                if (infoStruct.PreviousFolder == -1)
                    return null;

                return folderMap[infoStruct.PreviousFolder];
            }
        }

        public string FullDirectoryName
        {
            get
            {
                StringBuilder builder = new StringBuilder();
                BigFileFolder folder = this;
                while (folder != null)
                {
                    builder.Insert(0, '\\');
                    builder.Insert(0, folder.Name);
                    folder = folder.ParentFolder;
                }
                return builder.ToString();
            }
        }

        public BigFileFolder this[string subFolderName]
        {
            get
            {
                foreach (BigFileFolder folder in subFolders)
                    if (folder.Name == subFolderName)
                        return folder;

                return null;
            }
        }

        public BigFileFolder(short _folderIndex, IBigFileFolderInfo _infoStruct, Dictionary<short, BigFileFolder> _folderMap)
        {
            folderIndex = _folderIndex;
            infoStruct = _infoStruct;
            folderMap = _folderMap;
            name = infoStruct.Name.EncodeToGoodString();
        }
    }
}
