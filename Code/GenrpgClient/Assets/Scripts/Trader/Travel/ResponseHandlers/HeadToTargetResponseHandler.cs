using Assets.Scripts.ClientEvents.UI;
using Assets.Scripts.FloatingText.ClientEvents;
using Assets.Scripts.Login.Messages.Core;
using Assets.Scripts.Trader.ClientEvents;
using Assets.Scripts.Trader.Travel.ClientEvents;
using OxDb.SharedGame.Attributes.Services;
using OxDb.SharedGame.Core.PlayerData;
using OxDb.SharedGame.Trader.Constants;
using OxDb.SharedGame.Trader.Travel.WebApi;
using OxDb.SharedGame.UI.Constants;
using System.Threading;
using System.Threading.Tasks;
using UnityEngine;

namespace Assets.Scripts.Trader.MessageHandlers.Travelling
{
    public class HeadToTargetResponseHandler : BaseClientWebResponseHandler<HeadToTargetResponse>
    {
        private ICalcAttributeService _calcAttributeService = null;
        protected override async Awaitable InnerProcess(HeadToTargetResponse response, CancellationToken token)
        {
            if (!string.IsNullOrEmpty(response.ErrorMessage))
            {
                _dispatcher.Dispatch(new ShowFloatingText(response.ErrorMessage, EFloatingTextArt.Error));
                return;
            }

            CoreData coreData = _gs.ch.Get<CoreData>();

            coreData.Vars[TraderVars.FromX] = response.FromX;
            coreData.Vars[TraderVars.FromY] = response.FromY;
            coreData.Vars[TraderVars.ToX] = response.ToX;
            coreData.Vars[TraderVars.ToY] = response.ToY;
            coreData.Vars[TraderVars.CityId] = response.ToCityId;
            coreData.Vars[TraderVars.DistanceGone] = 0;
            coreData.Vars[TraderVars.TotalDistanceToTarget] = response.TotalDistanceToTarget;

            long oldFlags = coreData.Vars[TraderVars.Flags];
            coreData.Vars[TraderVars.Flags] = response.NewTraderFlags;

            if (oldFlags != response.NewTraderFlags)
            {
                coreData.Vars[TraderVars.Flags] = response.NewTraderFlags;
                await _calcAttributeService.CalcBuffs(_gs.ch);
            }

            _dispatcher.Dispatch(new CloseScreen(ScreenNames.TraderCityRoads));
            _dispatcher.Dispatch(new UpdateTraderHUD() { FullRefresh = true });
            _dispatcher.Dispatch(new UpdateTraderMapAngle());
            await Task.CompletedTask;
        }
    }
}
