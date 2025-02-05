using System;
using System.Collections.Generic;
using UnityEngine;
using Object = UnityEngine.Object;

public static class SkinnedTool
{
    [Serializable]
    public struct SkinInfo
    {
        public SkinnedMeshRenderer SkinnedMeshRenderer;
        public List<string> BonesInfo;
        public string RootBoneInfo;
    }

    public static List<SkinInfo> SaveBonesInfo(Transform root, bool isOriginMesh = true)
    {
        List<SkinInfo> skinInfoList = new List<SkinInfo>();

#if UNITY_EDITOR
        List<SkinnedMeshRenderer> skinnedMeshRendererList = new List<SkinnedMeshRenderer>();
        root.GetComponentsInChildren(true, skinnedMeshRendererList);
        if (skinnedMeshRendererList.Count == 0) return skinInfoList;

        Transform assetTransform = root;
        if (isOriginMesh)
        {
            SkinnedMeshRenderer tempMeshRenderer = skinnedMeshRendererList[0];
            Mesh sharedMesh = tempMeshRenderer.sharedMesh;

            string assetPath = UnityEditor.AssetDatabase.GetAssetPath(sharedMesh);
            Transform asset = UnityEditor.AssetDatabase.LoadAssetAtPath<Transform>(assetPath);
            assetTransform = UnityEngine.Object.Instantiate(asset) as Transform;

            if (assetTransform == null) return skinInfoList;

            Renderer[] renderers = assetTransform.GetComponentsInChildren<Renderer>(true);

            foreach (var temRenderer in renderers)
            {
                Material[] materials = FindMaterialsByName(temRenderer.name, root.gameObject);
                if (materials != null)
                {
                    temRenderer.materials = materials;
                }
            }
        }

        foreach (SkinnedMeshRenderer skinnedMeshRenderer in
                 assetTransform.GetComponentsInChildren<SkinnedMeshRenderer>(true))
        {
            if (skinnedMeshRenderer == null) continue;

            skinnedMeshRenderer.transform.SetParent(root);
            skinnedMeshRenderer.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
            skinnedMeshRenderer.receiveShadows = false;

            SkinInfo skinInfo = new SkinInfo
            {
                SkinnedMeshRenderer = skinnedMeshRenderer,
                BonesInfo = new List<string>()
            };
            foreach (var t in skinnedMeshRenderer.bones)
            {
                skinInfo.BonesInfo.Add(t.name);
            }

            skinInfo.RootBoneInfo = skinnedMeshRenderer.rootBone.name;
            skinInfoList.Add(skinInfo);
        }

        UnityEditor.EditorUtility.SetDirty(root.gameObject);

        if (isOriginMesh)
        {
            Object.DestroyImmediate(assetTransform.gameObject);
        }
#endif

        return skinInfoList;
    }

    private static Material[] FindMaterialsByName(string targetName, GameObject targetHasMaterial)
    {
        Renderer[] renderers = targetHasMaterial.GetComponentsInChildren<Renderer>(true);

        foreach (var temRenderer in renderers)
        {
            if (temRenderer.name == targetName)
            {
                return temRenderer.sharedMaterials;
            }
        }

        return null;
    }

    public static void BindSkeleton(GameObject target, List<SkinInfo> skinInfos)
    {
        foreach (var skinInfo in skinInfos)
        {
            Transform[] newBones = new Transform[skinInfo.BonesInfo.Count];
            for (int i = 0; i < skinInfo.BonesInfo.Count; i++)
            {
                newBones[i] = FindChildRecursion(target.transform, skinInfo.BonesInfo[i]);
            }


            skinInfo.SkinnedMeshRenderer.bones = newBones;
            skinInfo.SkinnedMeshRenderer.rootBone = FindChildRecursion(target.transform, skinInfo.RootBoneInfo);
        }
    }

    // 递归查找
    private static Transform FindChildRecursion(Transform targetTransform, string name)
    {
        if (targetTransform.name == name) return targetTransform;

        foreach (Transform childTransform in targetTransform)
        {
            if (childTransform.name == name)
            {
                return childTransform;
            }
            else
            {
                Transform resultTransform = FindChildRecursion(childTransform, name);

                if (resultTransform != null)
                {
                    return resultTransform;
                }
            }
        }

        return null;
    }
}