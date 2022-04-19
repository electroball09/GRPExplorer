﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using GRPExplorerLib;
using System.Diagnostics;
using System.IO;
using GRPExplorerLib.Util;
using GRPExplorerLib.Logging;
using GRPExplorerLib.BigFile.Versions;

namespace GRPExplorerLib.BigFile
{
    public class BigFileUtil
    {
        public struct DiagData
        {
            public float CreateFolderMap;
            public float ParentMapping;
            public float CreateFilesList;
            public float CreateKeyAndNumMappings;
            public float MapFilesToFolders;

            internal void DebugLog(ILogProxy log)
            {
                log.Debug("");
                log.Debug(" > DiagData Dump:");
                log.Debug("           CreateFolderMap: " + CreateFolderMap + "ms");
                log.Debug("             ParentMapping: " + ParentMapping + "ms");
                log.Debug("           CreateFilesList: " + CreateFilesList + "ms");
                log.Debug("   CreateKeyAndNumMappings: " + CreateKeyAndNumMappings + "ms");
                log.Debug("         MapFilesToFolders: " + MapFilesToFolders + "ms");
                log.Debug("");
            }
        }

        private ILogProxy log = LogManager.GetLogProxy("BigFileUtil");
        private Stopwatch stopwatch = new Stopwatch();
        private IOBuffers buffers = new IOBuffers();
        public IOBuffers IOBuffers { get { return buffers; } }
        public DiagData diagData;

        private IBigFileVersion version;
        public IBigFileVersion BigFileVersion { get { return version; } set { version = value; } }

        public int CalculateFolderOffset(ref BigFileSegmentHeader segmentHeader, ref BigFileHeaderStruct header)
        {
            return CalculateFolderOffset(this.version, ref segmentHeader, ref header);
        }

        public static int CalculateFolderOffset(IBigFileVersion version, ref BigFileSegmentHeader segmentHeader, ref BigFileHeaderStruct header)
        {
            if (version == null)
                throw new NullReferenceException("There's no version!  Can't calculate folder offset!");

            IBigFileFileInfo tmpFileInfo = version.CreateFileInfo();
            int baseSize = (segmentHeader.InfoOffset + header.StructSize) + (header.Files * tmpFileInfo.StructSize);
            baseSize = (((baseSize - 1) / 8) + 1) * 8; // align to 8 bytes
            return baseSize;
        }

        public int CalculateDataOffset(ref BigFileSegmentHeader segmentHeader, ref BigFileHeaderStruct header)
        {
            return CalculateDataOffset(this.version, ref segmentHeader, ref header);
        }

        public static int CalculateDataOffset(IBigFileVersion version, ref BigFileSegmentHeader segmentHeader, ref BigFileHeaderStruct header)
        {
            if (version == null)
                throw new NullReferenceException("There's no version!  Can't calculate data offset!");

            IBigFileFolderInfo tmpFolderInfo = version.CreateFolderInfo();
            int folderOffset = CalculateFolderOffset(version, ref segmentHeader, ref header);
            int dataOffset = folderOffset + (header.Folders * tmpFolderInfo.StructSize);
            dataOffset = (((dataOffset - 1) / 8) + 1) * 8; // align to 8 bytes;
            return dataOffset;
        }

        public BigFileFolder CreateRootFolderTree(IBigFileFolderInfo[] folderInfos)
        {
            log.Info("Creating root folder tree...");

            stopwatch.Reset();
            stopwatch.Start();

            log.Info("Creating folder map, number of folders: " + folderInfos.Length);
            Dictionary<short, BigFileFolder> folderMap = new Dictionary<short, BigFileFolder>();
            for (short i = 0; i < folderInfos.Length; i++)
            {
                folderMap.Add(i, new BigFileFolder(i, folderInfos[i], folderMap));
            }

            diagData.CreateFolderMap = stopwatch.ElapsedMilliseconds;

            log.Info("Folder map created!");

            stopwatch.Reset();
            stopwatch.Start();

            log.Info("Mapping folders to their parents...");

            for (short i = 0; i < folderInfos.Length; i++)
            {
                if (folderInfos[i].PreviousFolder != -1)
                {
                    folderMap[folderInfos[i].PreviousFolder].SubFolders.Add(folderMap[i]);
                }
            }

            log.Info("Folders mapped!");

            diagData.ParentMapping = stopwatch.ElapsedMilliseconds;

            stopwatch.Reset();
            
            return folderMap[0];
        }

        public FileMappingData CreateFileMappingData(BigFileFolder rootFolder, IBigFileFileInfo[] fileInfos)
        {
            log.Info("Creating mapping data...");
            log.Info("Creating files list...  Count: " + fileInfos.Length);

            stopwatch.Reset();
            stopwatch.Start();

            FileMappingData mappingData = new FileMappingData();

            YetiObject[] filesList = new YetiObject[fileInfos.Length];

            for (int i = 0; i < fileInfos.Length; i++)
            {
                YetiObject newFile = null;
                if (fileInfos[i] != null)
                {
                    newFile = new YetiObject(fileInfos[i], rootFolder.FolderMap[fileInfos[i].Folder]);
                    newFile.MappingData = mappingData;
                    filesList[i] = newFile;
                }
                else
                    log.Error(string.Format("File info at index {0} is null!", i));
            }

            diagData.CreateFilesList = stopwatch.ElapsedMilliseconds;

            log.Info("List created!");

            log.Info("Creating file mappings...");

            stopwatch.Reset();
            stopwatch.Start();

            Dictionary<int, YetiObject> fileKeyMapping = new Dictionary<int, YetiObject>();

            for (int i = 0; i < filesList.Length; i++)
            {
                if (fileInfos[i]?.Name == null)
                    continue;

                if (!fileKeyMapping.ContainsKey(fileInfos[i].Key))
                    fileKeyMapping.Add(fileInfos[i].Key, filesList[i]);
                else
                    log.Error("File key mapping already contains key " + fileInfos[i].Key + " (File: " + filesList[i].Name + ")");

                if (filesList[i].FileInfo.FileNumber == -1)
                {
                    log.Debug(string.Format("File number is -1! (key:{0:X8}) (offset:{1:X8})", fileInfos[i].Key, fileInfos[i].Offset));
                }
            }

            log.Info("Mappings created!");

            foreach (KeyValuePair<short, BigFileFolder> kvp in rootFolder.FolderMap)
            {
                kvp.Value.FileMap = mappingData;
            }

            diagData.CreateKeyAndNumMappings = stopwatch.ElapsedMilliseconds;

            stopwatch.Reset();

            mappingData.FilesList = filesList;
            mappingData.KeyMapping = fileKeyMapping;

            log.Info("mappingData count: {0}", mappingData.FilesList.Length);

            return mappingData;
        }

        public void MapFilesToFolders(BigFileFolder rootFolder, FileMappingData mapping)
        {
            log.Info("Mapping files to folders...");

            stopwatch.Reset();
            stopwatch.Start();

            for (int i = 0; i < mapping.FilesList.Length; i++)
            {
                YetiObject file = mapping.FilesList[i];
                if (file != null)
                {
                    BigFileFolder folder = rootFolder.FolderMap[mapping.FilesList[i].FileInfo.Folder];
                    if (folder != null)
                        folder.ChildObjects.Add(mapping.FilesList[i]);
                }
            }

            diagData.MapFilesToFolders = stopwatch.ElapsedMilliseconds;

            stopwatch.Reset();
        }

        public UnpackedFolderMapAndFilesList CreateFolderTreeAndFilesListFromDirectory(DirectoryInfo dir, UnpackedRenamedFileMapping mapping, FileMappingData defaultMappingData)
        {
            log.Info("Creating folder tree and files list from directory " + dir.FullName);
            IBigFileFileInfo[] fileInfos = new IBigFileFileInfo[mapping.KeyMap.Count];
            List<IBigFileFolderInfo> folderInfos = new List<IBigFileFolderInfo>();
            Dictionary<short, BigFileFolder> folderMap = new Dictionary<short, BigFileFolder>();
            short folderCount = 0;
            int fileCount = 0;

            Dictionary<string, UnpackedRenamedFileMapping.RenamedFileMappingData> temp = new Dictionary<string, UnpackedRenamedFileMapping.RenamedFileMappingData>(mapping.RenamedMap);

            BigFileFolder recursion(DirectoryInfo directory, string dirName, BigFileFolder parentFolder)
            {
                IBigFileFolderInfo folderInfo = version.CreateFolderInfo();
                folderInfo.Unknown_01 = 0;
                folderInfo.PreviousFolder = parentFolder != null ? parentFolder.FolderIndex : (short)-1;
                folderInfo.NextFolder = -1;
                folderInfo.Unknown_02 = 0;
                folderInfo.Name = parentFolder == null ? //oh my lawdy what is this EDIT 4/5/2018: what the fuck
                        "/".EncodeToBadString(length: 54) :
                        directory.Name.EncodeToBadString(length: 54);
                folderInfos.Add(folderInfo);

                BigFileFolder thisFolder = new BigFileFolder(folderCount, folderInfo, folderMap);
                folderMap.Add(folderCount, thisFolder);

                foreach (FileInfo file in directory.GetFiles())
                {
                    if (file.Name.EndsWith(".header"))
                        continue;

                    string fileName = dirName + "//" + file.Name;
                    UnpackedRenamedFileMapping.RenamedFileMappingData mappingData = mapping[fileName];
                    temp.Remove(fileName);
                    IBigFileFileInfo fileInfo = version.CreateFileInfo();
                    defaultMappingData[mappingData.Key].FileInfo.Copy(fileInfo);
                    fileInfo.Key = mappingData.Key;
                    //fileInfo.FileNumber = fileCount;
                    fileInfo.Name = mappingData.OriginalName.EncodeToBadString(length: 60);
                    fileInfo.Folder = folderCount;

                    log.Debug("Add file " + file.FullName);

                    fileInfos[fileCount] = fileInfo;
                    fileCount++;
                }

                folderCount++;

                foreach (DirectoryInfo dirInfo in directory.GetDirectories())
                {
                    if (parentFolder == null) //ONLY THE FIRST RECURSION, PREVENTS ADDING WRONG FOLDERS WHEN PACKING
                        thisFolder.SubFolders.Add(recursion(dirInfo, dirInfo.Name, thisFolder));
                    else
                        thisFolder.SubFolders.Add(recursion(dirInfo, dirName + "/" + dirInfo.Name, thisFolder));
                }

                return thisFolder;
            }

            recursion(dir, "", null);

            if (fileCount != mapping.KeyMap.Count)
            {
                log.Error(string.Format("Missing {0} files!", temp.Count));
                foreach (KeyValuePair<string, UnpackedRenamedFileMapping.RenamedFileMappingData> kvp in temp)
                    log.Error(string.Format("     >{0}", kvp.Value.FileName));
            }

            return new UnpackedFolderMapAndFilesList()
            {
                folderMap = folderMap,
                filesList = fileInfos,
                foldersList = folderInfos.ToArray(),
            };
        }

        public void AddReferencesToObject(YetiObject obj, int[] header)
        {
            log.Debug("Loading file references for object: " + obj.Name);
            log.Debug("  Reference count: " + header.Length.ToString());

            if (obj.ObjectReferences != null)
            {
                log.Debug("Object {0} (key{1:X8}) already has references loaded!", obj.Name, obj.FileInfo.Key);
                return;
            }

            YetiObject[] references = new YetiObject[header.Length];

            for (int i = 0; i < header.Length; i++)
            {
                YetiObject reference = obj.MappingData[header[i]];
                if (reference != null)
                {
                    if (!reference.ReferencedBy.Contains(obj))
                        reference.ReferencedBy.Add(obj);
                }
                references[i] = reference;
            }

            obj.ObjectReferences = references;
        }

        public void SortFolderTree(BigFileFolder folder, bool recursive = true, bool sortFiles = true)
        {
            List<BigFileFolder> newFolder = folder.SubFolders.OrderBy(f => f.Name).ToList();
            for (int i = 0; i < folder.SubFolders.Count; i++)
                folder.SubFolders[i] = newFolder[i];

            if (sortFiles)
            {
                List<YetiObject> newFiles = folder.ChildObjects.OrderBy(f => f.Name).ToList();
                for (int i = 0; i < folder.ChildObjects.Count; i++)
                    folder.ChildObjects[i] = newFiles[i];
            }

            if (recursive)
                foreach (BigFileFolder folder2 in folder.SubFolders)
                    SortFolderTree(folder2);
        }

        public void DebugLogRootFolderTree(BigFileFolder rootFolder)
        {
            Action<BigFileFolder, int> recursion = null;
            recursion = (folder, depth) =>
            {
                string folderString = "";
                string fileString = "     ";
                for (int i = 0; i < depth; i++)
                {
                    folderString += "-";
                    fileString += " ";
                }
                log.Debug(folderString + Encoding.Default.GetString(folder.InfoStruct.Name));
                foreach (YetiObject file in folder.ChildObjects)
                    log.Debug(fileString + file.Name);
                foreach (BigFileFolder subFolder in folder.SubFolders)
                    recursion.Invoke(subFolder, depth + 1);
            };

            recursion.Invoke(rootFolder, 0);
        }
    }

    public struct UnpackedFolderMapAndFilesList
    {
        public Dictionary<short, BigFileFolder> folderMap;
        public IBigFileFileInfo[] filesList;
        public IBigFileFolderInfo[] foldersList;
    }

    public class UnpackedRenamedFileMapping
    {
        public struct RenamedFileMappingData
        {
            public int Key;
            public string OriginalName;
            public string FileName;

            public void WriteToStream(Stream stream, byte[] buffer)
            {
                Key.ToByteArray(buffer, 0);
                OriginalName.Length.ToByteArray(buffer, 4);
                FileName.Length.ToByteArray(buffer, 8);
                Encoding.Default.GetBytes(OriginalName, 0, OriginalName.Length, buffer, 12);
                Encoding.Default.GetBytes(FileName, 0, FileName.Length, buffer, 12 + OriginalName.Length);

                stream.Write(buffer, 0, 12 + OriginalName.Length + FileName.Length);
            }

            public void ReadFromStream(Stream stream, byte[] buffer)
            {
                stream.Read(buffer, 0, 12);
                Key = BitConverter.ToInt32(buffer, 0);
                int origLength = BitConverter.ToInt32(buffer, 4);
                int nameLength = BitConverter.ToInt32(buffer, 8);
                stream.Read(buffer, 12, origLength + nameLength);
                OriginalName = Encoding.Default.GetString(buffer, 12, origLength);
                FileName = Encoding.Default.GetString(buffer, 12 + origLength, nameLength);
            }

            internal void DebugLog(ILogProxy log)
            {
                log.Debug(" > RenamedFileMappingData Dump:");
                log.Debug("             Key: " + Key);
                log.Debug("    OriginalName: " + OriginalName);
                log.Debug("        FileName: " + FileName);
            }
        }

        public RenamedFileMappingData this[int key]
        {
            get
            {
                return keyMap[key];
            }
            set
            {
                if (!keyMap.ContainsKey(key))
                {
                    keyMap.Add(key, value);
                    renamedMap.Add(value.FileName, value);
                }
                else
                {
                    keyMap[key] = value;
                    renamedMap[value.FileName] = value;
                }
            }
        }

        public RenamedFileMappingData this[string key]
        {
            get
            {
                return renamedMap[key];
            }
            set
            {
                if (!renamedMap.ContainsKey(key))
                {
                    renamedMap.Add(key, value);
                    keyMap.Add(value.Key, value);
                }
                else
                {
                    renamedMap[key] = value;
                    keyMap[value.Key] = value;
                }
            }
        }

        private Dictionary<int, RenamedFileMappingData> keyMap = new Dictionary<int, RenamedFileMappingData>();
        public Dictionary<int, RenamedFileMappingData> KeyMap { get { return keyMap; } }
        private Dictionary<string, RenamedFileMappingData> renamedMap = new Dictionary<string, RenamedFileMappingData>();
        public Dictionary<string, RenamedFileMappingData> RenamedMap { get { return renamedMap; } }
    }

    public class FileMappingData
    {
        private YetiObject[] filesList;
        public YetiObject[] FilesList { get { return filesList; } set { filesList = value; } }
        private Dictionary<int, YetiObject> keyMapping;
        public Dictionary<int, YetiObject> KeyMapping { get { return keyMapping; } set { keyMapping = value; } }

        public YetiObject this[int key]
        {
            get
            {
                if (keyMapping.ContainsKey(key))
                    return keyMapping[key];
                else
                    return null;
            }
        }

        public FileMappingData()
        {

        }

        public FileMappingData(YetiObject[] _filesList, Dictionary<int, YetiObject> _keyMapping)
        {
            filesList = _filesList;
            keyMapping = _keyMapping;
        }
    }
}
