using System;
using UniFramework.WebRequest;
using UnityEngine;

public class WebRequestCustomData
{
}

public static class WebRequestUtility
{
    public static string BASE_URL;

    private static string GetFullURL(string url)
    {
        return $"{BASE_URL}/{url}";
    }

    private static void CompletedEvent<T>(WebRequestBase webRequestBase, string response, Action<T> successAction = null,
        Action failAction = null)
    {
        switch (webRequestBase.Status)
        {
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
                failAction?.Invoke();
                break;
            }
        }
    }

    public static WebRequestBase Get<T>(string url, Action<T> successAction = null, Action failAction = null,
        WebRequestCustomData webRequestCustomData = null)
    {
        string fullURL = GetFullURL(url);
        WebRequestGet webRequestGet = new WebRequestGet(fullURL);
        webRequestGet.SendRequest();
        webRequestGet.Completed += webRequestBase =>
        {
            CompletedEvent(webRequestBase, webRequestGet.GetResponse(), successAction, failAction);
        };

        return webRequestGet;
    }

    public static WebRequestBase Post<T>(string url, object requestBody, Action<T> successAction = null,
        Action failAction = null,
        WebRequestCustomData webRequestCustomData = null)
    {
        string fullURL = GetFullURL(url);
        WebRequestPost webRequestPost = new WebRequestPost(fullURL);
        webRequestPost.SendRequest(JsonUtility.ToJson(requestBody));
        webRequestPost.Completed += webRequestBase =>
        {
            CompletedEvent(webRequestBase, webRequestPost.GetResponse(), successAction, failAction);
        };

        return webRequestPost;
    }

    public static WebRequestBase Put<T>(string url, object requestBody, Action<T> successAction = null,
        Action failAction = null,
        WebRequestCustomData webRequestCustomData = null)
    {
        string fullURL = GetFullURL(url);
        WebRequestPut webRequestPut = new WebRequestPut(fullURL);
        webRequestPut.SendRequest(JsonUtility.ToJson(requestBody));
        webRequestPut.Completed += webRequestBase =>
        {
            CompletedEvent(webRequestBase, webRequestPut.GetResponse(), successAction, failAction);
        };

        return webRequestPut;
    }
}