using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Win32;
using System.Runtime.CompilerServices;

namespace GRPExplorerGUI
{
    internal static class Settings
    {
        const string REGISTRY_SUBKEY = "SOFTWARE\\GRPExplorer";
        const string EXTRACT_SUBDIRECTORY = "\\extract";

        static readonly RegistryKey registryKey;

        static Settings()
        {
            registryKey = Registry.CurrentUser.OpenSubKey(REGISTRY_SUBKEY, true);
            if (registryKey == null)
            {
                registryKey = Registry.CurrentUser.CreateSubKey(REGISTRY_SUBKEY, true);
            }
        }

        static object GetValue([CallerMemberName] string key = "")
        {
            return registryKey.GetValue(key);
        }

        static void SetValue(object value, [CallerMemberName] string key = "")
        {
            registryKey.SetValue(key, value);
        }

        public static string LastBigfilePath
        {
            get
            {
                object obj = GetValue();
                if (obj == null)
                {
                    SetValue(Environment.CurrentDirectory);
                    return GetValue() as string;
                }
                return obj as string;
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
                object obj = GetValue();
                if (obj == null)
                {
                    SetValue(Environment.CurrentDirectory + EXTRACT_SUBDIRECTORY);
                    return GetValue() as string;
                }
                return obj as string;
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
                object obj = GetValue();
                if (obj == null)
                {
                    SetValue(Environment.CurrentDirectory + EXTRACT_SUBDIRECTORY);
                    return GetValue() as string;
                }
                return obj as string;
            }
            set
            {
                SetValue(value);
            }
        }
    }
}
