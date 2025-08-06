using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.Networking;
using UnityEngine;

namespace UniFramework.WebRequest
{
    public sealed class WebRequestPost : WebRequestBase
    {
        public WebRequestPost(string url) : base(url)
        {
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
                DoSendRequestWithRetry(post, timeout, headers, retryCount);
                int tempRetryCount = retryCount;
                _retryAction = () =>
                {
                    DoSendRequestWithRetry(post, timeout, headers, tempRetryCount);
                };
            }
        }

        private void DoSendRequestWithRetry(string post, int timeout, Dictionary<string, string> headers, int remainingRetryCount)
        {
            _webRequest = UnityWebRequest.Post(URL, post, "application/json");
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
                    DoSendRequestWithRetry(post, timeout, headers, remainingRetryCount - 1);
                }
                else
                {
                    CompleteInternal(op);
                }
            };
        }
    }
}