using GRPExplorerLib.BigFile;
using GRPExplorerLib.Logging;
using System.IO;

namespace GRPExplorerLib.YetiObjects
{
    public class YetiCubemap : YetiObjectArchetype
    {
        public struct CubemapFace
        {
            public int Width { get; private set; }
            public byte[] Data { get; private set; }

            public CubemapFace(int width, byte[] data)
            {
                Width = width;
                Data = data;
            }
        }

        public override YetiObjectType Identifier => YetiObjectType.cub;

        public YetiCubemap CubemapPC { get; private set; }
        public YetiCubemap CubemapX360 { get; private set; }
        public YetiCubemap CubemapPS3 { get; private set; }

        public bool IsMasterCubemap { get; private set; } = false;

        public CubemapFace[] Cubemap { get; private set; }

        public override void Load(byte[] buffer, int size, YetiObject[] objectReferences)
        {
            if (size == 0)
            {
                IsMasterCubemap = true;
                CubemapPC = objectReferences[0].ArchetypeAs<YetiCubemap>();
                CubemapX360 = objectReferences[1].ArchetypeAs<YetiCubemap>();
                CubemapPS3 = objectReferences[2].ArchetypeAs<YetiCubemap>();
                return;
            }

            if (Object.Name.Contains("360") || Object.Name.Contains("PS3"))
                return;

            Cubemap = new CubemapFace[6];
            using (MemoryStream ms = new MemoryStream(buffer, 0, size))
            using (BinaryReader br = new BinaryReader(ms))
            {
                ms.Seek(5, SeekOrigin.Begin);

                if (br.ReadByte() != 0)
                    return;

                ms.Seek(16, SeekOrigin.Begin);

                for (int i = 0; i < 6; i++)
                {
                    log.Info($"Reading face {i}   key: {Object.FileInfo.Key:X8}  size: {size}  ms.Length{ms.Length}  ms.Position{ms.Position}");
                    int width = br.ReadInt32();
                    int numBytes = width * width * 8; //square image * 8 bytes per pixel (RGBAHalf)
                    byte[] data = new byte[numBytes];
                    ms.Read(data, 0, numBytes);

                    Cubemap[i] = new CubemapFace(width, data);
                }
            }
        }

        public override void Log(ILogProxy log)
        {
            log.Info($"YETICUBEMAP IsMasterCubemap: {IsMasterCubemap}  CubemapPC: {CubemapPC?.Object.FileInfo.Key:X8}  CubemapX360: {CubemapX360?.Object.FileInfo.Key:X8}  CubemapPS3: {CubemapPS3?.Object.FileInfo.Key:X8}  ");
        }
    }
}
