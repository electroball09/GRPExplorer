using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using GRPExplorerLib.Logging;
using GRPExplorerLib.BigFile.Versions;

namespace GRPExplorerLib.BigFile.Extra
{
    public class BigFileFileExtensionsList
    {
        private ILogProxy log = LogManager.GetLogProxy("BigFileFileExtensionGenerator");
        private BigFile _bigFile;

        public BigFileFileExtensionsList(BigFile bigFile)
        {
            _bigFile = bigFile;
        }

        public Dictionary<short, string> LoadFileExtensionsListFromLoadReport(FileInfo loadReportFile)
        {
            if (!loadReportFile.Exists)
            {
                log.Error("File \"{0}\" does not exist!", loadReportFile.FullName);
                return null;
            }

            log.Info("Loading a file extensions list from load report {0}", loadReportFile.Name);

            Dictionary<short, string> extensionsList = new Dictionary<short, string>();
            using (StreamReader sr = new StreamReader(loadReportFile.FullName))
            {
                string line = "";
                string typeStr = "";
                string keyStr = "";
                while ((line = sr.ReadLine()) != null)
                {
                    //LOAD type(tga) [key: b3104ea8]  SetData:1
                    int ind = line.IndexOf("LOAD type(");
                    if (ind == -1)
                        ind = line.IndexOf("FREE type(");
                    if (ind == -1)
                        continue;

                    typeStr = new string(line.Skip(ind + 10).Take(3).ToArray());
                    keyStr = new string(line.Skip(ind + 21).TakeWhile(c => c != ']').ToArray());

                    int key = Convert.ToInt32(keyStr, 16);
                    BigFileFile bigFileFile = _bigFile.FileMap[key];
                    if (bigFileFile == null)
                    {
                        log.Error("File [key: {0:X8}] does not exist within the bigfile!", key);
                        continue;
                    }

                    short type = bigFileFile.FileInfo.FileType;

                    if (extensionsList.ContainsKey(type))
                    {
                        if (extensionsList[type] != typeStr)
                            log.Error("Multiple file extensions for short {0:X4}", type);
                    }
                    else
                    {
                        extensionsList.Add(type, typeStr);
                    }
                }
            }

            return extensionsList;
        }

        public Dictionary<short, string> LoadFileExtensionsList(FileInfo fileInfo)
        {
            log.Info("Loading a file extensions list from file {0}", fileInfo.FullName);

            Dictionary<short, string> extensionsList = new Dictionary<short, string>();
            using (StreamReader sr = new StreamReader(fileInfo.FullName))
            {
                string line = "";
                string typeStr = "";
                string extStr = "";
                while ((line = sr.ReadLine()) != null)
                {
                    typeStr = line.Substring(0, 4);
                    extStr = line.Substring(5, 3);

                    short type = Convert.ToInt16(typeStr, 16);
                    extensionsList.Add(type, extStr);
                }
            }
            return extensionsList;
        }

        public void WriteFileExtensionsListToFile(Dictionary<short, string> extensionsList, bool overwrite = false, string fileName = BigFileConst.EXTENSIONS_LIST_FILE_NAME + BigFileConst.GRP_EXPLORER_EXTENSION)
        {
            log.Info("Writing a file extensions list to file {0}", fileName);

            if (File.Exists(fileName) && !overwrite)
            {
                Dictionary<short, string> tmpList = LoadFileExtensionsList(new FileInfo(fileName));
                foreach (KeyValuePair<short, string> kvp in tmpList)
                {
                    if (!extensionsList.ContainsKey(kvp.Key))
                        extensionsList.Add(kvp.Key, kvp.Value);
                }
            }

            using (StreamWriter sw = new StreamWriter(fileName))
            {
                foreach (KeyValuePair<short, string> kvp in extensionsList)
                {
                    sw.WriteLine("{0:X4}={1}", kvp.Key, kvp.Value);
                }
            }
        }

        public void VerifyFileTypes(Dictionary<short, string> extensionsList)
        {
            List<short> tmpList = new List<short>();

            foreach (IBigFileFileInfo fileInfo in _bigFile.RawFileInfos)
                if (!extensionsList.ContainsKey(fileInfo.FileType))
                    if (!tmpList.Contains(fileInfo.FileType))
                        tmpList.Add(fileInfo.FileType);

            if (tmpList.Count > 0)
            {
                log.Error("Found file types with no extension: ");
                foreach (short s in tmpList)
                    log.Error("   {0:X4}", s);
            }
        }
    }
}
