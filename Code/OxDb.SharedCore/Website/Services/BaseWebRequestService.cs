using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using OxDb.SharedCore.Interfaces;
using OxDb.SharedCore.Logalytics.Interfaces;
using OxDb.SharedCore.Serialization.Interfaces;
using OxDb.SharedCore.Tasks.Services;

namespace OxDb.SharedCore.WebRequests.Services
{
    public enum HttpMethodType
    {
        Get,
        Post,
        Put,
        Delete
    }

    public enum HttpContentType
    {
        None,
        Json,
        FormUrlEncoded
    }

    public class WebRequestOptions
    {
        public HttpMethodType Method { get; set; } = HttpMethodType.Get;
        public HttpContentType ContentType { get; set; } = HttpContentType.None;
        public string? AuthToken { get; set; }
        public Dictionary<string, string> Headers { get; } = new Dictionary<string, string>();
        public object? JsonBody { get; set; }
        public Dictionary<string, string>? FormBody { get; set; }
        public int MaxRetries { get; set; } = 3;
        public int TimeoutMilliseconds { get; set; } = 15000;
    }

    public class ResponseEnvelope<TResponse>
    {
        public bool Success { get; set; }
        public string? ErrorMessage { get; set; }
        public byte[]? RawBytes { get; set; }
        public string? RawString => RawBytes != null ? Encoding.UTF8.GetString(RawBytes) : null;

        private TResponse? _response;
        public TResponse? Response => _response;

        public bool SetResponse(object? obj)
        {
            if (obj is TResponse respo)
            {
                _response = respo;
                return true;
            }
            _response = default;
            return obj == null && typeof(TResponse).IsValueType == false;
        }
    }

    public interface IWebRequestService : IInitializable
    {
        ValueTask<ResponseEnvelope<TResponse>> SendAsync<TResponse>(string url, WebRequestOptions options, CancellationToken token = default) where TResponse : class;

        void SendSync<TResponse>(string url, WebRequestOptions options, Action<ResponseEnvelope<TResponse>> callback) where TResponse : class;
    }

    public abstract class BaseWebRequestService : IWebRequestService
    {
        protected ITextSerializer _textSerializer = null!;
        protected ILogService _logService = null!;
        protected ITaskService _taskService = null;


        public abstract Task Initialize(CancellationToken token);

        // Core transport execution layer implemented by HttpClient or UnityWebRequest targets
        protected abstract ValueTask<RawHttpResponse> ExecuteTransportAsync(string url, WebRequestOptions options, CancellationToken token);

        public async ValueTask<ResponseEnvelope<TResponse>> SendAsync<TResponse>(string url, WebRequestOptions options, CancellationToken token = default) where TResponse : class
        {
            ResponseEnvelope<TResponse> envelope = new ResponseEnvelope<TResponse>();
            int retryCount = 0;

            while (retryCount < options.MaxRetries)
            {
                try
                {
                    RawHttpResponse rawResult = await ExecuteTransportAsync(url, options, token);

                    if (rawResult.IsSuccess)
                    {
                        envelope.RawBytes = rawResult.Data;

                        // 1. Target expects a raw byte array
                        if (typeof(TResponse) == typeof(byte[]))
                        {
                            envelope.SetResponse(rawResult.Data);
                            envelope.Success = true;
                            return envelope;
                        }

                        // 2. Target expects a plain string
                        if (typeof(TResponse) == typeof(string))
                        {
                            envelope.SetResponse(envelope.RawString);
                            envelope.Success = true;
                            return envelope;
                        }

                        // 3. Target expects an object model to deserialize
                        if (envelope.RawString != null)
                        {
                            try
                            {
                                TResponse deserialized = _textSerializer.Deserialize<TResponse>(envelope.RawString);
                                envelope.SetResponse(deserialized);
                                envelope.Success = true;
                                return envelope;
                            }
                            catch (Exception ex)
                            {
                                envelope.ErrorMessage = $"Deserialization error for {typeof(TResponse).Name}: {ex.Message}";
                                return envelope;
                            }
                        }
                    }

                    envelope.ErrorMessage = $"HttpFailure: {rawResult.StatusCode} -- {rawResult.ErrorMessage}";
                }
                catch (Exception ex)
                {
                    _logService.Exception(ex, $"WebRequest failed on attempt {retryCount} to {url}");
                    envelope.ErrorMessage = ex.Message;
                }

                retryCount++;
                if (retryCount < options.MaxRetries)
                {
                    await DelayDelayPlatformSpecific(retryCount, token);
                }
            }

            return envelope;
        }

        protected virtual async ValueTask DelayDelayPlatformSpecific(int retryCount, CancellationToken token)
        {
            await Task.Delay(500 * (int)Math.Pow(2, retryCount), token);
        }

        public virtual void SendSync<TResponse>(string url, WebRequestOptions options, Action<ResponseEnvelope<TResponse>> callback) where TResponse : class
        {
            _taskService.ForgetValueTask(SendWithCallback(url, options, callback),false);
        }

        protected virtual async ValueTask SendWithCallback<TResponse>(string url, WebRequestOptions options, Action<ResponseEnvelope<TResponse>> callback) where TResponse : class
        {
            ResponseEnvelope<TResponse> envelope = await SendAsync<TResponse>(url, options);
            callback?.Invoke(envelope);
        }


        protected struct RawHttpResponse
        {
            public bool IsSuccess { get; set; }
            public int StatusCode { get; set; }
            public byte[]? Data { get; set; }
            public string? ErrorMessage { get; set; }
        }

    }
}