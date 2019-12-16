using System;
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

        public IEnumerable<YetiObject> LoadObjectRecursive(YetiObject obj)
        {
            HashSet<YetiObject> loadedList = new HashSet<YetiObject>();

            IEnumerable<YetiObject> Recurse(YetiObject toLoad)
            {
                log.Info("Loading object {0}", toLoad.NameWithExtension);

                BigFileFileRead fileRead = bigFile.FileReader.ReadFile(toLoad, buffer, bigFile.FileReader.DefaultFlags);
                if (fileRead.dataSize != -1)
                {
                    bigFile.FileUtil.AddReferencesToObject(toLoad, fileRead.header);
                    toLoad.Load(buffer[fileRead.dataSize], fileRead.dataSize);

                    yield return toLoad;

                    foreach (YetiObject refObj in toLoad.ObjectReferences)
                    {
                        if (refObj != null && !loadedList.Contains(refObj))
                        {
                            loadedList.Add(refObj);
                            foreach (YetiObject nextLoad in Recurse(refObj))
                            {
                                yield return nextLoad;
                            }
                        }
                    }
                }
            }

            foreach (YetiObject loadedObj in Recurse(obj))
            {
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
            int count = 0;


            foreach (BigFileFileRead fileRead in bigFile.FileReader.ReadAllFiles(files, bigFile.FileUtil.IOBuffers, bigFile.FileReader.DefaultFlags))
            {
                if (fileRead.dataSize == -1)
                    continue;

                bigFile.FileUtil.AddReferencesToObject(files[count], fileRead.header);
                count++;
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
