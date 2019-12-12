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

        public override void Load(byte[] buffer, int size, YetiObject[] objectReferences)
        {
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
                float bbx = br.ReadSingle();
                float bby = br.ReadSingle();
                float bbz = br.ReadSingle();

                int startOff = (int)ms.Position;

                ms.Seek(dataOff, SeekOrigin.Current);

                Vertices = new Vector3[VertexCount];
                UVs = new Vector2[VertexCount];

                for (int i = 0; i < VertexCount; i++)
                {
                    short vx = br.ReadInt16();
                    short vy = br.ReadInt16();
                    short vz = br.ReadInt16();
                    short vw = br.ReadInt16();
                    short tu = br.ReadInt16();
                    short tv = (short)(br.ReadInt16() * -1); //idk why

                    Vertices[i] = new Vector3((vx * vw) / -655350f, (vy * vw) / 655350f, (vz * vw) / 655350f);
                    UVs[i] = new Vector2(tu / 1024f, tv / 1024f);

                    ms.Seek(20, SeekOrigin.Current);
                }

                LogManager.Info(string.Format("current pos: {0}", ms.Position));

                Faces = new int[FaceCount];

                for (int i = 0; i < FaceCount / 3; i++)
                {
                    Faces[i * 3 + 0] = br.ReadUInt16();
                    Faces[i * 3 + 1] = br.ReadUInt16();
                    Faces[i * 3 + 2] = br.ReadUInt16();
                }

                for (int i = 0; i < FaceCount; i++)
                {
                    if (Faces[i] >= VertexCount)
                        LogManager.Error(string.Format("index: {0}  vert: {1}", i, Faces[i]));
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
