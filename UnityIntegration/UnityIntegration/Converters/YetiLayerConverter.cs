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
    public class YetiLayerConverter : YetiObjectConverter
    {
        public override Type ArchetypeType => typeof(YetiLayer);

        public override void Convert(YetiObject yetiObject, GameObject parentObject, YetiWorldLoadContext context)
        {
            cYetiLayer layerCmp = null;
            if (!context.g_ObjectRepository.GetRepository<cYetiLayer>().TryGetValue(yetiObject, out layerCmp))
            {
                GameObject layerObj = new GameObject(yetiObject.NameWithExtension);
                layerObj.transform.parent = context.worldComponent.LayersObject.transform;

                layerCmp = cYetiObjectReference.AddYetiComponent<cYetiLayer>(layerObj, yetiObject);
                layerCmp.SetLayer(yetiObject.ArchetypeAs<YetiLayer>());

                context.g_ObjectRepository.GetRepository<cYetiLayer>().Add(yetiObject, layerCmp);
            }

            Components.Add(layerCmp);
        }
    }
}
