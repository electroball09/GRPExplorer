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
        public static Material matReference;

        public Material Material;

        public void LoadMaterial(YetiMaterial yetiMaterial, List<cYetiTexture> textures, YetiObjectRepository objectRepository)
        {
            if (!matReference)
                matReference = (Material)Resources.Load("BasicMaterial");

            if (objectRepository.GetRepository<Material>().ContainsKey(yetiObject))
            {
                Material = objectRepository.GetRepository<Material>()[yetiObject];
            }
            else
            {
                Material = Instantiate(matReference);

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
