using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.Networking;

namespace UniFramework.WebRequest
{
    public sealed class WebRequestGet : WebRequestBase
    {
        public override string kHttpVerb => UnityWebRequest.kHttpVerbGET;

        public WebRequestGet(string url) : base(url)
        {
        }

        /// <summary>
        /// 发送GET请求
        /// </summary>
        /// <param name="timeout">超时：从请求开始计时</param>
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
            _webRequest = new UnityWebRequest(URL, UnityWebRequest.kHttpVerbGET);
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
                    DoSendRequestWithRetry(timeout, headers, remainingRetryCount - 1);
                }
                else
                {
                    CompleteInternal(op);
                }
            };
        }

        /// <summary>
        /// 获取下载的字节数据
        /// </summary>
        public byte[] GetData()
        {
            if (_webRequest != null && IsDone())
                return _webRequest.downloadHandler.data;
            else
                return null;
        }
    }
}