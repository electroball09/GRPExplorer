using GRPExplorerLib.BigFile;
using GRPExplorerLib.Logging;
using System;
using System.IO;

namespace GRPExplorerLib.YetiObjects
{
    public class YetiLayer : YetiObjectArchetype
    {
        public override YetiObjectType Identifier => YetiObjectType.lay;

        public int NumberThatShouldBeOne { get; private set; }
        public string LayerName { get; private set; } = "";
        public int Flags01 { get; private set; }
        public int Flags02 { get; private set; }
        public int Flags03 { get; private set; }

        public override void Load(byte[] buffer, int size, YetiObject[] objectReferences)
        {
            if (size != 80)
            {
                log.Error($"Layer file size is not 80!  size: {size}  key: {Object.FileInfo.Key:X8}");
                return;
            }

            using (MemoryStream ms = new MemoryStream(buffer))
            using (BinaryReader br = new BinaryReader(ms))
            {
                NumberThatShouldBeOne = br.ReadInt32();
                if (NumberThatShouldBeOne != 1)
                    log.Error($"Layer number is not one! ({NumberThatShouldBeOne})    key: {Object.FileInfo.Key:X8}");
                LayerName = Util.StringUtil.ReadNullTerminatedString(ms);

                ms.Seek(0x44, SeekOrigin.Begin);

                Flags01 = br.ReadInt32();
                Flags02 = br.ReadInt32();
                Flags03 = br.ReadInt32();
            }
        }

        public override void Log(ILogProxy log)
        {
            log.Info($"YETILAYER number: {NumberThatShouldBeOne}  name: {LayerName}  flags: {Flags01:X8}{Flags02:X8}{Flags03:X8}");
        }
    }
}
