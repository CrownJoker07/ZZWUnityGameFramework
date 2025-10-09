using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BoundsTool
{
    private static readonly Vector3[] s_Corners = new Vector3[4];

    public static Bounds GetRectTransformBounds(Transform root, RectTransform child, Transform excludeTransform = null, bool isWorld = false)
    {
        return CalculateRelativeRectTransformBounds(root, child, excludeTransform, isWorld);
    }

    private static Bounds CalculateRelativeRectTransformBounds(Transform root, Transform child, Transform excludeTransform = null, bool isWorld = false)
    {
        List<RectTransform> componentsInChildren = new List<RectTransform>();
        child.GetComponentsInChildren(false, componentsInChildren);

        if (excludeTransform)
        {
            RectTransform[] temRectTransforms = excludeTransform.GetComponentsInChildren<RectTransform>(false);
            foreach (var temRectTransform in temRectTransforms)
            {
                componentsInChildren.Remove(temRectTransform);
            }
        }

        if (componentsInChildren.Count == 0)
            return new Bounds(Vector3.zero, Vector3.zero);
        Vector3 vector3_1 = new Vector3(float.MaxValue, float.MaxValue, float.MaxValue);
        Vector3 vector3_2 = new Vector3(float.MinValue, float.MinValue, float.MinValue);
        Matrix4x4 worldToLocalMatrix = root.worldToLocalMatrix;
        int index1 = 0;
        for (int length = componentsInChildren.Count; index1 < length; ++index1)
        {
            componentsInChildren[index1].GetWorldCorners(s_Corners);
            for (int index2 = 0; index2 < 4; ++index2)
            {
                Vector3 lhs = isWorld ? s_Corners[index2] : worldToLocalMatrix.MultiplyPoint3x4(s_Corners[index2]);
                vector3_1 = Vector3.Min(lhs, vector3_1);
                vector3_2 = Vector3.Max(lhs, vector3_2);
            }
        }
        Bounds rectTransformBounds = new Bounds(vector3_1, Vector3.zero);
        rectTransformBounds.Encapsulate(vector3_2);
        return rectTransformBounds;
    }
}
