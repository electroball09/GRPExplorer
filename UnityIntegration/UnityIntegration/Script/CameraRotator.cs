using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace UnityIntegration.Script
{
    public class CameraRotator : MonoBehaviour
    {
        [Header("config")]
        public bool InvertPitch = false;
        public float sensitivity = 1f;
        public float sensMultYaw = 1f;
        public float sensMultPitch = 1f;

        [Header("debug")]
        public float pitch;
        public float yaw;
        public bool active = false;

        void Start()
        {
            Vector3 rot = transform.rotation.eulerAngles;
            pitch = rot.x;
            yaw = rot.y;
        }

        private void ToggleCursor()
        {
            Cursor.visible = !Cursor.visible;
            active = !Cursor.visible;
            Cursor.lockState = Cursor.visible ? CursorLockMode.None : CursorLockMode.Locked;
        }

        void Update()
        {
            if (Input.GetKeyDown(KeyCode.Z))
            {
                ToggleCursor();
            }
        }

        void LateUpdate()
        {
            if (!active)
                return;

            pitch += Input.GetAxis("Mouse Y") * sensitivity * sensMultPitch * (InvertPitch ? 1f : -1f);
            pitch = Mathf.Clamp(pitch, -89.9f, 89.9f);
            yaw += Input.GetAxis("Mouse X") * sensitivity * sensMultYaw;

            transform.rotation = Quaternion.Euler(pitch, yaw, 0);
        }

        void OnGUI()
        {
            Rect rect = new Rect(0f, Screen.height - 25f, 300f, 25f);
            GUI.Label(rect, (active ? "Controls Active " : "Controls Inactive ") + "(Press Z To Toggle)");
        }
    }
}
