using System;
using UnityEngine;

public class DebugTool
{
    /*
     * 日志工具 版本: V1.0.0，设计思路：
     * 1. 支持日志、警告、报错三种方式的日志输出
     * 2. 支持按功能区分日志，方便筛选查看
     * 3. 不输出日志时，日志逻辑必须不影响游戏逻辑，使用委托的形式输出日志文案
     * 4. 支持自定义颜色
     * 5. 支持自定义对象跳转
     * 6. 功能类型支持后缀直接忽略，方便控制开启或关闭日志
     * 7. 支持一键开启或关闭所有日志
     */
    private enum LogType
    {
        Log = 0,
        Warning = 1,
        Error = 2,
    }

    private const string LogIgnoreTag = "_Ignore";

    public static bool EnableLog = true;
    public static bool EnableWarning = true;
    public static bool EnableError = true;

    private static string LogString(string function, Func<string> logContent, string color)
    {
        if (color != null)
        {
            return $"{function}_Log:<color=#{color}>{logContent?.Invoke()}</color>";
        }
        else
        {
            return $"{function}_Log:{logContent?.Invoke()}";
        }
    }

    private static void Log(LogType logType, string function, Func<string> logContent, string color = null,
        UnityEngine.Object context = null)
    {
        if (function.Contains(LogIgnoreTag)) return;

        // 非编辑器模式无法输出颜色和跳转，因此设空两个参数
#if !UNITY_EDITOR
        color = null;
        context = null;
#endif

        switch (logType)
        {
            case LogType.Log:
            {
                if (!EnableLog) return;

                Debug.Log(LogString(function, logContent, color), context);
                break;
            }
            case LogType.Warning:
            {
                if (!EnableWarning) return;

                Debug.LogWarning(LogString(function, logContent, color), context);
                break;
            }
            case LogType.Error:
            {
                if (!EnableError) return;

                Debug.LogError(LogString(function, logContent, color), context);
                break;
            }
        }
    }

    public static void Log(string function, Func<string> logContent, string color = null,
        UnityEngine.Object context = null)
    {
        Log(LogType.Log, function, logContent, color, context);
    }

    public static void Warning(string function, Func<string> logContent, string color = null,
        UnityEngine.Object context = null)
    {
        Log(LogType.Warning, function, logContent, color, context);
    }

    public static void Error(string function, Func<string> logContent, string color = null,
        UnityEngine.Object context = null)
    {
        Log(LogType.Error, function, logContent, color, context);
    }
}