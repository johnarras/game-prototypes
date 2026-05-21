using System.Threading;
using System.Threading.Tasks;
using UnityEngine;

public class PingHandler : BaseClientMapMessageHandler<OxDb.SharedGame.Pings.Messages.Ping>
{

    protected override async Awaitable InnerProcess(OxDb.SharedGame.Pings.Messages.Ping msg, CancellationToken token)
    {
        await Task.CompletedTask;
    }
}


