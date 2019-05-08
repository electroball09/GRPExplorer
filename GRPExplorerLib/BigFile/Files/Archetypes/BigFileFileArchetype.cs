using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Reflection;
using GRPExplorerLib.BigFile.Versions;

namespace GRPExplorerLib.BigFile.Files.Archetypes
{
    public abstract class BigFileFileArchetype
    {
        public abstract short Identifier { get; }

        public abstract void Load(byte[] rawData);
    }

    public class DefaultFileArchetype : BigFileFileArchetype
    {
        public override short Identifier => 0x0000;

        public override void Load(byte[] rawData)
        {
            return;
        }
    }

    public static class FileArchetypeFactory
    {
        static readonly List<BigFileFileArchetype> archetypes = new List<BigFileFileArchetype>();

        static FileArchetypeFactory()
        {
            IEnumerable<Type> archetypeTypes = Assembly.GetAssembly(typeof(BigFileFileArchetype)).GetTypes().Where(t => t.IsSubclassOf(typeof(BigFileFileArchetype)));
            foreach (Type t in archetypeTypes)
            {
                archetypes.Add((BigFileFileArchetype)Activator.CreateInstance(t, null));
            }
        }

        public static BigFileFileArchetype GetArchetype(IBigFileFileInfo fileInfo)
        {
            foreach (BigFileFileArchetype a in archetypes)
            {
                if (fileInfo.FileType == a.Identifier)
                    return (BigFileFileArchetype)Activator.CreateInstance(a.GetType(), null);
            }

            return new DefaultFileArchetype();
        }
    }
}
