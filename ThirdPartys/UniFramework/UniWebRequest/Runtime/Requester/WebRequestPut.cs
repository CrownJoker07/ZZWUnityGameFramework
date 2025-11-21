using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.Networking;
using UnityEngine;

namespace UniFramework.WebRequest
{
    public sealed class WebRequestPut : WebRequestBase
    {
        public override string kHttpVerb => UnityWebRequest.kHttpVerbPUT;

        public WebRequestPut(string url) : base(url)
        {
        }

        /// <summary>
        /// 发送PUT请求
        /// </summary>
        /// <param name="put">PUT的文本内容</param>
        /// <param name="timeout">超时：从请求开始计时</param>
        /// <param name="retryCount">重试次数</param>
        public void SendRequest(string put, int timeout = 0, Dictionary<string, string> headers = null, int retryCount = 0)
        {
            if (_webRequest == null)
            {
                DoSendRequestWithRetry(put, timeout, headers, retryCount);
                int tempRetryCount = retryCount;
                _retryAction = () =>
                {
                    DoSendRequestWithRetry(put, timeout, headers, tempRetryCount);
                };
            }
        }

        private void DoSendRequestWithRetry(string put, int timeout, Dictionary<string, string> headers, int remainingRetryCount)
        {
            _webRequest = UnityWebRequest.Put(URL, put);
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
                    DoSendRequestWithRetry(put, timeout, headers, remainingRetryCount - 1);
                }
                else
                {
                    CompleteInternal(op);
                }
            };
        }
    }
}