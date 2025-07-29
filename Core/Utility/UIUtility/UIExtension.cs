using UnityEngine;

public static class UIExtension
{
    public static RectTransform GetRectTransform(this MonoBehaviour monoBehaviour)
    {
        if (!monoBehaviour) return null;

        return monoBehaviour.transform as RectTransform;
    }
}
