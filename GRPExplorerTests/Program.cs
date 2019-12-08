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
using GRPExplorerLib.YetiObjects;
using GRPExplorerLib.Util;
using Org.BouncyCastle.Crypto;
using Org.BouncyCastle.Crypto.Engines;
using Org.BouncyCastle.Crypto.Parameters;
using System.Numerics;

namespace GRPExplorerTests
{
    static class Out
    {
        const string log_file = "log.txt";
        static StreamWriter log_sw = File.CreateText(log_file);

        public static void Write(string format, params object[] args)
        {
            Console.Write(format, args);
            log_sw.Write(format, args);
            log_sw.Flush();
        }

        public static void Write(string msg)
        {
            Console.Write(msg);
            log_sw.Write(msg);
            log_sw.Flush();
        }

        public static void WriteLine(string format, params object[] args)
        {
            Console.WriteLine(format, args);
            log_sw.WriteLine(format, args);
            log_sw.Flush();
        }

        public static void WriteLine(string msg)
        {
            Console.WriteLine(msg);
            log_sw.WriteLine(msg);
            log_sw.Flush();
        }

        public static string ReadLine()
        {
            string str = Console.ReadLine();
            log_sw.WriteLine(str);
            return str;
        }

        public static void Clear()
        {
            Console.Clear();
            log_sw.WriteLine("");
            log_sw.WriteLine("CLEAR");
            log_sw.WriteLine("");
            log_sw.Flush();
        }
    }

    static class Helper
    {
        public static byte[] StringToByteArrayFastest(string hex)
        {
            if (hex.Length % 2 == 1)
                throw new Exception("The binary key cannot have an odd number of digits");

            byte[] arr = new byte[hex.Length >> 1];

            for (int i = 0; i < hex.Length >> 1; ++i)
            {
                arr[i] = (byte)((GetHexVal(hex[i << 1]) << 4) + (GetHexVal(hex[(i << 1) + 1])));
            }

            return arr;
        }

        public static int GetHexVal(char hex)
        {
            int val = (int)hex;
            //For uppercase A-F letters:
            return val - (val < 58 ? 48 : 55);
            //For lowercase a-f letters:
            //return val - (val < 58 ? 48 : 87);
            //Or the two combined, but a bit slower:
            //return val - (val < 58 ? 48 : (val < 97 ? 55 : 87));
        }
    }

    class Program
    {
        class LogInterface : IGRPExplorerLibLogInterface
        {
            public LogFlags CombineFlags(LogFlags original)
            {
                return original;
            }

            public void Debug(string msg)
            {
                Console.ForegroundColor = ConsoleColor.Cyan;
                Out.WriteLine(msg);
                Console.ForegroundColor = ConsoleColor.White;
            }

            public void Error(string msg)
            {
                Console.ForegroundColor = ConsoleColor.Red;
                Out.WriteLine(msg);
                Console.ForegroundColor = ConsoleColor.White;
            }

            public void Info(string msg)
            {
                Out.WriteLine(msg);
            }
        }

        static ILogProxy log = LogManager.GetLogProxy(">");

        static Action[] funcs =
        {
            TestTextures,
            ProcessBigmap,
            CompareFiles,
            CompareAllFiles,
            LogFiles,
            CheckFiles,
            LogFolderUnknowns,
            DecryptOasis,
            LogDatatables,
            LoadFilesOfType,
        };

        static void Main(string[] args)
        {
            LogManager.LogInterface = new LogInterface();

            for (int i = 0; i < funcs.Length; i++)
            {
                Out.WriteLine("{0}: {1}", i, funcs[i].Method.Name);
            }
            Out.Write("Choice: ");
            int num = int.Parse(Out.ReadLine());
            Out.Clear();
            funcs[num]();
            Out.WriteLine("");
            Out.WriteLine("Routine ended");
            Out.ReadLine();
        }

        static void CheckFiles()
        {
            Out.Write("File path: ");
            string path = Out.ReadLine();
            if (!File.Exists(path) && !Directory.Exists(path))
                Environment.Exit(69);

            LogManager.GlobalLogFlags = LogFlags.Error | LogFlags.Info;

            BigFile bigFile = BigFile.OpenBigFile(path);
            bigFile.LoadFromDisk();

            for (int i = 0; i < bigFile.FileMap.FilesList.Length; i++)
            {
                YetiObject file = bigFile.FileMap.FilesList[i];

                if ((file.FileInfo.Flags & 0x00FF0000) != 0)
                {
                    LogManager.GlobalLogFlags = LogFlags.All;
                    file.FileInfo.DebugLog(log);
                    LogManager.GlobalLogFlags = LogFlags.Error | LogFlags.Info;
                }
            }
        }

        static void CompareAllFiles()
        {
            LogManager.GlobalLogFlags = LogFlags.Error | LogFlags.Info;

            Out.Write("Bigfile 1: ");
            string path1 = Out.ReadLine();
            Out.Write("Bigfile 2: ");
            string path2 = Out.ReadLine();

            IOBuffers buffer1 = new IOBuffers();
            IOBuffers buffer2 = new IOBuffers();

            BigFile file1 = BigFile.OpenBigFile(path1);
            BigFile file2 = BigFile.OpenBigFile(path2);

            if (file1 == null)
            {
                Out.WriteLine("file 1 null");
                Out.ReadLine();
                Environment.Exit(911);
            }
            if (file2 == null)
            {
                Out.WriteLine("file 2 null");
                Out.ReadLine();
                Environment.Exit(911);
            }

            file1.LoadFromDisk();
            Out.Write("Press enter...");
            Out.ReadLine();
            file2.LoadFromDisk();
            Out.Write("Press enter...");
            Out.ReadLine();
            Out.Clear();

            int missingFiles = 0;
            if (file1.FileMap.FilesList.Length != file2.FileMap.FilesList.Length)
            {
                Out.WriteLine("Files count don't match!");
                foreach (YetiObject file1file in file1.FileMap.FilesList)
                {
                    if (file2.FileMap[file1file.FileInfo.Key] == null)
                    {
                        missingFiles++;
                        Out.WriteLine("Found file {0} (key: {1:X8}) that appears in the first bigfile but not the second!", file1file.Name, file1file.FileInfo.Key);
                    }
                }
                Out.ReadLine();
                foreach (YetiObject file2file in file2.FileMap.FilesList)
                {
                    if (file1.FileMap[file2file.FileInfo.Key] == null)
                    {
                        missingFiles++;
                        Out.WriteLine("Found file {0} (key: {1:X8}) that appears in the second bigfile but not the first!", file2file.Name, file2file.FileInfo.Key);
                    }
                }
                Out.ReadLine();
            }

            Out.WriteLine("");
            Out.WriteLine("Found {0} file discrepancies between the two bigfiles", missingFiles);
            Out.Write("Would you like to continue? ");
            Out.ReadLine();

            Out.Clear();

            IEnumerator<int[]> headers1 = file1.FileReader.ReadAllHeaders(file1.FileMap.FilesList, buffer1, file1.FileReader.DefaultFlags).GetEnumerator();
            IEnumerator<int[]> headers2 = file2.FileReader.ReadAllHeaders(file2.FileMap.FilesList, buffer1, file2.FileReader.DefaultFlags).GetEnumerator();

            void CompareHeaders(YetiObject file1file, YetiObject file2file, int[] a, int[] b)
            {
                bool foundError = a.Length != b.Length;
                if (!foundError)
                {
                    for (int i = 0; i < a.Length; i++)
                    {
                        if (a[i] != b[i])
                        {
                            foundError = true;
                            break;
                        }
                    }
                }
                if (foundError)
                {
                    Out.WriteLine("File {0} has a header discrepancy!");
                    Out.WriteLine("File 1 header length: {0}", a.Length);
                    Out.WriteLine("File 2 header length: {1}", b.Length);
                    for (int i = 0; i < a.Length; i++)
                        Out.Write("{0:X8} ", a[i]);
                    for (int i = 0; i < b.Length; i++)
                        Out.Write("{0:X8} ", b[i]);
                }
            }


        }

        static void CompareFiles()
        {
            LogManager.GlobalLogFlags = LogFlags.Error | LogFlags.Info;

            Out.Write("Bigfile 1: ");
            string path1 = Out.ReadLine();
            Out.Write("Bigfile 2: ");
            string path2 = Out.ReadLine();

            IOBuffers buffer1 = new IOBuffers();
            IOBuffers buffer2 = new IOBuffers();

            Out.Write("File key: ");
            int key = 0;
            key = Convert.ToInt32(Out.ReadLine(), 16);
            if (key == 0)
                Environment.Exit(420);

            BigFile file1 = BigFile.OpenBigFile(path1);
            BigFile file2 = BigFile.OpenBigFile(path2);

            if (file1 == null)
            {
                Out.WriteLine("file 1 null");
                Out.ReadLine();
                Environment.Exit(911);
            }
            if (file2 == null)
            {
                Out.WriteLine("file 2 null");
                Out.ReadLine();
                Environment.Exit(911);
            }

            file1.LoadFromDisk();
            Out.Write("Press enter...");
            Out.ReadLine();
            file2.LoadFromDisk();
            Out.Write("Press enter...");
            Out.ReadLine();

            YetiObject bigFileFile1 = file1.FileMap[key];
            YetiObject bigFileFile2 = file2.FileMap[key];

            int[] header1 = file1.FileReader.ReadFileHeader(bigFileFile1, buffer1, file1.FileReader.DefaultFlags);
            int[] header2 = file2.FileReader.ReadFileHeader(bigFileFile2, buffer2, file2.FileReader.DefaultFlags);
            
            int size1 = file1.FileReader.ReadFileRaw(bigFileFile1, buffer1, file1.FileReader.DefaultFlags);
            int size2 = file2.FileReader.ReadFileRaw(bigFileFile2, buffer2, file2.FileReader.DefaultFlags);

            int chksum1 = 0;
            int chksum2 = 0;
            for (int i = 0; i < size1; i++)
                chksum1 += buffer1[size1][i];
            for (int i = 0; i < size2; i++)
                chksum2 += buffer2[size2][i];

            Out.Clear();

            Out.WriteLine("Size 1: " + size1);
            Out.WriteLine("Checksum 1: " + chksum1);
            Out.WriteLine("Size 2: " + size2);
            Out.WriteLine("Checksum 2: " + chksum2);

            Out.Write("Header 1, length: {0} : ", header1.Length);
            for (int i = 0; i < header1.Length; i++)
                Out.Write("{0:X8} ", header1[i]);
            Out.Write("\nHeader 2, length: {0} : ", header2.Length);
            for (int i = 0; i < header2.Length; i++)
                Out.Write("{0:X8} ", header2[i]);

            Out.WriteLine("");

            LogManager.GlobalLogFlags = LogFlags.All;

            bigFileFile1.FileInfo.DebugLog(log);
            bigFileFile2.FileInfo.DebugLog(log);

        }

        static void ProcessBigmap()
        {
            Out.Write("File path: ");
            string path = Out.ReadLine();
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
                    Out.WriteLine("{0:X4} {1:X4}", keyA, keyB);

                    if (keyB != 0)
                    {
                        Out.Write("Press enter...");
                        Out.ReadLine();
                    }
                }
            }
        }

        static void TestTextures()
        {
            Out.Write("File path: ");
            string path = Out.ReadLine();
            if (!File.Exists(path))
                Environment.Exit(69);

            Out.Write("\nType: ");
            string type = Out.ReadLine();
            YetiTextureFormat format;
            if (!Enum.TryParse(type, out format))
                Environment.Exit(420);
            
            LogManager.GlobalLogFlags = LogFlags.Error | LogFlags.Info;

            PackedBigFile bigFile = new PackedBigFile(new FileInfo(path));
            bigFile.LoadFromDisk();

            List<YetiObject> textureFiles = bigFile.RootFolder.GetAllObjectsOfArchetype<YetiTextureMetadata>();
            bigFile.FileLoader.LoadAll(textureFiles);
            foreach (YetiObject file in textureFiles)
            {
                YetiTextureMetadata archetype = file.ArchetypeAs<YetiTextureMetadata>();
                if (archetype.Format == format)
                    Out.WriteLine(file.FullFolderPath + file.Name + " " + string.Format("{0:X2} {1} {2}", archetype.Format, archetype.Width, archetype.Height));
            }
        }

        static void LogFiles()
        {
            Out.Write("File path: ");
            string path = Out.ReadLine();
            if (!File.Exists(path))
                Environment.Exit(69);

            LogManager.GlobalLogFlags = LogFlags.Error | LogFlags.Info;

            PackedBigFile bigFile = new PackedBigFile(new FileInfo(path));
            bigFile.LoadFromDisk();

            List<YetiObject> files = bigFile.RootFolder.GetAllObjectsOfArchetype<YetiCurve>();
            bigFile.FileLoader.LoadAll(files);
            foreach (YetiObject file in files)
            {
                YetiCurve archetype = file.ArchetypeAs<YetiCurve>();
                log.Info(file.Name);
                archetype.Log(log);
            }
        }

        static void LogFolderUnknowns()
        {
            Out.Write("File path: ");
            string path = Out.ReadLine();
            if (!File.Exists(path))
                Environment.Exit(69);

            LogManager.GlobalLogFlags = LogFlags.Error | LogFlags.Info;

            PackedBigFile bigFile = new PackedBigFile(new FileInfo(path));
            bigFile.LoadFromDisk();

            void FolderRecursion(BigFileFolder folder)
            {
                if (false)//(folder.InfoStruct.Unknown_03 != -1)
                    if (folder.FolderMap.ContainsKey(folder.InfoStruct.Unknown_03))
                        goto next;

                Out.WriteLine(folder.Name);
                if (folder.InfoStruct.NextFolder != -1)
                    if (!folder.FolderMap.ContainsKey(folder.InfoStruct.NextFolder))
                        Out.WriteLine(string.Format("next  {0:X4}", folder.InfoStruct.NextFolder));
                    else
                        Out.WriteLine(string.Format("next  {0}", folder.FolderMap[folder.InfoStruct.NextFolder].Name));

                if (folder.InfoStruct.PreviousFolder != -1)
                    if (!folder.FolderMap.ContainsKey(folder.InfoStruct.PreviousFolder))
                        Out.WriteLine(string.Format("prev  {0:X4}", folder.InfoStruct.PreviousFolder));
                    else
                        Out.WriteLine(string.Format("prev  {0}", folder.FolderMap[folder.InfoStruct.PreviousFolder].Name));

                if (true)//(folder.InfoStruct.Unknown_01 != -1)
                    if (!folder.FolderMap.ContainsKey(folder.InfoStruct.Unknown_01))
                        Out.WriteLine(string.Format("un01  {0:X4}", folder.InfoStruct.Unknown_01));
                    else
                        Out.WriteLine(string.Format("un01  {0}", folder.FolderMap[folder.InfoStruct.Unknown_01].Name));

                if (true)//(folder.InfoStruct.Unknown_02 != -1)
                    if (!folder.FolderMap.ContainsKey(folder.InfoStruct.Unknown_02))
                        Out.WriteLine(string.Format("un02  {0:X4}", folder.InfoStruct.Unknown_02));
                    else
                        Out.WriteLine(string.Format("un02  {0}", folder.FolderMap[folder.InfoStruct.Unknown_02].Name));

                if (true)//(folder.InfoStruct.Unknown_03 != -1)
                    if (!folder.FolderMap.ContainsKey(folder.InfoStruct.Unknown_03))
                        Out.WriteLine(string.Format("un03  {0:X4}", folder.InfoStruct.Unknown_03));
                    else
                        Out.WriteLine(string.Format("un03  {0}", folder.FolderMap[folder.InfoStruct.Unknown_03].Name));
                Out.WriteLine("");

                next:
                foreach (BigFileFolder f in folder.SubFolders)
                    FolderRecursion(f);
            }

            FolderRecursion(bigFile.RootFolder);
        }

        static void DecryptOasis()
        {
            Out.Write("File path: ");
            string path = Out.ReadLine();
            if (!File.Exists(path))
                Environment.Exit(69);

            const string OasisKey = "570462DC49E9E51F0B55F30287A5C7CD";
            const string OutputFile = "OASIS_OUTPUT.txt";
            byte[] OasisKeyBytes = Encoding.ASCII.GetBytes(OasisKey);
            Out.WriteLine(OasisKeyBytes.Length.ToString());
            //Array.Reverse(OasisKeyBytes);

            TwofishEngine tf1 = new TwofishEngine();
            TwofishEngine tf2 = new TwofishEngine();
            tf1.Init(false, new KeyParameter(OasisKeyBytes, 0, OasisKeyBytes.Length));
            tf2.Init(true, new KeyParameter(OasisKeyBytes, 0, OasisKeyBytes.Length));

            byte[] buf = new byte[tf1.GetBlockSize()];
            byte[] output = new byte[tf1.GetBlockSize()];

            Out.WriteLine(string.Format("Decrypt Key: {0}", OasisKey));
            Out.WriteLine(string.Format("Twofish block size: {0}", tf1.GetBlockSize()));
            Out.WriteLine(string.Format("Output file: {0}", OutputFile));

            using (FileStream fsin = File.OpenRead(path))
            using (FileStream fsout = File.Create(OutputFile))
            using (MemoryStream ms = new MemoryStream(buf))
            {
                Out.WriteLine(string.Format("File Stream Length: {0}", fsin.Length));

                Out.ReadLine();

                while (fsin.Position < fsin.Length)
                {
                    fsin.Read(buf, 0, buf.Length);
                    int size = tf1.ProcessBlock(buf, 0, output, 0);
                    fsout.Write(output, 0, size);
                    Out.WriteLine(string.Format("position {0}   size {1}", fsin.Position.ToString(), size));
                }
            }
        }

        static void LogDatatables()
        {
            const string CSVDir = "DataTables\\";

            Directory.CreateDirectory(CSVDir);

            Out.Write("File path: ");
            string path = Out.ReadLine();
            if (!File.Exists(path))
                Environment.Exit(69);

            LogManager.GlobalLogFlags = LogFlags.Error | LogFlags.Info;

            PackedBigFile bigFile = new PackedBigFile(new FileInfo(path));
            bigFile.LoadFromDisk();

            List<YetiObject> files = bigFile.RootFolder.GetAllObjectsOfArchetype<YetiDataTable>();

            bigFile.FileLoader.LoadAll(files);

            foreach (YetiObject f in files)
            {
                f.Archetype.Log(log);

                using (StreamWriter sw = File.CreateText(CSVDir + f.Name + ".csv"))
                {
                    YetiDataTable dt = f.ArchetypeAs<YetiDataTable>();
                    for (int i = 0; i < dt.NumColumns; i++)
                        sw.Write(dt[i].ColumnName + ",");
                    sw.Write("\n");

                    for (int i = 0; i < dt.NumRows; i++)
                    {
                        for (int j = 0; j < dt.NumColumns; j++)
                        {
                            sw.Write(dt[j][i] + ",");
                        }
                        sw.Write("\n");
                    }
                }
            }
        }

        static void LoadFilesOfType()
        {
            Out.Write("File path: ");
            string path = Out.ReadLine();
            if (!File.Exists(path))
                Environment.Exit(69);

            Out.Write("Type: ");
            string t = Out.ReadLine();
            YetiObjectType type;
            Enum.TryParse(t, out type);
            if (type == YetiObjectType.NONE)
                return;

            LogManager.GlobalLogFlags = LogFlags.Error | LogFlags.Info;

            PackedBigFile bigFile = new PackedBigFile(new FileInfo(path));
            bigFile.LoadFromDisk();
            //LogManager.GlobalLogFlags = LogFlags.Error | LogFlags.Info | LogFlags.Debug;

            List<YetiObject> objects = bigFile.RootFolder.GetAllObjectsOfType(type);
            bigFile.FileLoader.LoadReferences(objects);
            bigFile.FileLoader.LoadAll(objects);

            foreach (YetiObject obj in objects)
            {
                obj.Archetype.Log(log);
            }
        }
    }
}
