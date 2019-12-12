using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine;
using GRPExplorerLib.BigFile;
using GRPExplorerLib.YetiObjects;
using System.Numerics;

namespace UnityIntegration
{
    public class MeshLoaderTest : MonoBehaviour
    {
        public string FileKeyHex = "0x";
        public bool ClickMe = false;

        Mesh mesh;

        void Start()
        {
            gameObject.AddComponent<MeshFilter>();
            gameObject.AddComponent<MeshRenderer>();
            mesh = GetComponent<MeshFilter>().mesh;
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
            //mesh.vertices = meshData.Vertices;
        }
    }
}
