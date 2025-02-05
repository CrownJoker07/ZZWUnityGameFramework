using System;
using System.Collections.Generic;
using UnityEngine;

/*
 * 自动绑定工具 版本: V2.1.0，设计思路：
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
 * 11. 分离各种代码模板逻辑按钮，使其更自由复制 - v1.0.3
 * 12. 绑定逻辑改为当前脚本顶部自动生成属性进行绑定，避免手动复制和操作，即绑即用 - v2.1.0
 */
public class PrefabBinderTool : MonoBehaviour
{
    public static bool IsShowInspector = true;
    
    [Serializable]
    public class BindInfo
    {
        public string key = string.Empty;
        public Component component = null;
        public string fieldInfoName = string.Empty;
        
#if UNITY_EDITOR
        [NonSerialized] public int componentTypeIndex = 0;
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

                    for (var index = 0; index < _componentTypes.Count; index++)
                    {
                        Type componentType = _componentTypes[index];

                        if (componentType != component.GetType()) continue;
                        
                        componentTypeIndex = index;
                        break;
                    }
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

        public string GetFieldInfoName()
        {
            if (string.IsNullOrEmpty(fieldInfoName))
            {
                fieldInfoName = GetName() + "_" + component.GetType().Name;
            }
            
            return fieldInfoName;
        }

        public void SetKey()
        {
            fieldInfoName = string.Empty;

            fieldInfoName = GetFieldInfoName();
            
            key = GetLocalIdentfierInFile(component).ToString();
        }

        private static long GetLocalIdentfierInFile(UnityEngine.Object obj)
        {
            System.Reflection.PropertyInfo info = typeof(UnityEditor.SerializedObject).GetProperty("inspectorMode",
                System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            UnityEditor.SerializedObject sObj = new UnityEditor.SerializedObject(obj);
            info.SetValue(sObj, UnityEditor.InspectorMode.Debug, null);
            UnityEditor.SerializedProperty localIdProp = sObj.FindProperty("m_LocalIdentfierInFile");
            return localIdProp.longValue;
        }
        
        public void Refresh()
        {
            _componentNames = null;
        }

        public static string GetAlphanumeric(string input)
        {
            // 使用正则表达式匹配字母和数字
            string pattern = "[^a-zA-Z0-9]";
            string replacement = "";
            string result = System.Text.RegularExpressions.Regex.Replace(input, pattern, replacement);
            return result;
        }

        public string GetFiledInfoString()
        {
            string fullTypeName = component.GetType().FullName;

            string filedInfoString =
                $"        private {fullTypeName} {GetFieldInfoName()} =>\n            PrefabBinderTool.GetTarget<{fullTypeName}>(\"{key}\");";

            return filedInfoString;
        }
#endif
    }

    // 目标脚本
    public Component targetComponent = null;
    
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
        if (!PrefabBinderTool.IsShowInspector)
        {
            base.OnInspectorGUI();
            return;
        }

        DrawTargetComponent();

        GUILayout.BeginHorizontal();
        {
            DrawCreateCodeButton();
        }
        GUILayout.EndHorizontal();
        
        GUILayout.Space(10);
        
        DrawAutoBindArea();

        DrawBindInfos();

        GUILayout.Space(10);

        DrawRefreshButton();
    }

    private void DrawTargetComponent()
    {
        GUILayout.BeginHorizontal();
        {
            GUILayout.Label("TargetComponent:");

            Component oldComponent = _prefabBinderTool.targetComponent;
            
            _prefabBinderTool.targetComponent =
                UnityEditor.EditorGUILayout.ObjectField(_prefabBinderTool.targetComponent, typeof(Component), true) as
                    Component;

            if (oldComponent != _prefabBinderTool.targetComponent)
            {
                UnityEditor.EditorUtility.SetDirty(_prefabBinderTool.gameObject);
            }
        }
        GUILayout.EndHorizontal();
    }

    private void DrawCreateCodeButton()
    {
        if (GUILayout.Button("CreateCode"))
        {
            CreateCode();
        }
    }

    private void CreateCode()
    {
        if (_prefabBinderTool.targetComponent == null)
        {
            return;
        }
        
        // 获取目标脚本的类名和命名空间
        string fullName = _prefabBinderTool.targetComponent.GetType().FullName;
        
        if (string.IsNullOrEmpty(fullName))
        {
            return;
        }
        
        // 获取类型
        Type scriptType = Type.GetType(fullName);

        if (scriptType == null)
        {
            return;
        }

        // 获取类名
        string className = scriptType.Name;

        // 获取命名空间
        string namespaceName = scriptType.Namespace;

        string autoBindTag = "// AutoBindFieldInfo";
        
        string tempCodeString = $@"
        {autoBindTag}
        {GetAllCodeString()}
        {autoBindTag}
";
        
        string scriptPath = GetScriptPath(_prefabBinderTool.targetComponent as MonoBehaviour);
        
        // 先从 ScriptPath 读取所有文本
        string scriptText = System.IO.File.ReadAllText(scriptPath);
        
        // 从scriptText 找到被// AutoBindFieldInfo包住的文本
        int startIndex = scriptText.IndexOf(autoBindTag, StringComparison.Ordinal);
        int endIndex = scriptText.IndexOf(autoBindTag, startIndex + 1, StringComparison.Ordinal);
        endIndex += autoBindTag.Length;
        
        // 前面的空格
        startIndex -= 8;
        // 后面的空格
        endIndex += 2;
        
        if (startIndex > 0 && endIndex > 0)
        {
            // 删除掉包住的所有内容
            scriptText = scriptText.Remove(startIndex, endIndex - startIndex);
        }

        scriptText = InsertCodeIntoScript(scriptText, className, tempCodeString);

        // 将 tempCodeString 写入新文件
        System.IO.File.WriteAllText(scriptPath, scriptText);
        
        // 刷新 AssetDatabase
        UnityEditor.AssetDatabase.Refresh();
    }
    
    public string InsertCodeIntoScript(string scriptText, string className, string tempCodeString)
    {
        // 找到类名定义的行
        string classDeclaration = $"public class {className}";
        int classIndex = scriptText.IndexOf(classDeclaration, StringComparison.Ordinal);

        // 找到类名定义行的结束位置（即分号或大括号）
        int classEndIndex = scriptText.IndexOfAny(new char[] { '{', ';' }, classIndex + classDeclaration.Length);

        // 确保在类名定义的下一行插入代码
        int insertIndex = classEndIndex + 1;
        
        // 插入 tempCodeString
        scriptText = scriptText.Insert(insertIndex, tempCodeString);

        return scriptText;
    }

    private static string GetScriptPath(MonoBehaviour monoBehaviour)
    {
        // 获取组件的类型
        Type componentType = monoBehaviour.GetType();
        
        // 获取脚本对象
        UnityEditor.MonoScript script = UnityEditor.MonoScript.FromMonoBehaviour(monoBehaviour);
        
        if (script == null)
        {
            Debug.LogError($"Could not find script for component of type {componentType.FullName}");
            return null;
        }
        
        // 获取脚本的路径
        string scriptPath = UnityEditor.AssetDatabase.GetAssetPath(script);
        return scriptPath;
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
                    if (temObject is GameObject gameObject)
                    {
                        _prefabBinderTool.AddBindInfo(gameObject.transform);
                    }
                    else if(temObject is Component component)
                    {
                        _prefabBinderTool.AddBindInfo(component); 
                    }
                }
                
                serializedObject.ApplyModifiedProperties();

                UnityEditor.DragAndDrop.AcceptDrag();
                
                UnityEditor.EditorUtility.SetDirty(_prefabBinderTool.gameObject);
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

            GUILayout.Space(3f);
            
            GUILayout.BeginHorizontal("box");
            {
                if (GUILayout.Button("", "ToggleMixed", GUILayout.ExpandWidth(true),
                        GUILayout.ExpandHeight(true)))
                {
                    UnityEditor.Undo.RecordObject(_prefabBinderTool, "Remove binding info");

                    _prefabBinderTool.bindInfos.RemoveAt(index);
                    index--;
                    continue;
                }

                GUILayout.BeginVertical();
                {
                    GUILayout.BeginHorizontal();
                    {
                        GUILayout.Label("FieldInfoName:", GUILayout.Width(90));
                        
                        string oldFieldInfoName = bindInfo.fieldInfoName;
                        bindInfo.fieldInfoName = UnityEditor.EditorGUILayout.TextField(bindInfo.GetFieldInfoName());
                        if (oldFieldInfoName != bindInfo.fieldInfoName)
                        {
                            UnityEditor.EditorUtility.SetDirty(_prefabBinderTool.gameObject); 
                        } 
                    }
                    GUILayout.EndHorizontal();

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
                            
                            UnityEditor.EditorUtility.SetDirty(_prefabBinderTool.gameObject);
                        }

                        if (bindInfo.component != null)
                        {
                            string[] componentTypesNames = bindInfo.componentTypesNames;

                            int oldComponentTypeIndex = bindInfo.componentTypeIndex;
                            bindInfo.componentTypeIndex =
                                UnityEditor.EditorGUILayout.Popup(bindInfo.componentTypeIndex, componentTypesNames);
                            if (oldComponentTypeIndex != bindInfo.componentTypeIndex)
                            {
                                UnityEditor.Undo.RecordObject(_prefabBinderTool, "Change binding info");

                                bindInfo.component = bindInfo.component.gameObject.GetComponent(bindInfo.componentType);
                                bindInfo.SetKey();
                                _prefabBinderTool.CheckKeyIsCorrect();
                            }
                        }
                        else
                        {
                            Debug.LogError($"name:{_prefabBinderTool.transform.name}, 存在空节点请处理"); 
                        }
                    }
                    GUILayout.EndHorizontal();
                }
                GUILayout.EndVertical();
            }
            GUILayout.EndHorizontal();
        }
    }

    private void DrawRefreshButton()
    {
        if (GUILayout.Button("Refresh"))
        {
            for (var index = 0; index < _prefabBinderTool.bindInfos.Count; index++)
            {
                PrefabBinderTool.BindInfo bindInfo = _prefabBinderTool.bindInfos[index];
                bindInfo.Refresh();
            }
        }
    }

    private string GetBindCodeString()
    {
        string temFiledInfoString = string.Empty;
        foreach (var bindInfo in _prefabBinderTool.bindInfos)
        {
            temFiledInfoString += "\n";
            temFiledInfoString += bindInfo.GetFiledInfoString();
            temFiledInfoString += "\n";
        }  
        
        return temFiledInfoString;
    }

    private string GetAllCodeString()
    {
        string codeString = string.Empty;

        string temFiledInfoString = @"private PrefabBinderTool _prefabBinderTool;
        private PrefabBinderTool PrefabBinderTool
        {
            get
            {
                if (_prefabBinderTool == null)
                {
                    _prefabBinderTool = GetComponent<PrefabBinderTool>();
                }

                return _prefabBinderTool;
            }
        }";
            
        codeString += temFiledInfoString;

        codeString += "\n";
            
        codeString += GetBindCodeString();

        return codeString;
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
                    Debug.LogError($"name:{prefabBinderTool.transform.name}, 存在空节点请处理");
                }
            }
            
            prefabBinderTool.AutoBind();
        }
    }

    public static void AutoBind(this PrefabBinderTool prefabBinderTool)
    {
        Transform[] transforms = prefabBinderTool.GetComponentsInChildren<Transform>();

        if (transforms == null) return;

        bool isChange = false;
        foreach (Transform transform in transforms)
        {
            if (!transform.name.Contains(SuffixTag)) continue;

            // 预制体内部的节点不进行自动绑定
            if (UnityEditor.PrefabUtility.IsPartOfAnyPrefab(transform) &&
                !UnityEditor.PrefabUtility.IsAnyPrefabInstanceRoot(transform.gameObject)) continue;
            if (transform.parent != null &&
                transform.parent.GetComponentInParent<PrefabBinderTool>() != prefabBinderTool) continue;

            bool isSuccess = prefabBinderTool.AddBindInfo(transform, true);

            isChange = isSuccess || isChange;
        }

        if (isChange)
        {
            UnityEditor.EditorUtility.SetDirty(prefabBinderTool.gameObject);
        }
    }

    public static bool AddBindInfo(this PrefabBinderTool prefabBinderTool, Component component, bool isAutoBind = false)
    {
        if (prefabBinderTool.CheckIsExist(component, isAutoBind))
        {
            return false;
        }

        PrefabBinderTool.BindInfo bindInfo = new PrefabBinderTool.BindInfo()
        {
            component = component,
        };

        prefabBinderTool.bindInfos.Add(bindInfo);

        bindInfo.SetKey();
        prefabBinderTool.CheckKeyIsCorrect();
        
        return true;
    }

    private static bool CheckIsExist(this PrefabBinderTool prefabBinderTool, Component component, bool isAutoBind = false)
    {
        foreach (var bindInfo in prefabBinderTool.bindInfos)
        {
            if(bindInfo.component == null) continue;
            
            if (bindInfo.component == component ||
                (isAutoBind && bindInfo.component.transform == component))
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
                if (bindInfo.component == null) continue;
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