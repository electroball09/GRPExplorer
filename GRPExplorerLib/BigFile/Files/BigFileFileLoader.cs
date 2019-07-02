using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using GRPExplorerLib.Util;

namespace GRPExplorerLib.BigFile.Files
{
    public class BigFileFileLoader
    {
        BigFile bigFile;
        IOBuffers buffer = new IOBuffers();

        public BigFileFileLoader(BigFile _bigFile)
        {
            bigFile = _bigFile;
        }

        /// <summary>
        /// LOAD REFERENCES IS NOT FINISHED
        /// </summary>
        /// <param name="filesToLoad"></param>
        /// <param name="loadReferences"></param>
        public void LoadFiles(List<BigFileFile> filesToLoad, bool loadReferences = true)
        {
            foreach (BigFileFile file in filesToLoad)
            {
                int size = bigFile.FileReader.ReadFileData(file, buffer, BigFileFlags.Decompress);
                if (size != -1)
                    file.Load(buffer[size], size);
                else
                    Console.WriteLine("Couldn't read file " + file.FullFolderPath + file.Name);
            }
        }
    }
}
