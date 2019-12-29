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

        public MeshFilter meshFilter;
        public MeshRenderer meshRenderer;

        public void LoadMesh(YetiMeshData meshData)
        {
            if (!meshMat)
                meshMat = (Material)Resources.Load("MeshTestMat");

            //gameObject.transform.localPosition = meshData.CenterOffset.ConvertToUnity().ConvertYetiToUnityCoords();

            meshFilter = gameObject.AddComponent<MeshFilter>();
            meshRenderer = gameObject.AddComponent<MeshRenderer>();

            Vector3[] vertices = new Vector3[meshData.VertexCount];
            Color[] vColors = new Color[meshData.VertexCount];
            Vector2[] uvs = new Vector2[meshData.VertexCount];

            for (int i = 0; i < meshData.VertexCount; i++)
            {
                YetiVertex v = meshData.Vertices[i];
                vertices[i] = v.vertexData.pos.ConvertToUnity();
                vColors[i] = new Color(v.vertexColor.X, v.vertexColor.Y, v.vertexColor.Z, v.vertexColor.W);
                uvs[i] = meshData.Vertices[i].vertexData.uv.ConvertToUnity();
            }

            Mesh mesh = new Mesh
            {
                vertices = vertices,
                uv = uvs,
                triangles = meshData.Triangles,
                colors = vColors,
            };
            mesh.RecalculateBounds();
            mesh.RecalculateNormals();
            mesh.RecalculateTangents();

            meshFilter.mesh = mesh;

            meshRenderer.material = meshMat;

            calcBounds = mesh.bounds.size;
            yetiBounds = meshData.CenterOffset.ConvertToUnity().ConvertYetiToUnityCoords();
        }
    }
}
