using System;
using System.Collections.Generic;
using System.Reflection;
using UnityEngine;
using Object = UnityEngine.Object;

#if UNITY_EDITOR
public class EditorWindowTool : UnityEditor.EditorWindow
{
    private Vector2 _scrollViewPosition = Vector2.zero; // 记录滚动条位置

    [UnityEditor.MenuItem("Tools/EditorWindowTool", false, 0)]
    public static void OpenCreateNewBuildingWindow()
    {
        GetWindow<EditorWindowTool>(false, "EditorWindowTool", true).Show();
    }

    private UnityEditor.SerializedObject _serializedObject;
    private List<EditorWindowToolBase> _editorWindowToolBases = new List<EditorWindowToolBase>();

    private void OnEnable()
    {
        _serializedObject = new UnityEditor.SerializedObject(this);

        Type[] types = Assembly.GetExecutingAssembly().GetTypes();
        Type targetType = typeof(EditorWindowToolBase);

        foreach (var editorWindowToolBase in _editorWindowToolBases)
        {
            Object.DestroyImmediate(editorWindowToolBase);
        }

        _editorWindowToolBases.Clear();
        foreach (var type in types)
        {
            Type baseType = type.BaseType; //获取基类

            if (baseType == null) continue;

            if (baseType.Name != targetType.Name) continue;

            System.Type temType = System.Type.GetType(type.FullName, true);
            _editorWindowToolBases.Add(ScriptableObject.CreateInstance(temType) as EditorWindowToolBase);
        }

        foreach (var editorWindowToolBase in _editorWindowToolBases)
        {
            editorWindowToolBase.OnEnable(this);
        }
    }

    private void OnGUI()
    {
        _scrollViewPosition = GUILayout.BeginScrollView(_scrollViewPosition);
        foreach (var editorWindowToolBase in _editorWindowToolBases)
        {
            editorWindowToolBase.OnGUI(this);
        }

        GUILayout.EndScrollView();
    }
}

public class EditorWindowToolBase : ScriptableObject
{
    private bool _isFoldOut = false;

    public virtual void OnEnable(UnityEditor.EditorWindow editorWindow)
    {
    }

    public virtual void OnGUI(UnityEditor.EditorWindow editorWindow)
    {
        GUILayout.Space(10);

        _isFoldOut = UnityEditor.EditorGUILayout.Foldout(_isFoldOut, this.GetType().ToString());
        if (_isFoldOut)
        {
            CustomerOnGUI(editorWindow);
        }
    }

    protected virtual void CustomerOnGUI(UnityEditor.EditorWindow editorWindow)
    {
    }
}
#endif