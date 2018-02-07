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

        public FileMappingData CreateFileMappings(BigFileFolder rootFolder, BigFileFileInfo[] fileInfos)
        {
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

        int logCount = 0;
        public void DebugLogRootFolderTree(BigFileFolder folder, int depth = 0)
        {
            logCount++;

            string folderString = logCount.ToString();
            string fileString = "     ";
            for (int i = 0; i < depth; i++)
            {
                folderString += "-";
                fileString += " ";
            }
            foreach (BigFileFile file in folder.Files)
                log.Debug(fileString + file.Name);
            log.Debug(folderString + Encoding.Default.GetString(folder.InfoStruct.Name));
            foreach (BigFileFolder folder2 in folder.SubFolders)
                DebugLogRootFolderTree(folder2, depth + 1);

            logCount = 0;
        }

        short folderCount = 0;
        public BigFileFolder CreateFolderTreeFromDirectory(DirectoryInfo dir, BigFileFolder parentFolder = null)
        {
            BigFileFolderInfo folderInfo = new BigFileFolderInfo()
            {
                Unknown_01 = int.MaxValue,
                PreviousFolder = -1,
                NextFolder = -1,
                Unknown_02 = -1,
                Name = Encoding.Default.GetBytes(dir.Name)
            };

            bool isFirst = false;
            BigFileFolder thisFolder = null;
            if (parentFolder == null)
            {
                isFirst = true;
                log.Info("Creating BigFileFolder tree from directory " + dir.FullName);
                folderCount = 0;
                folderInfo.PreviousFolder = -1;
                Dictionary<short, BigFileFolder> folderMap = new Dictionary<short, BigFileFolder>();
                thisFolder = new BigFileFolder(folderCount, folderInfo, folderMap);
            }
            else
            {
                folderInfo.PreviousFolder = parentFolder.FolderIndex;
                thisFolder = new BigFileFolder(folderCount, folderInfo, parentFolder.FolderMap);
            }
            log.Debug(string.Format("Adding folder " + thisFolder.Name + " with key {0:X4}", thisFolder.FolderIndex));
            thisFolder.FolderMap.Add(thisFolder.FolderIndex, thisFolder);
            folderCount++;

            foreach (DirectoryInfo newInfo in dir.GetDirectories())
            {
                thisFolder.SubFolders.Add(CreateFolderTreeFromDirectory(newInfo, thisFolder));
            }

            if (isFirst)
            {
                log.Info("Created " + folderCount + " folders");
            }

            return thisFolder;
        }

        //public BigFileFileInfo[] CreateUnpackedFileInfos(DirectoryInfo dir, UnpackedRenamedFileMapping mapping)
        //{

        //}

        public void MapRenamedFilesToFolderTree(DirectoryInfo dir, BigFileFolder rootFolder, UnpackedRenamedFileMapping mapping)
        //public void MapRenamedFilesToFolderTree(DirectoryInfo dir)
        {


            Action<DirectoryInfo> recursion = null;
            recursion = (DirectoryInfo directory) =>
            {
                foreach (FileInfo fileInfo in directory.GetFiles())
                {
                    
                }

                foreach (DirectoryInfo directory2 in directory.GetDirectories())
                {
                    recursion.Invoke(directory2);
                }
            };
            recursion.Invoke(dir);
        }
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
