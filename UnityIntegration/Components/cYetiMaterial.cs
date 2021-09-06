using GRPExplorerLib.YetiObjects;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using UnityIntegration.Converters;

namespace UnityIntegration.Components
{
    public class cYetiMaterial : cYetiObjectReference
    {
        public static Material basicMaterial;

        class MatOverride
        {
            public string Name;
            public Material Mat;

            public static implicit operator MatOverride(string str) => new MatOverride() { Name = str };
        }

        static Dictionary<int, MatOverride> customShaderOverrides = new Dictionary<int, MatOverride>()
        {
            { Convert.ToInt32("AD00A2AD", 16), "FootstepCutout" },
            { Convert.ToInt32("AD00F0A4", 16), "TransparentAdditive" },
            { Convert.ToInt32("6B800408", 16), "UnlitTransparent" },
        };

        static void CheckMaterials()
        {
            if (!basicMaterial)
                basicMaterial = (Material)Resources.Load("BasicMaterial");
            foreach (var kvp in customShaderOverrides)
            {
                if (!kvp.Value.Mat)
                {
                    var m = (Material)Resources.Load(kvp.Value.Name);
                    Debug.LogWarning($"precached material {m.name}");
                    kvp.Value.Mat = m;
                }
            }
        }

        public Material Material;

        void OnDestroy()
        {
            DestroyImmediate(Material);
        }

        public void LoadMaterial(YetiMaterial yetiMaterial, List<cYetiTexture> textures, YetiObjectRepository objectRepository)
        {
            CheckMaterials();

            if (objectRepository.GetRepository<Material>().ContainsKey(yetiObject))
            {
                Material = objectRepository.GetRepository<Material>()[yetiObject];
            }
            else
            {
                if (customShaderOverrides.TryGetValue(yetiMaterial.ShaderObject.FileInfo.Key, out var overrideMat))
                    Material = Instantiate(overrideMat.Mat);
                else
                    Material = Instantiate(basicMaterial);

                if (textures.Count > 0)
                    Material.SetTexture("_MainTex", textures[0].texture);

                foreach (var tex in textures)
                    if (tex.yetiObject.Name.Contains("NM"))
                    {
                        
                        Material.SetTexture("_BumpMap", tex.texture);
                        break;
                    }

                objectRepository.GetRepository<Material>().Add(yetiObject, Material);
            }
        }
    }
}
