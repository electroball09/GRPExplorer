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
        /// <param name="filesToLoad"></param>
        /// <param name="loadReferences"></param>
        public void LoadFiles(List<BigFileFile> filesToLoad)
        {
            foreach (BigFileFile file in filesToLoad)
            {
                log.Debug("Loading file {0} (key:{1:X8})", file, file.FileInfo.Key);
                int[] header = bigFile.FileReader.ReadFileHeader(file, buffer, bigFile.FileReader.DefaultFlags);
                int size = bigFile.FileReader.ReadFileData(file, buffer, BigFileFlags.Decompress);
                if (size != -1)
                {
                    bigFile.FileUtil.AddFileReferencesToFile(file, header);
                    file.Load(buffer[size], size);
                }
                else
                    log.Error("Couldn't read file " + file.FullFolderPath + file.Name);
            }
        }

        public void LoadReferences(List<BigFileFile> files)
        {
            LoadReferences(ListExtensions.GetInternalArray(files));
        }

        public void LoadReferences(BigFileFile[] files)
        {
            int count = 0;
            foreach (int[] header in bigFile.FileReader.ReadAllHeaders(files, bigFile.FileUtil.IOBuffers, bigFile.FileReader.DefaultFlags))
            {
                bigFile.FileUtil.AddFileReferencesToFile(files[count], header);
                count++;
            }
        }
    }
}
