using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using GRPExplorerLib.BigFile;
using GRPExplorerLib.YetiObjects;
using UnityEngine;

namespace UnityIntegration.Converters
{
    public class YetiMeshConverter : YetiObjectConverter
    {
        public override Type ArchetypeType => typeof(YetiMeshMetadata);

        public override void Convert(YetiObject yetiObject, GameObject gameObject)
        {
            
        }
    }

    public class YetiMeshDataConverter : YetiObjectConverter
    {
        public override Type ArchetypeType => typeof(YetiMeshData);

        Material meshMat = (Material)Resources.Load("MeshTestMat");

        public override void Convert(YetiObject yetiObject, GameObject gameObject)
        {
            YetiMeshData meshData = yetiObject.ArchetypeAs<YetiMeshData>();

            GameObject thisObj = new GameObject(yetiObject.NameWithExtension);
            thisObj.transform.SetParent(gameObject.transform, false);

            MeshFilter meshFilter = thisObj.AddComponent<MeshFilter>();
            MeshRenderer renderer = thisObj.AddComponent<MeshRenderer>();

            Mesh mesh = new Mesh();

            mesh.vertices = meshData.Vertices.ConvertToUnity();
            mesh.uv = meshData.UVs.ConvertToUnity();
            mesh.triangles = meshData.Faces;
            mesh.RecalculateBounds();
            mesh.RecalculateNormals();
            mesh.RecalculateTangents();

            meshFilter.mesh = mesh;

            renderer.material = meshMat;
        }
    }
}
