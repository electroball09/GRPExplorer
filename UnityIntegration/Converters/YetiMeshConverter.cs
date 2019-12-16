using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using GRPExplorerLib.BigFile;
using GRPExplorerLib.YetiObjects;
using UnityEngine;
using UnityIntegration.Components;

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

            GameObject thisObj = new GameObject(yetiObject.NameWithExtension);
            thisObj.transform.SetParent(gameObject.transform, false);

            cYetiObjectReference objRef = thisObj.AddComponent<cYetiObjectReference>();
            objRef.Key = yetiObject.FileInfo.Key;
            cYetiMesh meshCmp = thisObj.AddComponent<cYetiMesh>();
            meshCmp.LoadMesh(meshData);
        }
    }
}
