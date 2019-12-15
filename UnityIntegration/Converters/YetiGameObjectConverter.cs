using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using GRPExplorerLib.BigFile;
using GRPExplorerLib.YetiObjects;
using UnityEngine;

namespace UnityIntegration.Converters
{
    public class YetiGameObjectConverter : YetiObjectConverter
    {
        public override Type ArchetypeType => typeof(YetiGameObject);

        public override void Convert(YetiObject yetiObject, GameObject gameObject)
        {
            
        }
    }
}
