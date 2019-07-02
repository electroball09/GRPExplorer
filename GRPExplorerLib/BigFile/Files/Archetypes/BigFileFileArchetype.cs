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
        public abstract short Identifier { get; }

        public abstract void Load(byte[] rawData);

        public abstract void Log(ILogProxy log);
    }

    public class DefaultFileArchetype : BigFileFileArchetype
    {
        public override short Identifier => 0x0000;

        public override void Load(byte[] rawData)
        {
            return;
        }

        public override void Log(ILogProxy log)
        {
            
        }
    }

    public static class FileArchetypeFactory
    {
        static readonly Dictionary<short, BigFileFileArchetype> archetypes = new Dictionary<short, BigFileFileArchetype>();

        static FileArchetypeFactory()
        {
            IEnumerable<Type> archetypeTypes = Assembly.GetAssembly(typeof(BigFileFileArchetype)).GetTypes().Where(t => t.IsSubclassOf(typeof(BigFileFileArchetype)));
            foreach (Type t in archetypeTypes)
            {
                BigFileFileArchetype archetype = (BigFileFileArchetype)Activator.CreateInstance(t, null);
                archetypes.Add(archetype.Identifier, archetype);
            }
        }

        public static BigFileFileArchetype GetArchetype(IBigFileFileInfo fileInfo)
        {
            if (archetypes.ContainsKey(fileInfo.FileType))
                return (BigFileFileArchetype)Activator.CreateInstance(archetypes[fileInfo.FileType].GetType(), null);

            return new DefaultFileArchetype();
        }
    }
}
