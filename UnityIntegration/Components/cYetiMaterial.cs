using GRPExplorerLib.YetiObjects;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace UnityIntegration.Components
{
    public class cYetiMaterial : cYetiObjectReference
    {
        public static Material matReference;

        public Material Material;

        public void LoadMaterial(YetiMaterial yetiMaterial)
        {
            if (!matReference)
                matReference = (Material)Resources.Load("BasicMaterial");

            Material = Instantiate(matReference);
        }

        public void SetTextures(List<cYetiTexture> textures)
        {
            if (textures.Count > 0)
                Material.SetTexture("_MainTex", textures[0].texture);
        }
    }
}
