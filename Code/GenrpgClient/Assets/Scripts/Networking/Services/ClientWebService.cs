#define SHOW_SEND_RECEIVE_MESSAGES
#undef SHOW_SEND_RECEIVE_MESSAGES

using Assets.Scripts.Awaitables;
using Assets.Scripts.ClientEvents;
using Assets.Scripts.Login.Messages;
using Genrpg.Shared.Client.Core;
using Genrpg.Shared.Client.Tokens;
using Genrpg.Shared.Core.Constants;
using Genrpg.Shared.GameAuth.WebApi.RefreshToken;
using Genrpg.Shared.GameSettings;
using Genrpg.Shared.HelperClasses;
using Genrpg.Shared.Interfaces;
using Genrpg.Shared.Logging.Interfaces;
using Genrpg.Shared.Serialization.Interfaces;
using Genrpg.Shared.Serialization.Services;
using Genrpg.Shared.Website.Interfaces;
using Genrpg.Shared.Website.Messages;
using Genrpg.Shared.Website.Messages.Error;
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

public delegate void WebResultsHandler(string txt, List<FullWebRequest> requests, CancellationToken token);


public enum EWebRequestState
{
    Pending,
    Complete,
}


public class SecurityData
{
    public string BasicAuthToken { get; set; }
    public string SessionToken { get; set; }
}

public class FullWebRequest
{
    public IWebRequest Request;
    public CancellationToken Token;
    public Type ResponseType { get; set; }
    public object ResponseObject { get; set; }
    public ErrorResponse ErrorResponse { get; set; }
    public EWebRequestState State { get; set; } = EWebRequestState.Pending;
}

public interface IClientWebService : IInitializable, IGameTokenService
{
    void SendAccountAuthWebRequest(IAccountAuthRequest loginRequest, CancellationToken token);
    Awaitable<T> SendAccountAuthWebRequestAsync<T>(IAccountAuthRequest userRequest, CancellationToken token);


    void SendGameAuthWebRequest(IGameAuthRequest loginRequest, CancellationToken token);
    Awaitable<T> SendGameAuthWebRequestAsync<T>(IGameAuthRequest userRequest, CancellationToken token);

    void SendClientUserWebRequest(IClientUserRequest data, CancellationToken token);
    Awaitable<T> SendClientUserWebRequestAsync<T>(IClientUserRequest userRequest, CancellationToken token);

    void SendNoUserWebRequest(INoUserRequest data, CancellationToken token);
    Awaitable<T> SendNoUserWebRequestAsync<T>(INoUserRequest userRequest, CancellationToken token);

    void HandleResponses(string txt, List<FullWebRequest> requests, CancellationToken token);

    Awaitable<TResponseType> SendRequest<TResponseType>(string url, HttpMethod method, object requestData = null, SecurityData security = null)
        where TResponseType : class;

}


public class ClientWebService : IClientWebService
{

    private bool _showRequestLogs = false;

    private class ResultHandlerPair
    {
        public IWebResponse Result { get; set; } = null;
        public IClientWebResponseHandler Handler { get; set; } = null;
    }

    private Dictionary<string, WebRequestQueue> _queues = new Dictionary<string, WebRequestQueue>();

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

    public ClientWebService()
    {
    }

    // Web endpoints.
    public const string GameClientEndpoint = "/game-client";
    public const string AccountAuthEndpoint = "/account-auth";
    public const string GameAuthEndpoint = "/game-auth";
    public const string NoUserEndpoint = "/nouser";
    public const string RefreshTokenEndpoint = "/refresh-token";

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
        _queues[AccountAuthEndpoint] = new WebRequestQueue(_gs, token, webServerURL + AccountAuthEndpoint, UserRequestDelaySeconds, _showRequestLogs, _logService, this, _serializer, _gameData, null);
        _queues[GameAuthEndpoint] = new WebRequestQueue(_gs, token, webServerURL + GameAuthEndpoint, UserRequestDelaySeconds, _showRequestLogs, _logService, this, _serializer, _gameData, _queues[AccountAuthEndpoint]);
        _queues[GameClientEndpoint] = new WebRequestQueue(_gs, token, webServerURL + GameClientEndpoint, UserRequestDelaySeconds, _showRequestLogs, _logService, this, _serializer, _gameData, _queues[GameAuthEndpoint]);
        _queues[NoUserEndpoint] = new WebRequestQueue(_gs, token, webServerURL + NoUserEndpoint, 0, _showRequestLogs, _logService, this, _serializer, _gameData, null);
        foreach (var queue in _queues.Values)
        {
            _loc.Resolve(queue);
        }

        _updateService.AddUpdate(this, ProcessRequestQueues, UpdateTypes.Late, token);


        await Task.CompletedTask;
    }

    public void HandleResponses(string txt, List<FullWebRequest> requests, CancellationToken token)
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
                responsePair.Handler.Process(responsePair.Result, token);
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
        private IAwaitableService _awaitableService = null;
        private ITextSerializer _serializer = null;
        private IGameData _gameData = null;

        private bool _showRequestLogs = false;

        public WebRequestQueue(IClientGameState gs, CancellationToken token, string fullEndpoint, float delaySeconds, bool showRequestLogs, ILogService logService, IClientWebService _clientWebService,
            ITextSerializer serializer, IGameData gameData, WebRequestQueue parentQueue)
        {
            _gs = gs;
            _parentQueue = parentQueue;
            _logService = logService;
            _serializer = serializer;
            _gameData = gameData;
            _showRequestLogs = showRequestLogs;
            this._clientWebService = _clientWebService;
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

            WebServerRequestSet requestSet = new WebServerRequestSet()
            {
                GameUserId = _gs.GameUserId,
            };

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
                    SessionToken = _gs.SessionState.SessionToken,
                };
            }

            _awaitableService.ForgetAwaitable(req.SendRequest(_logService, _clientWebService, _fullEndpoint, envelope, _pending.ToList(), HandleResults, security, fullRequestSource.Token));
        }

        public void HandleResults(string txt, List<FullWebRequest> requests, CancellationToken token)
        {
            _clientWebService.HandleResponses(txt, requests, token);
            _lastResponseReceivedTime = DateTime.UtcNow;
            _pending.Clear();
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

    public void SendAccountAuthWebRequest(IAccountAuthRequest authRequest, CancellationToken token)
    {
        SendRequest(AccountAuthEndpoint, authRequest, token);
    }

    public async Awaitable<T> SendAccountAuthWebRequestAsync<T>(IAccountAuthRequest userRequest, CancellationToken token)
    {
        return await SendWebRequestAsync<T>(AccountAuthEndpoint, userRequest, token);
    }


    public void SendGameAuthWebRequest(IGameAuthRequest authRequest, CancellationToken token)
    {
        SendRequest(GameAuthEndpoint, authRequest, token);
    }

    public async Awaitable<T> SendGameAuthWebRequestAsync<T>(IGameAuthRequest userRequest, CancellationToken token)
    {
        return await SendWebRequestAsync<T>(GameAuthEndpoint, userRequest, token);
    }


    public void SendClientUserWebRequest(IClientUserRequest userRequest, CancellationToken token)
    {
        SendRequest(GameClientEndpoint, userRequest, token);
    }

    public async Awaitable<T> SendClientUserWebRequestAsync<T>(IClientUserRequest userRequest, CancellationToken token)
    {
        return await SendWebRequestAsync<T>(GameClientEndpoint, userRequest, token);
    }


    public void SendNoUserWebRequest(INoUserRequest noUserRequest, CancellationToken token)
    {
        SendRequest(NoUserEndpoint, noUserRequest, token);
    }

    public async Awaitable<T> SendNoUserWebRequestAsync<T>(INoUserRequest userRequest, CancellationToken token)
    {
        return await SendWebRequestAsync<T>(NoUserEndpoint, userRequest, token);
    }

    private FullWebRequest SendRequest(string endpoint, IWebRequest loginRequest, CancellationToken token, Type responseType = null)
    {
        if (_queues.TryGetValue(endpoint, out WebRequestQueue queue))
        {
            return queue.AddRequest(loginRequest, token, responseType);
        }
        return null;
    }

    private async Awaitable<T> SendWebRequestAsync<T>(string endpoint, IWebRequest webRequest, CancellationToken token)
    {
        FullWebRequest fullRequest = SendRequest(endpoint, webRequest, token, typeof(T));

        while (fullRequest.State == EWebRequestState.Pending)
        {
            await Awaitable.NextFrameAsync(token);
        }

        return (T)fullRequest.ResponseObject;
    }

    public async Awaitable<TResponseType> SendRequest<TResponseType>(string url, HttpMethod method, object requestData = null, SecurityData security = null) where TResponseType : class
    {

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
                if (!string.IsNullOrEmpty(security.SessionToken))
                {
                    request.SetRequestHeader("Authorization", "Bearer " + security.SessionToken);
                }
            }


            request.downloadHandler = new DownloadHandlerBuffer();
            UnityWebRequestAsyncOperation operation = request.SendWebRequest();
            while (!operation.isDone) await System.Threading.Tasks.Task.Yield();

            if (request.result == UnityWebRequest.Result.Success)
            {
                if (typeof(TResponseType) == typeof(string) ||
                    typeof(TResponseType) == typeof(object))
                {
                    return request.downloadHandler.text as TResponseType;
                }
                else
                {
                    TResponseType response = textSerializer.Deserialize<TResponseType>(request.downloadHandler.text);
                    return response;
                }
            }
            else if (security != null && !string.IsNullOrEmpty(security.SessionToken) &&
                request.error.Contains("401 Unauthorized"))
            {

                if (await RefreshSessionTokenAsync(_token))
                {
                    security.SessionToken = _gs.SessionState.SessionToken;
                    return await SendRequest<TResponseType>(url, method, requestData, security);
                }
                else
                {


                    Debug.Log("Failed to refresh session token.");
                    return default(TResponseType);
                }
            }
            else
            {
                Debug.LogError($"Error: {request.error} - {request.downloadHandler.text}");
                return default(TResponseType);
            }
        }
    }

    public async Awaitable<bool> RefreshSessionTokenAsync(CancellationToken token)
    {

        WebServerRequestSet set = new WebServerRequestSet();
        set.Requests.Add(new RefreshGameTokenRequest()
        {
            RefreshToken = _gs.SessionState.RefreshToken,
            GameUserId = _gs.GameUserId,
        });

        WebServerRequestEnvelope envelope = new WebServerRequestEnvelope()
        {
            Json = _serializer.SerializeToString(set),
        };

        WebServerResponseSet responseSet = await SendRequest<WebServerResponseSet>(_configContainer.Config.GetWebEndpoint() + RefreshTokenEndpoint, HttpMethod.Post, envelope);

        if (responseSet != null)
        {
            RefreshGameTokenResponse response = (RefreshGameTokenResponse)responseSet.Responses.FirstOrDefault(x => x.GetType() == typeof(RefreshGameTokenResponse));

            if (response != null && !string.IsNullOrEmpty(response.SessionToken) && !string.IsNullOrEmpty(response.RefreshToken))
            {
                _gs.SessionState = response;
                return true;
            }
        }

        _dispatcher.Dispatch(new ShowSplashScreen() { Message = "Error Connecting to Server", ShowResetButton = true });
        // Need to Login again.
        return false;

    }
}


