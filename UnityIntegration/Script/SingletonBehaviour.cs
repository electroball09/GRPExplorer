using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace UnityIntegration.Script
{
    public class SingletonBehaviour<T> : MonoBehaviour where T : SingletonBehaviour<T>
    {
        public static T inst { get; private set; }

        void Start()
        {
            if (inst)
            {
                DestroyImmediate(this);
                return;
            }

            inst = this as T;
        }
    }
}
