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
        static readonly byte[] SWFHeader = { 0x46, 0x57, 0x53 }; // FWS
        static readonly byte[] FEUHeader = { 0x55, 0x45, 0x46 }; // UEF

        public static bool Convert(FileInfo fileInfo)
        {
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
                log.Info("{0} image refs, {1} .feu refs ( THIS IS VERY WRONG DON'T TRUST )", ref1count, ref2count);

                //seems like either the two numbers above are inaccurate or they don't do what i think they do...
                //...either way we can still convert the files

                //byte[] buffer = buffer2[IOBuffers.KB];
                //int ind = 0;
                //for (int i = 0; i < ref1count + ref2count; i++)
                //{
                //    byte b = 1;
                //    int j = 0;
                //    while ((b = buffer1[len][ind + 8]) != 0)
                //    {
                //        buffer[j] = b;

                //        j++;
                //        ind++;
                //    }

                //    string str = System.Text.Encoding.ASCII.GetString(buffer, 0, j);
                //    if (i < ref1count)
                //        log.Info("   IMAGE REFERENCE: {0}", str);
                //    else
                //        log.Info("   FEU REFERENCE: {0}", str);
                //}

                int index = IndexOf(buffer1[len], FEUHeader);

                log.Info("Removing first {0} bytes", index);

                byte[] buf2 = buffer2[len - index];
                Array.Copy(buffer1[len], index, buf2, 0, len - index);

                Array.Copy(SWFHeader, 0, buf2, 0, SWFHeader.Length);

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
