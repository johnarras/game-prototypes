using Assets.Scripts.ClientEvents;
using Assets.Scripts.ClientEvents.UI;
using Assets.Scripts.Crawler.Buffs.Services;
using Assets.Scripts.Crawler.Services.CrawlerMaps;
using Assets.Scripts.GameObjects;
using Genrpg.Shared.Buildings.Constants;
using Genrpg.Shared.Crawler.Constants;
using Genrpg.Shared.Crawler.Info.Services;
using Genrpg.Shared.Crawler.Maps.Constants;
using Genrpg.Shared.Crawler.Maps.Entities;
using Genrpg.Shared.Crawler.Options.Constants;
using Genrpg.Shared.Crawler.Parties.PlayerData;
using Genrpg.Shared.Crawler.Party.Services;
using Genrpg.Shared.Crawler.States.Constants;
using Genrpg.Shared.Crawler.States.Entities;
using Genrpg.Shared.Crawler.States.StateHelpers.Buildings;
using Genrpg.Shared.Crawler.TimeOfDay.Constants;
using Genrpg.Shared.Crawler.TimeOfDay.Services;
using Genrpg.Shared.UI.Constants;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.InputSystem;

namespace Genrpg.Shared.Crawler.States.StateHelpers.Guilds
{
    public class GuildMainHelper : BuildingStateHelper
    {
        private ITimeOfDayService _timeService = null;
        private ICrawlerMapService _mapService = null;
        private IScreenService _screenService = null;
        private IAssetService _assetService = null;
        private IBuffService _buffService = null;
        private IClientEntityService _clientEntityService = null;
        private IInfoService _infoService = null;
        private IPartyService _partyService = null;

        public override ECrawlerStates HelperKey => ECrawlerStates.GuildMain;
        public override long TriggerBuildingId() { return BuildingTypes.Guild; }

        public override async Task<CrawlerStateData> Init(CrawlerStateData currentState, CrawlerStateAction action, CancellationToken token)
        {
            CrawlerStateData stateData = CreateStateData();
            stateData.BGSpriteName = CrawlerClientConstants.BuildingImage;

            PartyData party = _crawlerService.GetParty();

            _statService.FullyRestParty(party);

            string txt = action.ExtraData as string;

            if (txt != null && txt == "GenerateWorld")
            {
                CrawlerMap map = _worldService.GetMap(party.CurrPos.MapId);

                if (map == null || map.CrawlerMapTypeId == CrawlerMapTypes.City)
                {
                    await _worldService.GenerateWorld(party);
                    _mapService.CleanMap();
                }
            }

            _partyService.AddClickPartyMemberButtons(stateData, party);

            stateData.Actions.Add(new CrawlerStateAction("Add Char", Key.A, ECrawlerStates.AddMember));
            stateData.Actions.Add(new CrawlerStateAction("Remove Char", Key.R, ECrawlerStates.RemoveMember));
            stateData.Actions.Add(new CrawlerStateAction("Delete Char", Key.D, ECrawlerStates.DeleteMember));
            stateData.Actions.Add(new CrawlerStateAction("Create Char", Key.C, ECrawlerStates.ChooseRace));
            stateData.Actions.Add(new CrawlerStateAction("New Maps", Key.N, ECrawlerStates.GuildMain, null, "GenerateWorld"));

            if (_optionsService.HasOption(party, CrawlerOptions.PartyUpgrades))
            {
                stateData.Actions.Add(new CrawlerStateAction("Upgrades", Key.U, ECrawlerStates.UpgradeParty));
            }
            stateData.Actions.Add(new CrawlerStateAction("Party Order", Key.P, ECrawlerStates.PartyOrder,
                () =>
                {
                    _crawlerService.ChangeState(ECrawlerStates.PartyOrder, token, ECrawlerStates.GuildMain);
                }));
            stateData.Actions.Add(new CrawlerStateAction("Info", Key.I, ECrawlerStates.GuildMain, onClickAction:
                () =>
                {
                    _dispatcher.Dispatch(new OpenScreen(ScreenNames.CrawlerInfo));
                }));
            if (party.ActiveParty.Count > 0)
            {
                stateData.Actions.Add(new CrawlerStateAction("Enter Map", Key.E, ECrawlerStates.ExploreWorld));
            }

            if (!party.HasFlag(PartyFlags.InGuildHall))
            {
                await _timeService.UpdateTime(party, ECrawlerTimeUpdateTypes.GuildHall);
            }
            party.AddFlags(PartyFlags.InGuildHall);

            stateData.Actions.Add(new CrawlerStateAction("", Key.Escape, ECrawlerStates.DoNotChangeState,
                () =>
                {
                    if (_screenService.GetScreen(ScreenNames.CrawlerMainMenu) == null)
                    {
                        _dispatcher.Dispatch(new OpenScreen(ScreenNames.CrawlerMainMenu));
                    }
                }, hideText: true));

            stateData.Actions.Add(new CrawlerStateAction(_buffService.GetMissingBuffsString(party), Key.None, ECrawlerStates.DoNotChangeState, null,
                pointerEnterAction: (GameObject go) =>
                {
                    GText gt = _clientEntityService.GetComponent<GText>(go);

                    if (gt != null)
                    {
                        List<string> lines = _infoService.GetInfoLines(_textService.GetLinkUnderMouse(gt));
                        if (lines.Count > 0)
                        {
                            _dispatcher.Dispatch(new ShowInfoPanelEvent() { Lines = lines });
                        }
                    }
                },

                pointerExitAction: (GameObject go) =>
                {
                    _dispatcher.Dispatch(new HideInfoPanelEvent());
                }
                ));




            while (_assetService.IsDownloading())
            {
                await Awaitable.NextFrameAsync(token);
            }

            _dispatcher.Dispatch(new CloseScreen(ScreenNames.Loading));

            return stateData;

        }
    }
}


