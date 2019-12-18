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
    public class YetiTextureConverter : YetiObjectConverter
    {
        public override Type ArchetypeType => typeof(YetiTextureMetadata);

        public override void Convert(YetiObject yetiObject, GameObject parentObject, YetiWorldLoadContext context)
        {
            GameObject thisObj = new GameObject(yetiObject.NameWithExtension);
            thisObj.transform.SetParent(parentObject.transform, false);

            YetiTextureMetadata mdata = yetiObject.ArchetypeAs<YetiTextureMetadata>();
            while (mdata.Passthrough != null)
            {
                GameObject nextObj = new GameObject(mdata.Object.NameWithExtension);
                nextObj.transform.SetParent(thisObj.transform, false);
                thisObj = nextObj;
                mdata = mdata.Passthrough;
            }

            cYetiTexture tex = cYetiObjectReference.AddYetiComponent<cYetiTexture>(thisObj, mdata.Object);
            tex.LoadTexture();

            Components.Add(tex);
        }
    }
}
