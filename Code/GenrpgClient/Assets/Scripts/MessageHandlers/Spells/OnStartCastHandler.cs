using OxDb.SharedGame.Spells.Messages;
using System.Threading;
using System.Threading.Tasks;

namespace OxDb.Client.MessageHandlers.Spells
{
    public class OnStartCastHandler : BaseClientMapMessageHandler<OnStartCast>
    {
        protected override async ValueTask InnerProcess(OnStartCast msg, CancellationToken token)
        {
            if (_objectManager.GetGridItem(msg.CasterId, out ClientMapObjectGridItem gridItem))
            {
                gridItem.Controller?.StartCasting(msg);
            }
            await Task.CompletedTask;
        }
    }
}


