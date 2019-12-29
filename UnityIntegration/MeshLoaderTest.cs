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

            if (LibManager.BigFile == null)
                return;

            int key = Convert.ToInt32(FileKeyHex, 16);

            YetiObject obj = LibManager.BigFile.FileMap[key];
            if (obj == null)
                return;

            YetiMeshData meshData = obj.ArchetypeAs<YetiMeshData>();
            if (meshData == null)
                return;

            LibManager.BigFile.FileLoader.LoadObjectSimple(obj);

            mesh.Clear();
            Vector3[] verts = meshData.RawVertices.ConvertToUnity();
            Vector2[] uvs = meshData.UVs.ConvertToUnity();
            mesh.vertices = verts;
            mesh.uv = uvs;
            mesh.triangles = meshData.Faces;
            mesh.RecalculateNormals();
            mesh.RecalculateBounds();

            bounds = new Vector3(meshData.CenterOffset.X, meshData.CenterOffset.Y, meshData.CenterOffset.Z);
            bounds2 = mesh.bounds.size;
        }
    }
}
