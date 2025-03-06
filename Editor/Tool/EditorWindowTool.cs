using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using UnityEngine;

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

        List<Type> types = FindAllTypes(typeof(EditorWindowToolBase));
        Type targetType = typeof(EditorWindowToolBase);

        foreach (var editorWindowToolBase in _editorWindowToolBases)
        {
            DestroyImmediate(editorWindowToolBase);
        }

        _editorWindowToolBases.Clear();
        foreach (var type in types)
        {
            Type baseType = type.BaseType; //获取基类

            if (baseType == null) continue;

            if (baseType.Name != targetType.Name) continue;

            _editorWindowToolBases.Add(ScriptableObject.CreateInstance(type) as EditorWindowToolBase);
        }

        foreach (var editorWindowToolBase in _editorWindowToolBases)
        {
            editorWindowToolBase.OnCustomEnable(this);
        }
    }

    public static List<Type> FindAllTypes(Type baseType)
    {
        List<Type> toolTypes = new List<Type>();
        Assembly[] assemblies = AppDomain.CurrentDomain.GetAssemblies();

        foreach (Assembly assembly in assemblies)
        {
            Type[] types;
            try
            {
                types = assembly.GetTypes();
            }
            catch (ReflectionTypeLoadException ex)
            {
                // 处理类型加载异常，记录可加载的类型
                types = ex.Types.Where(t => t != null).ToArray();
                Debug.LogWarning($"加载程序集 {assembly.FullName} 时部分类型失败: {ex.Message}");
            }
            catch (Exception ex)
            {
                Debug.LogError($"加载程序集 {assembly.FullName} 时出错: {ex}");
                continue;
            }

            foreach (Type type in types)
            {
                try
                {
                    // 筛选非抽象、非接口、且继承自 EditorWindowToolBase 的类
                    if (type != null && !type.IsAbstract && type.IsSubclassOf(baseType))
                    {
                        toolTypes.Add(type);
                    }
                }
                catch (Exception ex)
                {
                    Debug.LogError($"检查类型 {type?.Name} 时出错: {ex}");
                }
            }
        }

        return toolTypes;
    }

    private void OnGUI()
    {
        _scrollViewPosition = GUILayout.BeginScrollView(_scrollViewPosition);
        foreach (var editorWindowToolBase in _editorWindowToolBases)
        {
            editorWindowToolBase.OnFoldOutGUI(this);
        }

        GUILayout.EndScrollView();
    }
}

public class EditorWindowToolBase : ScriptableObject
{
    private bool _isFoldOut = false;

    public virtual void OnCustomEnable(UnityEditor.EditorWindow editorWindow)
    {
    }

    public virtual void OnFoldOutGUI(UnityEditor.EditorWindow editorWindow)
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