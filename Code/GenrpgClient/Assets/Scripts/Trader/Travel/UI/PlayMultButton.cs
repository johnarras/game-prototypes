using Assets.Scripts.Awaitables;
using Assets.Scripts.Trader.ClientEvents;
using Genrpg.Shared.Attributes.Services;
using Genrpg.Shared.Core.PlayerData;
using Genrpg.Shared.PlayMultiplier.Services;
using Genrpg.Shared.PlayMultiplier.WebApi;
using Genrpg.Shared.Trader.Constants;
using System;
using UnityEngine;

namespace Assets.Scripts.Trader.Travel.UI
{
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

            ShowData();
        }

        private int _queuedPlayMult = -1;

        private void ClickSetMultButton()
        {
            CoreData coreData = _gs.ch.Get<CoreData>();
            int maxMult = _playMultService.GetMaxMult(coreData);

            if (coreData.Vars[TraderVars.Mult] < maxMult)
            {
                coreData.Vars.Add(TraderVars.Mult, 1);
            }
            else
            {
                coreData.Vars[TraderVars.Mult] = 1;
            }

            ShowData();

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
                ShowData();
            }
        }

        private void ShowData()
        {
            CoreData coreData = _gs.ch.Get<CoreData>();
            _uiService.SetText(PlayMultText, "Travel " + coreData.Vars[TraderVars.Mult] + " Day" + (coreData.Vars[TraderVars.Mult] > 1 ? "s" : ""));

            long maxMult = _playMultService.GetMaxMult(coreData);

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
