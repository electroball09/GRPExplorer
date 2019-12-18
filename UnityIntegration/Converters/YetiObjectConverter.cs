using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using GRPExplorerLib.YetiObjects;
using GRPExplorerLib.BigFile.Files;
using GRPExplorerLib.BigFile;
using UnityEngine;
using GRPExplorerLib.Logging;
using UnityIntegration.Components;

namespace UnityIntegration.Converters
{

    public abstract class YetiObjectConverter
    {
        static readonly Dictionary<Type, YetiObjectConverter> typeList = new Dictionary<Type, YetiObjectConverter>();
        static readonly DefaultObjectConverter defaultConverter = new DefaultObjectConverter();

        static YetiObjectConverter()
        {
            IEnumerable<Type> archetypeTypes = System.Reflection.Assembly.GetAssembly(typeof(YetiObjectConverter)).GetTypes().Where(t => t.IsSubclassOf(typeof(YetiObjectConverter)));
            foreach (Type t in archetypeTypes)
            {
                YetiObjectConverter conv = (YetiObjectConverter)Activator.CreateInstance(t, null);
                typeList.Add(conv.ArchetypeType, conv);
            }
        }

        public static YetiObjectConverter GetConverter(YetiObject obj)
        {
            if (typeList.ContainsKey(obj.Archetype.GetType()))
                return (YetiObjectConverter)Activator.CreateInstance(typeList[obj.Archetype.GetType()].GetType());

            return defaultConverter;
        }

        protected ILogProxy log;

        protected YetiObjectConverter()
        {
            log = LogManager.GetLogProxy(GetType().Name);
        }

        public abstract Type ArchetypeType { get; }

        public virtual List<cYetiObjectReference> Components { get; } = new List<cYetiObjectReference>();

        public abstract void Convert(YetiObject yetiObject, GameObject parentObject, YetiWorldLoadContext context);
    }

    public class DefaultObjectConverter : YetiObjectConverter
    {
        public override Type ArchetypeType => typeof(DefaultFileArchetype);

        public override void Convert(YetiObject yetiObject, GameObject parentObject, YetiWorldLoadContext context)
        {
            context.worldObjects.Add(yetiObject);

            GameObject thisObj = new GameObject(yetiObject.NameWithExtension);
            if (parentObject)
                thisObj.transform.SetParent(parentObject.transform, false);

            cYetiObjectReference.AddYetiComponent<cYetiObjectReference>(thisObj, yetiObject);
        }
    }
}
