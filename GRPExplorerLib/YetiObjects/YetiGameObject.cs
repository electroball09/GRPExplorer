using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using GRPExplorerLib.BigFile;
using GRPExplorerLib.Logging;
using System.Numerics;
using System.IO;

namespace GRPExplorerLib.YetiObjects
{
    public enum YetiLightType
    {
        NONE, Point, Spot, Directional
    }

    public class YetiGameObject : YetiObjectArchetype
    {
        public override YetiObjectType Identifier => YetiObjectType.gao;

        public Matrix4x4 Matrix { get; private set; }
        public int TestValue { get; private set; }
        public int Flags { get; private set; }
        public int Size { get; private set; }

        public YetiLightType LightType;
        public byte r, g, b, a;
        public float intensity, intensity_2, radius, radius_max, radius_extra;

        public YetiLayer Layer { get; private set; }

        public override void Load(byte[] buffer, int size, YetiObject[] objectReferences)
        {
            Size = size;

            if (objectReferences.Length > 0)
            {
                var lastObjectRef = objectReferences[objectReferences.Length - 1];
                if (lastObjectRef != null && lastObjectRef.FileInfo.FileType == YetiObjectType.lay)
                    Layer = objectReferences[objectReferences.Length - 1].ArchetypeAs<YetiLayer>();
            }

            using (MemoryStream ms = new MemoryStream(buffer, 0, size))
            using (BinaryReader br = new BinaryReader(ms))
            {
                TestValue = br.ReadInt32();
                Flags = br.ReadInt32();
                br.ReadInt32();
                br.ReadByte();
                br.ReadByte();
                br.ReadByte();

                Matrix =
                    new Matrix4x4
                    (
                        m11: br.ReadSingle(),
                        m21: br.ReadSingle(),
                        m31: br.ReadSingle(),
                        m41: br.ReadSingle(),

                        m12: br.ReadSingle(),
                        m22: br.ReadSingle(),
                        m32: br.ReadSingle(),
                        m42: br.ReadSingle(),

                        m13: br.ReadSingle(),
                        m23: br.ReadSingle(),
                        m33: br.ReadSingle(),
                        m43: br.ReadSingle(),

                        m14: br.ReadSingle(),
                        m24: br.ReadSingle(),
                        m34: br.ReadSingle(),
                        m44: br.ReadSingle()
                    );

                try
                {
                    byte lt = br.ReadByte();
                    if (lt < 4)
                        LightType = (YetiLightType)lt;

                    ms.Seek(0x58, SeekOrigin.Begin);
                    b = br.ReadByte();
                    g = br.ReadByte();
                    r = br.ReadByte();
                    a = br.ReadByte();
                    intensity = br.ReadSingle();
                    intensity_2 = br.ReadSingle();
                    radius = br.ReadSingle();
                    radius_max = br.ReadSingle();
                    radius_extra = br.ReadSingle();
                }
                catch (Exception ex)
                {
                    //log.Error(ex.Message);
                }
            }
        }

        public override void Log(ILogProxy log)
        {
            log.Info("YETIGAMEOBJECT");
            log.Info("> {0}", Matrix.ToString());
        }
    }

    public class YetiGraphicObjectTable : YetiObjectArchetype
    {
        public override YetiObjectType Identifier => YetiObjectType.got;

        public override void Load(byte[] buffer, int size, YetiObject[] objectReferences)
        {
            
        }

        public override void Log(ILogProxy log)
        {
            log.Info("YETIGAMEOBJECTEX");
        }
    }
}
