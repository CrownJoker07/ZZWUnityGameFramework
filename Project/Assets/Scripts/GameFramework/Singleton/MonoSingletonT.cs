using UnityEngine;

public abstract class MonoSingleton<T>  : MonoBehaviour where T : MonoBehaviour
{
    private static T _instance;

    public static T instance
    {
        get
        {
            return _instance;
        }
    }

    protected virtual void Awake()
    {
        _instance = this as T;
    }
}
