using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using GRPExplorerLib.BigFile;
using GRPExplorerLib.BigFile.Files.Archetypes;
using GRPExplorerLib.Logging;
using System.IO;

public class CurveExplorerWindow : EditorWindow
{
    static List<CurveFileArchetype> archetypes;
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

        PackedBigFile bigFile = new PackedBigFile(new FileInfo(BigFilePath));
        bigFile.LoadFromDisk();

        List<BigFileFile> files = bigFile.RootFolder.GetAllFilesOfArchetype<CurveFileArchetype>();
        bigFile.FileLoader.LoadFiles(files);

        archetypes = new List<CurveFileArchetype>();
        foreach (BigFileFile file in files)
            archetypes.Add(file.ArchetypeAs<CurveFileArchetype>());

        options = new string[archetypes.Count];
        for (int i = 0; i < archetypes.Count; i++)
            options[i] = files[i].Name;
    }

    static void RefreshKeyframes()
    {
        CurveFileArchetype arch = archetypes[selected];
        Keyframe[] keyframes = new Keyframe[arch.Keyframes.Length - 1];

        for (int i = 0; i < arch.Keyframes.Length - 1; i++)
        {
            keyframes[i].time = arch.Keyframes[i].x;
            keyframes[i].value = arch.Keyframes[i].y;
            keyframes[i].inTangent = arch.Keyframes[i].@in;
            keyframes[i].outTangent = arch.Keyframes[i].@out;
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

        EditorGUILayout.CurveField(curve);

        EditorGUILayout.EndVertical();
    }
}
