using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using GRPExplorerLib;
using System.Diagnostics;
using System.IO;
using GRPExplorerLib.Util;

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

            internal void DebugLog(LogProxy log)
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

        private LogProxy log = new LogProxy("BigFileUtil");
        private Stopwatch stopwatch = new Stopwatch();
        private IOBuffers buffers = new IOBuffers();
        public DiagData diagData;

        public BigFileFolder CreateRootFolderTree(BigFileFolderInfo[] folderInfos)
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

        public FileMappingData CreateFileMappingData(BigFileFolder rootFolder, BigFileFileInfo[] fileInfos)
        {
            log.Info("Creating mapping data...");
            log.Info("Creating files list...  Count: " + fileInfos.Length);

            stopwatch.Reset();
            stopwatch.Start();

            BigFileFile[] filesList = new BigFileFile[fileInfos.Length];

            for (int i = 0; i < fileInfos.Length; i++)
            {
                filesList[i] = new BigFileFile(fileInfos[i], rootFolder.FolderMap[fileInfos[i].Folder]);
            }

            diagData.CreateFilesList = stopwatch.ElapsedMilliseconds;

            log.Info("List created!");

            log.Info("Creating file mappings...");

            stopwatch.Reset();
            stopwatch.Start();

            Dictionary<int, BigFileFile> fileKeyMapping = new Dictionary<int, BigFileFile>();
            Dictionary<int, BigFileFile> fileNumMapping = new Dictionary<int, BigFileFile>();

            for (int i = 0; i < filesList.Length; i++)
            {
                if (fileInfos[i].Name == null)
                    continue;

                if (!fileKeyMapping.ContainsKey(fileInfos[i].Key))
                    fileKeyMapping.Add(fileInfos[i].Key, filesList[i]);
                else
                    log.Error("File key mapping already contains key " + fileInfos[i].Key + " (File: " + filesList[i].Name + ")");

                if (filesList[i].FileInfo.FileNumber != -1)
                {
                    if (!fileNumMapping.ContainsKey(fileInfos[i].FileNumber))
                        fileNumMapping.Add(fileInfos[i].FileNumber, filesList[i]);
                    else
                        log.Error("File number mapping already contains key " + fileInfos[i].FileNumber + " " + filesList[i].Name);
                }
                else
                    log.Error(string.Format("File number is -1! (key:{0:X8})", fileInfos[i].Key));
            }

            log.Info("Mappings created!");

            diagData.CreateKeyAndNumMappings = stopwatch.ElapsedMilliseconds;

            stopwatch.Reset();

            return new FileMappingData(filesList, fileKeyMapping, fileNumMapping);
        }

        public void MapFilesToFolders(BigFileFolder rootFolder, FileMappingData mapping)
        {
            log.Info("Mapping files to folders...");

            stopwatch.Reset();
            stopwatch.Start();

            for (int i = 0; i < mapping.FilesList.Length; i++)
            {
                rootFolder.FolderMap[mapping.FilesList[i].FileInfo.Folder].Files.Add(mapping.FilesList[i]);
            }

            diagData.MapFilesToFolders = stopwatch.ElapsedMilliseconds;

            stopwatch.Reset();
        }

        public UnpackedFolderMapAndFilesList CreateFolderTreeAndFilesListFromDirectory(DirectoryInfo dir, UnpackedRenamedFileMapping mapping)
        {
            log.Info("Creating folder tree and files list from directory " + dir.FullName);
            BigFileFileInfo[] fileInfos = new BigFileFileInfo[mapping.KeyMap.Count];
            Dictionary<short, BigFileFolder> folderMap = new Dictionary<short, BigFileFolder>();
            short folderCount = 0;
            int fileCount = 0;

            Func<DirectoryInfo, string, BigFileFolder, BigFileFolder> recursion = null;
            recursion = (directory, dirName, parentFolder) =>
            {
                BigFileFolderInfo folderInfo = new BigFileFolderInfo()
                {
                    Unknown_01 = 0,
                    PreviousFolder = parentFolder != null ? parentFolder.FolderIndex : (short)0xFF,
                    NextFolder = -1,
                    Unknown_02 = 0,
                    Name = directory.Name.EncodeToBadString(length: 54),
                };

                BigFileFolder thisFolder = new BigFileFolder(folderCount, folderInfo, folderMap);
                folderMap.Add(folderCount, thisFolder);

                foreach (FileInfo file in directory.GetFiles())
                {
                    string fileName = dirName + "/" + file.Name;
                    UnpackedRenamedFileMapping.RenamedFileMappingData mappingData = mapping[fileName];
                    BigFileFileInfo fileInfo = new BigFileFileInfo()
                    {
                        CRC32 = new byte[4],
                        Key = mappingData.Key,
                        FileNumber = fileCount,
                        Name = mappingData.OriginalName.EncodeToBadString(length: 60),
                        Folder = folderCount,
                        Flags = 0x000f0000,
                    };

                    log.Debug("Add file " + file.FullName);

                    fileInfos[fileCount] = fileInfo;
                    fileCount++;
                }

                folderCount++;

                foreach (DirectoryInfo dirInfo in directory.GetDirectories())
                {
                    if (parentFolder == null)
                        thisFolder.SubFolders.Add(recursion.Invoke(dirInfo, dirInfo.Name, thisFolder));
                    else
                        thisFolder.SubFolders.Add(recursion.Invoke(dirInfo, dirName + "/" + dirInfo.Name, thisFolder));
                }

                return thisFolder;
            };

            recursion.Invoke(dir, "", null);

            return new UnpackedFolderMapAndFilesList()
            {
                folderMap = folderMap,
                filesList = fileInfos
            };
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
                foreach (BigFileFile file in folder.Files)
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
        public BigFileFileInfo[] filesList;
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

            internal void DebugLog(LogProxy log)
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
        private BigFileFile[] filesList;
        public BigFileFile[] FilesList { get { return filesList; } }
        private Dictionary<int, BigFileFile> keyMapping;
        public Dictionary<int, BigFileFile> KeyMapping { get { return keyMapping; } }
        private Dictionary<int, BigFileFile> numMapping;
        public Dictionary<int, BigFileFile> NumMapping { get { return numMapping; } }

        public FileMappingData(BigFileFile[] _filesList, Dictionary<int, BigFileFile> _keyMapping, Dictionary<int, BigFileFile> _numMapping)
        {
            filesList = _filesList;
            keyMapping = _keyMapping;
            numMapping = _numMapping;
        }
    }
}
