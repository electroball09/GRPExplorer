using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using GRPExplorerLib.YetiObjects;
using GRPExplorerLib.BigFile.Files;
using GRPExplorerLib.BigFile;
using UnityEngine;

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
                return typeList[obj.Archetype.GetType()];

            return defaultConverter;
        }

        public abstract Type ArchetypeType { get; }

        public abstract void Convert(YetiObject yetiObject, GameObject gameObject);
    }

    public class DefaultObjectConverter : YetiObjectConverter
    {
        public override Type ArchetypeType => typeof(DefaultFileArchetype);

        public override void Convert(YetiObject yetiObject, GameObject gameObject)
        {
            
        }
    }
}
