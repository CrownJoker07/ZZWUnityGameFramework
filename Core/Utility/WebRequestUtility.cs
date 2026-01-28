using System;
using System.Collections.Generic;
using UniFramework.WebRequest;
using UnityEngine;
using Newtonsoft.Json;
using System.IO;

public static class WebRequestUtility
{
    private const string Tag = "WebRequestUtilityTag";

    private static void CompletedEvent<T>(WebRequestBase webRequestBase, string response,
        Action<T> successAction = null, Action failAction = null, Func<T, string> decryptStringFunc = null)
    {
        string logString = string.Empty;
        switch (webRequestBase.Status)
        {
            case EReqeustStatus.InProgress:
            {
                break;
            }
            case EReqeustStatus.Succeed:
            {
                T bodyData = JsonConvert.DeserializeObject<T>(response);

#if UNITY_EDITOR
                logString = $"[{Tag}] Succeed {webRequestBase.ToString()}\n------------------\nResponseDescrypt:\n{decryptStringFunc?.Invoke(bodyData)}";
                // Debug.Log(logString);
#endif
                successAction?.Invoke(bodyData);
                break;
            }
            case EReqeustStatus.ProtocolError:
            case EReqeustStatus.ConnectionError:
            case EReqeustStatus.DataProcessingError:
            {
                T bodyData = JsonConvert.DeserializeObject<T>(response);

                logString = $"[{Tag}] Failed {webRequestBase.ToString()}\n------------------\nResponseDescrypt:\n{decryptStringFunc?.Invoke(bodyData)}";
                Debug.LogError(logString);
                failAction?.Invoke();
                break;
            }
            default:
            {
                T bodyData = JsonConvert.DeserializeObject<T>(response);

                logString = $"[{Tag}] Failed {webRequestBase.ToString()}\n------------------\nResponseDescrypt:\n{decryptStringFunc?.Invoke(bodyData)}";
                Debug.LogError(logString);
                failAction?.Invoke();
                break;
            }
        }

#if UNITY_EDITOR
        // 写进一个 txt 文件
        // LogToFile(logString + "\n\n\n");
#endif
    }

    // 写进一个 txt 文件， 重复写进一个 log 文件
    private static void LogToFile(string logString)
    {
        // 写进一个 txt 文件
        string logFilePath = Application.dataPath + "../WebRequestLog.txt";
        FileInfo fileInfo = new FileInfo(logFilePath);
        if (!fileInfo.Directory.Exists)
        {
            fileInfo.Directory.Create();
        }

        string oldLogString = string.Empty;
        if (fileInfo.Exists)
        {
            oldLogString = System.IO.File.ReadAllText(logFilePath);
        }
        System.IO.File.WriteAllText(logFilePath, oldLogString + logString);
    }

    public static void AddCompleted<T>(this WebRequestBase webRequestBase, Action<T> successAction = null, Action failAction = null, Func<T, string> decryptStringFunc = null)
    {
        webRequestBase.Completed += webRequestBase =>
        {
            CompletedEvent(webRequestBase, webRequestBase.GetResponse(), successAction, failAction, decryptStringFunc);
        };
    }

    public static WebRequestBase Get(string url, int timeout = 0, Dictionary<string, string> headers = null, int retryCount = 0)
    {
        // Debug.Log($"[{Tag}] ===== Get URL:{url}");
        WebRequestGet webRequestGet = new WebRequestGet(url);
        webRequestGet.SendRequest(timeout, headers, retryCount);

        return webRequestGet;
    }

    public static WebRequestBase Post(string url, WWWForm form, int timeout = 0, Dictionary<string, string> headers = null, int retryCount = 0)
    {
        // Debug.Log($"[{Tag}] ===== Post URL:{url}");
        WebRequestPost webRequestPost = new WebRequestPost(url);
        webRequestPost.SendRequest(form, timeout, headers, retryCount);

        return webRequestPost;
    }

    public static WebRequestBase Post(string url, object requestBody, int timeout = 0, Dictionary<string, string> headers = null, int retryCount = 0)
    {
        // Debug.Log($"[{Tag}] ===== Post URL:{url}");
        WebRequestPost webRequestPost = new WebRequestPost(url);
        webRequestPost.SendRequest(JsonConvert.SerializeObject(requestBody), timeout, headers, retryCount);

        return webRequestPost;
    }

    public static WebRequestBase Put(string url, object requestBody, int timeout = 0, Dictionary<string, string> headers = null, int retryCount = 0)
    {
        // Debug.Log($"[{Tag}] ===== Put URL:{url}");
        WebRequestPut webRequestPut = new WebRequestPut(url);
        webRequestPut.SendRequest(JsonConvert.SerializeObject(requestBody), timeout, headers, retryCount);

        return webRequestPut;
    }

    public static WebRequestBase Texture(string url, int timeout = 0)
    {
        // Debug.Log($"[{Tag}] ===== Texture URL:{url}");
        WebRequestTexture webRequestTexture = new WebRequestTexture(url);
        webRequestTexture.SendRequest(timeout);

        return webRequestTexture;
    }
}