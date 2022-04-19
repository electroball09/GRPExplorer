using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using GRPExplorerLib.BigFile;
using GRPExplorerLib.Logging;
using UnityEngine;
using System.IO;
using Microsoft.Win32;

namespace UnityIntegration
{
    public class LibManager : SingletonBehaviour<LibManager>
    {
        public string filePath = "C:\\";
        private BigFile m_bigFile;
        public static BigFile BigFile
        {
            get { return inst.m_bigFile; }
        }

        [RuntimeInitializeOnLoadMethod]
        static void Load()
        {
            LogManager.LogInterface = new UnityLogInterface();
            LogManager.GlobalLogFlags = LogFlags.Error | LogFlags.Info;

            new GameObject("LIB_MANAGER").AddComponent<LibManager>();
        }

        void Awake()
        {
            filePath = Settings.LastBigfileLoadPath;
        }

        void OnGUI()
        {
            if (BigFile == null)
            {
                DoLoadGUI();
            }
            else
            {
                DoLoadedGUI();
            }
        }

        private void DoLoadGUI()
        {
            Rect rect = new Rect(0, 0, 450f, 35f);
            filePath = GUI.TextField(rect, filePath);
            rect.width = 150f;
            rect.y += rect.height;
            if (GUI.Button(rect, "Load"))
            {
                Settings.LastBigfileLoadPath = filePath;
                IntegrationUtil.LoadBigFileInBackground(filePath,
                    (bigFile) =>
                    {
                        m_bigFile = bigFile;
                    });
            }
        }

        private void DoLoadedGUI()
        {
            Rect rect = new Rect(0, 0, 450f, 35f);
            GUI.Label(rect, BigFile.MetadataFileInfo.FullName);
        }
    }
}
