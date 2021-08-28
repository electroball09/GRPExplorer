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

        public override void Convert(YetiObject yetiObject, GameObject parentObject, YetiWorldLoadContext context)
        {
            YetiGameObject obj = yetiObject.ArchetypeAs<YetiGameObject>();

            GameObject gameObject = new GameObject(yetiObject.NameWithExtension);

            var yetiCmp = cYetiObjectReference.AddYetiComponent<cYetiGameObject>(gameObject, yetiObject);
            yetiCmp.SetYetiGameObject(obj);
            yetiCmp.UpdateTransformFromMatrix();

            //Matrix4x4 matrix = obj.Matrix.ToUnity();

            //Vector3 pos;
            //Quaternion rot;
            //Vector3 scale;
            //IntegrationUtil.DecomposeMatrix(ref matrix, out pos, out rot, out scale);

            //rot = rot.ConvertYetiToUnityRotation();
            //pos = pos.ConvertYetiToUnityCoords();

            context.worldObjects.Add(gameObject);
            if (parentObject)
            {
                gameObject.transform.SetParent(parentObject.transform, false);
            }

            if (yetiObject.Name.ToLower().Contains("light"))
            {
                var l = gameObject.AddComponent<Light>();
                l.type = LightType.Point;
                l.range = 7.5f;
                l.intensity = 1f;
            }

            cYetiObjectReference.AddYetiComponent<cYetiObjectReference>(gameObject, yetiObject);

            foreach (YetiObject subObj in yetiObject.ObjectReferences)
            {
                if (subObj == null)
                    continue;

                if (subObj.Is<YetiGraphicObjectTable>())
                    GetConverter(subObj).Convert(subObj, gameObject, context);
            }
        }
    }

    public class YetiGraphicObjectTableConverter : YetiObjectConverter
    {
        public override Type ArchetypeType => typeof(YetiGraphicObjectTable);

        public override void Convert(YetiObject yetiObject, GameObject parentObject, YetiWorldLoadContext context)
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

            Dictionary<YetiObject, cYetiMaterial> mats = new Dictionary<YetiObject, cYetiMaterial>();
            Dictionary<YetiObject, (cYetiMesh mesh, List<cYetiMaterial> materials)> meshes = new Dictionary<YetiObject, (cYetiMesh mesh, List<cYetiMaterial> materials)>();
            YetiObject currMesh = null;
            foreach (YetiObject subObj in yetiObject.ObjectReferences)
            {
                if (subObj == null)
                    continue;

                if (subObj.Is<YetiMeshMetadata>())
                {
                    currMesh = subObj;

                    YetiObjectConverter conv = GetConverter(subObj);
                    conv.Convert(subObj, gameObject, context);

                    meshes.Add(currMesh, (conv.Components[0] as cYetiMesh, new List<cYetiMaterial>()));
                }
                else if (subObj.Is<YetiMaterial>())
                {
                    cYetiMaterial mat;
                    if (!mats.ContainsKey(subObj))
                    {
                        YetiObjectConverter conv = GetConverter(subObj);
                        conv.Convert(subObj, gameObject, context);
                        mat = conv.Components[0] as cYetiMaterial;
                        mats.Add(subObj, mat);
                    }
                    else
                    {
                        mat = mats[subObj];
                    }

                    if (currMesh != null)
                    {
                        meshes[currMesh].materials.Add(mat);
                    }
                }
                else
                {
                    GetConverter(subObj).Convert(subObj, gameObject, context);
                }
            }

            if (meshes.Count > 0 && mats.Count > 0)
            {
                foreach (KeyValuePair<YetiObject, (cYetiMesh mesh, List<cYetiMaterial> materials)> kvp in meshes)
                {
                    kvp.Value.mesh.SetMaterials(kvp.Value.materials);
                }
            }
            else
            {
                throw new Exception(string.Format("mesh count: {0}   mats count: {1}", meshes.Count, mats.Count));
            }
        }
    }
}
