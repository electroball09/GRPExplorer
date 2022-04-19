using GRPExplorerLib.YetiObjects;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using UnityIntegration.Converters;
using UnityEngine.Rendering;
using System.IO;
using Unity.Collections;

namespace UnityIntegration.Components
{
    [RequireComponent(typeof(cYetiObjectReference))]
    public class cYetiMesh : cYetiObjectReference
    {
        delegate void ChangeShadowModeDelegate(bool isEnabled);
        static event ChangeShadowModeDelegate OnChangeShadowMode;
        static bool separateShadowCastersEnabled = true;
        public static void SwitchShadowMode()
        {
            separateShadowCastersEnabled = !separateShadowCastersEnabled;
            OnChangeShadowMode?.Invoke(separateShadowCastersEnabled);
        }

        static Material meshMat;
        static Material shadowMat;

        public int VertexCount;
        public int TriangleCount;

        public Vector3 calcBounds;
        public Vector3 yetiOffset;
        public float uniformScale;

        public MeshFilter meshFilter;
        public MeshRenderer meshRenderer;

        public GameObject shadowObject;
        public MeshFilter shadowFilter;
        public MeshRenderer shadowRenderer;

        public List<cYetiMaterial> materials;

        public YetiSubmeshData[] submeshData;

        void Awake()
        {
            OnChangeShadowMode += CYetiMesh_OnChangeShadowMode;
        }

        void OnDestroy()
        {
            OnChangeShadowMode -= CYetiMesh_OnChangeShadowMode;
        }

        private void CYetiMesh_OnChangeShadowMode(bool isEnabled)
        {
            if (isEnabled)
            {
                meshRenderer.shadowCastingMode = ShadowCastingMode.Off;
                shadowRenderer.enabled = true;
            }
            else
            {
                meshRenderer.shadowCastingMode = ShadowCastingMode.On;
                shadowRenderer.enabled = false;
            }
        }

        public void LoadMesh(YetiMeshData meshData, YetiObjectRepository objectRepository)
        {
            if (!meshMat)
            {
                meshMat = (Material)Resources.Load("MeshTestMat");
                shadowMat = (Material)Resources.Load("ShadowMat");
            }

            meshFilter = gameObject.AddComponent<MeshFilter>();
            meshRenderer = gameObject.AddComponent<MeshRenderer>();
            meshRenderer.sharedMaterial = meshMat;

            shadowObject = new GameObject("ShadowCaster");
            shadowObject.transform.parent = transform;
            shadowObject.transform.localPosition = Vector3.zero;
            shadowObject.transform.localRotation = Quaternion.identity;
            shadowFilter = shadowObject.AddComponent<MeshFilter>();
            shadowRenderer = shadowObject.AddComponent<MeshRenderer>();
            shadowRenderer.sharedMaterial = shadowMat;
            shadowRenderer.shadowCastingMode = ShadowCastingMode.ShadowsOnly;
            shadowRenderer.receiveShadows = false;

            CYetiMesh_OnChangeShadowMode(separateShadowCastersEnabled);

            Mesh mesh = null;
            if (objectRepository.GetRepository<Mesh>().ContainsKey(yetiObject))
            {
                mesh = objectRepository.GetRepository<Mesh>()[yetiObject];
            }
            else
            {
                mesh = new Mesh();
                NativeArray<YetiVertex> meshfloats = new NativeArray<YetiVertex>(meshData.VertexCount, Allocator.Temp);
                for (int i = 0; i < meshfloats.Length; i++)
                {
                    meshfloats[i] = meshData.Vertices[i];
                }
                VertexAttributeDescriptor[] descriptors = new VertexAttributeDescriptor[]
                {
                    new VertexAttributeDescriptor(VertexAttribute.Position, VertexAttributeFormat.Float32, 3, 0),
                    new VertexAttributeDescriptor(VertexAttribute.Color, VertexAttributeFormat.Float32, 4, 0),
                    new VertexAttributeDescriptor(VertexAttribute.TexCoord0, VertexAttributeFormat.Float32, 2, 0),
                    new VertexAttributeDescriptor(VertexAttribute.TexCoord1, VertexAttributeFormat.Float32, 2, 0),
                    new VertexAttributeDescriptor(VertexAttribute.TexCoord2, VertexAttributeFormat.Float32, 2, 0),
                    new VertexAttributeDescriptor(VertexAttribute.TexCoord3, VertexAttributeFormat.Float32, 2, 0),
                    new VertexAttributeDescriptor(VertexAttribute.TexCoord4, VertexAttributeFormat.Float32, 2, 0),
                };

                mesh.SetVertexBufferParams(meshData.VertexCount, descriptors);
                mesh.SetVertexBufferData(meshfloats, 0, 0, meshData.VertexCount);

                mesh.SetIndexBufferParams(meshData.IndexCount, IndexFormat.UInt16);
                NativeArray<ushort> indices = new NativeArray<ushort>(meshData.Indices, Allocator.Temp);
                mesh.SetIndexBufferData(indices, 0, 0, indices.Length);
                SubMeshDescriptor smd = new SubMeshDescriptor(0, meshData.IndexCount);
                mesh.SetSubMesh(0, smd);

                mesh.RecalculateBounds();
                mesh.RecalculateNormals();
                mesh.RecalculateTangents();

                submeshData = meshData.SubmeshData;

                objectRepository.GetRepository<Mesh>().Add(yetiObject, mesh);
            }

            meshFilter.mesh = mesh;
            shadowFilter.mesh = mesh;

            calcBounds = mesh.bounds.size;
            yetiOffset = meshData.CenterOffset.ConvertToUnity();//.ConvertYetiToUnityCoords();
            uniformScale = meshData.UniformScale;
            VertexCount = meshData.VertexCount;
            TriangleCount = meshData.IndexCount;
        }

        public void SetMaterials(List<cYetiMaterial> mats)
        {
            materials = mats;
            var m = new Material[mats.Count];
            for (int i = 0; i < mats.Count; i++)
                m[i] = mats[i].Material;
            meshRenderer.material = mats[0].Material;
        }

        public void SetLVMMaps(cYetiTexture lvm, cYetiTexture lvmColor)
        {
            MaterialPropertyBlock bl = new MaterialPropertyBlock();
            if (lvm && lvm.texture)
            {
                bl.SetTexture("_LVM", lvm?.texture);
            }
            if (lvmColor && lvmColor.texture)
            {
                bl.SetTexture("_LVMColor", lvmColor?.texture);
                bl.SetFloat("_LVMColorContribution", 1);
            }
            meshRenderer.SetPropertyBlock(bl);
        }
    }
}
