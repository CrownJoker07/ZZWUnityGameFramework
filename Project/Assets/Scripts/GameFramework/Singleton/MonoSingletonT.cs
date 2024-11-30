using UnityEngine;

public abstract class MonoSingleton<T>  : MonoBehaviour where T : MonoSingleton<T>
{
    protected virtual bool isDontDestroyOnLoad => true;
    private static T _instance;

    public static T instance
    {
        get
        {
            if (_instance == null)
            {
                GameObject gameObject = new GameObject(typeof(T).Name);

                DontDestroyOnLoad(gameObject);
                
                T temT = gameObject.AddComponent<T>();

                if (!temT.isDontDestroyOnLoad)
                {
                    Debug.LogError($"typeName:{typeof(T).Name} is not isDontDestroyOnLoad, but it created DontDestroyOnLoad");
                    return null;
                }
                
                _instance = temT;
            }
            
            return _instance;
        }
    }

    protected virtual void Awake()
    {
        if (!isDontDestroyOnLoad)
        {
            _instance = this as T;
        }
    }
}
