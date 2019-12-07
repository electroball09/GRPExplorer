using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using GRPExplorerLib.Util;
using GRPExplorerLib.BigFile.Versions;
using GRPExplorerLib.YetiObjects;

namespace GRPExplorerLib.BigFile
{
    public class BigFileFolder
    {
        public short FolderIndex { get; }

        private IBigFileFolderInfo infoStruct;
        public IBigFileFolderInfo InfoStruct { get { return infoStruct; } }
        public List<BigFileFolder> SubFolders { get; } = new List<BigFileFolder>();
        public Dictionary<short, BigFileFolder> FolderMap { get; }
        public List<BigFileFile> Files { get; } = new List<BigFileFile>();

        private string name;
        public string Name { get { return name.Trim(); } }

        private FileMappingData fileMap;
        public FileMappingData FileMap { get { return fileMap; } set { fileMap = value; } }

        public bool IsExpanded { get; set; } = false;

        public BigFileFolder ParentFolder
        {
            get
            {
                if (infoStruct.PreviousFolder == -1)
                    return null;

                return FolderMap[infoStruct.PreviousFolder];
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
                foreach (BigFileFolder folder in SubFolders)
                    if (folder.Name == subFolderName)
                        return folder;

                return null;
            }
        }

        public BigFileFolder(short _folderIndex, IBigFileFolderInfo _infoStruct, Dictionary<short, BigFileFolder> _folderMap)
        {
            FolderIndex = _folderIndex;
            infoStruct = _infoStruct;
            FolderMap = _folderMap;
            name = infoStruct.Name.EncodeToGoodString();
        }

        public List<BigFileFile> GetAllFilesOfArchetype<T>(List<BigFileFile> filesList = null) where T : YetiObjectArchetype
        {
            if (filesList == null)
                filesList = new List<BigFileFile>();
            foreach (BigFileFile file in Files)
            {
                if (file.Is<T>())
                    filesList.Add(file);
            }
            foreach (BigFileFolder folder in SubFolders)
            {
                folder.GetAllFilesOfArchetype<T>(filesList);
            }
            return filesList;
        }
    }
}
