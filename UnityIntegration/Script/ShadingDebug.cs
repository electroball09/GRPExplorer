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
        void OnGUI()
        {
            Rect rect = new Rect(0, (Screen.height / 2) - 150, 175, 25);
            DoSlider(ref rect, "Baked AO", "_BakedAOStrength");
            DoSlider(ref rect, "Baked Shadow", "_BakedShadowStrength");
            DoSlider(ref rect, "Indirect", "_IndirectStrength");
            DoSlider(ref rect, "Direct", "_DirectStrength");
            DoSlider(ref rect, "LVM_R", "_LVM_R");
            DoSlider(ref rect, "LVM_G", "_LVM_G");
            DoSlider(ref rect, "LVM_B", "_LVM_B");
            DoSlider(ref rect, "LVM_A", "_LVM_A");
        }

        private void DoSlider(ref Rect rect, string label, string shaderParam)
        {
            float oldX = rect.x;
            Label(ref rect, label);
            float value = Shader.GetGlobalFloat(shaderParam);
            float val = GUI.HorizontalSlider(rect, value, 0, 1);
            if (val != value)
                Shader.SetGlobalFloat(shaderParam, val);
            rect.x = oldX;
            rect.y += rect.height;
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
