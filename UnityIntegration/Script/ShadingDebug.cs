using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace UnityIntegration.Script
{
    public class ShadingDebug : MonoBehaviour
    {
        public float _BakedAOStrength = 0;
        public float _BakedShadowStrength = 0;
        public float _IndirectStrength = 0;
        public float _DirectStrength = 0;

        public float _LVMColorDebug = 0;

        void OnGUI()
        {
            Rect rect = new Rect(0, Screen.height / 2, 250, 25);
            _BakedAOStrength = DoSlider(ref rect, "Baked AO", "_BakedAOStrength", _BakedAOStrength);
            _BakedShadowStrength = DoSlider(ref rect, "Baked Shadow", "_BakedShadowStrength", _BakedShadowStrength);
            _IndirectStrength = DoSlider(ref rect, "Indirect", "_IndirectStrength", _IndirectStrength);
            _DirectStrength = DoSlider(ref rect, "Direct", "_DirectStrength", _DirectStrength);
            _LVMColorDebug = DoSlider(ref rect, "Debug", "_LVMColorDebug", _LVMColorDebug);
        }

        private float DoSlider(ref Rect rect, string label, string shaderParam, float value)
        {
            float oldX = rect.x;
            Label(ref rect, label);
            float val = GUI.HorizontalSlider(rect, value, 0, 1);
            if (val != value)
                Shader.SetGlobalFloat(shaderParam, val);
            rect.x = oldX;
            rect.y += rect.height;
            return val;
        }

        private void Label(ref Rect rect, string label)
        {
            float old = rect.width;
            rect.width = 150;
            GUI.Label(rect, label);
            rect.x += rect.width;
            rect.width = old;
        }
    }
}
