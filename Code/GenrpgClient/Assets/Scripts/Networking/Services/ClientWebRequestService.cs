using OxDb.Client.Awaitables;
using OxDb.Client.ClientEvents;
using OxDb.Client.Logalytics.ClientEvents;
using OxDb.Client.Login.Messages;
using OxDb.Client.Setup.Interfaces;
using OxDb.SharedCore.Core.Constants;
using OxDb.SharedCore.GameSettings;
using OxDb.SharedCore.HelperClasses;
using OxDb.SharedCore.Interfaces;
using OxDb.SharedCore.Logalytics.Interfaces;
using OxDb.SharedCore.Serialization.Interfaces;
using OxDb.SharedCore.Utils;
using OxDb.SharedCore.WebRequests.Services;
using OxDb.SharedCore.Website.Constants;
using OxDb.SharedCore.Website.Interfaces;
using OxDb.SharedCore.Website.Requests.Core;
using OxDb.SharedCore.Website.Requests.Interfaces;
using OxDb.SharedCore.Website.Responses.Core;
using OxDb.SharedCore.Website.Responses.Interfaces;
using OxDb.SharedGame.GameAuth.WebApi.RefreshToken;
using OxDb.SharedPlatform.Accounts.WebApi.AccountAuth;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.Networking;

namespace OxDb.Client.Networking.Services
{
    public delegate Awaitable WebResultsHandler(string txt, List<FullWebRequest> requests, CancellationToken token);


    public enum EWebRequestState
    {
        Pending,
        Complete,
    }


    public class SecurityData
    {
        public string BasicAuthToken { get; set; }
        public string FullToken { get; set; }
    }

    public class FullWebRequest
    {
        public IWebRequest Request;
        public CancellationToken Token;
        public Type ResponseType { get; set; }
        public object ResponseObject { get; set; }
        public EWebRequestState State { get; set; } = EWebRequestState.Pending;
    }

    public interface IClientWebRequestService : IInitializable, IGameTokenService, IWebRequestService
    {
        FullWebRequest SendMainServerRequest(IWebRequest request, CancellationToken token, Type responseType = null);
        Awaitable<T> SendMainServerRequestAsync<T>(IWebRequest webRequest, CancellationToken token) where T : class, IWebResponse;

        Awaitable HandleResponses(string txt, List<FullWebRequest> requests, CancellationToken token);

        Awaitable<ResponseEnvelope<TResponseType>> SendRawWebRequest<TResponseType>(string url, HttpMethod method, object requestData = null, SecurityData security = null)
            where TResponseType : class;

        string GetUserRequestId();

        WebServerRequestSet CreatePopulatedWebServerRequestSet();

        Awaitable<bool> RefreshSessionTokenAsync(CancellationToken token);

    }

    public class ClientWebRequestService : BaseWebRequestService, IClientWebRequestService
    {

        protected IServiceLocator _loc = null;
        protected IClientGameState _gs = null;
        protected IGameData _gameData = null;
        private IClientUpdateService _updateService = null;
        private IClientConfigContainer _configContainer = null;
        private IDispatcher _dispatcher = null;
        private IClientAppService _appService = null;

        protected override async ValueTask<RawHttpResponse> ExecuteTransportAsync(string url, WebRequestOptions options, CancellationToken token)
        {
            UnityWebRequest webRequest = options.Method switch
            {
                HttpMethodType.Post => new UnityWebRequest(url, "POST"),
                HttpMethodType.Put => new UnityWebRequest(url, "PUT"),
                HttpMethodType.Delete => new UnityWebRequest(url, "DELETE"),
                _ => UnityWebRequest.Get(url)
            };

            webRequest.timeout = options.TimeoutMilliseconds / 1000;
            webRequest.downloadHandler = new DownloadHandlerBuffer();

            if (!string.IsNullOrEmpty(options.AuthToken))
            {
                webRequest.SetRequestHeader("Authorization", $"Bearer {options.AuthToken}");
            }

            foreach (KeyValuePair<string, string> header in options.Headers)
            {
                webRequest.SetRequestHeader(header.Key, header.Value);
            }

            if (options.Method == HttpMethodType.Post || options.Method == HttpMethodType.Put)
            {
                if (options.ContentType == HttpContentType.Json && options.JsonBody != null)
                {
                    string json = base._textSerializer.SerializeToString(options.JsonBody);
                    byte[] bodyRaw = Encoding.UTF8.GetBytes(json);
                    webRequest.uploadHandler = new UploadHandlerRaw(bodyRaw);
                    webRequest.SetRequestHeader("Content-Type", "application/json");
                }
                else if (options.ContentType == HttpContentType.FormUrlEncoded && options.FormBody != null)
                {
                    // Unity natively encodes WWWForm payloads out of the box
                    WWWForm form = new WWWForm();
                    foreach (KeyValuePair<string, string> field in options.FormBody)
                    {
                        form.AddField(field.Key, field.Value);
                    }
                    webRequest.uploadHandler = new UploadHandlerRaw(form.data);
                    foreach (KeyValuePair<string, string> header in form.headers)
                    {
                        webRequest.SetRequestHeader(header.Key, header.Value);
                    }
                }
            }

            UnityWebRequestAsyncOperation operation = webRequest.SendWebRequest();

            // Wait asynchronously until the request returns complete
            while (!operation.isDone)
            {
                if (token.IsCancellationRequested)
                {
                    webRequest.Abort();
                    throw new TaskCanceledException();
                }
                await Task.Yield();
            }

            if (options.Headers.Values.Any(x => x.Contains("Bearer ")) &&
                !string.IsNullOrEmpty(webRequest.error) &&
               webRequest.error.Contains("401 Unauthorized"))
            {
                await RefreshSessionTokenAsync(token);
            }

            bool isSuccess = webRequest.result == UnityWebRequest.Result.Success;

            RawHttpResponse response = new RawHttpResponse
            {
                IsSuccess = isSuccess,
                StatusCode = (int)webRequest.responseCode,
                Data = webRequest.downloadHandler.data,
                ErrorMessage = isSuccess ? null : webRequest.error
            };

            webRequest.Dispose();
            return response;
        }

        private bool _showRequestLogs = false;

        private class ResultHandlerPair
        {
            public IWebResponse Result { get; set; } = null;
            public IClientWebResponseHandler Handler { get; set; } = null;
        }

        private Dictionary<Type, WebRequestQueue> _queues = new Dictionary<Type, WebRequestQueue>();

        private SetupDictionaryContainer<Type, IClientWebResponseHandler> _loginResponseHandlers = new SetupDictionaryContainer<Type, IClientWebResponseHandler>();


        CancellationTokenSource _webTokenSource = null;
        private CancellationToken _token;
        public void SetGameToken(CancellationToken token)
        {
            _webTokenSource?.Cancel();
            _webTokenSource?.Dispose();
            _webTokenSource = CancellationTokenSource.CreateLinkedTokenSource(token);
            _token = _webTokenSource.Token;
        }

        private const float UserRequestDelaySeconds = 0.3f;

        public override async Task Initialize(CancellationToken token)
        {

            _token = token;
            if (GameModeUtils.IsPureClientMode(_gs.GameMode))
            {
                return;
            }

            string webServerURL = _configContainer.Config.GetWebEndpoint();

            // Batch requests to fewer endpoints like in a realtime game.
            _queues[typeof(IAccountAuthRequest)] = new WebRequestQueue(_gs, token, webServerURL + CoreEndpoints.AccountAuth, UserRequestDelaySeconds, _showRequestLogs, _logService, this, _textSerializer, _gameData, _appService, null);
            _queues[typeof(IGameAuthRequest)] = new WebRequestQueue(_gs, token, webServerURL + CoreEndpoints.GameAuth, UserRequestDelaySeconds, _showRequestLogs, _logService, this, _textSerializer, _gameData, _appService, _queues[typeof(IAccountAuthRequest)]);
            _queues[typeof(IClientUserRequest)] = new WebRequestQueue(_gs, token, webServerURL + CoreEndpoints.GameClient, UserRequestDelaySeconds, _showRequestLogs, _logService, this, _textSerializer, _gameData, _appService, _queues[typeof(IGameAuthRequest)]);
            foreach (var queue in _queues.Values)
            {
                _loc.Resolve(queue);
            }

            _updateService.AddUpdate(this, ProcessRequestQueues, UpdateTypes.Late, token);

            await Task.CompletedTask;
        }

        public string GetUserRequestId()
        {
            foreach (WebRequestQueue queue in _queues.Values)
            {
                if (!string.IsNullOrEmpty(queue.RequestId))
                {
                    return queue.RequestId;
                }
            }
            return null;
        }

        public async Awaitable HandleResponses(string txt, List<FullWebRequest> requests, CancellationToken token)
        {
            try
            {
                WebServerResponseSet responseSet = _textSerializer.Deserialize<WebServerResponseSet>(txt);

                List<ResultHandlerPair> responsePairs = new List<ResultHandlerPair>();

                foreach (IWebResponse response in responseSet.Responses)
                {
                    if (_showRequestLogs)
                    {
                        _logService.Info("Web Response: " + response.GetType().Name);
                    }
                    bool foundAsyncRequest = false;
                    if (requests != null)
                    {
                        FullWebRequest request = requests.FirstOrDefault(x => x.ResponseType == response.GetType());
                        if (request != null)
                        {
                            request.ResponseObject = response;
                            foundAsyncRequest = true;
                        }
                    }
                    if (_loginResponseHandlers.TryGetValue(response.GetType(), out IClientWebResponseHandler handler))
                    {
                        responsePairs.Add(new ResultHandlerPair()
                        {
                            Result = response,
                            Handler = handler,
                        });
                    }
                    else if (!foundAsyncRequest)
                    {
                        _logService.Error("Unknown Message From Login Server: " + response.GetType().Name);
                    }
                }

                if (requests != null)
                {
                    foreach (FullWebRequest fullWebRequest in requests)
                    {
                        fullWebRequest.State = EWebRequestState.Complete;
                    }
                }

                responsePairs = responsePairs.OrderByDescending(x => x.Handler.Priority()).ToList();

                foreach (ResultHandlerPair responsePair in responsePairs)
                {
                    await responsePair.Handler.Process(responsePair.Result, token);
                }
            }
            catch (Exception ex)
            {
                _logService.Exception(ex, "ProcessWebResponses");
            }


        }


        private class WebRequestQueue
        {
            private List<FullWebRequest> _queue = new List<FullWebRequest>();
            private List<FullWebRequest> _pending = new List<FullWebRequest>();
            private float _delaySeconds;
            private CancellationToken _token;
            private IClientGameState _gs = null;
            private DateTime _lastResponseReceivedTime = DateTime.UtcNow;
            private WebRequestQueue _parentQueue;
            private List<WebRequestQueue> _childQueues = new List<WebRequestQueue>();
            private string _fullEndpoint;
            private ILogService _logService = null;
            private IClientWebRequestService _webRequestService = null;
            private ITextSerializer _serializer = null;
            private IAwaitableService _awaitableService = null;

            private bool _showRequestLogs = false;

            public WebRequestQueue(IClientGameState gs, CancellationToken token, string fullEndpoint, float delaySeconds, bool showRequestLogs,
                ILogService logService, IClientWebRequestService clientWebService,
                ITextSerializer serializer, IGameData gameData, IClientAppService appService, WebRequestQueue parentQueue)
            {
                _gs = gs;
                _parentQueue = parentQueue;
                _logService = logService;
                _serializer = serializer;
                _showRequestLogs = showRequestLogs;
                _webRequestService = clientWebService;
                if (_parentQueue != null)
                {
                    _parentQueue.AddChildQueue(this);
                }
                _delaySeconds = delaySeconds;
                _token = token;
                _fullEndpoint = fullEndpoint;

            }

            public void AddChildQueue(WebRequestQueue childQueue)
            {
                _childQueues.Add(childQueue);
            }

            public FullWebRequest AddRequest(IWebRequest request, CancellationToken token, Type responseType = null)
            {
                FullWebRequest fullWebRequest = new FullWebRequest() { Request = request, Token = token, ResponseType = responseType };
                _queue.Add(fullWebRequest);
                return fullWebRequest;
            }

            public bool HavePendingRequests()
            {
                return _pending.Count > 0 || (DateTime.UtcNow - _lastResponseReceivedTime).TotalSeconds < _delaySeconds;
            }

            public bool HaveRequests()
            {
                return _queue.Count > 0 || HavePendingRequests();
            }

            private string _requestId;
            public string RequestId => _requestId;

            public void ProcessRequests()
            {
                if (_parentQueue != null && _parentQueue.HaveRequests())
                {
                    return;
                }

                foreach (WebRequestQueue childQueue in _childQueues)
                {
                    if (childQueue.HavePendingRequests())
                    {
                        return;
                    }
                }

                if (HavePendingRequests() || _queue.Count < 1)
                {
                    return;
                }

                _pending = new List<FullWebRequest>(_queue);
                _queue.Clear();

                ClientWebRequest req = new ClientWebRequest();


                _requestId = HashUtils.NewGuid();
                WebServerRequestSet requestSet = _webRequestService.CreatePopulatedWebServerRequestSet();

                List<CancellationToken> allTokens = _pending.Select(x => x.Token).Distinct().ToList();
                allTokens.Add(_token);

                CancellationTokenSource fullRequestSource = CancellationTokenSource.CreateLinkedTokenSource(_token);

                requestSet.Requests.AddRange(_pending.Select(x => x.Request));

                if (_showRequestLogs)
                {
                    foreach (IWebRequest request in requestSet.Requests)
                    {
                        _logService.Info("Send Web Request: " + request.GetType().Name);
                    }
                }

                WebServerRequestEnvelope envelope = new WebServerRequestEnvelope()
                {
                    Json = _serializer.SerializeToString(requestSet)
                };

                SecurityData security = null;

                if (requestSet.Requests.Any(x => x is ISessionRequest sreq))
                {
                    security = new SecurityData()
                    {
                        FullToken = _gs.SessionState.FullToken,
                    };
                }

                _awaitableService.ForgetAwaitable(req.SendRequest(_logService, _webRequestService, _fullEndpoint, envelope, _pending.ToList(), HandleResults, security, fullRequestSource.Token));
            }

            public async Awaitable HandleResults(string txt, List<FullWebRequest> requests, CancellationToken token)
            {
                await _webRequestService.HandleResponses(txt, requests, token);
                _lastResponseReceivedTime = DateTime.UtcNow;
                _pending.Clear();
                _requestId = "";
            }
        }


        private void ProcessRequestQueues()
        {
            foreach (WebRequestQueue queue in _queues.Values)
            {
                queue.ProcessRequests();
            }
        }

        public CancellationToken GetToken()
        {
            return _token;
        }


        public FullWebRequest SendMainServerRequest(IWebRequest request, CancellationToken token, Type responseType = null)
        {
            foreach (Type t in _queues.Keys)
            {
                if (t.IsAssignableFrom(request.GetType()))
                {
                    _queues[t].AddRequest(request, token, responseType);
                    break;
                }
            }

            FullWebRequest fullRequest = new FullWebRequest()
            {
                Request = request,
                Token = token,
                ResponseType = responseType,
                State = EWebRequestState.Pending,
            };
            return fullRequest;
        }

        public async Awaitable<T> SendMainServerRequestAsync<T>(IWebRequest webRequest, CancellationToken token) where T : class, IWebResponse
        {
            FullWebRequest fullRequest = SendMainServerRequest(webRequest, token, typeof(T));

            while (fullRequest.State == EWebRequestState.Pending)
            {
                await Awaitable.NextFrameAsync(token);
            }

            return (T)fullRequest.ResponseObject;
        }


        public async Awaitable<ResponseEnvelope<TResponseType>> SendRawWebRequest<TResponseType>(string url, HttpMethod method, object requestData = null, SecurityData security = null) where TResponseType : class
        {
            WebRequestOptions opts = new WebRequestOptions()
            {
                Method = method == HttpMethod.Post ? HttpMethodType.Post : HttpMethodType.Get,
                ContentType = HttpContentType.Json,
            };

            if (requestData != null)
            {
                opts.JsonBody = requestData;
            }
            ;

            if (security != null)
            {
                if (!string.IsNullOrEmpty(security.BasicAuthToken))
                {
                    opts.Headers["Authorization"] = "Basic " + security.BasicAuthToken;
                }
                else if (!string.IsNullOrEmpty(security.FullToken))
                {
                    opts.Headers["Authorization"] = "Bearer " + security.FullToken;
                }
            }

            ResponseEnvelope<TResponseType> responseEnvelope = await SendAsync<TResponseType>(url, opts, _token);

            return responseEnvelope;
        }

        public async Awaitable<bool> RefreshSessionTokenAsync(CancellationToken token)
        {

            WebServerRequestSet set = CreatePopulatedWebServerRequestSet();

            set.Requests.Add(new RefreshGameTokenRequest()
            {
                RefreshToken = _gs.SessionState.RefreshToken,
                GameUserId = _gs.GameUserId,
            });

            WebServerRequestEnvelope requestEnvelope = new WebServerRequestEnvelope()
            {
                Json = _textSerializer.SerializeToString(set),
            };

            ResponseEnvelope<WebServerResponseSet> responseEnvelope = await SendRawWebRequest<WebServerResponseSet>(_configContainer.Config.GetWebEndpoint() + CoreEndpoints.RefreshToken, HttpMethod.Post, requestEnvelope);

            if (responseEnvelope.Response != null)
            {
                RefreshGameTokenResponse response = (RefreshGameTokenResponse)responseEnvelope.Response.Responses.FirstOrDefault(x => x.GetType() == typeof(RefreshGameTokenResponse));

                if (response != null && !string.IsNullOrEmpty(response.FullToken)
                    && !string.IsNullOrEmpty(response.RefreshToken)
                   && !string.IsNullOrEmpty(response.GameSessionId))
                {
                    _gs.SessionState = response;

                    _dispatcher.Dispatch(new UpdateDefaultLogalyticsPayload());
                    return true;
                }
            }

            _dispatcher.Dispatch(new ShowSplashScreen() { Message = "Error Connecting to Server", ShowResetButton = true });
            // Need to Login again.
            return false;

        }

        public WebServerRequestSet CreatePopulatedWebServerRequestSet()
        {
            WebServerRequestSet requestSet = new WebServerRequestSet()
            {
                GameUserId = _gs.GameUserId,
                ClientVersion = _appService.Version,
                ClientPlatform = _appService.GetPlatformName(),
                ClientSessionId = _gs.ClientSessionId,
                RequestId = GetUserRequestId(),
                ClientEnv = _configContainer.Config.Env,

            };
            return requestSet;
        }

    }
}