using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using GRPExplorerLib.Util;
using GRPExplorerLib.BigFile.Versions;
using GRPExplorerLib.YetiObjects;

namespace GRPExplorerLib.BigFile
{
    public class YetiObject
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
        public YetiObject[] ObjectReferences { get; set; }

        public List<YetiObject> ReferencedBy { get; } = new List<YetiObject>();

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

        public YetiObject(IBigFileFileInfo _fileInfo, BigFileFolder _parentFolder)
        {
            FileInfo = _fileInfo;
            ParentFolder = _parentFolder;
            name = FileInfo.Name.EncodeToGoodString();
            Archetype = YetiObjectArchetype.CreateArchetype(this);
            Archetype.Object = this;
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
            Archetype.Load(buffer, size, ObjectReferences);
        }

        public void Unload()
        {
            Archetype.Unload();
        }
    }
}
