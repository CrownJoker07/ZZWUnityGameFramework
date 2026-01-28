using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.Networking;
using UnityEngine;

namespace UniFramework.WebRequest
{
    public sealed class WebRequestPost : WebRequestBase
    {
        public override string kHttpVerb => UnityWebRequest.kHttpVerbPOST;

        public WebRequestPost(string url) : base(url)
        {
        }

        /// <summary>
        /// 发送POST请求
        /// </summary>
        /// <param name="form">POST的表单</param>
        /// <param name="timeout">超时：从请求开始计时</param>
        /// <param name="retryCount">重试次数</param>
        public void SendRequest_WWWForm(WWWForm form, int timeout = 0, Dictionary<string, string> headers = null, int retryCount = 0)
        {
            if (_webRequest == null)
            {
                DoSendRequestWithRetry(() => CreateUnityWebRequest(form), timeout, headers, retryCount);
                int tempRetryCount = retryCount;
                _retryAction = () =>
                {
                    DoSendRequestWithRetry(() => CreateUnityWebRequest(form), timeout, headers, tempRetryCount);
                };
            }
        }

        private UnityWebRequest CreateUnityWebRequest(WWWForm form)
        {
            _form = form;
            return UnityWebRequest.Post(URL, form);
        }

        /// <summary>
        /// 发送POST请求
        /// </summary>
        /// <param name="post">POST的文本内容</param>
        /// <param name="timeout">超时：从请求开始计时</param>
        /// <param name="retryCount">重试次数</param>
        public void SendRequest(string post, int timeout = 0, Dictionary<string, string> headers = null, int retryCount = 0)
        {
            if (_webRequest == null)
            {
                DoSendRequestWithRetry(() => CreateUnityWebRequest(post, headers), timeout, headers, retryCount);
                int tempRetryCount = retryCount;
                _retryAction = () =>
                {
                    DoSendRequestWithRetry(() => CreateUnityWebRequest(post, headers), timeout, headers, tempRetryCount);
                };
            }
        }

        private UnityWebRequest CreateUnityWebRequest(string post, Dictionary<string, string> headers)
        {
            string contentType = headers.ContainsKey(ContentType) ? headers[ContentType] : null;
            return UnityWebRequest.Post(URL, post, contentType);
        }

        private void DoSendRequestWithRetry(Func<UnityWebRequest> createUnityWebRequest, int timeout, Dictionary<string, string> headers, int remainingRetryCount)
        {
            _webRequest = createUnityWebRequest();
            SetRequestHeader(headers);
            DownloadHandlerBuffer handler = new DownloadHandlerBuffer();
            _webRequest.downloadHandler = handler;
            _webRequest.disposeDownloadHandlerOnDispose = true;
            _webRequest.timeout = timeout;
            _operation = _webRequest.SendWebRequest();

            _operation.completed += (op) =>
            {
                if (_webRequest.result != UnityWebRequest.Result.Success && remainingRetryCount > 0)
                {
                    // 释放旧的请求
                    _webRequest.Dispose();
                    _webRequest = null;
                    // 重试
                    DoSendRequestWithRetry(createUnityWebRequest, timeout, headers, remainingRetryCount - 1);
                }
                else
                {
                    CompleteInternal(op);
                }
            };
        }
    }
}