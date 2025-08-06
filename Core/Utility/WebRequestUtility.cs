using System;
using System.Collections.Generic;
using UniFramework.WebRequest;
using UnityEngine;

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
                T bodyData = JsonUtility.FromJson<T>(response);
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

        webRequestBase.Dispose();
    }

    public static WebRequestBase Get<T>(string url, Action<T> successAction = null, Action failAction = null,
        int timeout = 0, Dictionary<string, string> headers = null, int retryCount = 0)
    {
        WebRequestGet webRequestGet = new WebRequestGet(url);
        webRequestGet.SendRequest(timeout, headers, retryCount);
        webRequestGet.Completed += webRequestBase =>
        {
            CompletedEvent(webRequestBase, webRequestGet.GetResponse(), successAction, failAction);
        };

        return webRequestGet;
    }

    public static WebRequestBase Post<T>(string url, object requestBody, Action<T> successAction = null,
        Action failAction = null, int timeout = 0, Dictionary<string, string> headers = null)
    {
        WebRequestPost webRequestPost = new WebRequestPost(url);
        webRequestPost.SendRequest(JsonUtility.ToJson(requestBody), timeout, headers);
        webRequestPost.Completed += webRequestBase =>
        {
            CompletedEvent(webRequestBase, webRequestPost.GetResponse(), successAction, failAction);
        };

        return webRequestPost;
    }

    public static WebRequestBase Put<T>(string url, object requestBody, Action<T> successAction = null,
        Action failAction = null, int timeout = 0, Dictionary<string, string> headers = null)
    {
        WebRequestPut webRequestPut = new WebRequestPut(url);
        webRequestPut.SendRequest(JsonUtility.ToJson(requestBody), timeout, headers);
        webRequestPut.Completed += webRequestBase =>
        {
            CompletedEvent(webRequestBase, webRequestPut.GetResponse(), successAction, failAction);
        };

        return webRequestPut;
    }
}