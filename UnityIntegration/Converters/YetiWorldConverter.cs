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

    public class YetiWorldConverter : YetiObjectConverter
    {
        public override Type ArchetypeType => typeof(YetiWorld);

        public override object Convert(YetiObject yetiObject, GameObject parentObject, YetiWorldLoadContext context)
        {
            if (context.loadedWorlds.Contains(yetiObject.ArchetypeAs<YetiWorld>()))
                return null;

            GameObject gameObject = new GameObject("_WORLD " + yetiObject.Name);

            cYetiObjectReference.AddYetiComponent<cYetiObjectReference>(gameObject, yetiObject);
            cYetiWorld world = cYetiObjectReference.AddYetiComponent<cYetiWorld>(gameObject, yetiObject);

            world.StartCoroutine(world.LoadWorld(yetiObject.ArchetypeAs<YetiWorld>(), context));

            return null;
        }
    }
}
