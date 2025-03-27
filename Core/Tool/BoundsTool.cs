using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BoundsTool
{
    public static Bounds GetBounds(Transform root, Transform child)
    {
        if (child is RectTransform)
        {
            return RectTransformUtility.CalculateRelativeRectTransformBounds(root, child);
        }

        return new Bounds(Vector3.zero, Vector3.zero);
    }
}
