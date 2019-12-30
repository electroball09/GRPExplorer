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

namespace UnityIntegration.Components
{

    public class cYetiWorld : cYetiObjectReference
    {
        ILogProxy log = LogManager.GetLogProxy("cYetiWorld");

        public YetiWorldLoadContext loadContext;
        public YetiWorld thisWorld;

        public Action afterLoadedCallback;

        IEnumerator eLoadWorld(YetiWorld world, YetiWorldLoadContext context)
        {
            //LogManager.GlobalLogFlags = LogFlags.All;
            if (context == null)
                context = new YetiWorldLoadContext();

            thisWorld = world;

            loadContext = context.GetSubContext(world);
            loadContext.worldComponent = this;
            loadContext.g_loadedWorlds.Add(world);

            log.Info("loading world {0}", world.Object.Name);

            int count = 0;

            if (loadContext.g_loadedList.Count == 0)
                foreach (YetiObject obj in LibManager.BigFile.FileLoader.LoadObjectRecursive(world.Object, loadContext.g_loadedList))
                {
                    count++;
                    if (count >= 25)
                    {
                        count = 0;
                        yield return null;
                    }
                }

            YetiGameObjectList gol = world.GameObjectList;
            YetiWorldIncludeList wil = world.SubWorldList;

            foreach (YetiObject obj in world.Object.ObjectReferences)
            {
                if (obj != null &&
                    !obj.Is<YetiWorldIncludeList>() &&
                    !obj.Is<YetiWorld>())
                    YetiObjectConverter.GetConverter(obj).Convert(obj, gameObject, loadContext);
            }

            foreach (YetiObject obj in gol.ObjectList)
            {
                log.Info("Instantiating object {0}", obj.NameWithExtension);

                YetiObjectConverter.GetConverter(obj).Convert(obj, null, loadContext);

                count++;
                if (count >= 25)
                {
                    count = 0;
                    yield return null;
                }
            }

            if (wil != null)
                foreach (YetiWorld subWorld in wil.IncludeList)
                {
                    YetiObjectConverter.GetConverter(subWorld.Object).Convert(subWorld.Object, null, loadContext);
                }

            LogManager.GlobalLogFlags = LogFlags.Error | LogFlags.Info;

            afterLoadedCallback?.Invoke();
        }

        IEnumerator eUnloadWorld(Action afterUnloadedCallback)
        {
            log.Info("Unloading world {0}", yetiObject.NameWithExtension);

            int count = 0;

            foreach (GameObject obj in loadContext.worldObjects)
            {
                Destroy(obj);

                log.Info("Destroying object {0}", obj.name);

                count++;
                if (count > 50)
                {
                    count = 0;
                    yield return null;
                }
            }

            foreach (YetiWorldLoadContext subContext in loadContext.subContexts)
            {
                subContext.worldComponent.UnloadWorld(null);
            }

            if (loadContext.parentContext.currentWorld == null)
            {
                foreach (YetiObject obj in loadContext.g_loadedList)
                {
                    obj.Unload();

                    log.Info("Unloading object {0}", obj.NameWithExtension);

                    count++;
                    if (count > 50)
                    {
                        count = 0;
                        yield return null;
                    }
                }
            }

            Destroy(gameObject);

            afterUnloadedCallback?.Invoke();
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
