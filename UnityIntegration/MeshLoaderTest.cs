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
        }

        private void CopyArray(System.Numerics.Vector3[] from, UnityEngine.Vector3[] to)
        {
            for (int i = 0; i < from.Length; i++)
            {
                if (Test)
                {
                    to[i].x = -from[i].X / 1000f;
                    to[i].y = -from[i].Z / 1000f;
                    to[i].z = from[i].Y / 1000f;
                }
                else
                {
                    to[i].x = -from[i].X / 1000f;
                    to[i].y = from[i].Y / 1000f;
                    to[i].z = from[i].Z / 1000f;
                }
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
