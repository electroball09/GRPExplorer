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

        public IEnumerator LoadWorld(YetiWorld world, YetiWorldLoadContext context)
        {
            //LogManager.GlobalLogFlags = LogFlags.All;
            if (context == null)
                context = new YetiWorldLoadContext();

            YetiWorldLoadContext thisContext = context.GetSubContext(world);

            thisContext.g_loadedWorlds.Add(world);

            log.Info("loading world {0}", world.Object.Name);

            int count = 0;

            if (thisContext.g_loadedList.Count == 0)
                foreach (YetiObject obj in LibManager.BigFile.FileLoader.LoadObjectRecursive(world.Object, thisContext.g_loadedList))
                {
                    count++;
                    if (count >= 25)
                    {
                        count = 0;
                        yield return null;
                    }
                }

            YetiGameObjectList gol = world.GameObjectList;
            YetiSubWorldList wil = world.SubWorldList;

            foreach (YetiObject obj in world.Object.ObjectReferences)
            {
                if (obj != null &&
                    !obj.Is<YetiSubWorldList>() &&
                    !obj.Is<YetiWorld>())
                    YetiObjectConverter.GetConverter(obj).Convert(obj, gameObject, thisContext);
            }

            foreach (YetiObject obj in gol.ObjectList)
            {
                log.Info("Instantiating object {0}", obj.NameWithExtension);

                YetiObjectConverter.GetConverter(obj).Convert(obj, null, thisContext);

                count++;
                if (count >= 25)
                {
                    count = 0;
                    yield return null;
                }
            }

            if (wil != null)
                foreach (YetiWorld subWorld in wil.SubWorlds)
                {
                    YetiObjectConverter.GetConverter(subWorld.Object).Convert(subWorld.Object, null, thisContext);
                }

            LogManager.GlobalLogFlags = LogFlags.Error | LogFlags.Info;
        }
    }
}
