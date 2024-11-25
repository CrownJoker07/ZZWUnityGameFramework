using System;
using System.Collections.Generic;
using UnityEngine;

public class PrefabBinderTool : MonoBehaviour
{
    [Serializable]
    public class BindInfo
    {
        public string key = string.Empty;
        public GameObject gameObject = null;
    }

    public List<BindInfo> bindInfos = new List<BindInfo>();

    public T GetTarget<T>(string key) where T : UnityEngine.Object
    {
        GameObject temGameObject = GetGameObject(key);

        if (temGameObject == null) return null;

        if (typeof(T) == typeof(GameObject))
        {
            return (temGameObject.gameObject) as T;
        }
        else if (typeof(T) == typeof(Transform))
        {
            return (temGameObject.gameObject.transform) as T;
        }

        return temGameObject.gameObject.GetComponent<T>();
    }

    private GameObject GetGameObject(string key)
    {
        foreach (var bindInfo in bindInfos)
        {
            if (bindInfo.key == key)
            {
                return bindInfo.gameObject;
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
    public override void OnInspectorGUI()
    {
        OnDragUpdate();
    }

    public void OnDragUpdate()
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

                break;
            }
        }


        // if ((Event.current.type == EventType.DragUpdated || Event.current.type == EventType.DragExited) &&
        //     mExcelPathRect.Contains(Event.current.mousePosition))
        // {
        //
        //     if (DragAndDrop.paths != null && DragAndDrop.paths.Length > 0)
        //     {
        //         string retPath = DragAndDrop.paths[0];
        //         curPath = retPath;
        //     }
        // }

        // //绘制选定对象所有绑定的组件类型
        // drawTypes();
        // switch (e.type)
        // {
        //     case EventType.DragUpdated:
        //     case EventType.DragPerform:
        //         var index = getContainsIndex(dragArea, e.mousePosition);
        //         if (index < -1)
        //         {
        //             break;
        //         }
        //
        //         if (m_activeItemInfo == null)
        //         {
        //             newActiveItemInfo();
        //         }
        //
        //         DragAndDrop.visualMode = DragAndDropVisualMode.Copy;
        //         if (e.type == EventType.DragPerform && index >= 0)
        //         {
        //             if (m_activeItemInfo != null && m_activeItemInfo.gameObject != null)
        //             {
        //                 addComponent(m_assetsList, m_activeItemInfo, index);
        //             }
        //
        //             DragAndDrop.AcceptDrag();
        //         }
        //
        //         e.Use();
        //         break;
        //     case EventType.DragExited:
        //         m_activeItemInfo = null;
        //         m_typeRects = new Rect[0];
        //         break;
        //     default:
        //         break;
    }
}
#endif