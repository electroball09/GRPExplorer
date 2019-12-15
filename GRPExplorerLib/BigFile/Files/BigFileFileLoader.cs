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

        /// <summary>
        /// 
        /// </summary>
        /// <param name="objects"></param>
        /// <param name="loadReferences"></param>
        public void LoadAll(List<YetiObject> objects)
        {
            foreach (YetiObject file in objects)
            {
                log.Debug("Loading object {0} (key:{1:X8})", file, file.FileInfo.Key);
                //int[] header = bigFile.FileReader.ReadFileHeader(file, buffer, bigFile.FileReader.DefaultFlags);
                //int size = bigFile.FileReader.ReadFileData(file, buffer, BigFileFlags.Decompress);
                BigFileFileRead fileRead = bigFile.FileReader.ReadFile(file, buffer, bigFile.FileReader.DefaultFlags);
                if (fileRead.dataSize != -1)
                {
                    bigFile.FileUtil.AddReferencesToObject(file, fileRead.header);
                    file.Load(buffer[fileRead.dataSize], fileRead.dataSize);
                }
                else
                    log.Error("Couldn't read file " + file.FullFolderPath + file.Name);
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
