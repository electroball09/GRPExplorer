using GRPExplorerLib.YetiObjects;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace UnityIntegration.Components
{
    [RequireComponent(typeof(cYetiObjectReference))]
    public class cYetiMesh : cYetiObjectReference
    {
        static Material meshMat;

        public Vector3 calcBounds;
        public Vector3 yetiBounds;

        public void LoadMesh(YetiMeshData meshData)
        {
            if (!meshMat)
                meshMat = (Material)Resources.Load("MeshTestMat");

            MeshFilter meshFilter = gameObject.AddComponent<MeshFilter>();
            MeshRenderer renderer = gameObject.AddComponent<MeshRenderer>();

            Mesh mesh = new Mesh
            {
                vertices = meshData.Vertices.ConvertToUnity(),
                uv = meshData.UVs.ConvertToUnity(),
                triangles = meshData.Faces
            };
            mesh.RecalculateBounds();
            mesh.RecalculateNormals();
            mesh.RecalculateTangents();

            meshFilter.mesh = mesh;

            renderer.material = meshMat;

            calcBounds = mesh.bounds.size;
            yetiBounds = meshData.CenterOffset.ConvertToUnity().ConvertYetiToUnityCoords();
        }
    }
}
