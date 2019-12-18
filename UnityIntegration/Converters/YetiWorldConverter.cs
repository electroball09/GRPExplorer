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
    public class YetiWorldLoadContext
    {
        public HashSet<YetiObject> g_loadedList = new HashSet<YetiObject>();
        public HashSet<YetiWorld> g_loadedWorlds = new HashSet<YetiWorld>();

        public YetiWorld currentWorld;
        public YetiWorldLoadContext parentContext;
        public List<YetiObject> worldObjects = new List<YetiObject>();

        public List<YetiWorldLoadContext> subContexts = new List<YetiWorldLoadContext>();

        public YetiWorldLoadContext GetSubContext(YetiWorld world)
        {
            YetiWorldLoadContext context =
                new YetiWorldLoadContext()
                {
                    g_loadedList = g_loadedList,
                    g_loadedWorlds = g_loadedWorlds,
                    currentWorld = world,
                    parentContext = this
            };

            subContexts.Add(context);

            return context;
        }
    }

    public class YetiWorldConverter : YetiObjectConverter
    {
        public override Type ArchetypeType => typeof(YetiWorld);

        public override void Convert(YetiObject yetiObject, GameObject parentObject, YetiWorldLoadContext context)
        {
            if (context.g_loadedWorlds.Contains(yetiObject.ArchetypeAs<YetiWorld>()))
                return;

            GameObject gameObject = new GameObject("_WORLD " + yetiObject.Name);

            cYetiObjectReference.AddYetiComponent<cYetiObjectReference>(gameObject, yetiObject);
            cYetiWorld world = cYetiObjectReference.AddYetiComponent<cYetiWorld>(gameObject, yetiObject);

            world.StartCoroutine(world.LoadWorld(yetiObject.ArchetypeAs<YetiWorld>(), context));
        }
    }
}
