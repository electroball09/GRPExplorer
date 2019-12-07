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
        public abstract YetiObjectType Identifier { get; }
        public BigFileFile File { get; set; }

        public abstract void Load(byte[] buffer, int size, BigFileFile[] fileReferences);

        public virtual void Unload() { }

        public abstract void Log(ILogProxy log);
    }

    public class DefaultFileArchetype : YetiObjectArchetype
    {
        public override YetiObjectType Identifier => YetiObjectType.NONE;

        public override void Load(byte[] buffer, int size, BigFileFile[] fileReferences)
        {
            return;
        }

        public override void Log(ILogProxy log)
        {
            
        }
    }

    public static class FileArchetypeFactory
    {
        static readonly Dictionary<YetiObjectType, YetiObjectArchetype> archetypes = new Dictionary<YetiObjectType, YetiObjectArchetype>();

        static FileArchetypeFactory()
        {
            IEnumerable<Type> archetypeTypes = Assembly.GetAssembly(typeof(YetiObjectArchetype)).GetTypes().Where(t => t.IsSubclassOf(typeof(YetiObjectArchetype)));
            foreach (Type t in archetypeTypes)
            {
                YetiObjectArchetype archetype = (YetiObjectArchetype)Activator.CreateInstance(t, null);
                archetypes.Add(archetype.Identifier, archetype);
            }
        }

        public static YetiObjectArchetype CreateArchetype(this BigFileFile file)
        {
            if (archetypes.ContainsKey(file.FileInfo.FileType))
                return (YetiObjectArchetype)Activator.CreateInstance(archetypes[file.FileInfo.FileType].GetType());

            return new DefaultFileArchetype();
        }
    }
}
