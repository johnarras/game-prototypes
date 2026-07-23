using OxDb.Client.Networking.Services;
using OxDb.SharedCore.Logalytics.Interfaces;
using OxDb.SharedCore.WebRequests.Services;
using System.Collections.Generic;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using UnityEngine;

public class ClientWebRequest
{
    private IClientGameState _gs;
    private string _uri;
    private string _postData;
    private WebResultsHandler _handler = null;
    private ILogService _logService = null;
    private IClientWebRequestService _clientWebService = null;
    const int MaxTimes = 3;
    public async Awaitable SendRequest(ILogService logService, IClientWebRequestService webService, string uri, object postData, List<FullWebRequest> commands, WebResultsHandler handler, SecurityData security, CancellationToken token)
    {
        _logService = logService;
        _clientWebService = webService;
        _uri = uri;

        _handler = handler;
        for (int times = 0; times < MaxTimes; times++)
        {
            ResponseEnvelope<string> responseEnvelope = await _clientWebService.SendRawWebRequest<string>(_uri, HttpMethod.Post, postData, security);
            if (!string.IsNullOrEmpty(responseEnvelope.Response))
            {
                await handler(responseEnvelope.Response, commands, token);
                break;
            }
            else
            {
                await Task.Delay(300);
            }
        }
    }
}

