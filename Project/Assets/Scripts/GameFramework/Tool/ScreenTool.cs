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
}
