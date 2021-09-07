using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace UnityIntegration
{
    public class DirectionalLightManager : MonoBehaviour
    {
        public static DirectionalLightManager inst { get; private set; }

        new Light light;

        void Start()
        {
            if (inst)
            {
                Debug.LogWarning("wtf two directional lights");
            }

            inst = this;
            light = GetComponent<Light>();
        }

        public void MatchTransform(Transform t)
        {
            Debug.Log("dir light matching transform");
            transform.SetPositionAndRotation(t.position, t.rotation * Quaternion.Euler(0, 180f, 90f));
        }

        public void SetLightParams(Color color, float intensity)
        {
            light.color = color;
            Shader.SetGlobalColor("_DirectionalLightColor", color);
            light.intensity = intensity;
        }
    }
}
