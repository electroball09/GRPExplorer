using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using GRPExplorerLib.BigFile;
using GRPExplorerLib.YetiObjects;
using UnityEngine;
using UnityIntegration.Components;

namespace UnityIntegration.Converters
{
    public class YetiGameObjectConverter : YetiObjectConverter
    {
        public override Type ArchetypeType => typeof(YetiGameObject);

        public override void Convert(YetiObject yetiObject, GameObject parentObject)
        {
            if (parentObject)
            {
                return;
                log.Error("This YetiGameObject has a Unity GameObject parent! {0}", yetiObject.NameWithExtension);
            }

            YetiGameObject obj = yetiObject.ArchetypeAs<YetiGameObject>();

            Matrix4x4 matrix = obj.Matrix.ConvertToUnity();

            Vector3 pos;
            Quaternion rot;
            Vector3 scale;
            IntegrationUtil.DecomposeMatrix(ref matrix, out pos, out rot, out scale);

            rot = rot.ConvertYetiToUnityRotation();
            pos = pos.ConvertYetiToUnityCoords();

            GameObject gameObject = new GameObject(yetiObject.NameWithExtension);
            gameObject.transform.position = pos;
            gameObject.transform.rotation = rot;
            gameObject.transform.localScale = scale;

            cYetiObjectReference cmp = gameObject.AddComponent<cYetiObjectReference>();
            cmp.Key = yetiObject.FileInfo.Key;

            foreach (YetiObject subObj in yetiObject.ObjectReferences)
            {
                if (subObj == null)
                    continue;

                if (subObj.Is<YetiGameObject>())
                    continue;

                GetConverter(subObj).Convert(subObj, gameObject);
            }
        }
    }
}
