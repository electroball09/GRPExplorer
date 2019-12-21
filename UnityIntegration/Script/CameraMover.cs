using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace UnityIntegration.Script
{
    public class CameraMover : MonoBehaviour
    {
        [Header("config")]
        public float slowPercent = 0.2f;
        public int speedIndex = 5;
        public float[] speeds = new float[]
        {
            1f,
            2.5f,
            5f,
            10f,
            25f,
            50f,
            100f
        };
        public bool isSlow = false;

        void Update()
        {
            Vector2 scroll = Input.mouseScrollDelta;
            if (scroll.y > 0)
                speedIndex = Mathf.Clamp(speedIndex + 1, 0, speeds.Length - 1);
            if (scroll.y < 0)
                speedIndex = Mathf.Clamp(speedIndex - 1, 0, speeds.Length - 1);

            Vector3 dir = Vector3.zero;
            dir.x += Input.GetKey(KeyCode.D) ? 1 : 0;
            dir.x += Input.GetKey(KeyCode.A) ? -1 : 0;
            dir.z += Input.GetKey(KeyCode.W) ? 1 : 0;
            dir.z += Input.GetKey(KeyCode.S) ? -1 : 0;
            dir.y += Input.GetKey(KeyCode.E) ? 1 : 0;
            dir.y += Input.GetKey(KeyCode.Q) ? -1 : 0;
            dir.Normalize();

            isSlow = Input.GetKey(KeyCode.LeftShift);

            dir *= speeds[speedIndex];
            if (isSlow)
                dir *= slowPercent;

            dir *= Time.deltaTime;

            dir = transform.rotation * dir;

            transform.position += dir;
        }

        void OnGUI()
        {
            Rect rect = new Rect(Screen.width / 2 - 125, Screen.height - 25, 250, 25);
            string str = string.Format("Speed: {0}   Slow: {1}", speeds[speedIndex], isSlow);
            GUI.Label(rect, str);
            rect.y -= rect.height;
            if (GUI.Button(rect, "Toggle UV Debug"))
            {
                if (Shader.IsKeywordEnabled("_UV_DEBUG"))
                    Shader.DisableKeyword("_UV_DEBUG");
                else
                    Shader.EnableKeyword("_UV_DEBUG");
            }
        }
    }
}
