using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using GRPExplorerLib.BigFile;
using GRPExplorerLib.YetiObjects;
using UnityEngine;

namespace UnityIntegration.Components
{
    public class cYetiLight : cYetiObjectReference
    {
        delegate void LightsToggled(bool toggledOn);
        static event LightsToggled OnLightsToggled;
        public static bool isToggledOn { get; private set; } = true;

        Light unityLight;

        void Start()
        {
            OnLightsToggled += CYetiLight_OnLightsToggled;
        }

        private void CYetiLight_OnLightsToggled(bool toggledOn)
        {
            unityLight.enabled = toggledOn;
        }

        void OnDestroy()
        {
            OnLightsToggled -= CYetiLight_OnLightsToggled;
        }

        public static void ToggleAllLights()
        {
            isToggledOn = !isToggledOn;
            OnLightsToggled?.Invoke(isToggledOn);
        }

        public void SetLight(YetiGameObject obj)
        {
            unityLight = gameObject.AddComponent<Light>();

            if (obj.LightType == YetiLightType.Point)
                SetPointLight(obj, unityLight);
            else if (obj.LightType == YetiLightType.Spot)
                SetSpotLight(obj, unityLight);
            else if (obj.LightType == YetiLightType.Directional)
                SetDirectionalLight(obj);
            else
                Debug.LogError($"Invalid light type {(byte)obj.LightType:X2} on object {yetiObject.NameWithExtension} - {yetiObject.FileInfo.Key:X8}");
        }

        private void SetSpotLight(YetiGameObject obj, Light light)
        {
            light.type = LightType.Spot;
            light.color = new Color(obj.r / 255f, obj.g / 255f, obj.b / 255f, obj.a / 25f);
            light.range = obj.radius;
            light.spotAngle = Mathf.Max(obj.radius_max, obj.radius_extra);
            light.innerSpotAngle = Mathf.Min(obj.radius_max, obj.radius_extra);
            light.intensity = obj.intensity * 2;
        }

        private void SetPointLight(YetiGameObject obj, Light light)
        {
            light.type = LightType.Point;
            light.color = new Color(obj.r / 255f, obj.g / 255f, obj.b / 255f, obj.a / 25f);
            light.range = Mathf.Max(obj.radius, obj.radius_max);
            light.intensity = obj.intensity * 2;
        }

        private void SetDirectionalLight(YetiGameObject obj)
        {
            DirectionalLightManager.inst.MatchTransform(transform);
            DirectionalLightManager.inst.SetLightParams(new Color(obj.r / 255f, obj.g / 255f, obj.b / 255f, obj.a / 25f), obj.intensity);
        }
    }
}
