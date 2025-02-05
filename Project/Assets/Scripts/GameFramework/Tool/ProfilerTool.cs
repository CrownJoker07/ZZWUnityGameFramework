using UnityEngine;

public class ProfilerTool
{
    // 渲染 DrawCall 报警阈值
    public static int MaxBatches = 220;

    public static void Update()
    {
#if UNITY_EDITOR
        if (UnityEditor.UnityStats.batches >= MaxBatches)
        {
            Debug.LogError($"batches:{UnityEditor.UnityStats.batches} is Too much");
        }
#endif
    }
}
