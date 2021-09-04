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
    [CustomEditor(typeof(cYetiGameObject))]
    public class Matrix4x4Inspector : Editor
    {
        public override void OnInspectorGUI()
        {
            cYetiGameObject obj = target as cYetiGameObject;

            obj.TransformFromMatrix = EditorGUILayout.Toggle("Transform from matrix", obj.TransformFromMatrix);
            obj.UseYetiMatrix = EditorGUILayout.Toggle("Use Yeti Matrix", obj.UseYetiMatrix);
            obj.ConvertPosition = EditorGUILayout.Toggle("Convert Position", obj.ConvertPosition);
            obj.ConvertRotation = EditorGUILayout.Toggle("Convert Rotation", obj.ConvertRotation);

            DrawMatrix("Yeti", ref obj.YetiMatrix);

            EditorGUILayout.Space(20);

            DrawMatrix("Unity", ref obj.ConvertedMatrix);

            if (obj.yetiObject != null)
                obj.UpdateTransformFromMatrix();
        }

        public static void DrawMatrix(string name, ref Matrix4x4 matrix)
        {
            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.LabelField($"{name}: ");
            EditorGUILayout.BeginVertical();
            for (int i = 0; i < 4; i++)
            {
                EditorGUILayout.BeginHorizontal();
                for (int j = 0; j < 4; j++)
                {
                    matrix[i, j] = EditorGUILayout.FloatField(matrix[i, j]);
                }
                EditorGUILayout.EndHorizontal();
            }
            EditorGUILayout.EndVertical();
            EditorGUILayout.EndHorizontal();
        }
    }
}
