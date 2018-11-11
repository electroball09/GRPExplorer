using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using GRPExplorerLib.Logging;
using GRPExplorerLib.Util;

namespace GRPExplorerLib.BigFile.Extra
{
    public static class FEUtoSWF
    {
        static ILogProxy log = LogManager.GetLogProxy("FEUtoSWF");
        static readonly IOBuffers buffer1 = new IOBuffers();
        static readonly IOBuffers buffer2 = new IOBuffers();
        static readonly byte[] SWFHeader = { 0x46, 0x57, 0x53 };
        static readonly byte[] FEUHeader = { 0x55, 0x45, 0x46 };

        public static bool Convert(FileInfo fileInfo)
        {
            if (fileInfo.Extension != ".feu")
            {
                log.Error("File extension is not .feu");
                return false;
            }

            try
            {
                int len = 0;
                using (FileStream fs = File.OpenRead(fileInfo.FullName))
                {
                    log.Info("FEU file length is {0} KB", fs.Length / 1024f);
                    len = (int)fs.Length;
                    fs.Read(buffer1[fs.Length], 0, (int)fs.Length);
                }

                int ref1count = BitConverter.ToInt32(buffer1[len], 0);
                int ref2count = BitConverter.ToInt32(buffer1[len], 4);
                log.Info("{0} image refs, {1} .feu refs", ref1count, ref2count);

                int index = IndexOf(buffer1[len], FEUHeader);

                log.Info("Removing first {0} bytes", index);

                byte[] buf2 = buffer2[len - index];
                Array.Copy(buffer1[len], index, buf2, 0, len - index);

                buf2[0] = 0x46;
                buf2[1] = 0x57;
                buf2[2] = 0x53;

                string swfFileName = fileInfo.DirectoryName + "/" + Path.GetFileNameWithoutExtension(fileInfo.Name) + ".swf";

                using (FileStream fs = File.OpenWrite(swfFileName))
                {
                    fs.Write(buf2, 0, len - index);
                }

                log.Info("File created!");

                return true;
            }
            catch (Exception ex)
            {
                log.Error(ex.Message);
                return false;
            }
        }

        public static int IndexOf(byte[] arrayToSearchThrough, byte[] patternToFind)
        {
            if (patternToFind.Length > arrayToSearchThrough.Length)
                return -1;
            for (int i = 0; i < arrayToSearchThrough.Length - patternToFind.Length; i++)
            {
                bool found = true;
                for (int j = 0; j < patternToFind.Length; j++)
                {
                    if (arrayToSearchThrough[i + j] != patternToFind[j])
                    {
                        found = false;
                        break;
                    }
                }
                if (found)
                {
                    return i;
                }
            }
            return -1;
        }
    }
}
