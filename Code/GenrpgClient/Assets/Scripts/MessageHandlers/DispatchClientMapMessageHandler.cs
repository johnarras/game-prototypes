using OxDb.SharedGame.MapMessages.Interfaces;
using System.Threading;
using System.Threading.Tasks;

namespace OxDb.Client.MessageHandlers
{
    public abstract class DispatchClientMapMessageHandler<T> : BaseClientMapMessageHandler<T> where T : class, IMapApiMessage
    {
        protected override async ValueTask InnerProcess(T msg, CancellationToken token)
        {
            _dispatcher.Dispatch(msg);
            await Task.CompletedTask;
        }
    }
}


