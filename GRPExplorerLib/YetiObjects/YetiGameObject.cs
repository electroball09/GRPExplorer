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
    public class YetiGameObject : YetiObjectArchetype
    {
        public override YetiObjectType Identifier => YetiObjectType.gao;

        public Matrix4x4 Matrix { get; private set; }
        public int TestValue { get; private set; }
        public int Flags { get; private set; }
        public int Size { get; private set; }

        public override void Load(byte[] buffer, int size, YetiObject[] objectReferences)
        {
            Size = size;

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
                        m12: br.ReadSingle(),
                        m13: br.ReadSingle(),
                        m14: br.ReadSingle(),

                        m21: br.ReadSingle(),
                        m22: br.ReadSingle(),
                        m23: br.ReadSingle(),
                        m24: br.ReadSingle(),

                        m31: br.ReadSingle(),
                        m32: br.ReadSingle(),
                        m33: br.ReadSingle(),
                        m34: br.ReadSingle(),

                        m41: br.ReadSingle(),
                        m42: br.ReadSingle(),
                        m43: br.ReadSingle(),
                        m44: br.ReadSingle()
                    );
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
