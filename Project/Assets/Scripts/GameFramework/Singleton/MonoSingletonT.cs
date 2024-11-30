using UnityEngine;

public abstract class MonoSingleton<T> : MonoBehaviour where T : MonoSingleton<T>
{
    private static T _instance;

    public static T instance
    {
        get
        {
            if (_instance != null) return _instance;
            
            GameObject gameObject = new GameObject(typeof(T).Name);

            DontDestroyOnLoad(gameObject);

            T temT = gameObject.AddComponent<T>();

            _instance = temT;

            return _instance;
        }
    }

    protected virtual void Awake()
    {
        if (_instance == null)
        {
            _instance = this as T;
        }
        else
        {
            Debug.LogError(
                $"typeName:{typeof(T).Name} is Already Exist");
        }
    }
}