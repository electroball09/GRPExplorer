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
    public class YetiMeshMetadataConverter : YetiObjectConverter
    {
        public override Type ArchetypeType => typeof(YetiMeshMetadata);

        public override object Convert(YetiObject yetiObject, GameObject gameObject, YetiWorldLoadContext context)
        {
            GameObject thisObj = new GameObject(yetiObject.NameWithExtension);
            thisObj.transform.SetParent(gameObject.transform, false);

            cYetiObjectReference.AddYetiComponent<cYetiObjectReference>(thisObj, yetiObject);

            foreach (YetiObject subObj in yetiObject.ObjectReferences)
            {
                if (subObj == null)
                    continue;

                GetConverter(subObj).Convert(subObj, thisObj, context);
            }

            return null;
        }
    }

    public class YetiMeshDataConverter : YetiObjectConverter
    {
        public override Type ArchetypeType => typeof(YetiMeshData);

        public override object Convert(YetiObject yetiObject, GameObject gameObject, YetiWorldLoadContext context)
        {
            YetiMeshData meshData = yetiObject.ArchetypeAs<YetiMeshData>();

            GameObject thisObj = new GameObject(yetiObject.NameWithExtension);
            thisObj.transform.SetParent(gameObject.transform, false);

            cYetiObjectReference.AddYetiComponent<cYetiObjectReference>(thisObj, yetiObject);

            cYetiMesh meshCmp = cYetiObjectReference.AddYetiComponent<cYetiMesh>(thisObj, yetiObject);
            meshCmp.LoadMesh(meshData);

            return null;
        }
    }
}
