using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using UnityEditor;
using UnityIntegration.Components;

namespace UnityIntegrationEditor.Inspectors
{
    [CustomEditor(typeof(cYetiLayer))]
    public class cYetiLayerInspector : Editor
    {
        public override void OnInspectorGUI()
        {
            cYetiLayer layer = target as cYetiLayer;

            EditorGUILayout.TextField("1:", $"{layer.YetiLayer.NumberThatShouldBeOne}");
            EditorGUILayout.TextField("Name: ", $"{layer.YetiLayer.LayerName}");
            EditorGUILayout.TextField("Flags: ", $"{layer.YetiLayer.Flags01:X8}{layer.YetiLayer.Flags02:X8}{layer.YetiLayer.Flags03:X8}");
            EditorGUILayout.Space(10);
            EditorGUILayout.LabelField($"Is On: {layer.isToggledOn}");
            if (GUILayout.Button("Toggle Layer"))
            {
                layer.ToggleLayer();
            }
        }
    }
}
