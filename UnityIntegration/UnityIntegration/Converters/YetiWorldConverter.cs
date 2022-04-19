using System;
using System.Collections;
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
        public YetiObjectRepository g_ObjectRepository = new YetiObjectRepository();
        public HashSet<YetiObject> g_loadedList = new HashSet<YetiObject>();
        public HashSet<YetiWorld> g_loadedWorlds = new HashSet<YetiWorld>();

        public YetiWorld currentWorld;
        public cYetiWorld worldComponent;
        public YetiWorldLoadContext parentContext;
        public List<GameObject> worldObjects = new List<GameObject>();

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

    public class YetiObjectRepository
    {
        Dictionary<Type, IDictionary> repository = new Dictionary<Type, IDictionary>();

        public Dictionary<YetiObject, T> GetRepository<T>()
        {
            if (!repository.ContainsKey(typeof(T)))
                repository.Add(typeof(T), new Dictionary<YetiObject, T>());

            return repository[typeof(T)] as Dictionary<YetiObject, T>;
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
            if (parentObject)
                gameObject.transform.parent = parentObject.transform;

            cYetiObjectReference.AddYetiComponent<cYetiObjectReference>(gameObject, yetiObject);
            cYetiWorld world = cYetiObjectReference.AddYetiComponent<cYetiWorld>(gameObject, yetiObject);

            world.LoadWorld(yetiObject.ArchetypeAs<YetiWorld>(), context);

            Components.Add(world);
        }
    }
}
