using System;

/*
 * ResourceManager 版本: V1.0.0，设计思路：
 */
public class ResourceManager : MonoSingleton<ResourceManager>
{
    public enum ResourceLoadMode
    {
        None = 0,
        AssetBundle = 1,
        Editor = 2,
    }

    protected override void Awake()
    {
        base.Awake();
    }

    public void LoadAssetAsync<T>(string assetPath, Action<T> successAction, Action failureAction = null)
        where T : UnityEngine.Object
    {
        T temT = UnityEditor.AssetDatabase.LoadAssetAtPath<T>(assetPath);

        if (temT != null)
        {
            successAction?.Invoke(temT);
        }
        else
        {
            failureAction?.Invoke();
        }
    }

    public void UnloadAsset(UnityEngine.Object asset)
    {
        
    }
}
