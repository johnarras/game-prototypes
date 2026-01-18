using Genrpg.Shared.Logging.Interfaces;
using System.Collections.Generic;
using System.Net.Http;
using System.Threading;
using UnityEngine;

public class ClientWebRequest
{
    private IClientGameState _gs;
    private string _uri;
    private string _postData;
    private WebResultsHandler _handler = null;
    private ILogService _logService = null;
    private IClientWebService _clientWebService = null;
    const int MaxTimes = 3;
    public async Awaitable SendRequest(ILogService logService, IClientWebService webService, string uri, object postData, List<FullWebRequest> commands, WebResultsHandler handler, SecurityData security, CancellationToken token)
    {
        _logService = logService;
        _clientWebService = webService;
        _uri = uri;

        _handler = handler;
        for (int times = 0; times < MaxTimes; times++)
        {
            string text = await _clientWebService.SendRequest<string>(_uri, HttpMethod.Post, postData, security);
            if (!string.IsNullOrEmpty(text))
            {
                handler(text, commands, token);
                break;
            }
            else
            {
                await Awaitable.WaitForSecondsAsync(0.3f, token);
            }
        }
    }
}

