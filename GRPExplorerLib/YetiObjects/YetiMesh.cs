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
    public class YetiMeshData : YetiObjectArchetype
    {
        public override YetiObjectType Identifier => YetiObjectType.msd;

        public int VertexCount { get; private set; }
        public Vector3[] Vertices { get; private set; }
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

                Vertices = new Vector3[VertexCount];
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
                    float uv_v = br.ReadInt16() / -1024f;

                    Vertices[i] = (new Vector3(vertex_x, vertex_y, vertex_z) * vertex_scale);
                    UVs[i] = new Vector2(uv_u, uv_v);

                    ms.Seek(20, SeekOrigin.Current);
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
                        Vertices = new Vector3[0];
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
