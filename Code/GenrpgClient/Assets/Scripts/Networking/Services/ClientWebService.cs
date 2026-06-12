#define SHOW_SEND_RECEIVE_MESSAGES
#undef SHOW_SEND_RECEIVE_MESSAGES

using Assets.Scripts.Awaitables;
using Assets.Scripts.ClientEvents;
using Assets.Scripts.Logalytics.ClientEvents;
using Assets.Scripts.Login.Messages;
using Assets.Scripts.Setup.Interfaces;
using OxDb.SharedCore.Core.Constants;
using OxDb.SharedCore.GameSettings;
using OxDb.SharedCore.HelperClasses;
using OxDb.SharedCore.Interfaces;
using OxDb.SharedCore.Logalytics.Interfaces;
using OxDb.SharedCore.Serialization.Interfaces;
using OxDb.SharedCore.Serialization.Services;
using OxDb.SharedCore.Utils;
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
using UnityEditor;
using UnityEngine;
using UnityEngine.Networking;

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

public class ResponseEnvelope<TResponseType>
{
    public TResponseType ResponseData { get; set; }
    public string ErrorMessage { get; set; }
}

public interface IClientWebService : IInitializable, IGameTokenService
{
    FullWebRequest SendWebRequest(IWebRequest request, CancellationToken token, Type responseType = null);
    Awaitable<T> SendWebRequestAsync<T>(IWebRequest webRequest, CancellationToken token) where T : class, IWebResponse;

    Awaitable HandleResponses(string txt, List<FullWebRequest> requests, CancellationToken token);

    Awaitable<ResponseEnvelope<TResponseType>> SendRawWebRequest<TResponseType>(string url, HttpMethod method, object requestData = null, SecurityData security = null)
        where TResponseType : class;

    string GetUserRequestId();

    WebServerRequestSet CreatePopulatedWebServerRequestSet();

}


public class ClientWebService : IClientWebService
{

    private bool _showRequestLogs = false;

    private class ResultHandlerPair
    {
        public IWebResponse Result { get; set; } = null;
        public IClientWebResponseHandler Handler { get; set; } = null;
    }

    private Dictionary<Type, WebRequestQueue> _queues = new Dictionary<Type, WebRequestQueue>();

    private NewtonsoftTextSerializer textSerializer { get; set; } = new NewtonsoftTextSerializer();

    private SetupDictionaryContainer<Type, IClientWebResponseHandler> _loginResponseHandlers = new SetupDictionaryContainer<Type, IClientWebResponseHandler>();

    protected IServiceLocator _loc = null;
    protected IClientGameState _gs = null;
    protected IGameData _gameData = null;
    private IClientUpdateService _updateService = null;
    protected ILogService _logService = null;
    private ITextSerializer _serializer = null;
    private IClientConfigContainer _configContainer = null;
    private IDispatcher _dispatcher = null;
    private IClientAppService _appService = null;

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

    public async Task Initialize(CancellationToken token)
    {
        if (GameModeUtils.IsPureClientMode(_gs.GameMode))
        {
            return;
        }

        string webServerURL = _configContainer.Config.GetWebEndpoint();


        // Batch requests to fewer endpoints like in a realtime game.
        _queues[typeof(IAccountAuthRequest)] = new WebRequestQueue(_gs, token, webServerURL + CoreEndpoints.AccountAuth, UserRequestDelaySeconds, _showRequestLogs, _logService, this, _serializer, _gameData, _appService, null);
        _queues[typeof(IGameAuthRequest)] = new WebRequestQueue(_gs, token, webServerURL + CoreEndpoints.GameAuth, UserRequestDelaySeconds, _showRequestLogs, _logService, this, _serializer, _gameData, _appService, _queues[typeof(IAccountAuthRequest)]);
        _queues[typeof(IClientUserRequest)] = new WebRequestQueue(_gs, token, webServerURL + CoreEndpoints.GameClient, UserRequestDelaySeconds, _showRequestLogs, _logService, this, _serializer, _gameData, _appService, _queues[typeof(IGameAuthRequest)]);
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
            WebServerResponseSet responseSet = _serializer.Deserialize<WebServerResponseSet>(txt);

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
        private IClientWebService _clientWebService = null;
        private ITextSerializer _serializer = null;
        private IAwaitableService _awaitableService = null;

        private bool _showRequestLogs = false;

        public WebRequestQueue(IClientGameState gs, CancellationToken token, string fullEndpoint, float delaySeconds, bool showRequestLogs,
            ILogService logService, IClientWebService clientWebService,
            ITextSerializer serializer, IGameData gameData, IClientAppService appService, WebRequestQueue parentQueue)
        {
            _gs = gs;
            _parentQueue = parentQueue;
            _logService = logService;
            _serializer = serializer;
            _showRequestLogs = showRequestLogs;
            _clientWebService = clientWebService;
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
            WebServerRequestSet requestSet = _clientWebService.CreatePopulatedWebServerRequestSet();

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

            _awaitableService.ForgetAwaitable(req.SendRequest(_logService, _clientWebService, _fullEndpoint, envelope, _pending.ToList(), HandleResults, security, fullRequestSource.Token));
        }

        public async Awaitable HandleResults(string txt, List<FullWebRequest> requests, CancellationToken token)
        {
            await _clientWebService.HandleResponses(txt, requests, token);
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


    public FullWebRequest SendWebRequest(IWebRequest request, CancellationToken token, Type responseType = null)
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

    public async Awaitable<T> SendWebRequestAsync<T>(IWebRequest webRequest, CancellationToken token) where T : class, IWebResponse
    {
        FullWebRequest fullRequest = SendWebRequest(webRequest, token, typeof(T));

        while (fullRequest.State == EWebRequestState.Pending)
        {
            await Awaitable.NextFrameAsync(token);
        }

        return (T)fullRequest.ResponseObject;
    }


    public async Awaitable<ResponseEnvelope<TResponseType>> SendRawWebRequest<TResponseType>(string url, HttpMethod method, object requestData = null, SecurityData security = null) where TResponseType : class
    {

        ResponseEnvelope<TResponseType> responseEnvelope = new ResponseEnvelope<TResponseType>();
        using (UnityWebRequest request = new UnityWebRequest(url, method.ToString()))
        {
            if (requestData != null)
            {
                string jsonPayload = textSerializer.SerializeToString(requestData);
                byte[] bodyRaw = Encoding.UTF8.GetBytes(jsonPayload);
                request.uploadHandler = new UploadHandlerRaw(bodyRaw); ;
                request.SetRequestHeader("Content-Type", "application/json");
            }
            if (security != null)
            {
                if (!string.IsNullOrEmpty(security.BasicAuthToken))
                {
                    request.SetRequestHeader("Authorization", "Basic " + security.BasicAuthToken);
                }
                if (!string.IsNullOrEmpty(security.FullToken))
                {
                    request.SetRequestHeader("Authorization", "Bearer " + security.FullToken);
                }
            }

            request.downloadHandler = new DownloadHandlerBuffer();
            UnityWebRequestAsyncOperation operation = request.SendWebRequest();
            while (!operation.isDone)
            {
                await Task.Yield();
            }

            if (request.result == UnityWebRequest.Result.Success)
            {
                if (typeof(TResponseType) == typeof(string) ||
                    typeof(TResponseType) == typeof(object))
                {

                    responseEnvelope.ResponseData = request.downloadHandler.text as TResponseType;
                    return responseEnvelope;
                }
                else
                {
                    responseEnvelope.ResponseData = textSerializer.Deserialize<TResponseType>(request.downloadHandler.text);
                    return responseEnvelope;
                }
            }
            else if (security != null &&
                !string.IsNullOrEmpty(security.FullToken) &&
                request.error.Contains("401 Unauthorized"))
            {

                if (await RefreshSessionTokenAsync(_token))
                {
                    security.FullToken = _gs.SessionState.FullToken;
                    return await SendRawWebRequest<TResponseType>(url, method, requestData, security);
                }
                else
                {
                    _logService?.Error("Failed to refresh session token");
                    return responseEnvelope;
                }
            }
            else
            {
                responseEnvelope.ErrorMessage = request.error;

                if (Application.isPlaying)
                {
                    _logService?.Error($"Error: {request.error} - {request.downloadHandler.text}");
                }
                return responseEnvelope;
            }
        }
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
            Json = _serializer.SerializeToString(set),
        };

        ResponseEnvelope<WebServerResponseSet> responseEnvelope = await SendRawWebRequest<WebServerResponseSet>(_configContainer.Config.GetWebEndpoint() + CoreEndpoints.RefreshToken, HttpMethod.Post, requestEnvelope);

        if (responseEnvelope.ResponseData != null)
        {
            RefreshGameTokenResponse response = (RefreshGameTokenResponse)responseEnvelope.ResponseData.Responses.FirstOrDefault(x => x.GetType() == typeof(RefreshGameTokenResponse));

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


