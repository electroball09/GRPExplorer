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
    public struct YetiVertex
    {
        public Vector3 vertexPos;
        public Vector4 vertexColor;
        public Vector2 uv0;
        public Vector2 uv1;
        public Vector2 uv2;
        public Vector2 uv3;
        public Vector2 uv4;

        //public override string ToString()
        //{
        //    return string.Format("VERTEX: pos:{0}  uv1{4}   {1}   [Color:{2}]   {3}", vertexPos.ToString(), boneData.ToString(), vertexColor.ToString(), otherData.ToString(), uv0,ToString());
        //}
    }

    public struct YetiBoneData
    {
        //public float Weight1;
        //public float Weight2;
        //public float Weight3;
        //public float Weight4;

        //public byte Bone1;
        //public byte Bone2;
        //public byte Bone3;
        //public byte Bone4;

        public float[] datas;

        public override string ToString()
        {
            //return string.Format("[BoneData bone|weight: {0}|{1} {2}|{3} {4}|{5} {6}|{7}]", Bone1, Weight1, Bone2, Weight2, Bone3, Weight3, Bone4, Weight4);

            StringBuilder sb = new StringBuilder();
            sb.Append("[BoneData");
            for (int i = 0; i < datas.Length; i++)
            {
                sb.Append(" " + datas[i].ToString());
            }
            sb.Append("]");
            return sb.ToString();
        }
    }

    [Serializable]
    public struct YetiSubmeshData
    {
        public short UNK_01;
        public short UNK_02;
        public short UNK_03;
        public short UNK_04;
        public short UNK_05;

        public short UNK_11;
        public short UNK_12;
        public short UNK_13;
        public short UNK_14;
        public short UNK_15;

        public int NegativeOneSanityCheck;

        public uint[] UNK_ARR;

        public void Load(BinaryReader br)
        {
            UNK_01 = br.ReadInt16();
            UNK_02 = br.ReadInt16();
            UNK_03 = br.ReadInt16();
            UNK_04 = br.ReadInt16();
            UNK_05 = br.ReadInt16();
            UNK_11 = br.ReadInt16();
            UNK_12 = br.ReadInt16();
            UNK_13 = br.ReadInt16();
            UNK_14 = br.ReadInt16();
            UNK_15 = br.ReadInt16();
            NegativeOneSanityCheck = br.ReadInt32();
            if (NegativeOneSanityCheck != -1)
                LogManager.Error("Negative one sanity check failed wtf did you do");
            UNK_ARR = new uint[26];
            for (int i = 0; i < UNK_ARR.Length; i++)
                UNK_ARR[i] = br.ReadUInt32();
        }
    }

    public struct YetiOtherVertexData
    {
         //public byte UNK_01;
         //public byte UNK_02;
         //public byte UNK_03;
         //public byte UNK_04;
         //public byte UNK_05;
         //public byte UNK_06;
         //public byte UNK_07;
         //public byte UNK_08;

        public byte[] datas;

        public override string ToString()
        {
            //return string.Format("[Other Data UNK_01:{0} UNK_02:{1} UNK_03:{2} UNK_04:{3} UNK_05:{4} UNK_06:{5} UNK_07:{6} UNK_08:{7}]", UNK_01, UNK_02, UNK_03, UNK_04, UNK_05, UNK_06, UNK_07, UNK_08);

            StringBuilder sb = new StringBuilder();
            sb.Append("[OtherData");
            for (int i = 0; i < datas.Length; i++)
            {
                sb.Append(" " + datas[i].ToString());
            }
            sb.Append("]");
            return sb.ToString();
        }
    }

    public class YetiMeshData : YetiObjectArchetype
    {
        public override YetiObjectType Identifier => YetiObjectType.msd;

        public ushort VertexCount { get; private set; }
        public YetiVertex[] Vertices { get; private set; }
        public Vector3[] RawVertices { get; private set; }
        //public Vector2[] UVs { get; private set; }
        public int IndexCount { get; private set; }
        public ushort[] Indices { get; private set; }
        public Vector3 CenterOffset { get; private set; }
        public float UniformScale { get; private set; }
        public YetiSubmeshData[] SubmeshData { get; private set; }

        public float snorm16ToFloat(short val)
        {
            return Math.Max(val / 32767f, -1f);
        }

        public override void Load(byte[] buffer, int size, YetiObject[] objectReferences)
        {
            log.Debug("loading mesh {0}", Object.NameWithExtension);

            using (MemoryStream ms = new MemoryStream(buffer))
            using (BinaryReader br = new BinaryReader(ms))
            {
                ms.Seek(0x09, SeekOrigin.Begin);

                VertexCount = br.ReadUInt16();
                IndexCount = br.ReadInt32();

                ms.Seek(0x0F, SeekOrigin.Begin);

                int dataOff = br.ReadInt32();

                ms.Seek(0x19, SeekOrigin.Begin);

                int boneCount = br.ReadInt16();
                int boneSize = br.ReadInt32() * 2;
                br.ReadInt16();
                //0x21
                int submeshCount = br.ReadInt16(); 
                br.ReadInt32();
                br.ReadInt32(); //old vertex count
                br.ReadInt32();
                br.ReadInt32(); //old triangle count
                br.ReadInt32();
                //0x3B
                CenterOffset = new Vector3(br.ReadSingle(), br.ReadSingle(), br.ReadSingle());
                CenterOffset = new Vector3(-CenterOffset.Y, CenterOffset.Z, CenterOffset.X);
                UniformScale = br.ReadSingle();

                //0x47
                int startOff = (int)ms.Position;

                log.Debug("> dataOff: {0}  startOff: {1}", dataOff, startOff);
                log.Debug("> VertexCount: {0}  FaceCount: {1}", VertexCount, IndexCount);
                log.Debug("> BoneCount: {0}  SubmeshDataCount: {1}", boneCount, submeshCount);

                SubmeshData = new YetiSubmeshData[submeshCount];

                for (int i = 0; i < submeshCount; i++)
                {
                    YetiSubmeshData data = new YetiSubmeshData();

                    data.Load(br);

                    SubmeshData[i] = data;
                }

                ms.Seek(startOff + dataOff, SeekOrigin.Begin);

                Vertices = new YetiVertex[VertexCount];
                RawVertices = new Vector3[VertexCount];
                //UVs = new Vector2[VertexCount];

                for (int i = 0; i < VertexCount; i++)
                {
                    short x = br.ReadInt16();
                    short y = br.ReadInt16();
                    short z = br.ReadInt16();

                    float s = snorm16ToFloat(br.ReadInt16());
                    s *= UniformScale;

                    Vector3 vertPos = new Vector3(-snorm16ToFloat(y), snorm16ToFloat(z), snorm16ToFloat(x)) * s;
                    vertPos += CenterOffset;

                    RawVertices[i] = vertPos;

                    Vector2 uv = new Vector2(br.ReadInt16() / 1024f, br.ReadInt16() / 1024f);

                    Vector4 color = new Vector4(snorm16ToFloat(br.ReadInt16()), snorm16ToFloat(br.ReadInt16()), 0, 0);

                    YetiVertex vert = new YetiVertex()
                    {
                        vertexPos = vertPos,
                        vertexColor = color,
                        uv0 = uv,
                        uv1 = new Vector2(br.ReadInt16() / 65535f, br.ReadInt16() / 65535f),
                        uv2 = new Vector2(br.ReadInt16() / 65535f, br.ReadInt16() / 65535f),
                        uv3 = new Vector2(br.ReadInt16() / 65535f, br.ReadInt16() / 65535f),
                        uv4 = new Vector2(br.ReadInt16() / 65535f, br.ReadInt16() / 65535f),
                    };

                    Vertices[i] = vert;
                }


                //for (int i = 0; i < VertexCount; i++)
                //{
                //    //yeti seems to use right-handed z-up coordinates (at least for models)
                //    //  unity uses left-handed y-up coordinates
                //    //  for this reason, to fix the models in unity i'm switching the coords at load-time
                //    //    so Yeti  X  Y  Z  becomes
                //    //   in Unity  Z -X  Y
                //    //  kinda bonkers but it works
                //    float vertex_z = snorm16ToFloat(br.ReadInt16()); //  Yeti X
                //    float vertex_x = -snorm16ToFloat(br.ReadInt16()); // Yeti Y
                //    float vertex_y = snorm16ToFloat(br.ReadInt16()); //  Yeti Z
                //    float vertex_scale = snorm16ToFloat(br.ReadInt16());
                //    float uv_u = br.ReadInt16() / 1024f;
                //    float uv_v = br.ReadInt16() / 1024f;

                //    Vector3 pos = new Vector3(vertex_x, vertex_y, vertex_z) * vertex_scale;
                //    Vector2 uv = new Vector2(uv_u, uv_v);

                //    RawVertices[i] = pos;
                //    UVs[i] = uv;

                //    //YetiBoneData bData = new YetiBoneData()
                //    //{
                //    //    Weight1 = br.ReadByte() / 255f,
                //    //    Weight2 = br.ReadByte() / 255f,
                //    //    Weight3 = br.ReadByte() / 255f,
                //    //    Weight4 = br.ReadByte() / 255f,

                //    //    Bone1 = br.ReadByte(),
                //    //    Bone2 = br.ReadByte(),
                //    //    Bone3 = br.ReadByte(),
                //    //    Bone4 = br.ReadByte(),
                //    //};

                //    YetiBoneData bData = new YetiBoneData()
                //    {
                //        datas = new float[4],
                //    };
                //    for (int j = 0; j < 4; j++)
                //    {
                //        bData.datas[j] = snorm16ToFloat(br.ReadInt16());
                //        //byte b = br.ReadByte();
                //        //bData.datas[j] = (byte)(b == 255 ? 0 : b);
                //    }

                //    //Vector4 vertexColor = new Vector4()//;
                //    //{
                //    //    Y = snorm16ToFloat(br.ReadInt16()),
                //    //    X = snorm16ToFloat(br.ReadInt16()),
                //    //    Z = 0,
                //    //    W = 0
                //    //};
                //    Vector4 vertexColor = new Vector4()//;
                //    {
                //        Y = br.ReadByte() / 255f,
                //        X = br.ReadByte() / 255f,
                //        Z = br.ReadByte() / 255f,
                //        W = br.ReadByte() / 255f
                //    };

                //    //YetiOtherVertexData otherData = new YetiOtherVertexData()
                //    //{
                //    //   UNK_01 = br.ReadByte(),
                //    //   UNK_02 = br.ReadByte(),
                //    //   UNK_03 = br.ReadByte(),
                //    //   UNK_04 = br.ReadByte(),
                //    //   UNK_05 = br.ReadByte(),
                //    //   UNK_06 = br.ReadByte(),
                //    //   UNK_07 = br.ReadByte(),
                //    //   UNK_08 = br.ReadByte()
                //    //};
                //    YetiOtherVertexData otherData = new YetiOtherVertexData()
                //    {
                //        datas = new byte[8]
                //    };
                //    for (int j = 0; j < 8; j++)
                //    {
                //        otherData.datas[j] = br.ReadByte();
                //    }

                //    Vertices[i] = new YetiVertex()
                //    {
                //        vertexPos = pos,
                //        uv0 = uv,
                //        boneData = bData,
                //        vertexColor = vertexColor,
                //        otherData = otherData
                //    };

                //    //log.Debug(Vertices[i].ToString());
                //}

                log.Debug("> current pos: {0}", ms.Position); 

                Indices = new ushort[IndexCount];

                for (int i = 0; i < IndexCount / 3; i++)
                {
                    //also due to the coordinate flipping from above triangles have to be loaded in this order
                    Indices[i * 3 + 2] = br.ReadUInt16();
                    Indices[i * 3 + 1] = br.ReadUInt16();
                    Indices[i * 3 + 0] = br.ReadUInt16();
                }

                for (int i = 0; i < IndexCount; i++)
                {
                    if (Indices[i] >= VertexCount)
                    {
                        log.Error("Mesh {0} ({1:X8}) has invalid triangle indices!", Object.NameWithExtension, Object.FileInfo.Key);
                        VertexCount = 0;
                        IndexCount = 0;
                        Vertices = new YetiVertex[0];
                        RawVertices = new Vector3[0];
                        Indices = new ushort[0];
                        //UVs = new Vector2[0];
                    }
                }
            }
        }

        public override void Log(ILogProxy log)
        {
            log.Info("YETIMESHDATA vertices: {0}  faces: {1}", VertexCount, IndexCount);
        }
    }

    public class YetiMeshMetadata : YetiObjectArchetype
    {
        public override YetiObjectType Identifier => YetiObjectType.msh;

        public YetiMeshData MeshData { get; private set; }

        public override void Load(byte[] buffer, int size, YetiObject[] objectReferences)
        {
            MeshData = objectReferences[0].ArchetypeAs<YetiMeshData>();
        }

        public override void Log(ILogProxy log)
        {
            log.Info("YETIMESHMETADATA");
        }
    }
}
