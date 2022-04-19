﻿using System;
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

            if (LibManager.BigFile == null)
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
            test.currentFilePath = EditorGUILayout.TextField(test.currentFilePath);
            EditorGUILayout.EndHorizontal();
            EditorGUILayout.EndVertical();
        }

        void GUI_Texture(TextureLoaderTest test)
        {
            test.textureType = (GRPExplorerLib.YetiObjects.YetiTextureFormat)EditorGUILayout.EnumPopup(test.textureType);
            EditorGUILayout.BeginHorizontal();
            test.ImportStart = EditorGUILayout.IntField("Import Start:", test.ImportStart);
            test.ImportCount = EditorGUILayout.IntField("Import Count:", test.ImportCount);
            EditorGUILayout.EndHorizontal();
            if (GUILayout.Button("Update"))
            {
                test.ChangeDisplayedTextures();
                return;
            }

            EditorGUILayout.Space();

            test.ImportAs = (TextureFormat)EditorGUILayout.EnumPopup("Import As:", test.ImportAs);
            test.Transparent = EditorGUILayout.Toggle("Transparent?", test.Transparent);
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
