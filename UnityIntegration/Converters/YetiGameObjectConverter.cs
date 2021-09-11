using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using GRPExplorerLib.BigFile;
using GRPExplorerLib.YetiObjects;
using UnityEngine;
using UnityIntegration.Components;
using UnityIntegration.Script;

namespace UnityIntegration.Converters
{
    public class YetiGameObjectConverter : YetiObjectConverter
    {
        public override Type ArchetypeType => typeof(YetiGameObject);

        public override void Convert(YetiObject yetiObject, GameObject parentObject, YetiWorldLoadContext context)
        {
            YetiGameObject obj = yetiObject.ArchetypeAs<YetiGameObject>();

            GameObject gameObject = new GameObject(yetiObject.NameWithExtension);

            GameObject prim = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            prim.transform.parent = gameObject.transform;
            prim.transform.localPosition = Vector3.zero;
            prim.transform.localScale = Vector3.one * 0.3f;
            prim.AddComponent<GAOIdentifier>();

            cYetiObjectReference.AddYetiComponent<cYetiObjectReference>(gameObject, yetiObject);

            var yetiCmp = cYetiObjectReference.AddYetiComponent<cYetiGameObject>(gameObject, yetiObject);
            yetiCmp.SetYetiGameObject(obj);
            yetiCmp.UpdateTransformFromMatrix();

            context.worldObjects.Add(gameObject);
            if (parentObject)
            {
                gameObject.transform.SetParent(parentObject.transform, false);
            }

            if (obj.LightType != YetiLightType.NONE)
            {
                cYetiObjectReference.AddYetiComponent<cYetiLight>(gameObject, yetiObject).SetLight(obj);
            }

            YetiObjectConverter lvm1Conv = null;
            YetiObjectConverter lvm2Conv = null;

            foreach (YetiObject subObj in yetiObject.ObjectReferences)
            {
                YetiObject lvm1 = null;
                YetiObject lvm2 = null;

                if (subObj != null)
                {
                    if (subObj.Name.Contains("LVM"))
                    {
                        if (subObj.Name.Contains("LVMColor"))
                            lvm2 = subObj;
                        else
                            lvm1 = subObj;
                    }
                }

                if (lvm1 != null && lvm1.FileInfo.FileType == YetiObjectType.tga)
                {
                    lvm1Conv = GetConverter(lvm1);
                    lvm1Conv.Convert(lvm1, gameObject, context);
                }

                if (lvm2 != null && lvm2.FileInfo.FileType == YetiObjectType.tga)
                {
                    lvm2Conv = GetConverter(lvm2);
                    lvm2Conv.Convert(lvm2, gameObject, context);
                }
            }

            YetiObjectConverter gotConverter = null;
            foreach (YetiObject subObj in yetiObject.ObjectReferences)
            {
                if (subObj == null)
                    continue;

                if (subObj.Is<YetiGraphicObjectTable>())
                {
                    gotConverter = GetConverter(subObj);
                    (gotConverter as YetiGraphicObjectTableConverter).LVMMap = lvm1Conv?.Components[0] as cYetiTexture;
                    (gotConverter as YetiGraphicObjectTableConverter).LVMColorMap = lvm2Conv?.Components[0] as cYetiTexture;
                    gotConverter.Convert(subObj, gameObject, context);
                }

                if (subObj.Is<YetiLayer>())
                {
                    var layerConv = GetConverter(subObj) as YetiLayerConverter;
                    layerConv.Convert(subObj, gameObject, context);
                    yetiCmp.SetYetiLayer(layerConv.Components[0] as cYetiLayer);
                }

                if (subObj.Is<YetiCubemap>())
                {
                    GetConverter(subObj).Convert(subObj, gameObject, context);
                }
            }
        }
    }

    public class YetiGraphicObjectTableConverter : YetiObjectConverter
    {
        public override Type ArchetypeType => typeof(YetiGraphicObjectTable);

        public cYetiTexture LVMMap;
        public cYetiTexture LVMColorMap;

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
                    kvp.Value.mesh.SetLVMMaps(LVMMap, LVMColorMap);
                }
            }
            else
            {
                throw new Exception(string.Format("mesh count: {0}   mats count: {1}", meshes.Count, mats.Count));
            }
        }
    }
}
