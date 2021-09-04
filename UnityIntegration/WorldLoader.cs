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
using UnityIntegration.Components;
using UnityIntegration.Script;

namespace UnityIntegration
{
    public class WorldLoader : MonoBehaviour
    {
        public enum BrowserMode
        {
            CurrentWorld,
            Worlds,
            Files,
            Layers
        }

        public bool showMenu = true;

        ILogProxy log = LogManager.GetLogProxy("WorldLoader");

        public int NumObjectsLoadedPerFrame = 10;
        public int ObjectsPerPage = 50;
        public int CurrentPage = 1;
        public string fileKey = "0x";
        public BrowserMode mode = BrowserMode.Worlds;

        List<YetiObject> worldObjects;
        bool isLoading = false;
        Vector2 scrollPos = Vector2.zero;
        Vector2 scrollPos2 = Vector2.zero;

        YetiWorldLoadContext context;

        void Update()
        {
            if (Input.GetKeyDown(KeyCode.LeftControl))
                showMenu = !showMenu;
        }

        void OnGUI()
        {
            DoBrowsingGUI();
        }

        private void UnloadWorld()
        {

        }

        private void DoNoMenuGUI()
        {
            Rect rect = new Rect(Screen.width - 250f, 0, 250f, 25f);
            GUI.Label(rect, "LCTRL for menu");
        }

        private void DoBrowsingGUI()
        {
            if (!showMenu)
            {
                DoNoMenuGUI();
                return;
            }

            if (LibManager.BigFile == null)
                return;

            Rect rect = new Rect(Screen.width - 350f - 100f, 0, 100f, 25f);
            if (mode != BrowserMode.CurrentWorld)
            {
                if (GUI.Button(rect, "Current World"))
                    mode = BrowserMode.CurrentWorld;
            }
            else
                GUI.Label(rect, "Current World");
            rect.y += rect.height;
            if (mode != BrowserMode.Worlds)
            {
                if (GUI.Button(rect, "Worlds"))
                    mode = BrowserMode.Worlds;
            }
            else
                GUI.Label(rect, "Worlds");
            rect.y += rect.height;
            if (mode != BrowserMode.Files)
            {
                if (GUI.Button(rect, "Files"))
                    mode = BrowserMode.Files;
            }
            else
                GUI.Label(rect, "Files");
            rect.y += rect.height;
            if (mode != BrowserMode.Layers)
            {
                if (GUI.Button(rect, "Layers"))
                    mode = BrowserMode.Layers;
            }
            else
                GUI.Label(rect, "Layers");

            switch (mode)
            {
                case BrowserMode.Worlds:
                default:
                    DoLoadWorldGUI();
                    break;
                case BrowserMode.CurrentWorld:
                    DoWorldBrowserGUI();
                    break;
                case BrowserMode.Files:
                    break;
            }
        }

        private void DoLoadWorldGUI()
        {
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
                    if (isLoading)
                        continue;

                    void LoadWorld()
                    {
                        isLoading = true;
                        context = new YetiWorldLoadContext();
                        YetiObjectConverter conv = YetiObjectConverter.GetConverter(obj);
                        conv.Convert(obj, null, context);
                        (conv.Components[0] as cYetiWorld).afterLoadedCallback = () => isLoading = false;
                    }

                    if (context != null)
                    {
                        isLoading = true;
                        YetiWorldLoadContext c = context;
                        while (c.parentContext != null)
                            c = c.parentContext;
                        context.subContexts[0].worldComponent.UnloadWorld(LoadWorld);
                        break;
                    }

                    LoadWorld();
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

            Rect rect = new Rect(Screen.width - 350f, 0, 350f, 25f);
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
                    CurrentPage = 1;
                    return;
                }
                rect.y += rect.height;
            }
            float w = rect.width;
            rect.width = rect.width / 2;
            int numPages = (context.worldObjects.Count / ObjectsPerPage) + 1;
            if (CurrentPage != 1)
            {
                if (GUI.Button(rect, "<-- prev"))
                {
                    CurrentPage--;
                }
            }
            rect.x += rect.width;
            if (CurrentPage < numPages)
            {
                if (GUI.Button(rect, "next -->"))
                {
                    CurrentPage++;
                }
            }
            rect.x -= rect.width;
            rect.width = w;
            rect.y += rect.height;
            int numObjToDisplay = Mathf.Min(ObjectsPerPage, context.worldObjects.Count - (CurrentPage * ObjectsPerPage));
            Rect scrollRect = new Rect(rect.x, rect.y, rect.width, Screen.height - rect.y);
            Rect viewRect = new Rect(0, 0, rect.width, rect.height * numObjToDisplay);
            scrollPos2 = GUI.BeginScrollView(scrollRect, scrollPos2, viewRect);
            Rect btnRect = new Rect(0, 0, scrollRect.width, rect.height);
            for (int i = 0; i < numObjToDisplay; i++)
            {
                int ind = ((CurrentPage - 1) * ObjectsPerPage) + i;
                GameObject obj = context.worldObjects[ind];
                cYetiObjectReference objRef = obj.GetComponent<cYetiObjectReference>();
                if (GUI.Button(btnRect, objRef.yetiObject.NameWithExtension))
                {
                    CameraMover.inst.InterpToObject(obj);
                }
                btnRect.y += btnRect.height;
            }
            GUI.EndScrollView();
        }

        private void DoLayersGUI()
        {

        }
    }
}
