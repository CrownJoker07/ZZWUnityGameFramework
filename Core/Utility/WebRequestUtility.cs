using System;
using System.Collections.Generic;
using UniFramework.WebRequest;
using UnityEngine;
using Newtonsoft.Json;

public static class WebRequestUtility
{
    private static void CompletedEvent<T>(WebRequestBase webRequestBase, string response,
        Action<T> successAction = null, Action failAction = null)
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
                successAction?.Invoke(bodyData);
                break;
            }
            case EReqeustStatus.ProtocolError:
            case EReqeustStatus.ConnectionError:
            case EReqeustStatus.DataProcessingError:
            {
                Debug.LogError(
                    $"URL:{webRequestBase.URL}\nResponse:{response}\nCode:{webRequestBase.ResponseCode}\nError:{webRequestBase.RequestError}");
                failAction?.Invoke();
                break;
            }
            default:
            {
                Debug.LogError(
                    $"URL:{webRequestBase.URL}\nResponse:{response}\nCode:{webRequestBase.ResponseCode}\nError:{webRequestBase.RequestError}");
                failAction?.Invoke();
                break;
            }
        }
    }

    public static WebRequestBase Get(string url, int timeout = 0, Dictionary<string, string> headers = null, int retryCount = 0)
    {
        WebRequestGet webRequestGet = new WebRequestGet(url);
        webRequestGet.SendRequest(timeout, headers, retryCount);

        return webRequestGet;
    }

    public static void AddCompleted<T>(this WebRequestBase webRequestBase, Action<T> successAction = null, Action failAction = null)
    {
        webRequestBase.Completed += webRequestBase =>
        {
            CompletedEvent(webRequestBase, webRequestBase.GetResponse(), successAction, failAction);
        };
    }

    public static WebRequestBase Post(string url, object requestBody, int timeout = 0, Dictionary<string, string> headers = null, int retryCount = 0)
    {
        WebRequestPost webRequestPost = new WebRequestPost(url);
        webRequestPost.SendRequest(JsonConvert.SerializeObject(requestBody), timeout, headers, retryCount);

        return webRequestPost;
    }

    public static WebRequestBase Put(string url, object requestBody, int timeout = 0, Dictionary<string, string> headers = null, int retryCount = 0)
    {
        WebRequestPut webRequestPut = new WebRequestPut(url);
        webRequestPut.SendRequest(JsonConvert.SerializeObject(requestBody), timeout, headers, retryCount);

        return webRequestPut;
    }
}