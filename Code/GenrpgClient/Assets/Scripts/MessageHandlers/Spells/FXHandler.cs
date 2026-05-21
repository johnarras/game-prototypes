
using OxDb.SharedGame.Spells.Messages;
using System.Threading;
using System.Threading.Tasks;
using UnityEngine;

namespace Assets.Scripts.MessageHandlers.Spells
{
    public class FXHandler : BaseClientMapMessageHandler<FX>
    {
        protected IFxService _fxService = null;
        protected override async Awaitable InnerProcess(FX msg, CancellationToken token)
        {
            _fxService.ShowFX(msg, token);
            await Task.CompletedTask;
        }
    }
}


