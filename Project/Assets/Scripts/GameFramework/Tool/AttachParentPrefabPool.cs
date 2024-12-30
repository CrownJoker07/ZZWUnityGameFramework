using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public interface IObjectPrefab
{
    public Component GetComponent();

    public void Spawn();

    public void DeSpawn();
}

public class AttachParentPrefabPool : MonoBehaviour
{
    private struct ObjectPrefabStruct
    {
        public int Index;
        public IObjectPrefab ObjectPrefab;

        public static ObjectPrefabStruct Create(int index, IObjectPrefab objectPrefab)
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

    private Dictionary<int, List<IObjectPrefab>> _objectPrefabSimplePoolDictionary =
        new Dictionary<int, List<IObjectPrefab>>();

    private void Awake()
    {
        for (var index = 0; index < prefabGameObjectList.Count; index++)
        {
            GameObject prefabGameObject = prefabGameObjectList[index];
            prefabGameObject.SetActive(false);

            IObjectPrefab objectPrefab = prefabGameObject.GetComponent<IObjectPrefab>();

            if (objectPrefab == null)
            {
            }
            else
            {
                _objectPrefabList.Add(ObjectPrefabStruct.Create(index, objectPrefab));
                _objectPrefabSimplePoolDictionary[index] = new List<IObjectPrefab>();
            }
        }
    }

    public IObjectPrefab Spawn(int index, Transform parent)
    {
        Component component = null;

        if (_objectPrefabSimplePoolDictionary.TryGetValue(index, out List<IObjectPrefab> objectPrefabs))
        {
            if (objectPrefabs.Count > 0)
            {
                IObjectPrefab objectPrefab = objectPrefabs[0];
                objectPrefabs.RemoveAt(0);

                component = objectPrefab.GetComponent();
            }
            else
            {
                IObjectPrefab objectPrefab = GetObjectPrefab(index);

                component = Instantiate(objectPrefab.GetComponent());
            }
        }

        if (component != null)
        {
            component.GetComponent<IObjectPrefab>().Spawn();

            Transform componentTransform = component.transform;
            componentTransform.SetParent(parent);
            componentTransform.localPosition = Vector3.zero;
            componentTransform.localEulerAngles = Vector3.zero;
            componentTransform.localScale = Vector3.one;
        }
        else
        {
            return default;
        }

        return component.GetComponent<IObjectPrefab>();
    }

    public T Spawn<T>(int index, Transform parent) where T : Component
    {
        IObjectPrefab objectPrefab = Spawn(index, parent);

        if (objectPrefab.GetComponent() is T temComponent)
        {
            return temComponent;
        }

        return default;
    }

    public IObjectPrefab GetObjectPrefab(int index)
    {
        foreach (var objectPrefabStruct in _objectPrefabList)
        {
            if (objectPrefabStruct.Index == index)
            {
                return objectPrefabStruct.ObjectPrefab;
            }
        }

        return null;
    }

    public void DeSpawn(int index, IObjectPrefab objectPrefab)
    {
        if (_objectPrefabSimplePoolDictionary.TryGetValue(index, out List<IObjectPrefab> objectPrefabs))
        {
            objectPrefab.GetComponent().transform.SetParent(this.transform);
            objectPrefab.DeSpawn();

            objectPrefabs.Add(objectPrefab);
        }
        else
        {
        }
    }
}