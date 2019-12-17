using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityIntegration.Components;
using UnityEngine;
using UnityEditor;

namespace UnityIntegrationEditor.Inspectors
{
    [CustomEditor(typeof(cYetiObjectReference))]
    public class cYetiObjectReferenceInspector : Editor
    {
        public override void OnInspectorGUI()
        {
            cYetiObjectReference cmp = target as cYetiObjectReference;

            Draw(cmp);
        }

        public static void Draw(cYetiObjectReference cmp)
        {
            EditorGUILayout.TextField("Key:", string.Format("{0:X8}", cmp.yetiObject.FileInfo.Key));
        }
    }
}
