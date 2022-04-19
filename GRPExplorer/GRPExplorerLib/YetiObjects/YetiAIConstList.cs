using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using GRPExplorerLib.Logging;
using GRPExplorerLib.BigFile;
using GRPExplorerLib.Util;
using System.IO;
using System.Numerics;
using System.Xml;

namespace GRPExplorerLib.YetiObjects
{
    public class YetiAIConstList : YetiObjectArchetype
    {
        public override YetiObjectType Identifier => YetiObjectType.cst;

        public Dictionary<string, AIConstValueBase> ConstList { get; private set; } = new Dictionary<string, AIConstValueBase>();

        public override void Load(byte[] buffer, int size, YetiObject[] objectReferences)
        {
            using (MemoryStream ms = new MemoryStream(buffer))
            using (BinaryReader br = new BinaryReader(ms))
            {
                if (br.ReadInt64() == 8391171954870665532)
                    LoadNewConstList(ms, br);
                else
                    LoadOldConstList(ms, br);
            }
        }

        private void LoadOldConstList(MemoryStream ms, BinaryReader br)
        {
            int numConsts = br.ReadInt32();

            for (int i = 0; i < numConsts; i++)
            {
                byte type = br.ReadByte();
                string name = StringUtil.ReadNullTerminatedString(ms);

                AIConstValueBase val = null;
                switch (type)
                {
                    case 1:
                        val = new AIConstValueInt();
                        break;
                    case 2:
                        val = new AIConstValueFloat();
                        break;
                    case 3:
                        val = new AIConstValueVector();
                        break;
                    default:
                        log.Error("Oh shit what kind of value is this?? ({0})", type);
                        break;
                }

                val.ReadValue(br);

                ConstList.Add(name, val);
            }
        }

        private void LoadNewConstList(MemoryStream ms, BinaryReader br)
        {
            ms.Seek(0, SeekOrigin.Begin);
            using (XmlReader xr = XmlReader.Create(ms))
            {
                try
                {
                    while (xr.Read())
                    {
                        if (xr.IsStartElement("Const"))
                        {
                            string name = xr.GetAttribute("Name");
                            string type = xr.GetAttribute("Type");

                            AIConstValueBase val = null;
                            switch (type)
                            {
                                case "INT":
                                    val = new AIConstValueInt();
                                    break;
                                case "FLT":
                                    val = new AIConstValueFloat();
                                    break;
                                case "VEC":
                                    val = new AIConstValueVector();
                                    break;
                            }

                            val.ReadValue(xr);

                            ConstList.Add(name, val);
                        }
                    }
                }
                catch (XmlException ex) { } //for some reason an exception is thrown on the </AIConst> node
            }
        }

        public override void Log(ILogProxy log)
        {
            log.Info("AI CONST LIST - num consts: {0}", ConstList.Count);
            foreach (KeyValuePair<string, AIConstValueBase> kvp in ConstList)
            {
                log.Info("    {0} => {1}", kvp.Key, kvp.Value.ToString());
            }
        }
    }

    public abstract class AIConstValueBase
    {
        public abstract void ReadValue(BinaryReader br);
        public abstract void ReadValue(XmlReader xr);
    }

    public class AIConstValueInt : AIConstValueBase
    {
        private int value;

        public override void ReadValue(BinaryReader br)
        {
            value = br.ReadInt32();
        }

        public override void ReadValue(XmlReader xr)
        {
            value = xr.ReadElementContentAsInt();
        }

        public override string ToString()
        {
            return value.ToString();
        }
    }

    public class AIConstValueFloat : AIConstValueBase
    {
        private float value;

        public override void ReadValue(BinaryReader br)
        {
            value = br.ReadSingle();
        }

        public override void ReadValue(XmlReader xr)
        {
            value = xr.ReadElementContentAsFloat();
        }

        public override string ToString()
        {
            return value.ToString();
        }
    }

    public class AIConstValueVector : AIConstValueBase
    {
        private Vector3 value;

        public override void ReadValue(BinaryReader br)
        {
            value = new Vector3()
            {
                X = br.ReadSingle(),
                Y = br.ReadSingle(),
                Z = br.ReadSingle()
            };
        }

        public override void ReadValue(XmlReader xr)
        {
            value = new Vector3();

            xr.Read(); 
            xr.Read(); // dunno why i need this twice
            value.X = xr.ReadElementContentAsFloat();
            xr.Read();
            value.Y = xr.ReadElementContentAsFloat();
            xr.Read();
            value.Z = xr.ReadElementContentAsFloat();
        }

        public override string ToString()
        {
            return value.ToString();
        }
    }
}
