using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using GRPExplorerLib.Util;
using GRPExplorerLib.BigFile.Versions;
using GRPExplorerLib.YetiObjects;

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

        public string NameWithExtension
        {
            get
            {
                return string.Format("{0}.{1}", Name, FileInfo.FileType);
            }
        }
        public IBigFileFileInfo FileInfo { get; set; }
        public BigFileFolder ParentFolder { get; }
        public BigFileFile[] FileReferences { get; set; }

        public List<BigFileFile> ReferencedBy { get; } = new List<BigFileFile>();

        public FileMappingData MappingData { get; set; }
        public YetiObjectArchetype Archetype { get; private set; }

        public string FullFolderPath
        {
            get
            {
                BigFileFolder folder = ParentFolder;
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
            FileInfo = _fileInfo;
            ParentFolder = _parentFolder;
            name = FileInfo.Name.EncodeToGoodString();
            Archetype = this.CreateArchetype();
            Archetype.File = this;
        }

        public bool Is<T>() where T : YetiObjectArchetype
        {
            return Archetype is T;
        }

        public T ArchetypeAs<T>() where T : YetiObjectArchetype
        {
            if (Is<T>())
                return Archetype as T;
            return null;
        }

        public void Load(byte[] buffer, int size)
        {
            Archetype.Load(buffer, size, FileReferences);
        }

        public void Unload()
        {
            Archetype.Unload();
        }
    }
}
