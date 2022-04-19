using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using GRPExplorerLib.BigFile;
using GRPExplorerLib.YetiObjects;
using GRPExplorerLib.Logging;
using System.IO;
using System.Threading;
using UnityIntegration;

namespace UnityIntegrationEditor
{
    public class CurveExplorerWindow : EditorWindow
    {
        static List<YetiCurve> archetypes;
        static YetiCurve arch;
        static string[] options;
        static int selected = 0;
        static string BigFilePath = "";
        static AnimationCurve curve = new AnimationCurve();
        static UnityLogInterface logInterface;

        [MenuItem("GRPExplorer/Curve Explorer")]
        static void ShowWindow()
        {
            GetWindow(typeof(CurveExplorerWindow));
        }

        static void LoadBigFile()
        {
            if (string.IsNullOrEmpty(BigFilePath))
                return;

            if (!File.Exists(BigFilePath))
                return;

            IntegrationUtil.LoadBigFileInBackground
                (BigFilePath,
                (bigFile) =>
                {
                    List<YetiObject> files = bigFile.RootFolder.GetAllObjectsOfArchetype<YetiCurve>();
                    bigFile.FileLoader.LoadAllSimpleNoYield(files);

                    archetypes = new List<YetiCurve>();
                    foreach (YetiObject file in files)
                        archetypes.Add(file.ArchetypeAs<YetiCurve>());

                    options = new string[archetypes.Count];
                    for (int i = 0; i < archetypes.Count; i++)
                        options[i] = files[i].Name;
                });
        }

        static void RefreshKeyframes()
        {
            arch = archetypes[selected];
            if (arch.Keyframes.Length == 0)
            {
                Debug.Log("Curve keyframe count was zero");
                curve.keys = new Keyframe[0];
                return;
            }

            Keyframe[] keyframes = new Keyframe[arch.KeyframeCount];

            for (int i = 0; i < arch.KeyframeCount; i++)
            {
                keyframes[i].time = arch.Keyframes[i].x;
                keyframes[i].value = arch.Keyframes[i].y;
                keyframes[i].inTangent = arch.Keyframes[i].inTangent;
                keyframes[i].outTangent = arch.Keyframes[i].outTangent;
            }

            curve.keys = keyframes;
        }

        void OnGUI()
        {
            if (logInterface == null)
                logInterface = new UnityLogInterface();

            LogManager.LogInterface = logInterface;
            LogManager.GlobalLogFlags = LogFlags.Info | LogFlags.Error;

            EditorGUILayout.BeginVertical();
            BigFilePath = EditorGUILayout.TextField(BigFilePath);
            if (GUILayout.Button("Load"))
                LoadBigFile();
            EditorGUILayout.EndVertical();

            if (archetypes == null)
                return;

            EditorGUILayout.BeginVertical();
            EditorGUILayout.Space();
            EditorGUILayout.Space();
            EditorGUILayout.Space();
            EditorGUILayout.Space();
            int oldSelected = selected;
            selected = EditorGUILayout.Popup(selected, options);
            if (oldSelected != selected)
                RefreshKeyframes();

            if (arch == null)
                return;

            EditorGUILayout.CurveField(curve);

            EditorGUILayout.Space();

            EditorGUILayout.TextField(string.Format("Key: {0:X8}", arch.Object.FileInfo.Key));
            EditorGUILayout.TextField("Curve keyframe count: " + arch.KeyframeCount);
            for (int i = 0; i < arch.KeyframeCount; i++)
            {
                EditorGUILayout.BeginVertical();
                EditorGUILayout.BeginHorizontal();
                EditorGUILayout.TextField(arch.Keyframes[i].x.ToString());
                EditorGUILayout.TextField(arch.Keyframes[i].y.ToString());
                EditorGUILayout.TextField(arch.Keyframes[i].inTangent.ToString());
                EditorGUILayout.TextField(arch.Keyframes[i].outTangent.ToString());
                EditorGUILayout.EndHorizontal();
                EditorGUILayout.TextField(i + " - " + System.Convert.ToString(arch.Keyframes[i].flags, 2).PadLeft(8, '0'));
                EditorGUILayout.Space();
                EditorGUILayout.EndVertical();
            }

            EditorGUILayout.EndVertical();
        }
    }
}