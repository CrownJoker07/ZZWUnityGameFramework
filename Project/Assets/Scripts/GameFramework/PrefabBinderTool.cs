using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using UnityEngine;

/*
 * 自动绑定工具 版本: V1.0.2，设计思路：
 * 1. 目的为了减少机械的序列化引用对象，加快开发效率
 * 2. 可以手动一键绑定对象
 * 3. 添加对象时，添加_AB后缀可以自动添加绑定对象
 * 4. 删除对象时，会自动检测并删除相关引用
 * 5. 可以引用指定类型，减少获取时使用 GetGetComponent 带来的性能消耗
 * 6. 一键导出代码，包含添加点击事件等常用方法
 * 7. 引用对象带有标记，方便查看引用对象
 * 8. 自动绑定工具只对自己非预制体对象有效，防止影响其他预制体
 * 9. 所有逻辑内聚一个脚本，方便迁移
 * 10. 属性命名简约化，防止与 Key 相同导致无法混淆 - v1.0.1
 */
public class PrefabBinderTool : MonoBehaviour
{
    [Serializable]
    public class BindInfo
    {
        public string key = string.Empty;
        public Component component = null;

#if UNITY_EDITOR
        public int componentTypeIndex = 0;
        private string[] _componentNames;
        private List<Type> _componentTypes;

        public string[] componentTypesNames
        {
            get
            {
                if (_componentNames == null || _componentTypes == null)
                {
                    _componentTypes = new List<Type>();
                    List<string> componentNames = new List<string>();
                    Component[] components = component.GetComponents(typeof(Component));
                    foreach (var component in components)
                    {
                        componentNames.Add(component.GetType().Name);
                        _componentTypes.Add(component.GetType());
                    }

                    _componentNames = componentNames.ToArray();
                }

                return _componentNames;
            }
            set { _componentNames = value; }
        }

        public Type componentType => _componentTypes[componentTypeIndex];

        private string GetName()
        {
            string name = component.name;
            name = name.Replace(PrefabBinderTool_Static.SuffixTag, "");
            name = GetAlphanumeric(name); 
            
            return name;
        }

        public void SetKey()
        {
            key = $"{GetName()}_{component.GetType().Name}";
        }

        public static string GetAlphanumeric(string input)
        {
            // 使用正则表达式匹配字母和数字
            string pattern = "[^a-zA-Z0-9]";
            string replacement = "";
            string result = Regex.Replace(input, pattern, replacement);
            return result;
        }

        public string GetFiledInfoString()
        {
            string fullTypeName = component.GetType().FullName;

            string filedInfoString = $"private {fullTypeName} {GetName()} => prefabBinderTool.GetTarget<{fullTypeName}>(\"{key}\");";

            return filedInfoString;
        }

        public string GetAwakeCodeString()
        {
            string awakeCodeString = string.Empty;

            switch (component)
            {
                case UnityEngine.UI.Button button:
                {
                    awakeCodeString = $"    {GetName()}.onClick.AddListener(OnClick_{GetName()});";
                    break;
                }
            }

            return awakeCodeString;
        }

        public string GetMethodCodeString()
        {
            string methodCodeString = string.Empty;

            switch (component)
            {
                case UnityEngine.UI.Button button:
                {
                    methodCodeString = @$"private void OnClick_{GetName()}()" + "\n" + "{" + "\n" + "}";
                    break;
                }
            }

            return methodCodeString;
        }
#endif
    }

    public List<BindInfo> bindInfos = new List<BindInfo>();

    public T GetTarget<T>(string key) where T : UnityEngine.Object
    {
        Component targetComponent = GetTargetComponent(key);

        if (targetComponent == null) return null;

        if (targetComponent is T temT)
        {
            return temT;
        }
        else if (typeof(T) == typeof(GameObject))
        {
            return (targetComponent.gameObject) as T;
        }
        else if (typeof(T) == typeof(Transform))
        {
            return (targetComponent.transform) as T;
        }

        return targetComponent.gameObject.GetComponent<T>();
    }

    private Component GetTargetComponent(string key)
    {
        foreach (var bindInfo in bindInfos)
        {
            if (bindInfo.key == key)
            {
                return bindInfo.component;
            }
        }

        Debug.LogError($"Can't find GameObject with key: {key}, Name:{transform.name}");

        return null;
    }
}

#if UNITY_EDITOR
[UnityEditor.CustomEditor(typeof(PrefabBinderTool))]
public class PrefabBinderTool_Editor : UnityEditor.Editor
{
    private PrefabBinderTool _prefabBinderTool;

    private void OnEnable()
    {
        _prefabBinderTool = (PrefabBinderTool)target;

        PrefabBinderTool_Static.AddPrefabBinderTool(_prefabBinderTool);

        _prefabBinderTool.CheckKeyIsCorrect();
    }

    private void OnDisable()
    {
        PrefabBinderTool_Static.RemovePrefabBinderTool(_prefabBinderTool);
    }

    public override void OnInspectorGUI()
    {
        DrawAutoBindArea();

        DrawBindInfos();

        GUILayout.Space(10);

        DrawAutoBindButton();

        DrawAllCodeButton();
    }

    private void DrawAutoBindArea()
    {
        GUI.color = Color.green;
        //绘制一个监听区域
        Rect dragArea = GUILayoutUtility.GetRect(0f, 40f, GUILayout.ExpandWidth(true), GUILayout.ExpandHeight(true));
        GUI.Box(dragArea, "Drag Game Object here Can Auto Bind");

        Event currentEvent = Event.current;

        if (!dragArea.Contains(currentEvent.mousePosition)) return;

        switch (currentEvent.type)
        {
            case EventType.DragUpdated:
            {
                //改变鼠标的外表  
                UnityEditor.DragAndDrop.visualMode = UnityEditor.DragAndDropVisualMode.Copy;
                break;
            }
            case EventType.DragPerform:
            {
                UnityEngine.Object[] objects = UnityEditor.DragAndDrop.objectReferences;

                foreach (var temObject in objects)
                {
                    _prefabBinderTool.AddBindInfo(((GameObject)temObject).transform);
                }

                serializedObject.ApplyModifiedProperties();

                UnityEditor.DragAndDrop.AcceptDrag();
                break;
            }
        }
    }

    private void DrawBindInfos()
    {
        GUI.color = Color.white;

        for (var index = 0; index < _prefabBinderTool.bindInfos.Count; index++)
        {
            PrefabBinderTool.BindInfo bindInfo = _prefabBinderTool.bindInfos[index];

            GUILayout.BeginHorizontal("box");
            {
                if (bindInfo.component == null || GUILayout.Button("", "ToggleMixed", GUILayout.ExpandWidth(true),
                        GUILayout.ExpandHeight(true)))
                {
                    UnityEditor.Undo.RecordObject(_prefabBinderTool, "Remove binding info");

                    _prefabBinderTool.bindInfos.RemoveAt(index);
                    index--;
                    continue;
                }

                GUILayout.BeginVertical();
                {
                    GUILayout.Label(bindInfo.key);

                    GUILayout.BeginHorizontal();
                    {
                        Component oldComponent = bindInfo.component;
                        bindInfo.component =
                            UnityEditor.EditorGUILayout.ObjectField(bindInfo.component, typeof(Component), true) as
                                Component;

                        if (oldComponent != bindInfo.component)
                        {
                            bindInfo.componentTypeIndex = 0;
                            bindInfo.componentTypesNames = null;
                        }

                        int oldComponentTypeIndex = bindInfo.componentTypeIndex;
                        bindInfo.componentTypeIndex =
                            UnityEditor.EditorGUILayout.Popup(bindInfo.componentTypeIndex,
                                bindInfo.componentTypesNames);
                        if (oldComponentTypeIndex != bindInfo.componentTypeIndex)
                        {
                            UnityEditor.Undo.RecordObject(_prefabBinderTool, "Change binding info");

                            bindInfo.component = bindInfo.component.gameObject.GetComponent(bindInfo.componentType);
                            bindInfo.SetKey();
                            _prefabBinderTool.CheckKeyIsCorrect();
                        }
                    }
                    GUILayout.EndHorizontal();
                }
                GUILayout.EndVertical();
            }
            GUILayout.EndHorizontal();
        }
    }

    private void DrawAutoBindButton()
    {
        if (GUILayout.Button("Auto Bind"))
        {
            _prefabBinderTool.AutoBind();
        }
    }

    private void DrawAllCodeButton()
    {
        if (GUILayout.Button("All Code"))
        {
            string codeString = string.Empty;

            string temFiledInfoString = "[SerializeField] private PrefabBinderTool prefabBinderTool;";
            temFiledInfoString += "\n";
            foreach (var bindInfo in _prefabBinderTool.bindInfos)
            {
                temFiledInfoString += "\n";
                temFiledInfoString += bindInfo.GetFiledInfoString();
            }

            string temScript = "private void Awake()\n{\n###\n}";

            string temAwakeCodeStrings = string.Empty;
            for (var index = 0; index < _prefabBinderTool.bindInfos.Count; index++)
            {
                PrefabBinderTool.BindInfo bindInfo = _prefabBinderTool.bindInfos[index];
                string awakeCodeString = bindInfo.GetAwakeCodeString();

                if (!string.IsNullOrEmpty(awakeCodeString))
                {
                    if (index != 0)
                    {
                        temAwakeCodeStrings += "\n";
                    }

                    temAwakeCodeStrings += awakeCodeString;
                }
            }

            string temMethodCodeString = string.Empty;
            foreach (var bindInfo in _prefabBinderTool.bindInfos)
            {
                string methodCodeString = bindInfo.GetMethodCodeString();

                if (!string.IsNullOrEmpty(methodCodeString))
                {
                    temMethodCodeString += "\n\n";
                    temMethodCodeString += methodCodeString;
                }
            }

            codeString += temFiledInfoString;

            codeString += "\n";

            if (!string.IsNullOrEmpty(temAwakeCodeStrings))
            {
                codeString += "\n" + temScript.Replace("###", temAwakeCodeStrings);
            }

            codeString += temMethodCodeString;

            GUIUtility.systemCopyBuffer = codeString;
        }
    }
}

public static class PrefabBinderTool_Static
{
    public const string SuffixTag = "_AB";

    public static List<PrefabBinderTool> prefabBinderTools = new List<PrefabBinderTool>();

    public static void AddPrefabBinderTool(PrefabBinderTool prefabBinderTool)
    {
        prefabBinderTools.Add(prefabBinderTool);
    }

    public static void RemovePrefabBinderTool(PrefabBinderTool prefabBinderTool)
    {
        prefabBinderTools.Remove(prefabBinderTool);
    }

    [UnityEditor.InitializeOnLoadMethod]
    private static void Load()
    {
        UnityEditor.EditorApplication.hierarchyWindowItemOnGUI += OnHierarchyWindowItemOnGUI;

        UnityEditor.SceneManagement.PrefabStage.prefabSaved += OnPrefabSaved;
    }

    private static void OnPrefabSaved(GameObject gameObject)
    {
        List<PrefabBinderTool> prefabBinderTools = new List<PrefabBinderTool>();
        gameObject.GetComponentsInChildren(true, prefabBinderTools);

        foreach (var prefabBinderTool in prefabBinderTools)
        {
            if (UnityEditor.PrefabUtility.IsPartOfPrefabInstance(prefabBinderTool)) continue;

            for (var index = 0; index < prefabBinderTool.bindInfos.Count; index++)
            {
                PrefabBinderTool.BindInfo bindInfo = prefabBinderTool.bindInfos[index];

                if (bindInfo.component == null)
                {
                    UnityEditor.Undo.RecordObject(prefabBinderTool, "Remove binding info");
                    prefabBinderTool.bindInfos.RemoveAt(index);

                    index--;
                }
            }

            prefabBinderTool.AutoBind();
        }
    }

    public static void AutoBind(this PrefabBinderTool prefabBinderTool)
    {
        Transform[] transforms = prefabBinderTool.GetComponentsInChildren<Transform>();

        if (transforms == null) return;

        foreach (Transform transform in transforms)
        {
            if (!transform.name.Contains(SuffixTag)) continue;

            if (UnityEditor.PrefabUtility.IsPartOfPrefabInstance(transform) &&
                !UnityEditor.PrefabUtility.IsAnyPrefabInstanceRoot(transform.gameObject)) continue;

            prefabBinderTool.AddBindInfo(transform);
        }

        prefabBinderTool.bindInfos.Sort((a, b) => String.Compare(a.key, b.key, StringComparison.Ordinal));
    }

    public static void AddBindInfo(this PrefabBinderTool prefabBinderTool, Component component)
    {
        if (prefabBinderTool.CheckIsExist(component))
        {
            return;
        }

        PrefabBinderTool.BindInfo bindInfo = new PrefabBinderTool.BindInfo()
        {
            component = component,
        };

        prefabBinderTool.bindInfos.Add(bindInfo);

        bindInfo.SetKey();
        prefabBinderTool.CheckKeyIsCorrect();
    }

    private static bool CheckIsExist(this PrefabBinderTool prefabBinderTool, Component component)
    {
        foreach (var bindInfo in prefabBinderTool.bindInfos)
        {
            if (bindInfo.component == component || bindInfo.component.transform == component)
            {
                return true;
            }
        }

        return false;
    }

    // 检测所有 Key 是否唯一
    public static void CheckKeyIsCorrect(this PrefabBinderTool prefabBinderTool)
    {
        List<string> keys = new List<string>();
        foreach (var bindInfo in prefabBinderTool.bindInfos)
        {
            if (keys.Contains(bindInfo.key))
            {
                UnityEditor.EditorUtility.DisplayDialog("Error", $"Key {bindInfo.key} already exists!", "ok");
                break;
            }

            keys.Add(bindInfo.key);
        }
    }

    private static void OnHierarchyWindowItemOnGUI(int instanceID, Rect selectionRect)
    {
        GameObject gameObject = UnityEditor.EditorUtility.InstanceIDToObject(instanceID) as GameObject;

        if (gameObject == null)
        {
            return;
        }

        foreach (var prefabBinderTool in prefabBinderTools)
        {
            foreach (var bindInfo in prefabBinderTool.bindInfos)
            {
                if (bindInfo.component.gameObject != gameObject) continue;

                Rect rect = new Rect(selectionRect)
                {
                    x = 34,
                    width = 80
                };
                GUIStyle style = new GUIStyle
                {
                    normal =
                    {
                        textColor = Color.yellow
                    },
                    active =
                    {
                        textColor = Color.red
                    }
                };
                GUI.Label(rect, "★", style);
            }
        }
    }
}
#endif