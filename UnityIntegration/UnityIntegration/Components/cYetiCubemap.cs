using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using GRPExplorerLib.YetiObjects;
using UnityEngine;
using UnityIntegration.Converters;

namespace UnityIntegration.Components
{
    public class cYetiCubemap : cYetiObjectReference
    {
        public Texture2D[] Textures;

        public Cubemap cubemap;

        public void LoadCubemapFaces(YetiCubemap _cubemap)
        {
            Textures = new Texture2D[6];

            for (int i = 0; i < 6; i++)
            {
                Textures[i] = new Texture2D(_cubemap.Cubemap[i].Width, _cubemap.Cubemap[i].Width, TextureFormat.RGBAHalf, false);
                Textures[i].LoadRawTextureData(_cubemap.Cubemap[i].Data);
                Textures[i].Apply();
            }

            cubemap = new Cubemap(_cubemap.Cubemap[0].Width, TextureFormat.RGBAHalf, true);
            cubemap.SetPixelData(_cubemap.Cubemap[2].Data, 0, CubemapFace.PositiveZ);
            cubemap.SetPixelData(_cubemap.Cubemap[3].Data, 0, CubemapFace.NegativeZ);
            cubemap.SetPixelData(_cubemap.Cubemap[0].Data, 0, CubemapFace.PositiveX);
            cubemap.SetPixelData(_cubemap.Cubemap[1].Data, 0, CubemapFace.NegativeX);
            cubemap.SetPixelData(_cubemap.Cubemap[4].Data, 0, CubemapFace.PositiveY);
            cubemap.SetPixelData(_cubemap.Cubemap[5].Data, 0, CubemapFace.NegativeY);
            cubemap.Apply();
        }

        public void SetMainCubemap()
        {
            Shader.SetGlobalTexture("_Cubemap", cubemap);
        }
    }
}
