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
        static Action[] funcs =
        {
            TestTextures,
            ProcessBigmap,
        };

        static void Main(string[] args)
        {
            for (int i = 0; i < funcs.Length; i++)
            {
                Console.WriteLine("{0}: {1}", i, funcs[i].Method.Name);
            }
            Console.Write("Choice: ");
            int num = int.Parse(Console.ReadLine());
            Console.Clear();
            funcs[num]();
        }

        static void ProcessBigmap()
        {
            Console.Write("File path: ");
            string path = Console.ReadLine();
            if (!File.Exists(path))
                Environment.Exit(69);

            byte[] tmpBytes = new byte[8];

            using (FileStream fs = File.OpenRead(path))
            {
                while (fs.Read(tmpBytes, 0, 8) != 0)
                {
                    Array.Reverse(tmpBytes, 0, 4);
                    Array.Reverse(tmpBytes, 4, 4);
                    int keyA = BitConverter.ToInt32(tmpBytes, 0);
                    int keyB = BitConverter.ToInt32(tmpBytes, 4);
                    Console.WriteLine("{0:X4} {1:X4}", keyA, keyB);

                    if (keyB != 0)
                    {
                        Console.Write("Press enter...");
                        Console.ReadLine();
                    }
                }
            }

            Console.Write("End!");
            Console.ReadLine();
        }

        static void TestTextures()
        {
            Console.Write("File path: ");
            string path = Console.ReadLine();
            if (!File.Exists(path))
                Environment.Exit(69);

            Console.Write("\nType: ");
            string type = Console.ReadLine();
            TextureFormat format;
            if (!Enum.TryParse(type, out format))
                Environment.Exit(420);
            
            LogManager.GlobalLogFlags = LogFlags.Error | LogFlags.Info;

            PackedBigFile bigFile = new PackedBigFile(new FileInfo(path));
            bigFile.LoadFromDisk();

            List<BigFileFile> textureFiles = bigFile.RootFolder.GetAllFilesOfArchetype<TextureMetadataFileArchetype>();
            bigFile.FileLoader.LoadFiles(textureFiles);
            foreach (BigFileFile file in textureFiles)
            {
                TextureMetadataFileArchetype archetype = file.ArchetypeAs<TextureMetadataFileArchetype>();
                if (archetype.Format == format)
                    Console.WriteLine(file.FullFolderPath + file.Name + " " + string.Format("{0:X2} {1} {2}", archetype.Format, archetype.Width, archetype.Height));
            }

            Console.ReadLine();
        }
    }
}
