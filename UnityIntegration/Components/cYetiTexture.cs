using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using GRPExplorerLib.YetiObjects;
using UnityEngine;

namespace UnityIntegration.Components
{
    public class cYetiTexture : cYetiObjectReference
    {
        public Texture2D texture;

        public Dictionary<YetiTextureFormat, TextureFormat> formatMap = new Dictionary<YetiTextureFormat, TextureFormat>()
        {
            { YetiTextureFormat.AO, TextureFormat.R8 },
            { YetiTextureFormat.DXT1_1, TextureFormat.DXT1 },
            { YetiTextureFormat.DXT1_2, TextureFormat.DXT1 },
            { YetiTextureFormat.DXT5_1, TextureFormat.DXT5 },
            { YetiTextureFormat.DXT5_2, TextureFormat.DXT5 },
            { YetiTextureFormat.BGRA32, TextureFormat.BGRA32 },
            { YetiTextureFormat.RGBA32, TextureFormat.RGBA32 }
        };

        public void LoadTexture()
        {
            YetiTextureMetadata mdata = yetiObject.ArchetypeAs<YetiTextureMetadata>();
            YetiTexturePayload payload = mdata.Payload;

            TextureFormat format;
            if (!formatMap.ContainsKey(mdata.Format))
            {
                throw new Exception(mdata.Format.ToString() + "\n" + mdata.Object.Name + "\n" + string.Format("{0:X8}", mdata.Object.FileInfo.Key));
            }

            texture = new Texture2D(mdata.Width, mdata.Height, formatMap[mdata.Format], false)
            {
                wrapMode = TextureWrapMode.Clamp
            };
            texture.LoadRawTextureData(payload.Data);
            texture.Apply();
        }
    }
}
