using UnityEngine;

public class ScreenTool
{
    /// <summary>
    /// 设置分辨率
    /// </summary>
    /// <param name="pixelWidth"></param>
    public static void SetResolutionKeepAspect(int pixelWidth)
    {
        float scale = Display.main.systemWidth / (float)pixelWidth;
        Screen.SetResolution(pixelWidth, (int)(Display.main.systemHeight / scale), Screen.fullScreen);
    }

    public static float GetScreenDiagonalInches()
    {
        // 获取屏幕分辨率
        float screenWidthPixels = Display.main.systemWidth;
        float screenHeightPixels = Display.main.systemHeight;

        // 获取屏幕 DPI
        float screenDPI = Screen.dpi;

        if (screenDPI <= 0)
        {
            screenDPI = 96f;
        }

        // 计算屏幕对角线尺寸（英寸）
        float screenDiagonalPixels = Mathf.Sqrt((screenWidthPixels * screenWidthPixels) +
            (screenHeightPixels * screenHeightPixels)
        );

        float screenDiagonalInches = screenDiagonalPixels / screenDPI;
        
        return screenDiagonalInches;
    }
    
    public static bool IsInScreenByWorldPosition(Camera camera, Vector3 worldPosition, float safeOffset = 0.1f)
    {
        Vector3 viewportPoint = camera.WorldToViewportPoint(worldPosition);
            
        // 检查视口坐标的 x 和 y 是否在 [0, 1] 范围内
        bool isInView = viewportPoint.x >= 0 - safeOffset && viewportPoint.x <= 1 + safeOffset &&
                        viewportPoint.y >= 0 - safeOffset && viewportPoint.y <= 1 + safeOffset &&
                        viewportPoint.z > 0; // z > 0 表示点在相机前方
            
        return isInView;
    }
}
