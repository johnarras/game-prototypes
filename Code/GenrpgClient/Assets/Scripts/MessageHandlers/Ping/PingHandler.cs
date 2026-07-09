using System.Threading;
using System.Threading.Tasks;

public class PingHandler : BaseClientMapMessageHandler<OxDb.SharedGame.Pings.Messages.Ping>
{

    protected override async ValueTask InnerProcess(OxDb.SharedGame.Pings.Messages.Ping msg, CancellationToken token)
    {
        await Task.CompletedTask;
    }
}


