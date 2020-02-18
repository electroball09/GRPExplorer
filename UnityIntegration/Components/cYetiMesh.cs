using GRPExplorerLib.YetiObjects;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using UnityIntegration.Converters;

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

        public List<cYetiMaterial> materials;

        public void LoadMesh(YetiMeshData meshData, YetiObjectRepository objectRepository)
        {
            if (!meshMat)
                meshMat = (Material)Resources.Load("MeshTestMat");

            //gameObject.transform.localPosition = meshData.CenterOffset.ConvertToUnity().ConvertYetiToUnityCoords();

            meshFilter = gameObject.AddComponent<MeshFilter>();
            meshRenderer = gameObject.AddComponent<MeshRenderer>();

            Mesh mesh = null;
            if (objectRepository.GetRepository<Mesh>().ContainsKey(yetiObject))
            {
                mesh = objectRepository.GetRepository<Mesh>()[yetiObject];
            }
            else
            {
                Vector3[] vertices = new Vector3[meshData.VertexCount];
                Color[] vColors = new Color[meshData.VertexCount];
                Vector2[] uvs = new Vector2[meshData.VertexCount];

                for (int i = 0; i < meshData.VertexCount; i++)
                {
                    YetiVertex v = meshData.Vertices[i];
                    vertices[i] = v.vertexPos.ConvertToUnity();
                    //vColors[i] = new Color(v.vertexColor.X, v.vertexColor.Y, v.vertexColor.Z, v.vertexColor.W);
                    vColors[i] = new Color(v.boneData.datas[4] / 255f, v.boneData.datas[5] / 255f, v.boneData.datas[6] / 255f, v.boneData.datas[7] / 255f);
                    vColors[i] *= new Color(v.boneData.datas[0] / 255f, v.boneData.datas[1] / 255f, v.boneData.datas[2] / 255f, v.boneData.datas[3] / 255f);
                    uvs[i] = meshData.Vertices[i].uv0.ConvertToUnity();
                }

                mesh = new Mesh
                {
                    vertices = vertices,
                    uv = uvs,
                    triangles = meshData.Triangles,
                    colors = vColors,
                };
                mesh.RecalculateBounds();
                mesh.RecalculateNormals();
                mesh.RecalculateTangents();

                objectRepository.GetRepository<Mesh>().Add(yetiObject, mesh);
            }

            meshFilter.mesh = mesh;

            meshRenderer.material = meshMat;

            calcBounds = mesh.bounds.size;
            yetiBounds = meshData.CenterOffset.ConvertToUnity().ConvertYetiToUnityCoords();
        }

        public void SetMaterials(List<cYetiMaterial> mats)
        {
            materials = mats;
            meshRenderer.material = mats[0].Material;
        }
    }
}
