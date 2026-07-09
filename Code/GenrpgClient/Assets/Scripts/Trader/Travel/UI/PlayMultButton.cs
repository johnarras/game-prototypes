using Assets.Scripts.Awaitables;
using Assets.Scripts.Trader.ClientEvents;
using OxDb.SharedCore.Client.Interfaces;
using OxDb.SharedGame.Attributes.Services;
using OxDb.SharedGame.Core.PlayerData;
using OxDb.SharedGame.PlayMultiplier.Services;
using OxDb.SharedGame.PlayMultiplier.WebApi;
using OxDb.SharedGame.Trader.Constants;
using System;
using System.Threading;
using System.Threading.Tasks;
using UnityEngine;

namespace Assets.Scripts.Trader.Travel.UI
{
    public class UpdateMaxPlayMult : IClientEvent
    {

    }

    public class PlayMultButton : BaseBehaviour
    {

        private ISharedPlayMultService _playMultService = null;
        private IClientWebService _webService = null;
        private ICalcAttributeService _calcAttributeService = null;
        private IAwaitableService _awaitableService = null;

        public GImage PlayMultBG;
        public GText PlayMultText;


        public GButton SetMultButton;

        public Color MaxTierColor;
        public Color LowerTierColor;


        private int _playMultRequestsSent = 0;
        public override void Init()
        {
            _dispatcher.AddListener<SetPlayMultResponse>(OnSetPlayMultResponse, GetToken());

            _uiService.SetButton(SetMultButton, GetName(), ClickSetMultButton);

            _dispatcher.AddListener<UpdateMaxPlayMult>(OnUpdateMaxPlayMult, GetToken());

            _ = ShowData();
        }

        private int _queuedPlayMult = -1;

        private async ValueTask ClickSetMultButton(CancellationToken token)
        {
            CoreData coreData = await _gs.ch.GetAsync<CoreData>();
            int maxMult = await _playMultService.GetMaxMult(_gs.ch);

            if (coreData.Vars[TraderVars.Mult] < maxMult)
            {
                coreData.Vars.Add(TraderVars.Mult, 1);
            }
            else
            {
                coreData.Vars[TraderVars.Mult] = 1;
            }

            _ = ShowData();

            if (_playMultRequestsSent > 0)
            {
                _queuedPlayMult = coreData.Vars[TraderVars.Mult];
                return;
            }
            else
            {
                _queuedPlayMult = -1;
            }
            _playMultRequestsSent++;
            _webService.SendWebRequest(new SetPlayMultRequest() { PlayMult = coreData.Vars[TraderVars.Mult] }, GetToken());
        }

        private void OnSetPlayMultResponse(SetPlayMultResponse response)
        {
            --_playMultRequestsSent;

            if (_playMultRequestsSent <= 0)
            {
                if (_queuedPlayMult > 0)
                {
                    _playMultRequestsSent++;
                    _webService.SendWebRequest(new SetPlayMultRequest() { PlayMult = _queuedPlayMult }, GetToken());
                    _queuedPlayMult = 0;
                    return;
                }

                CoreData coreData = _gs.ch.Get<CoreData>();
                coreData.Vars[TraderVars.Mult] = response.NewPlayMult;
                coreData.Vars[TraderVars.MultBonusSpeed] = response.MultBonusSpeed;
                _ = ShowData();
            }
        }

        private void OnUpdateMaxPlayMult(UpdateMaxPlayMult updateMult)
        {
            _ = ShowData();
        }

        private async ValueTask ShowData()
        {
            CoreData coreData = _gs.ch.Get<CoreData>();
            _uiService.SetText(PlayMultText, "Travel " + coreData.Vars[TraderVars.Mult] + " Day" + (coreData.Vars[TraderVars.Mult] > 1 ? "s" : ""));

            long maxMult = await _playMultService.GetMaxMult(_gs.ch);

            PlayMultBG?.SetColor(coreData.Vars[TraderVars.Mult] < maxMult ? LowerTierColor : MaxTierColor);


            _awaitableService.ForgetAwaitable(ShowDataAsync());

        }

        private async Awaitable ShowDataAsync()
        {

            try
            {
                await _calcAttributeService.CalcBuffs(_gs.ch);

                _dispatcher.Dispatch(new UpdateTraderHUD());
            }
            catch (Exception e)
            {
                _logService.Exception(e, "PlayMultButtonShowData");
            }
        }
    }
}
