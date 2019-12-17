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

        List<YetiObject> worldObjects;
        bool didLoad = false;
        Vector2 scrollPos = Vector2.zero;

        void OnGUI()
        {
            if (didLoad)
                return;

            if (LibManager.BigFile == null)
                return;

            if (worldObjects == null)
                worldObjects = LibManager.BigFile.RootFolder.GetAllObjectsOfArchetype<YetiWorld>();

            Rect scrollRectPos = new Rect(Screen.width - 350f, 0, 350f, Screen.height);
            Rect scrollRect = new Rect(0, 0, 350f, worldObjects.Count * 25f);
            scrollPos = GUI.BeginScrollView(scrollRectPos, scrollPos, scrollRect);
            Rect btnRect = new Rect(0, 0, 350f, 25f);
            foreach (YetiObject obj in worldObjects)
            {
                if (GUI.Button(btnRect, obj.Name))
                {
                    YetiObjectConverter.GetConverter(obj).Convert(obj, null, new YetiWorldLoadContext());
                    didLoad = true;
                    break;
                }

                btnRect.y += btnRect.height;
            }
            GUI.EndScrollView();

            //fileKey = GUI.TextField(rect, fileKey);
            //rect.y += rect.height;
            //if (GUI.Button(rect, "Load"))
            //{
            //    int key = Convert.ToInt32(fileKey, 16);

            //    YetiObject obj = LibManager.BigFile.FileMap[key];
            //    if (obj == null)
            //        return;

            //    YetiWorld world = obj.ArchetypeAs<YetiWorld>();
            //    if (world == null)
            //        return;

            //    YetiObjectConverter.GetConverter(obj).Convert(obj, null, new YetiWorldLoadContext());
            //}
        }
    }
}
