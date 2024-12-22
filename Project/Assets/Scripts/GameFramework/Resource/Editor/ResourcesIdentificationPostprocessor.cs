using UnityEditor;

public class ResourcesIdentificationPostprocessor : AssetPostprocessor
{
    private void OnPreprocessAsset()
    {
        if (ResourceIdentificationTool.CheckIsNeedInitResourceIdentificationInfo(assetPath))
        {
            ResourceIdentificationTool.InitResourceIdentificationInfoMapInProject();
        }
    }
}