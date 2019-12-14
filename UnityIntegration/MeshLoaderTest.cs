using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine;
using GRPExplorerLib.BigFile;
using GRPExplorerLib.YetiObjects;

namespace UnityIntegration
{
    public class MeshLoaderTest : MonoBehaviour
    {
        public string FileKeyHex = "0x";
        public bool ClickMe = false;
        public bool Test = true;
        public int[] TriangleOrder = new int[3];
        public float rot = 180;
        public Vector3 rotDir = Vector3.right;

        [Header("debug")]
        public Vector3 bounds;
        public Vector3 bounds2;

        Mesh mesh;

        Material material;

        void Start()
        {
            gameObject.AddComponent<MeshFilter>();
            gameObject.AddComponent<MeshRenderer>();
            mesh = GetComponent<MeshFilter>().mesh;

            material = (Material)Resources.Load("MeshTestMat");

            GetComponent<MeshRenderer>().material = material;
        }

        void Update()
        {
            if (!ClickMe)
                return;

            ClickMe = false;

            if (TextureLoaderTest.m_bigFile == null)
                return;

            int key = Convert.ToInt32(FileKeyHex, 16);

            YetiObject obj = TextureLoaderTest.m_bigFile.FileMap[key];
            if (obj == null)
                return;

            YetiMeshData meshData = obj.ArchetypeAs<YetiMeshData>();
            if (meshData == null)
                return;

            YetiMeshData.TriangleOrder = TriangleOrder;

            TextureLoaderTest.m_bigFile.FileLoader.LoadAll(new List<YetiObject>() { obj });

            mesh.Clear();
            Vector3[] verts = new Vector3[meshData.VertexCount];
            Vector2[] uvs = new Vector2[meshData.VertexCount];
            CopyArray(meshData.Vertices, verts);
            CopyArray(meshData.UVs, uvs);
            mesh.vertices = verts;
            mesh.uv = uvs;
            mesh.triangles = meshData.Faces;
            mesh.RecalculateNormals();
            mesh.RecalculateBounds();

            bounds = new Vector3(meshData.BoundingBox.X, meshData.BoundingBox.Y, meshData.BoundingBox.Z);
            bounds2 = mesh.bounds.size;

            //mesh.bounds = new Bounds(bounds, mesh.bounds.size);
        }

        private void CopyArray(System.Numerics.Vector3[] from, UnityEngine.Vector3[] to)
        {
            for (int i = 0; i < from.Length; i++)
            {
                if (Test)
                {
                    to[i].x = from[i].X;
                    to[i].y = from[i].Y;
                    to[i].z = from[i].Z;
                }
                else
                {
                    to[i].x = from[i].X;
                    to[i].y = -from[i].Y;
                    to[i].z = from[i].Z;
                }

                //Quaternion quat = Quaternion.AngleAxis(rot, rotDir);
                //Matrix4x4 matrix = Matrix4x4.TRS(Vector3.zero, Quaternion.identity, new Vector3(1, -1, 1));

                //to[i] = matrix * to[i];
            }
        }

        private void CopyArray(System.Numerics.Vector2[] from, UnityEngine.Vector2[] to)
        {
            for (int i = 0; i < from.Length; i++)
            {
                to[i].x = from[i].X;
                to[i].y = from[i].Y;
            }
        }
    }
}
