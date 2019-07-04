using System;
using System.Collections.Generic;
using System.Text;
using UnityIntegration;
using UnityEngine;
using UnityEditor;
using GRPExplorerLib.Util;

namespace UnityIntegrationEditor
{
    [CustomEditor(typeof(TextureLoaderTest))]
    public class TextureLoaderTestEditor : Editor
    {
        public override void OnInspectorGUI()
        {
            TextureLoaderTest test = target as TextureLoaderTest;

            if (!Application.isPlaying)
            {
                EditorGUILayout.LabelField("Please enter play mode first!");
                return;
            }

            if (test.m_bigFile == null)
            {
                GUI_Load(test);
            }
            else
            {
                GUI_Texture(test);
            }
        }

        void GUI_Load(TextureLoaderTest test)
        {
            EditorGUILayout.BeginVertical();
            EditorGUILayout.BeginHorizontal();
            if (GUILayout.Button("Load", GUILayout.Width(75)))
            {
                test.LoadBigFile();
            }
            test.currentFilePath = EditorGUILayout.TextField(test.currentFilePath);
            EditorGUILayout.EndHorizontal();
            test.currentTextureType = EditorGUILayout.TextField(test.currentTextureType);
            EditorGUILayout.EndVertical();
        }

        void GUI_Texture(TextureLoaderTest test)
        {
            EditorGUILayout.BeginHorizontal();
            if (GUILayout.Button("Load", GUILayout.Width(75)))
            {
                test.LoadTextureFile();
            }
            test.sel = EditorGUILayout.Popup(test.sel, test.fileNames.GetInternalArray());
            EditorGUILayout.EndHorizontal();
        }
    }
}
