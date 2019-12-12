using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using GRPExplorerLib.Logging;
using GRPExplorerLib.BigFile;

namespace GRPExplorerLib.YetiObjects
{
    public enum YetiTextureFormat : byte
    {
        AO = 0x0F,
        DXT5_1 = 0x0A,
        DXT5_2 = 0x0C,
        DXT1_1 = 0x08,
        DXT1_2 = 0x09,
        BGRA32 = 0x04,
        RGBA32 = 0x05,
        XBOX_A = 0x27,
        XBOX_B = 0x28
    }

    public class YetiTextureMetadata : YetiObjectArchetype
    {
        public override YetiObjectType Identifier => YetiObjectType.tga;

        public ushort Width { get; private set; }
        public ushort Height { get; private set; }
        public YetiTextureFormat Format { get; private set; }
        public YetiTexturePayload Payload { get; private set; }

        public override void Load(byte[] buffer, int size, YetiObject[] objectReferences)
        {
            if (size < 10)
                return;

            Width = BitConverter.ToUInt16(buffer, 4);
            Height = BitConverter.ToUInt16(buffer, 6);
            Format = (YetiTextureFormat)buffer[9];

            if (objectReferences.Length == 0)
            {
                LogManager.Error("WTF " + Object.Name);
                return;
            }

            Payload = objectReferences[0]?.ArchetypeAs<YetiTexturePayload>();
        }

        public override void Log(ILogProxy log)
        {
            log.Info("w:{0} h:{1} format:{2}", Width, Height, Format);
        }
    }

    public class YetiTexturePayload : YetiObjectArchetype
    {
        public override YetiObjectType Identifier => YetiObjectType.txd;

        public byte[] Data { get; private set; }

        public override void Load(byte[] buffer, int size, YetiObject[] objectReferences)
        {
            Data = new byte[size - 8];
            Array.Copy(buffer, 8, Data, 0, Data.Length);

            int hmm = BitConverter.ToInt32(buffer, 0);
            if (hmm != 1)
                LogManager.Error("wtf! value is " + hmm);
        }

        public override void Unload()
        {
            Data = null;
        }

        public override void Log(ILogProxy log)
        {
            log.Info("TEXTUREPAYLOADFILEARCHETYPE");
        }
    }
}
