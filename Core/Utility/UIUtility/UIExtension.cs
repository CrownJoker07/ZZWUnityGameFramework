using UnityEngine;

public static class UIExtension
{
    public static RectTransform GetRectTransform(this MonoBehaviour monoBehaviour)
    {
        return monoBehaviour.transform as RectTransform;
    }
}
