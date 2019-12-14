using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Reflection;
using GRPExplorerLib.BigFile.Versions;
using GRPExplorerLib.Logging;
using GRPExplorerLib.BigFile.Files;
using GRPExplorerLib.BigFile;

namespace GRPExplorerLib.YetiObjects
{
    public abstract class YetiObjectArchetype
    {
        static readonly Dictionary<YetiObjectType, YetiObjectArchetype> archetypes = new Dictionary<YetiObjectType, YetiObjectArchetype>();

        static YetiObjectArchetype()
        {
            IEnumerable<Type> archetypeTypes = Assembly.GetAssembly(typeof(YetiObjectArchetype)).GetTypes().Where(t => t.IsSubclassOf(typeof(YetiObjectArchetype)));
            foreach (Type t in archetypeTypes)
            {
                YetiObjectArchetype archetype = (YetiObjectArchetype)Activator.CreateInstance(t, null);
                archetypes.Add(archetype.Identifier, archetype);
            }
        }

        public static YetiObjectArchetype CreateArchetype(YetiObject obj)
        {
            if (archetypes.ContainsKey(obj.FileInfo.FileType))
                return (YetiObjectArchetype)Activator.CreateInstance(archetypes[obj.FileInfo.FileType].GetType());

            return new DefaultFileArchetype();
        }

        public abstract YetiObjectType Identifier { get; }
        public YetiObject Object { get; set; }

        public abstract void Load(byte[] buffer, int size, YetiObject[] objectReferences);

        public virtual void Unload() { }

        public abstract void Log(ILogProxy log);
    }

    public class DefaultFileArchetype : YetiObjectArchetype
    {
        public override YetiObjectType Identifier => YetiObjectType.NONE;

        public override void Load(byte[] buffer, int size, YetiObject[] objectReferences)
        {
            return;
        }

        public override void Log(ILogProxy log)
        {
            
        }
    }
}
