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
        public bool showMenu = true;

        ILogProxy log = LogManager.GetLogProxy("WorldLoader");

        public int NumObjectsLoadedPerFrame = 10;
        public string fileKey = "0x";

        List<YetiObject> worldObjects;
        bool didLoad = false;
        Vector2 scrollPos = Vector2.zero;

        YetiWorldLoadContext context;

        void OnGUI()
        {
            DoLoadGUI();
            DoWorldBrowserGUI();
        }

        private void DoLoadGUI()
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
                    context = new YetiWorldLoadContext();
                    YetiObjectConverter.GetConverter(obj).Convert(obj, null, context);
                    didLoad = true;
                    break;
                }

                btnRect.y += btnRect.height;
            }
            GUI.EndScrollView();
        }

        private void DoWorldBrowserGUI()
        {
            if (context == null)
                return;

            if (Input.GetKeyDown(KeyCode.Tab))
                showMenu = !showMenu;

            if (!showMenu)
                return;

            Rect rect = new Rect(Screen.width - 250f, 0, 250f, 25f);
            if (context.parentContext != null)
            {
                if (GUI.Button(rect, " <- " + (context.parentContext.currentWorld != null ? context.parentContext.currentWorld.Object.Name : "---")))
                {
                    context = context.parentContext;
                    return;
                }
                rect.y += rect.height;
            }
            GUI.Label(rect, context.currentWorld != null ? context.currentWorld.Object.Name : "---");
            rect.y += rect.height;
            foreach (YetiWorldLoadContext subContext in context.subContexts)
            {
                if (GUI.Button(rect, subContext.currentWorld.Object.Name))
                {
                    context = subContext;
                    return;
                }
                rect.y += rect.height;
            }
        }
    }
}
