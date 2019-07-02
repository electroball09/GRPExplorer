using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using GRPExplorerLib.Logging;

namespace GRPExplorerLib.BigFile.Files.Archetypes
{
    public enum TextureFormat : byte
    {
        AO = 0x0F,
        DXT5_1 = 0x0A,
        DXT5_2 = 0x0C,
        DXT1_1 = 0x08,
        DXT1_2 = 0x09,
        RGBA32_1 = 0x04,
        RGBA32_2 = 0x05,
        XBOX_A = 0x27,
        XBOX_B = 0x28
    }

    public class TextureMetadataFileArchetype : BigFileFileArchetype
    {
        public override short Identifier => 0x0020;

        public ushort Width { get; private set; }
        public ushort Height { get; private set; }
        public TextureFormat Format { get; private set; }

        public override void Load(byte[] rawData)
        {
            if (rawData.Length < 10)
                return;

            Width = BitConverter.ToUInt16(rawData, 4);
            Height = BitConverter.ToUInt16(rawData, 6);
            Format = (TextureFormat)rawData[9];
        }

        public override void Log(ILogProxy log)
        {
            log.Info("w:{0} h:{1} format:{2}", Width, Height, Format);
        }
    }

    public class TexturePayloadFileArchetype : BigFileFileArchetype
    {
        public override short Identifier => 0x007E;

        public override void Load(byte[] rawData)
        {
            
        }

        public override void Log(ILogProxy log)
        {
            log.Info("TEXTUREPAYLOADFILEARCHETYPE");
        }
    }
}
