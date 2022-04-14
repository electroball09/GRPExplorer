﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using GRPExplorerLib.Util;
using GRPExplorerLib.Logging;

namespace GRPExplorerLib.BigFile.Files
{
    public class BigFileFileLoader
    {
        ILogProxy log = LogManager.GetLogProxy("BigFileFileLoader");
        BigFile bigFile;
        IOBuffers buffer = new IOBuffers();

        public BigFileFileLoader(BigFile _bigFile)
        {
            bigFile = _bigFile;
        }

        public IEnumerable<YetiObject> LoadAllSimple(List<YetiObject> objects)
        {
            foreach (BigFileFileRead fileRead in bigFile.FileReader.ReadAllFiles(objects, buffer, bigFile.FileReader.DefaultFlags))
            {
                log.Debug("Loading object {0} (key:{1:X8})", fileRead.file, fileRead.file.FileInfo.Key);
                if (fileRead.dataSize != -1)
                {
                    bigFile.FileUtil.AddReferencesToObject(fileRead.file, fileRead.header);
                    fileRead.file.Load(buffer[fileRead.dataSize], fileRead.dataSize);

                    yield return fileRead.file;
                }
            }
        }

        public void LoadAllSimpleNoYield(List<YetiObject> objects)
        {
            foreach (YetiObject obj in LoadAllSimple(objects))
            {
                log.Debug("  Loaded with no yield");
            }
        }

        public struct ObjectLoad
        {
            public YetiObject yetiObject;
            public int dataSize;
        }

        public IEnumerable<ObjectLoad> LoadObjectRecursive(YetiObject obj, HashSet<YetiObject> alreadyLoaded)
        {
            HashSet<YetiObject> loadedList;
            if (alreadyLoaded != null)
                loadedList = alreadyLoaded;
            else
                loadedList = new HashSet<YetiObject>();

            IEnumerable<ObjectLoad> Recurse(YetiObject toLoad)
            {
                if (loadedList.Contains(toLoad))
                    yield break;

                loadedList.Add(toLoad);

                log.Debug("Loading object {0}", toLoad.NameWithExtension);

                BigFileFileRead fileRead = bigFile.FileReader.ReadFile(toLoad, buffer, bigFile.FileReader.DefaultFlags);
                if (fileRead.dataSize != -1)
                {
                    bigFile.FileUtil.AddReferencesToObject(toLoad, fileRead.header);
                    toLoad.Load(buffer[fileRead.dataSize], fileRead.dataSize);

                    yield return new ObjectLoad() { yetiObject = toLoad, dataSize = fileRead.dataSize };

                    foreach (YetiObject refObj in toLoad.ObjectReferences)
                    {
                        if (refObj != null)
                        {
                            foreach (ObjectLoad nextLoad in Recurse(refObj))
                            {
                                yield return nextLoad;
                            }
                        }
                    }
                }
            }

            foreach (ObjectLoad loadedObj in Recurse(obj))
            {
                if (loadedObj.dataSize > 0)
                    yield return loadedObj;
            }
        }

        public void LoadObjectSimple(YetiObject obj)
        {
            BigFileFileRead fileRead = bigFile.FileReader.ReadFile(obj, buffer, bigFile.FileReader.DefaultFlags);
            if (fileRead.dataSize != -1)
            {
                bigFile.FileUtil.AddReferencesToObject(obj, fileRead.header);
                obj.Load(buffer[fileRead.dataSize], fileRead.dataSize);
            }
        }

        public void LoadReferences(List<YetiObject> files)
        {
            foreach (BigFileFileRead fileRead in bigFile.FileReader.ReadAllFiles(files, bigFile.FileUtil.IOBuffers, bigFile.FileReader.DefaultFlags))
            {
                if (fileRead.IsError())
                    continue;

                bigFile.FileUtil.AddReferencesToObject(fileRead.file, fileRead.header);
            }
        }

        public List<YetiObject> BuildLoadList(YetiObject rootObj)
        {
            HashSet<YetiObject> objList = new HashSet<YetiObject>();

            void Recurse(YetiObject obj)
            {
                if (obj == null)
                    return;

                if (!objList.Contains(obj))
                    objList.Add(obj);

                foreach (YetiObject obj2 in obj.ObjectReferences)
                    Recurse(obj2);
            }

            Recurse(rootObj);

            return objList.ToList();
        }
    }
}
