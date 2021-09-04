using GRPExplorerLib.BigFile;
using GRPExplorerLib.YetiObjects;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using GRPExplorerLib.Logging;
using UnityIntegration.Converters;
using UnityIntegration.Script;
using System.Threading;

namespace UnityIntegration.Components
{

    public class cYetiWorld : cYetiObjectReference
    {
        ILogProxy log = LogManager.GetLogProxy("cYetiWorld");

        public YetiWorldLoadContext loadContext;
        public YetiWorld thisWorld;

        public GameObject LayersObject;

        public Action afterLoadedCallback;

        Thread bgLoadThread;

        void OnApplicationQuit()
        {
            if (bgLoadThread == null) return;
            bgLoadThread.Abort();
            bgLoadThread = null;
        }

        IEnumerator eLoadWorld(YetiWorld world, YetiWorldLoadContext context)
        {
            if (bgLoadThread != null)
            {
                log.Error("Load thread exists!");
                yield break;
            }

            for (int i = 0; i < 3; i++)
                yield return null;

            int count = 0;

            if (context == null)
                context = new YetiWorldLoadContext();

            thisWorld = world;

            loadContext = context.GetSubContext(world);
            loadContext.worldComponent = this;
            loadContext.g_loadedWorlds.Add(world);

            DebugMsg dMsg = DebugMsgMgr.inst.NewPermMessage("Loading world " + yetiObject.NameWithExtension);

            void tLoadWorld()
            {
                log.Info("loading world {0}", world.Object.Name);

                if (loadContext.g_loadedList.Count == 0)
                    foreach (YetiObject obj in LibManager.BigFile.FileLoader.LoadObjectRecursive(world.Object, loadContext.g_loadedList))
                    {
                        count++;
                    }

                bgLoadThread = null;

                log.Info("Background loading thread ended");
            }

            bgLoadThread = new Thread(new ThreadStart(tLoadWorld))
            {
                Name = yetiObject.NameWithExtension + " - LOAD"
            };
            bgLoadThread.Start();

            while (bgLoadThread != null)
            {
                yield return null;
            }

            //LogManager.GlobalLogFlags = LogFlags.All;

            LayersObject = new GameObject("LAYERS");
            LayersObject.transform.parent = transform;

            YetiGameObjectList gol = world.GameObjectList;
            YetiWorldIncludeList wil = world.SubWorldList;

            foreach (YetiObject obj in world.Object.ObjectReferences)
            {
                if (obj != null &&
                    !obj.Is<YetiWorldIncludeList>() &&
                    !obj.Is<YetiWorld>())
                    YetiObjectConverter.GetConverter(obj).Convert(obj, gameObject, loadContext);
            }

            dMsg.Text = "Instantiating world " + yetiObject.NameWithExtension;

            foreach (YetiObject obj in gol.ObjectList)
            {
                //log.Info("Instantiating object {0}", obj.NameWithExtension);

                YetiObjectConverter.GetConverter(obj).Convert(obj, null, loadContext);

                count++;
                if (count >= 25)
                {
                    count = 0;
                    yield return null;
                }
            }

            DebugMsgMgr.inst.RemoveMsg(dMsg);

            if (wil != null)
                foreach (YetiWorld subWorld in wil.IncludeList)
                {
                    if (subWorld != null)
                        YetiObjectConverter.GetConverter(subWorld.Object).Convert(subWorld.Object, gameObject, loadContext);
                }

            LogManager.GlobalLogFlags = LogFlags.Error | LogFlags.Info;

            log.Info("World {0} loaded!", yetiObject.NameWithExtension);

            GC.Collect();

            afterLoadedCallback?.Invoke();
        }

        IEnumerator eUnloadWorld(Action afterUnloadedCallback)
        {
            for (int i = 0; i < 3; i++)
                yield return null;

            log.Info("Unloading world {0}", yetiObject.NameWithExtension);

            if (!cYetiLight.isToggledOn)
                cYetiLight.ToggleAllLights();
            if (!GAOIdentifier.areIdentifiersVisible)
                GAOIdentifier.ToggleIdentifiersVisible();

            DebugMsg dMsg = DebugMsgMgr.inst.NewPermMessage("Destroying world " + yetiObject.NameWithExtension);

            int count = 0;

            foreach (GameObject obj in loadContext.worldObjects)
            {
                Destroy(obj);

                //log.Info("Destroying object {0}", obj.name);

                count++;
                if (count > 50)
                {
                    count = 0;
                    yield return null;
                }
            }

            dMsg.Text = "Unloading world " + yetiObject.NameWithExtension;

            if (loadContext.parentContext.currentWorld == null)
            {
                foreach (YetiObject obj in loadContext.g_loadedList)
                {
                    obj.Unload();

                    //log.Info("Unloading object {0}", obj.NameWithExtension);

                    count++;
                    if (count > 50)
                    {
                        count = 0;
                        yield return null;
                    }
                }
            }

            DebugMsgMgr.inst.RemoveMsg(dMsg);

            foreach (YetiWorldLoadContext subContext in loadContext.subContexts)
            {
                subContext.worldComponent.UnloadWorld(null);
            }

            log.Info("World {0} unloaded!", yetiObject.NameWithExtension);

            GC.Collect();

            afterUnloadedCallback?.Invoke();

            Destroy(gameObject);
        }

        public void LoadWorld(YetiWorld world, YetiWorldLoadContext context)
        {
            StartCoroutine(eLoadWorld(world, context));
        }

        public void UnloadWorld(Action afterUnloadedCallback)
        {
            StartCoroutine(eUnloadWorld(afterUnloadedCallback));
        }
    }
}
