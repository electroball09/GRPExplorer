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

        public override void Convert(YetiObject yetiObject, GameObject gameObject, YetiWorldLoadContext context)
        {
            context.worldObjects.Add(yetiObject);

            GameObject thisObj = new GameObject(yetiObject.NameWithExtension);
            thisObj.transform.SetParent(gameObject.transform, false);

            cYetiObjectReference.AddYetiComponent<cYetiObjectReference>(thisObj, yetiObject);

            foreach (YetiObject subObj in yetiObject.ObjectReferences)
            {
                if (subObj == null)
                    continue;

                YetiObjectConverter conv = GetConverter(subObj);
                conv.Convert(subObj, thisObj, context);
                Components.Add(conv.Components[0]);
            }
        }
    }

    public class YetiMeshDataConverter : YetiObjectConverter
    {
        public override Type ArchetypeType => typeof(YetiMeshData);

        public override void Convert(YetiObject yetiObject, GameObject gameObject, YetiWorldLoadContext context)
        {
            context.worldObjects.Add(yetiObject);

            YetiMeshData meshData = yetiObject.ArchetypeAs<YetiMeshData>();

            GameObject thisObj = new GameObject(yetiObject.NameWithExtension);
            thisObj.transform.SetParent(gameObject.transform, false);

            cYetiObjectReference.AddYetiComponent<cYetiObjectReference>(thisObj, yetiObject);

            cYetiMesh meshCmp = cYetiObjectReference.AddYetiComponent<cYetiMesh>(thisObj, yetiObject);
            meshCmp.LoadMesh(meshData);

            Components.Add(meshCmp);
        }
    }
}
