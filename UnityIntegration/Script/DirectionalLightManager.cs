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

        void FixedUpdate()
        {
            Shader.SetGlobalVector("_DirectionalLightDirection", new Vector4(transform.forward.x, transform.forward.y, transform.forward.z, 1));
            Shader.SetGlobalFloat("_DirectionalLightEnabled", light.enabled ? 1f : 0f);
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
            Shader.SetGlobalFloat("_DirectionalLightIntensity", intensity);
            light.intensity = intensity;
        }
    }
}
