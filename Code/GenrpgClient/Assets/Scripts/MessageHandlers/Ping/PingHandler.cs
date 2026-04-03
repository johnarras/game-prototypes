using System.Threading;
using UnityEngine;

public class PingHandler : BaseClientMapMessageHandler<Genrpg.Shared.Pings.Messages.Ping>
{

    protected override async Awaitable InnerProcess(Genrpg.Shared.Pings.Messages.Ping msg, CancellationToken token)
    {
    }
}


