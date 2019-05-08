using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using GRPExplorerLib;
using GRPExplorerLib.BigFile;
using GRPExplorerLib.Logging;
using GRPExplorerLib.BigFile.Files;
using GRPExplorerLib.BigFile.Files.Archetypes;

namespace GRPExplorerTests
{
    class Program
    {
        static void Main(string[] args)
        {
            Console.Write("File path: ");
            string path = Console.ReadLine();
            if (!File.Exists(path))
                Environment.Exit(69);

            LogManager.GlobalLogFlags = LogFlags.Error | LogFlags.Info;

            PackedBigFile bigFile = new PackedBigFile(new FileInfo(path));
            bigFile.LoadFromDisk();

            List<BigFileFile> textureFiles = bigFile.RootFolder.GetAllFilesOfArchetype<TextureMetadataFileArchetype>();
            bigFile.FileLoader.LoadFiles(textureFiles);
            foreach (BigFileFile file in textureFiles)
            {
                TextureMetadataFileArchetype archetype = file.ArchetypeAs<TextureMetadataFileArchetype>();
                if (archetype.Format != 0x0C
                    && archetype.Format != 0x09
                    && archetype.Format != 0x00
                    && archetype.Format != 0x08
                    && archetype.Format != 0x04
                    && archetype.Format != 0x27
                    && archetype.Format != 0x28
                    && archetype.Format != 0x05
                    && archetype.Format != 0x0F
                    && archetype.Format != 0x0A)
                    Console.WriteLine(file.FullFolderPath + file.Name + " " + string.Format("{0:X2} {1} {2}", archetype.Format, archetype.Width, archetype.Height));
            }

            Console.ReadLine();
        }
    }
}
