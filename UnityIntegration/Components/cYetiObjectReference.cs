using GRPExplorerLib.BigFile;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace UnityIntegration.Components
{
    public class cYetiObjectReference : MonoBehaviour
    {
        public static T AddYetiComponent<T>(GameObject obj, YetiObject yetiObject) where T : cYetiObjectReference
        {
            T objRef = obj.AddComponent<T>();
            objRef.yetiObject = yetiObject;
            return objRef;
        }

        public YetiObject yetiObject { get; private set; }
    }
}
