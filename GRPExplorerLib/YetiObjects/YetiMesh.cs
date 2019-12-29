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
        public YetiVertexData vertexData;
        public YetiBoneData boneData;
        public Vector4 vertexColor;
        public YetiOtherVertexData otherData;
    }

    public struct YetiVertexData
    {
        public Vector3 pos;
        public Vector2 uv;
    }

    public struct YetiBoneData
    {
        public float Weight1;
        public float Weight2;
        public float Weight3;
        public float Weight4;

        public byte Bone1;
        public byte Bone2;
        public byte Bone3;
        public byte Bone4;
    }

    public struct YetiOtherVertexData
    {
         public byte UNK_01;
         public byte UNK_02;
         public byte UNK_03;
         public byte UNK_04;
         public byte UNK_05;
         public byte UNK_06;
         public byte UNK_07;
         public byte UNK_08;
    }

    public class YetiMeshData : YetiObjectArchetype
    {
        public override YetiObjectType Identifier => YetiObjectType.msd;

        public int VertexCount { get; private set; }
        public YetiVertex[] Vertices { get; private set; }
        public Vector3[] RawVertices { get; private set; }
        public Vector2[] UVs { get; private set; }
        public int FaceCount { get; private set; }
        public int[] Faces { get; private set; }
        public Vector3 CenterOffset { get; private set; }

        private float snorm16ToFloat(short val)
        {
            return Math.Max(val / 32767f, -1f);
        }

        public override void Load(byte[] buffer, int size, YetiObject[] objectReferences)
        {
            log.Debug("loading mesh {0}", Object.NameWithExtension);

            using (MemoryStream ms = new MemoryStream(buffer))
            using (BinaryReader br = new BinaryReader(ms))
            {
                ms.Seek(0x0F, SeekOrigin.Begin);

                int dataOff = br.ReadInt32();

                ms.Seek(0x19, SeekOrigin.Begin);

                int boneCount = br.ReadInt16();
                int boneSize = br.ReadInt32() * 2;
                br.ReadInt16();
                int boneCount2 = br.ReadInt16();
                br.ReadInt32();
                VertexCount = br.ReadInt32();
                br.ReadInt32();
                FaceCount = br.ReadInt32();
                br.ReadInt32();
                br.ReadInt32();
                CenterOffset = new Vector3(br.ReadSingle(), br.ReadSingle(), br.ReadSingle());
                CenterOffset = new Vector3(-CenterOffset.Y, CenterOffset.Z, CenterOffset.X);

                int startOff = (int)ms.Position;

                log.Debug("> dataOff: {0}  startOff: {1}", dataOff, startOff);
                log.Debug("> VertexCount: {0}  FaceCount: {1}", VertexCount, FaceCount);
                log.Debug("> BoneCount: {0}  BoneCount2: {1}", boneCount, boneCount2);

                ms.Seek(dataOff, SeekOrigin.Current);

                Vertices = new YetiVertex[VertexCount];
                RawVertices = new Vector3[VertexCount];
                UVs = new Vector2[VertexCount];

                for (int i = 0; i < VertexCount; i++)
                {
                    //yeti seems to use right-handed z-up coordinates (at least for models)
                    //  unity uses left-handed y-up coordinates
                    //  for this reason, to fix the models in unity i'm switching the coords at load-time
                    //    so Yeti  X  Y  Z  becomes
                    //   in Unity  Z -X  Y
                    //  kinda bonkers but it works
                    float vertex_z = snorm16ToFloat(br.ReadInt16()); //  Yeti X
                    float vertex_x = -snorm16ToFloat(br.ReadInt16()); // Yeti Y
                    float vertex_y = snorm16ToFloat(br.ReadInt16()); //  Yeti Z
                    float vertex_scale = snorm16ToFloat(br.ReadInt16());
                    float uv_u = br.ReadInt16() / 1024f;
                    float uv_v = br.ReadInt16() / 1024f;

                    YetiVertexData vData = new YetiVertexData()
                    {
                        pos = new Vector3(vertex_x, vertex_y, vertex_z) * vertex_scale,
                        uv = new Vector2(uv_u, uv_v)
                    };

                    RawVertices[i] = (new Vector3(vertex_x, vertex_y, vertex_z) * vertex_scale);
                    UVs[i] = new Vector2(uv_u, uv_v);

                    YetiBoneData bData = new YetiBoneData()
                    {
                        Weight1 = br.ReadByte() / 255f,
                        Weight2 = br.ReadByte() / 255f,
                        Weight3 = br.ReadByte() / 255f,
                        Weight4 = br.ReadByte() / 255f,

                        Bone1 = br.ReadByte(),
                        Bone2 = br.ReadByte(),
                        Bone3 = br.ReadByte(),
                        Bone4 = br.ReadByte(),
                    };

                    Vector4 vertexColor = new Vector4()
                    {
                        X = br.ReadByte() / 255f,
                        Y = br.ReadByte() / 255f,
                        Z = br.ReadByte() / 255f,
                        W = br.ReadByte() / 255f
                    };

                    YetiOtherVertexData otherData = new YetiOtherVertexData()
                    {
                       UNK_01 = br.ReadByte(),
                       UNK_02 = br.ReadByte(),
                       UNK_03 = br.ReadByte(),
                       UNK_04 = br.ReadByte(),
                       UNK_05 = br.ReadByte(),
                       UNK_06 = br.ReadByte(),
                       UNK_07 = br.ReadByte(),
                       UNK_08 = br.ReadByte()
                    };

                    Vertices[i] = new YetiVertex()
                    {
                        vertexData = vData,
                        boneData = bData,
                        vertexColor = vertexColor,
                        otherData = otherData
                    };

                    //ms.Seek(12, SeekOrigin.Current);
                }

                log.Debug("> current pos: {0}", ms.Position); 

                Faces = new int[FaceCount];

                for (int i = 0; i < FaceCount / 3; i++)
                {
                    //also due to the coordinate flipping from above triangles have to be loaded in this order
                    Faces[i * 3 + 2] = br.ReadUInt16();
                    Faces[i * 3 + 1] = br.ReadUInt16();
                    Faces[i * 3 + 0] = br.ReadUInt16();
                }

                for (int i = 0; i < FaceCount; i++)
                {
                    if (Faces[i] >= VertexCount)
                    {
                        log.Error("Mesh {0} ({1:X8}) has invalid triangle indexes!", Object.NameWithExtension, Object.FileInfo.Key);
                        VertexCount = 0;
                        FaceCount = 0;
                        RawVertices = new Vector3[0];
                        Faces = new int[0];
                        UVs = new Vector2[0];
                    }
                }
            }
        }

        public override void Log(ILogProxy log)
        {
            log.Info("YETIMESHDATA vertices: {0}  faces: {1}", VertexCount, FaceCount);
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
