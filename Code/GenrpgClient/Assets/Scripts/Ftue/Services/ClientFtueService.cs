
using Assets.Scripts.Awaitables;
using Assets.Scripts.ClientEvents.UI;
using OxDb.SharedCore.Utils;
using OxDb.SharedGame.Characters.PlayerData;
using OxDb.SharedGame.Ftue.Constants;
using OxDb.SharedGame.Ftue.Services;
using OxDb.SharedGame.Ftue.Settings.Steps;
using OxDb.SharedGame.UI.Constants;
using System.Threading.Tasks;
using UnityEngine;

namespace Assets.Scripts.Ftue.Services
{
    public class ClientFtueService : FtueService
    {
        protected IDispatcher _dispatcher = null;
        protected IAwaitableService _awaitableService = null;

        public override FtueStep StartStep(IRandom random, Character ch, long ftueStepId)
        {
            FtueStep newStep = base.StartStep(random, ch, ftueStepId);

            if (newStep == null)
            {
                return null;
            }

            _awaitableService.ForgetAwaitable(ClientStartOpen(newStep));

            return newStep;
        }

        private async Awaitable ClientStartOpen(FtueStep newStep)
        {
            // Maybe open another screen or do something else before showing the popup.


            if (newStep.FtuePopupTypeId != FtuePopupTypes.NoWindow)
            {
                _dispatcher.Dispatch(new OpenScreen(ScreenNames.Ftue, newStep));
            }
            await Task.CompletedTask;
        }
    }
}


