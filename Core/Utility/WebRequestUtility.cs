using System;
using System.Collections.Generic;
using UniFramework.WebRequest;
using UnityEngine;
using Newtonsoft.Json;

public static class WebRequestUtility
{
    private const string Tag = "WebRequestUtilityTag";

    private static void CompletedEvent<T>(WebRequestBase webRequestBase, string response,
        Action<T> successAction = null, Action failAction = null, Func<T, string> decryptStringFunc = null)
    {
        switch (webRequestBase.Status)
        {
            case EReqeustStatus.InProgress:
            {
                break;
            }
            case EReqeustStatus.Succeed:
            {
                T bodyData = JsonConvert.DeserializeObject<T>(response);

                Debug.Log(
                    $"[{Tag}] URL({webRequestBase.kHttpVerb}):{webRequestBase.URL}\nRequestBodyString:\n{webRequestBase.RequestBodyString}\nResponseDescrypt:\n{decryptStringFunc?.Invoke(bodyData)}\nResponse:\n{response}\nCode:{webRequestBase.ResponseCode}");

                successAction?.Invoke(bodyData);
                break;
            }
            case EReqeustStatus.ProtocolError:
            case EReqeustStatus.ConnectionError:
            case EReqeustStatus.DataProcessingError:
            {
                Debug.LogError(
                    $"[{Tag}] URL({webRequestBase.kHttpVerb}):{webRequestBase.URL}\nRequestBodyString:\n{webRequestBase.RequestBodyString}\nResponse:\n{response}\nCode:{webRequestBase.ResponseCode}\nError:{webRequestBase.RequestError}");
                failAction?.Invoke();
                break;
            }
            default:
            {
                Debug.LogError(
                    $"[{Tag}] URL({webRequestBase.kHttpVerb}):{webRequestBase.URL}\nRequestBodyString:\n{webRequestBase.RequestBodyString}\nResponse:\n{response}\nCode:{webRequestBase.ResponseCode}\nError:{webRequestBase.RequestError}");
                failAction?.Invoke();
                break;
            }
        }
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
        Debug.Log($"[{Tag}] ===== Get URL:{url}");
        WebRequestGet webRequestGet = new WebRequestGet(url);
        webRequestGet.SendRequest(timeout, headers, retryCount);

        return webRequestGet;
    }

    public static WebRequestBase Post(string url, object requestBody, int timeout = 0, Dictionary<string, string> headers = null, int retryCount = 0)
    {
        Debug.Log($"[{Tag}] ===== Post URL:{url}");
        WebRequestPost webRequestPost = new WebRequestPost(url);
        webRequestPost.SendRequest(JsonConvert.SerializeObject(requestBody), timeout, headers, retryCount);

        return webRequestPost;
    }

    public static WebRequestBase Put(string url, object requestBody, int timeout = 0, Dictionary<string, string> headers = null, int retryCount = 0)
    {
        Debug.Log($"[{Tag}] ===== Put URL:{url}");
        WebRequestPut webRequestPut = new WebRequestPut(url);
        webRequestPut.SendRequest(JsonConvert.SerializeObject(requestBody), timeout, headers, retryCount);

        return webRequestPut;
    }
}