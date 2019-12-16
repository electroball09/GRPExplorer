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

        IEnumerator LoadWorld(YetiWorld world)
        {
            //LogManager.GlobalLogFlags = LogFlags.All;

            log.Info("loading world {0}", world.Object.Name);

            int count = 0;

            foreach (YetiObject obj in LibManager.BigFile.FileLoader.LoadObjectRecursive(world.Object))
            {
                count++;
                if (count >= NumObjectsLoadedPerFrame)
                {
                    count = 0;
                    yield return null;
                }
            }

            YetiGameObjectList gol = world.GameObjectList;
            if (gol == null)
            {
                throw new Exception("wtf");
            }

            foreach (YetiObject obj in gol.ObjectList)
            {
                log.Info("Instantiating object {0}", obj.NameWithExtension);

                YetiObjectConverter.GetConverter(obj).Convert(obj, null);

                foreach (YetiObject subObj in obj.ObjectReferences)
                {
                    if (subObj == null)
                        continue;

                    GameObject subGameObject = new GameObject(subObj.NameWithExtension);
                    subGameObject.transform.parent = gameObject.transform;
                    subGameObject.transform.localPosition = Vector3.zero;
                    subGameObject.transform.localRotation = Quaternion.identity;

                    YetiObjectConverter.GetConverter(subObj).Convert(subObj, subGameObject);

                    count++;
                    if (count >= NumObjectsLoadedPerFrame)
                    {
                        count = 0;
                        yield return null;
                    }
                }
            }

            LogManager.GlobalLogFlags = LogFlags.Error | LogFlags.Info;
        }


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

                StartCoroutine(LoadWorld(world));
            }
        }
    }
}
