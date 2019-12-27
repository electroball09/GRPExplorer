using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using GRPExplorerLib.BigFile;
using GRPExplorerLib.Logging;
using UnityEngine;
using System.IO;

namespace UnityIntegration
{
    public class LibManager : MonoBehaviour
    {
        public string filePath = "C:\\";
        private BigFile m_bigFile;
        public static BigFile BigFile
        {
            get { return inst.m_bigFile; }
        }

        static LibManager inst { get; set; }

        [RuntimeInitializeOnLoadMethod]
        static void Load()
        {
            LogManager.LogInterface = new UnityLogInterface();
            LogManager.GlobalLogFlags = LogFlags.Error | LogFlags.Info;

            inst = new GameObject("LIB_MANAGER").AddComponent<LibManager>();
        }

        void Start()
        {
            if (inst != this)
                Destroy(this);

            inst = this;

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
