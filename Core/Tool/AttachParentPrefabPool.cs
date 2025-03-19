using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public interface IObjectPrefab
{
    public void Spawn();

    public void DeSpawn();
}

/*
 * AttachParentPrefabPool 版本: V2.0.0，设计思路：
 * 1. 依附父级生命周期的小型对象池
 * 2. IObjectPrefab接口去除 GetComponent 方法，靠 Unity 的 GetComponent 实现对应逻辑 --- v1.1.0
 * 3. 移除所有 GetComponent 逻辑，靠类型判断实现 --- v1.2.0
 * 4. 支持非IObjectPrefab的对象 --- V2.0.0
 * 5. 不再强制设置 克隆 GameObject 的 Active属性 --- v2.0.0
 */
public class AttachParentPrefabPool : MonoBehaviour
{
    private class ObjectPrefabStruct
    {
        public int Index;
        public GameObject ObjectPrefab;
        public Component ObjectPrefabComponent;

        public static ObjectPrefabStruct Create(int index, GameObject objectPrefab)
        {
            return new ObjectPrefabStruct()
            {
                Index = index,
                ObjectPrefab = objectPrefab,
            };
        }
    }

    [SerializeField] private List<GameObject> prefabGameObjectList = new List<GameObject>();

    private List<ObjectPrefabStruct> _objectPrefabList = new List<ObjectPrefabStruct>();

    private Dictionary<int, List<Component>> _objectPrefabPoolDictionary =
        new Dictionary<int, List<Component>>();

    private Transform ParentTransform => this.transform;

    private void Awake()
    {
        for (var index = 0; index < prefabGameObjectList.Count; index++)
        {
            GameObject prefabGameObject = prefabGameObjectList[index];
            
            prefabGameObject.transform.SetParent(ParentTransform);
            prefabGameObject.transform.localPosition = Vector3.zero;

            _objectPrefabList.Add(ObjectPrefabStruct.Create(index, prefabGameObject));
            _objectPrefabPoolDictionary[index] = new List<Component>();
        }

        transform.localScale = Vector3.zero;
    }

    public Component Spawn(int index, Transform parent, Type type)
    {
        Component tempComponent = default;

        if (_objectPrefabPoolDictionary.TryGetValue(index, out List<Component> objectPrefabs))
        {
            if (objectPrefabs.Count > 0)
            {
                Component objectPrefab = objectPrefabs[0];
                objectPrefabs.RemoveAt(0);

                tempComponent = objectPrefab;
            }
            else
            {
                Component objectPrefabComponent = GetObjectPrefabComponent(index, type);

                if (objectPrefabComponent != null)
                {
                    tempComponent = Instantiate(objectPrefabComponent);
                }
                else
                {
                    GameObject tempGameObject =  GetObjectPrefabGameObject(index);
                    tempComponent = Instantiate(tempGameObject).GetComponent(type);
                }
            }
        }

        if (tempComponent != null)
        {
            if (tempComponent is IObjectPrefab iObjectPrefab)
            {
                iObjectPrefab.Spawn();
            }

            Transform componentTransform = tempComponent.transform;
            componentTransform.SetParent(parent);
            componentTransform.localPosition = Vector3.zero;
            componentTransform.localEulerAngles = Vector3.zero;
            componentTransform.localScale = Vector3.one;
        }
        else
        {
            return null;
        }

        return tempComponent;
    }

    public T Spawn<T>(int index, Transform parent) where T : Component
    {
        return Spawn(index, parent, typeof(T)) as T;
    }

    public Component GetObjectPrefabComponent(int index, Type type)
    {
        foreach (var objectPrefabStruct in _objectPrefabList)
        {
            if (objectPrefabStruct.Index != index) continue;

            if (objectPrefabStruct.ObjectPrefabComponent == null && type.IsSubclassOf(typeof(Component)))
            {
                objectPrefabStruct.ObjectPrefabComponent = objectPrefabStruct.ObjectPrefab.GetComponent(type);
            }

            return objectPrefabStruct.ObjectPrefabComponent;
        }

        return null;
    }

    private GameObject GetObjectPrefabGameObject(int index)
    {
        foreach (var objectPrefabStruct in _objectPrefabList)
        {
            if (objectPrefabStruct.Index != index) continue;

            return objectPrefabStruct.ObjectPrefab;
        }

        return null; 
    }

    public void DeSpawn(int index, Component monoBehaviour)
    {
        if (_objectPrefabPoolDictionary.TryGetValue(index, out List<Component> objectPrefabs))
        {
            monoBehaviour.transform.SetParent(ParentTransform);
            monoBehaviour.transform.localPosition = Vector3.zero;

            if (monoBehaviour is IObjectPrefab iObjectPrefab)
            {
                iObjectPrefab.DeSpawn();
            }

            objectPrefabs.Add(monoBehaviour);
        }
        else
        {
        }
    }
}