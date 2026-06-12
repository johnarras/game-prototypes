
using Assets.Scripts.Awaitables;
using Assets.Scripts.ClientEvents.UI;
using OxDb.SharedCore.Logalytics.Constants;
using OxDb.SharedCore.Logalytics.Interfaces;
using OxDb.SharedGame.DataStores.Categories.PlayerData.Units;
using OxDb.SharedGame.Ftue.Constants;
using OxDb.SharedGame.Ftue.Services;
using OxDb.SharedGame.Ftue.Settings.Steps;
using OxDb.SharedGame.UI.Constants;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;

namespace Assets.Scripts.Ftue.Services
{
    public class ClientFtueService : FtueService
    {
        protected IDispatcher _dispatcher = null;
        protected IAwaitableService _awaitableService = null;
        protected IAnalyticsService _analyticsService = null;


        protected override void ShowAnalytics(string analyticsEventName, FtueStep step)
        {

            if (step == null)
            {
                return;
            }

            _analyticsService.TrackEvent(analyticsEventName, new Dictionary<string, string>()
            {
                [AnalyticsKeys.FtueStepName] = step.Name,
            },
            new Dictionary<string, double>() { [AnalyticsKeys.FtueStepId] = step.IdKey });
        }

        public override async Task<FtueStep> StartNextStep(IUnitDataLookup lookup)
        {
            FtueStep newStep = await base.StartNextStep(lookup);

            if (newStep == null)
            {
                return null;
            }

            _awaitableService.ForgetAwaitable(ShowClientFtueStep(newStep));

            return newStep;
        }

        private async Awaitable ShowClientFtueStep(FtueStep newStep)
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


