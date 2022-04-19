using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Win32;
using System.Runtime.CompilerServices;
using System.IO;

namespace GRPExplorerGUI
{
    internal static class Settings
    {
        const string REGISTRY_SUBKEY = "SOFTWARE\\GRPExplorer";
        static readonly string EXTRACT_SUBDIRECTORY = Environment.CurrentDirectory + "\\extract";

        static readonly RegistryKey registryKey;

        static Settings()
        {
            registryKey = Registry.CurrentUser.OpenSubKey(REGISTRY_SUBKEY, true);
            if (registryKey == null)
            {
                registryKey = Registry.CurrentUser.CreateSubKey(REGISTRY_SUBKEY, true);
            }
        }

        static object GetValue([CallerMemberName] string key = "", object defaultValue = null)
        {
            object obj = registryKey.GetValue(key);
            if (obj == null)
            {
                SetValue(key: key, value: defaultValue);
                return GetValue(key);
            }
            return obj;
        }

        static void SetValue(object value, [CallerMemberName] string key = "")
        {
            registryKey.SetValue(key, value);
        }

        public static string LastBigfilePath
        {
            get
            {
                return GetValue(defaultValue: Environment.CurrentDirectory) as string;
            }
            set
            {
                SetValue(value);
            }
        }

        public static string LastUnpackedBigfilePath
        {
            get
            {
                return GetValue(defaultValue: Environment.CurrentDirectory) as string;
            }
            set
            {
                SetValue(value);
            }
        }

        public static string LastExtractPath
        {
            get
            {
                if (!Directory.Exists(EXTRACT_SUBDIRECTORY))
                    Directory.CreateDirectory(EXTRACT_SUBDIRECTORY);
                return GetValue(defaultValue: EXTRACT_SUBDIRECTORY) as string;
            }
            set
            {
                SetValue(value);
            }
        }

        public static string LastFEUPath
        {
            get
            {
                if (!Directory.Exists(EXTRACT_SUBDIRECTORY))
                    Directory.CreateDirectory(EXTRACT_SUBDIRECTORY);
                return GetValue(defaultValue: EXTRACT_SUBDIRECTORY) as string;
            }
            set
            {
                SetValue(value);
            }
        }
    }
}
