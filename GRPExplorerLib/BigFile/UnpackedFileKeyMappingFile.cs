using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using GRPExplorerLib.Util;

namespace GRPExplorerLib.BigFile
{
    public class UnpackedFileKeyMappingFile
    {
        private FileInfo fileInfo;

        private LogProxy log = new LogProxy("UnpackedFileKeyMappingFile");

        byte[] tempBuffer = new byte[300];

        public UnpackedFileKeyMappingFile(DirectoryInfo dir)
        {
            fileInfo = new FileInfo(dir.FullName + "\\" + BigFileConst.UNPACK_FILE_MAPPING_NAME);
        }

        public void SaveMappingData(UnpackedRenamedFileMapping mapping)
        {
            using (FileStream fs = File.Create(fileInfo.FullName))
            {
                foreach (KeyValuePair<int, UnpackedRenamedFileMapping.RenamedFileMappingData> kvp in mapping.Map)
                {
                    fs.WriteByte(0x1C);

                    kvp.Value.WriteToStream(fs, tempBuffer);
                }
            }
        }

        public UnpackedRenamedFileMapping LoadMappingData()
        {
            if (!fileInfo.Exists)
                throw new Exception("Can't read from null file!");

            UnpackedRenamedFileMapping mapping = new UnpackedRenamedFileMapping();

            using (FileStream fs = File.OpenRead(fileInfo.FullName))
            {
                int sig = fs.ReadByte();
                while (sig != -1)
                {
                    UnpackedRenamedFileMapping.RenamedFileMappingData data = new UnpackedRenamedFileMapping.RenamedFileMappingData();

                    data.ReadFromStream(fs, tempBuffer);

                    mapping.Map.Add(data.Key, data);

                    sig = fs.ReadByte();
                }
            }

            return mapping;
        }
    }
}
