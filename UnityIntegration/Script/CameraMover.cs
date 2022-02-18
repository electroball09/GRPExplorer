using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using UnityIntegration.Components;

namespace UnityIntegration.Script
{
    public class CameraMover : SingletonBehaviour<CameraMover>
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
        public float InterpToSpeed = 25f;
        public float InterpToDist = 2.5f;

        Coroutine interpCoroutine;

        void Start()
        {
            GetComponent<Camera>().depthTextureMode = DepthTextureMode.DepthNormals;
        }

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

        void DebugShaderToggle(Rect rect, string text, string keyword)
        {
            if (Shader.IsKeywordEnabled(keyword))
            {
                if (GUI.Button(rect, $">{text}"))
                    Shader.DisableKeyword(keyword);
            }
            else
            {
                if (GUI.Button(rect, text))
                    Shader.EnableKeyword(keyword);
            }
        }

        void DebugShaderValue(Rect rect, string text, string keyword)
        {
            if (Shader.GetGlobalFloat(keyword) == 0)
            {
                if (GUI.Button(rect, text))
                    Shader.SetGlobalFloat(keyword, 1f);
            }
            else
            {
                if (GUI.Button(rect, $">{text}"))
                    Shader.SetGlobalFloat(keyword, 0);
            }
        }

        int debugCoord = 0;
        void OnGUI()
        {
            Rect rect = new Rect(Screen.width / 2 - 200, Screen.height - 25, 250, 25);
            string str = string.Format("Speed (scroll): {0}   Slow (shift): {1}", speeds[speedIndex], isSlow);
            GUI.Label(rect, str);
            rect.y -= rect.height;
            rect.width = rect.width / 2;
            Rect rect2 = new Rect(rect);
            rect.y -= rect2.height;
            string newCoord = GUI.TextField(rect2, debugCoord.ToString());
            int.TryParse(newCoord, out debugCoord);
            debugCoord = Mathf.Clamp(debugCoord, 0, 4);
            rect2.x += rect2.width;
            if (GUI.Button(rect2, "Tgl Shadow Mode"))
                cYetiMesh.SwitchShadowMode();
            if (GUI.Button(rect, "GAO idents Tgl"))
                GAOIdentifier.ToggleIdentifiersVisible();
            rect.x += rect.width;
            DebugShaderValue(rect, "UV Debug", $"_DEBUG_UV{debugCoord}");
            rect.x += rect.width;
            DebugShaderToggle(rect, "Debug", "_DEBUG_VIEW");
            rect.x += rect.width;
            DebugShaderValue(rect, "Normal Debug", "_DEBUG_NORMAL");
            rect.x -= rect.width * 3;
            rect.y -= rect.height;
            DebugShaderToggle(rect, "LVM Debug", "_ENABLE_LVM_DEBUG");
            rect.x += rect.width;
            if (GUI.Button(rect, "Toggle lights"))
                cYetiLight.ToggleAllLights();
            rect.x += rect.width;
            DebugShaderValue(rect, "VC Debug", "_DEBUG_VERTEX_COLOR");
        }

        IEnumerator InterpCoroutine(GameObject obj)
        {
            float dist = (transform.position - obj.transform.position).magnitude;
            Vector3 initialDir = -transform.forward;

            Vector3 targetPos = obj.transform.position + initialDir * InterpToDist;

            while ((transform.position - targetPos).magnitude > 0.01f)
            {
                transform.position = Vector3.Lerp(transform.position, targetPos, Time.deltaTime * InterpToSpeed);
                yield return null;
            }

            interpCoroutine = null;
        }

        public void InterpToObject(GameObject obj)
        {
            if (interpCoroutine == null)
                interpCoroutine = StartCoroutine(InterpCoroutine(obj));
        }
    }
}
