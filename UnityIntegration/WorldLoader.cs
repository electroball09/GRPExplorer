using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using GRPExplorerLib.BigFile;
using GRPExplorerLib.YetiObjects;
using System.Collections;
using UnityIntegration.Converters;
using GRPExplorerLib.Logging;

namespace UnityIntegration
{
    public class WorldLoader : MonoBehaviour
    {
        ILogProxy log = LogManager.GetLogProxy("WorldLoader");

        public int NumObjectsLoadedPerFrame = 10;
        public string fileKey = "0x";

        void OnGUI()
        {
            if (LibManager.BigFile == null)
                return;

            Rect rect = new Rect(Screen.width - 150f, 0, 150f, 35f);
            fileKey = GUI.TextField(rect, fileKey);
            rect.y += rect.height;
            if (GUI.Button(rect, "Load"))
            {
                int key = Convert.ToInt32(fileKey, 16);

                YetiObject obj = LibManager.BigFile.FileMap[key];
                if (obj == null)
                    return;

                YetiWorld world = obj.ArchetypeAs<YetiWorld>();
                if (world == null)
                    return;

                YetiObjectConverter.GetConverter(obj).Convert(obj, null, new YetiWorldLoadContext());
            }
        }
    }
}
