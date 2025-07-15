using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using System.IO;
using System;
using System.Globalization;
using System.Text;

public class ArchivingTool : EditorWindow
{
    [MenuItem("Tools/存档工具")]
    public static void OpenWindow()
    {
        ArchivingTool window = EditorWindow.GetWindow(typeof(ArchivingTool)) as ArchivingTool;

        if (window == null) return;

        window.Show();
    }

    private const string ArchivingPath = "Assets/Editor/Archiving";
    private const string RealArchivingPath = "Assets/GameAsset/Archiving";
    private Vector2 _scrollViewPosition;
    private string _archivingName = String.Empty;
    private string _screeningTag = String.Empty;
    private static List<FileInfo> _fileInfoList = new List<FileInfo>();

    private static void GetFiles(DirectoryInfo directory, string pattern, ref List<FileInfo> fileList)
    {
        if (directory == null || !directory.Exists || string.IsNullOrEmpty(pattern)) return;

        foreach (FileInfo info in directory.GetFiles(pattern))
        {
            fileList.Add(info);
        }

        foreach (DirectoryInfo info in directory.GetDirectories())
        {
            GetFiles(info, pattern, ref fileList);
        }
    }

    private void OnEnable()
    {
        RefreshUI();
    }

    private void OnGUI()
    {
        EditorGUILayout.BeginHorizontal();
        {
            if (GUILayout.Button("刷新"))
            {
                RefreshUI();
            }

            if (GUILayout.Button("跳转编辑器存档位置"))
            {
                UnityEngine.Object obj = UnityEditor.AssetDatabase.LoadAssetAtPath<UnityEngine.Object>(ArchivingPath);
                AssetDatabase.OpenAsset(obj);
            }

            if (GUILayout.Button("跳转真机存档位置"))
            {
                UnityEngine.Object obj = UnityEditor.AssetDatabase.LoadAssetAtPath<UnityEngine.Object>(RealArchivingPath);
                AssetDatabase.OpenAsset(obj);
            }

            if (GUILayout.Button("清空本地数据"))
            {
                PlayerPrefsUtility.DeleteAllData();
            }
        }
        EditorGUILayout.EndHorizontal();

        GUILayout.Space(10);

        _scrollViewPosition = EditorGUILayout.BeginScrollView(_scrollViewPosition);
        {
            foreach (var fileInfo in _fileInfoList)
            {
                if (!string.IsNullOrEmpty(_screeningTag))
                {
                    if (!fileInfo.Name.Contains(_screeningTag)) continue;
                }

                EditorGUILayout.BeginHorizontal();
                {
                    EditorGUILayout.TextArea(
                        fileInfo.Name.Substring(0, fileInfo.Name.Length - fileInfo.Extension.Length),
                        GUILayout.Height(30));

                    if (GUILayout.Button("Load", GUILayout.Height(30), GUILayout.Width(40)))
                    {
                        LoadArchiving(fileInfo);

                        if (!UnityEditor.EditorApplication.isPlaying)
                        {
                            UnityEditor.EditorApplication.isPlaying = true;
                        }
                    }

                    EditorGUILayout.LabelField($"{fileInfo.LastWriteTime.ToString(CultureInfo.InvariantCulture)}",
                        GUILayout.Height(30), GUILayout.Width(125));
                }
                EditorGUILayout.EndHorizontal();
            }
        }
        EditorGUILayout.EndScrollView();


        EditorGUILayout.BeginHorizontal();
        {
            EditorGUILayout.LabelField("名称:", GUILayout.Height(30), GUILayout.Width(30));
            _archivingName = EditorGUILayout.TextField(_archivingName, GUILayout.Height(30));
            EditorGUILayout.Space();

            if (!string.IsNullOrEmpty(_screeningTag) && string.IsNullOrEmpty(_archivingName))
            {
                _screeningTag = string.Empty;
            }

            if (GUILayout.Button("筛选", GUILayout.Height(30)))
            {
                if (!string.IsNullOrEmpty(_archivingName))
                {
                    _screeningTag = _archivingName;
                }
            }

            if (GUILayout.Button("导入", GUILayout.Height(30)))
            {
                if (!string.IsNullOrEmpty(GUIUtility.systemCopyBuffer))
                {
                    PlayerPrefsUtility.LoadDataJson_Base64(GUIUtility.systemCopyBuffer);
                }
            }

            if (GUILayout.Button("Save", GUILayout.Height(30)))
            {
                if (!string.IsNullOrEmpty(_archivingName))
                {
                    SaveArchiving();
                }
            }
        }
        EditorGUILayout.EndHorizontal();
    }

    private void LoadArchiving(FileInfo fileInfo)
    {
        TextAsset textAsset = AssetDatabase.LoadAssetAtPath<TextAsset>(GetAssetPath(fileInfo.FullName));

        PlayerPrefsUtility.LoadDataJson(textAsset.text);
    }

    private static string GetAssetPath(string fullPath)
    {
        int index = fullPath.IndexOf("Assets", StringComparison.Ordinal);
        return fullPath.Substring(index, fullPath.Length - index);
    }

    private void SaveArchiving()
    {
        string dataJson = PlayerPrefsUtility.GetDataJson();

        File.WriteAllText($"{ArchivingPath}/{_archivingName}.json", dataJson,
            Encoding.UTF8);

        AssetDatabase.Refresh();

        RefreshUI();
    }

    private void RefreshUI()
    {
        _fileInfoList.Clear();
        GetFiles(new DirectoryInfo(ArchivingPath), "*.json",
            ref _fileInfoList);
        _fileInfoList.Sort((a, b) => { return a.LastWriteTime.CompareTo(b.LastWriteTime); });
    }
}