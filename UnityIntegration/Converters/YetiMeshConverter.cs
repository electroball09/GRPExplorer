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

        public override void Convert(YetiObject yetiObject, GameObject gameObject)
        {
            YetiMeshData meshData = yetiObject.ArchetypeAs<YetiMeshData>();

            MeshFilter meshFilter = gameObject.AddComponent<MeshFilter>();
            MeshRenderer renderer = gameObject.AddComponent<MeshRenderer>();

            Mesh mesh = new Mesh();

            mesh.vertices = meshData.Vertices.ConvertToUnity();
            mesh.uv = meshData.UVs.ConvertToUnity();
            mesh.triangles = meshData.Faces;
            mesh.RecalculateBounds();
            mesh.RecalculateNormals();
            mesh.RecalculateTangents();

            meshFilter.mesh = mesh;
        }
    }
}
