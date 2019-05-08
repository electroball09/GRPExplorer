using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace GRPExplorerLib.BigFile.Files.Archetypes
{
    public class TextureMetadataFileArchetype : BigFileFileArchetype
    {
        public override short Identifier => 0x0020;

        public ushort Width { get; private set; }
        public ushort Height { get; private set; }
        public byte Format { get; private set; }

        public override void Load(byte[] rawData)
        {
            if (rawData.Length < 10)
                return;

            Width = BitConverter.ToUInt16(rawData, 4);
            Height = BitConverter.ToUInt16(rawData, 6);
            Format = rawData[9];
        }
    }

    public class TexturePayloadFileArchetype : BigFileFileArchetype
    {
        public override short Identifier => 0x007E;

        public override void Load(byte[] rawData)
        {
            
        }
    }
}
