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
            transform.SetPositionAndRotation(t.position, t.rotation);
            Vector3 euler = transform.rotation.eulerAngles;
            euler.x += 180f;
            euler.y += 90f;

            transform.rotation = Quaternion.Euler(euler);
        }

        public void SetLightParams(Color color, float intensity)
        {
            light.color = color;
            light.intensity = intensity;
        }
    }
}
