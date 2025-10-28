using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BoundsTool
{
    private static readonly Vector3[] s_Corners = new Vector3[4];

    public static Bounds GetBounds(Transform targetTransform, Transform excludeTransform = null, List<Transform> excludeTransforms = null)
    {
        if (targetTransform is RectTransform rectTransform)
        {
            return CalculateRelativeRectTransformBounds(rectTransform, excludeTransform, excludeTransforms);
        }
        else
        {
            var bounds = new Bounds(targetTransform.position, Vector3.zero);

            List<Transform> transforms = new List<Transform>();
            targetTransform.GetComponentsInChildren(false, transforms);

            if (excludeTransform)
            {
                Transform[] temTransforms = excludeTransform.GetComponentsInChildren<Transform>(false);
                foreach (var temRectTransform in temTransforms)
                {
                    transforms.Remove(temRectTransform);
                }
            }

            if (excludeTransforms != null)
            {
                foreach (var temTransform in excludeTransforms)
                {
                    transforms.Remove(temTransform);
                }
            }

            foreach (var transform in transforms)
            {
                var collider2D = transform.GetComponent<Collider2D>();
                if (collider2D != null)
                {
                    bounds.Encapsulate(collider2D.bounds);
                }
            }

            return bounds;
        }
    }

    private static Bounds CalculateRelativeRectTransformBounds(RectTransform child, Transform excludeTransform = null, List<Transform> excludeTransforms = null)
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

        if (excludeTransforms != null)
        {
            foreach (var temTransform in excludeTransforms)
            {
                if (temTransform is not RectTransform temRectTransform) continue;

                componentsInChildren.Remove(temRectTransform);
            }
        }

        if (componentsInChildren.Count == 0)
            return new Bounds(Vector3.zero, Vector3.zero);
        Vector3 vector3_1 = new Vector3(float.MaxValue, float.MaxValue, float.MaxValue);
        Vector3 vector3_2 = new Vector3(float.MinValue, float.MinValue, float.MinValue);

        int index1 = 0;
        for (int length = componentsInChildren.Count; index1 < length; ++index1)
        {
            componentsInChildren[index1].GetWorldCorners(s_Corners);
            for (int index2 = 0; index2 < 4; ++index2)
            {
                Vector3 lhs = s_Corners[index2];
                vector3_1 = Vector3.Min(lhs, vector3_1);
                vector3_2 = Vector3.Max(lhs, vector3_2);
            }
        }
        Bounds rectTransformBounds = new Bounds(vector3_1, Vector3.zero);
        rectTransformBounds.Encapsulate(vector3_2);
        return rectTransformBounds;
    }
}
