
using OxDb.SharedGame.Spells.Messages;
using System.Threading;
using System.Threading.Tasks;

namespace OxDb.Client.MessageHandlers.Spells
{
    public class FXHandler : BaseClientMapMessageHandler<FX>
    {
        protected IFxService _fxService = null;
        protected override async ValueTask InnerProcess(FX msg, CancellationToken token)
        {
            _fxService.ShowFX(msg, token);
            await Task.CompletedTask;
        }
    }
}


