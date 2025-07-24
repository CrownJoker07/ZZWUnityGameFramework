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
    private string _yearInput = String.Empty;
    private string _monthInput = String.Empty;
    private string _dayInput = String.Empty;
    private string _hourInput = String.Empty;
    private string _minuteInput = String.Empty;
    private string _secondInput = String.Empty;
    private bool _isRefresh;
    public static event Action<DateTime> OnJumpTimeAction;
    public static Func<DateTime> GetNowTimeFunc;

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

        EditorGUILayout.BeginHorizontal();
        {
            float height = 20;
            float width = 20;
            float labelWidth = 17;

            _isRefresh = GUILayout.Toggle(_isRefresh, "刷新", GUILayout.Height(height));
            if (_isRefresh)
            {
                DateTime dateTime = GetNowTimeFunc?.Invoke() ?? DateTime.Now;

                _yearInput = dateTime.Year.ToString();
                _monthInput = dateTime.Month.ToString();
                _dayInput = dateTime.Day.ToString();
                _hourInput = dateTime.Hour.ToString();
                _minuteInput = dateTime.Minute.ToString();
                _secondInput = dateTime.Second.ToString();
            }

            // 年输入框
            EditorGUILayout.LabelField("年:", GUILayout.Width(labelWidth), GUILayout.Height(height));
            _yearInput = EditorGUILayout.TextField(_yearInput, GUILayout.Width(40), GUILayout.Height(height));

            // 月输入框
            EditorGUILayout.LabelField("月:", GUILayout.Width(labelWidth), GUILayout.Height(height));
            _monthInput = EditorGUILayout.TextField(_monthInput, GUILayout.Width(width), GUILayout.Height(height));

            // 日输入框
            EditorGUILayout.LabelField("日:", GUILayout.Width(labelWidth), GUILayout.Height(height));
            _dayInput = EditorGUILayout.TextField(_dayInput, GUILayout.Width(width), GUILayout.Height(height));

            // 时输入框
            EditorGUILayout.LabelField("时:", GUILayout.Width(labelWidth), GUILayout.Height(height));
            _hourInput = EditorGUILayout.TextField(_hourInput, GUILayout.Width(width), GUILayout.Height(height));

            // 分输入框
            EditorGUILayout.LabelField("分:", GUILayout.Width(labelWidth), GUILayout.Height(height));
            _minuteInput = EditorGUILayout.TextField(_minuteInput, GUILayout.Width(width), GUILayout.Height(height));

            // 秒输入框
            EditorGUILayout.LabelField("秒:", GUILayout.Width(labelWidth), GUILayout.Height(height));
            _secondInput = EditorGUILayout.TextField(_secondInput, GUILayout.Width(width), GUILayout.Height(height));

            if (GUILayout.Button("跳转", GUILayout.Height(height)))
            {
                int year = int.Parse(_yearInput);
                int month = int.Parse(_monthInput);
                int day = int.Parse(_dayInput);
                int hour = int.Parse(_hourInput);
                int minute = int.Parse(_minuteInput);
                int second = int.Parse(_secondInput);

                DateTime targetTime = new DateTime(year, month, day, hour, minute, second);

                OnJumpTimeAction?.Invoke(targetTime);
            }
            if (GUILayout.Button("重置", GUILayout.Height(height)))
            {
                DateTime nowTime = DateTime.Now;

                _yearInput = nowTime.Year.ToString();
                _monthInput = nowTime.Month.ToString();
                _dayInput = nowTime.Day.ToString();
                _hourInput = nowTime.Hour.ToString();
                _minuteInput = nowTime.Minute.ToString();
                _secondInput = nowTime.Second.ToString();

                OnJumpTimeAction?.Invoke(nowTime);
            }

            if (GUILayout.Button("跨天", GUILayout.Height(height)))
            {
                int year = int.Parse(_yearInput);
                int month = int.Parse(_monthInput);
                int day = int.Parse(_dayInput);
                int hour = 0;
                int minute = 0;
                int second = 0;

                DateTime dateTime = new DateTime(year, month, day, hour, minute, second);
                dateTime = dateTime.AddDays(1);

                _yearInput = dateTime.Year.ToString();
                _monthInput = dateTime.Month.ToString();
                _dayInput = dateTime.Day.ToString();
                _hourInput = dateTime.Hour.ToString();
                _minuteInput = dateTime.Minute.ToString();
                _secondInput = dateTime.Second.ToString();

                OnJumpTimeAction?.Invoke(dateTime);
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