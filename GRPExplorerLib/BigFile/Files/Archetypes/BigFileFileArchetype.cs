using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Reflection;
using GRPExplorerLib.BigFile.Versions;
using GRPExplorerLib.Logging;

namespace GRPExplorerLib.BigFile.Files.Archetypes
{
    public abstract class BigFileFileArchetype
    {
        public abstract YetiFileType Identifier { get; }
        public BigFileFile File { get; set; }

        public abstract void Load(byte[] buffer, int size, BigFileFile[] fileReferences);

        public virtual void Unload() { }

        public abstract void Log(ILogProxy log);
    }

    public class DefaultFileArchetype : BigFileFileArchetype
    {
        public override YetiFileType Identifier => YetiFileType.NONE;

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
        static readonly Dictionary<YetiFileType, BigFileFileArchetype> archetypes = new Dictionary<YetiFileType, BigFileFileArchetype>();

        static FileArchetypeFactory()
        {
            IEnumerable<Type> archetypeTypes = Assembly.GetAssembly(typeof(BigFileFileArchetype)).GetTypes().Where(t => t.IsSubclassOf(typeof(BigFileFileArchetype)));
            foreach (Type t in archetypeTypes)
            {
                BigFileFileArchetype archetype = (BigFileFileArchetype)Activator.CreateInstance(t, null);
                archetypes.Add(archetype.Identifier, archetype);
            }
        }

        public static BigFileFileArchetype CreateArchetype(this BigFileFile file)
        {
            if (archetypes.ContainsKey(file.FileInfo.FileType))
                return (BigFileFileArchetype)Activator.CreateInstance(archetypes[file.FileInfo.FileType].GetType());

            return new DefaultFileArchetype();
        }
    }
}
