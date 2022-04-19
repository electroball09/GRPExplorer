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
    public class YetiCubemapConverter : YetiObjectConverter
    {
        public override Type ArchetypeType => typeof(YetiCubemap);

        public override void Convert(YetiObject yetiObject, GameObject parentObject, YetiWorldLoadContext context)
        {
            YetiCubemap cube = yetiObject.ArchetypeAs<YetiCubemap>();

            GameObject go = new GameObject(yetiObject.NameWithExtension);
            go.transform.parent = parentObject.transform;

            if (cube.IsMasterCubemap)
            {
                GetConverter(cube.CubemapPC.Object).Convert(cube.CubemapPC.Object, go, context);
                //GetConverter(cube.CubemapX360.Object).Convert(cube.CubemapX360.Object, go, context);
                //GetConverter(cube.CubemapPS3.Object).Convert(cube.CubemapPS3.Object, go, context);
            }
            else
            {
                var cmp = cYetiObjectReference.AddYetiComponent<cYetiCubemap>(go, yetiObject);
                cmp.LoadCubemapFaces(cube);
                cmp.SetMainCubemap();
                Components.Add(cmp);
            }
        }
    }
}
