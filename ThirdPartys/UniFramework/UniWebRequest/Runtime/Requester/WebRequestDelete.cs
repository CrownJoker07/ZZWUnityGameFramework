using System;
using System.Collections.Generic;
using UnityEngine.Networking;
using UnityEngine;

namespace UniFramework.WebRequest
{
    public sealed class WebRequestDelete : WebRequestBase
    {
        public override string kHttpVerb => "DELETE";

        public WebRequestDelete(string url) : base(url)
        {
        }

        /// <summary>
        /// 发送DELETE请求
        /// </summary>
        /// <param name="timeout">超时：从请求开始计时</param>
        /// <param name="retryCount">重试次数</param>
        public void SendRequest(int timeout = 0, Dictionary<string, string> headers = null, int retryCount = 0)
        {
            if (_webRequest == null)
            {
                DoSendRequestWithRetry(timeout, headers, retryCount);
                int tempRetryCount = retryCount;
                _retryAction = () =>
                {
                    DoSendRequestWithRetry(timeout, headers, tempRetryCount);
                };
            }
        }

        private void DoSendRequestWithRetry(int timeout, Dictionary<string, string> headers, int remainingRetryCount)
        {
            _webRequest = new UnityWebRequest(URL, "DELETE");
            DownloadHandlerBuffer handler = new DownloadHandlerBuffer();
            _webRequest.downloadHandler = handler;
            _webRequest.disposeDownloadHandlerOnDispose = true;
            SetRequestHeader(headers);
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
                    DoSendRequestWithRetry(timeout, headers, remainingRetryCount - 1);
                }
                else
                {
                    CompleteInternal(op);
                }
            };
        }
    }
}
