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
    public class YetiMaterialConverter : YetiObjectConverter
    {
        public override Type ArchetypeType => typeof(YetiMaterial);

        public override void Convert(YetiObject yetiObject, GameObject parentObject, YetiWorldLoadContext context)
        {
            YetiMaterial mat = yetiObject.ArchetypeAs<YetiMaterial>();

            GameObject obj = new GameObject(yetiObject.NameWithExtension);
            obj.transform.SetParent(parentObject.transform, false);

            cYetiObjectReference.AddYetiComponent<cYetiObjectReference>(obj, yetiObject);

            YetiObjectConverter conv = null;
            foreach (YetiTextureMetadata tga in mat.TextureRefs)
            {
                if (conv == null) conv = GetConverter(tga.Object);

                conv.Convert(tga.Object, obj, context);
            }

            cYetiMaterial matComponent = cYetiObjectReference.AddYetiComponent<cYetiMaterial>(obj, yetiObject);
            matComponent.LoadMaterial(mat);

            if (conv != null)
            {
                List<cYetiTexture> textures = new List<cYetiTexture>();
                foreach (cYetiObjectReference objRef in conv.Components)
                    textures.Add(objRef as cYetiTexture);
                matComponent.SetTextures(textures);
            }

            Components.Add(matComponent);

            GetConverter(mat.ShaderObject).Convert(mat.ShaderObject, obj, context);
        }
    }
}
