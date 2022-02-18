using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using GRPExplorerLib.Logging;
using GRPExplorerLib.BigFile;
using System.Numerics;
using System.IO;

namespace GRPExplorerLib.YetiObjects
{
    public struct YetiBone
    {
        public string name;
        public byte parent;
        public byte[] flags;
        public float[] floats;

        public override string ToString()
        {
            if (floats == null)
                return "YETIBONE oh no im not real D:";

            StringBuilder sb = new StringBuilder();
            sb.AppendLine($"YETIBONE name: {name}  parent: {parent:X2}  ");
            for (int i = 0; i < 4; i++)
            {
                StringBuilder sbd = new StringBuilder();
                sbd.Append("            ");
                for (int j = 0; j < 3; j++)
                {
                    sbd.Append("| ");
                    for (int k = 0; k < 4; k++)
                    {
                        sbd.Append($"{floats[(i * 4) + (j * 16) + k]:F3} ");
                    }
                }
                sbd.Append("|");
                sb.AppendLine(sbd.ToString());
            }
            return sb.ToString();

            for (int i = 0; i < floats.Length; i++)
            {
                sb.Append($"{floats[i]:F3} ");
            }
            return sb.ToString();
        }
    }

    public class YetiSkeleton : YetiObjectArchetype
    {
        public override YetiObjectType Identifier => YetiObjectType.ske;

        public byte NumBones { get; private set; }
        public YetiBone[] Bones { get; private set; }

        public override void Load(byte[] buffer, int size, YetiObject[] objectReferences)
        {
            using (MemoryStream ms = new MemoryStream(buffer, 0, size))
            using (BinaryReader br = new BinaryReader(ms))
            {
                ms.Seek(1, SeekOrigin.Begin);

                NumBones = br.ReadByte();
                Bones = new YetiBone[NumBones];
                if (br.ReadByte() > 0x00)
                    return;

                ms.Seek(7, SeekOrigin.Begin);

                for (int i = 0; i < NumBones; i++)
                {
                    Bones[i].parent = br.ReadByte();
                    Bones[i].flags = new byte[3];
                    ms.Read(Bones[i].flags, 0, 3);
                    Bones[i].floats = new float[48];
                    for (int j = 0; j < 47; j++)
                        Bones[i].floats[j] = br.ReadSingle();
                    if (i + 1 != NumBones)
                        Bones[i].floats[47] = br.ReadSingle();
                }

                for (int i = 0; i < NumBones; i++)
                {
                    StringBuilder sb = new StringBuilder();
                    byte len = br.ReadByte();
                    for (int j = 0; j < len; j++)
                        sb.Append(Convert.ToChar(br.ReadByte()));
                    Bones[i].name = sb.ToString();
                }
            }
        }

        public override void Log(ILogProxy log)
        {
            log.Info($"YETISKELETON numBones: {NumBones}");
            for (int i = 0; i < NumBones; i++)
            {
                log.Info($"{Bones[i]}");
            }
        }
    }
}
