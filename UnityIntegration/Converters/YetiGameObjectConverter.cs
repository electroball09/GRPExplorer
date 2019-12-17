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

        public override object Convert(YetiObject yetiObject, GameObject parentObject, YetiWorldLoadContext context)
        {
            YetiGameObject obj = yetiObject.ArchetypeAs<YetiGameObject>();

            Matrix4x4 matrix = obj.Matrix.ConvertToUnity();

            Vector3 pos;
            Quaternion rot;
            Vector3 scale;
            IntegrationUtil.DecomposeMatrix(ref matrix, out pos, out rot, out scale);

            rot = rot.ConvertYetiToUnityRotation();
            pos = pos.ConvertYetiToUnityCoords();

            GameObject gameObject = new GameObject(yetiObject.NameWithExtension);
            if (parentObject)
            {
                gameObject.transform.SetParent(parentObject.transform, false);
            }
            else
            {
                gameObject.transform.position = pos;
                gameObject.transform.rotation = rot;
                gameObject.transform.localScale = scale;
            }

            cYetiObjectReference.AddYetiComponent<cYetiObjectReference>(gameObject, yetiObject);

            foreach (YetiObject subObj in yetiObject.ObjectReferences)
            {
                if (subObj == null)
                    continue;

                if (subObj.Is<YetiGraphicObjectTable>())
                    GetConverter(subObj).Convert(subObj, gameObject, context);
            }

            return null;
        }
    }

    public class YetiGraphicObjectTableConverter : YetiObjectConverter
    {
        public override Type ArchetypeType => typeof(YetiGraphicObjectTable);

        public override object Convert(YetiObject yetiObject, GameObject parentObject, YetiWorldLoadContext context)
        {
            YetiGraphicObjectTable got = yetiObject.ArchetypeAs<YetiGraphicObjectTable>();

            GameObject gameObject = new GameObject(yetiObject.NameWithExtension);
            if (!parentObject)
            {
                log.Error("This GOT has no parent object! {0}", yetiObject.NameWithExtension);
            }
            else
            {
                gameObject.transform.SetParent(parentObject.transform, false);
            }

            cYetiObjectReference.AddYetiComponent<cYetiObjectReference>(gameObject, yetiObject);

            foreach (YetiObject subObj in yetiObject.ObjectReferences)
            {
                if (subObj == null)
                    continue;

                //if (subObj.Is<YetiGameObject>())
                //    continue;

                GetConverter(subObj).Convert(subObj, gameObject, context);
            }

            return null;
        }
    }
}
