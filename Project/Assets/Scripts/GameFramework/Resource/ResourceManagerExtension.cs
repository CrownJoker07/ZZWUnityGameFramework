using System;

public static class ResourceManagerExtension
{
    public static void LoadAssetASync<T>(this ResourceManager resourceManager, int assetID, Action<T> successAction,
        Action failureAction = null) where T : UnityEngine.Object
    {
        string assetPath = ResourceIdentificationTool.instance.GetAssetPathById(assetID);

        resourceManager.LoadAssetAsync<T>(assetPath, successAction, failureAction);
    }
}