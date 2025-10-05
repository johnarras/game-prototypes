using Assets.Scripts.Assets;
using Assets.Scripts.ClientEvents;
using Assets.Scripts.Crawler.Buffs.Services;
using Assets.Scripts.Crawler.Services.CrawlerMaps;
using Assets.Scripts.UI.Interfaces;
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
using Genrpg.Shared.Utils;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using UnityEngine;

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

        public override ECrawlerStates Key => ECrawlerStates.GuildMain;
        public override long TriggerBuildingId() { return BuildingTypes.Guild; }

        public override async Task<CrawlerStateData> Init(CrawlerStateData currentState, CrawlerStateAction action, CancellationToken token)
        {
            CrawlerStateData stateData = CreateStateData();
            stateData.BGSpriteName = CrawlerClientConstants.TavernImage;

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

            stateData.Actions.Add(new CrawlerStateAction("Add Char", 'A', ECrawlerStates.AddMember));
            stateData.Actions.Add(new CrawlerStateAction("Remove Char", 'R', ECrawlerStates.RemoveMember));
            stateData.Actions.Add(new CrawlerStateAction("Delete Char", 'D', ECrawlerStates.DeleteMember));
            stateData.Actions.Add(new CrawlerStateAction("Create Char", 'C', ECrawlerStates.ChooseRace));
            stateData.Actions.Add(new CrawlerStateAction("New Maps", 'N', ECrawlerStates.GuildMain, null, "GenerateWorld"));

            if (_optionsService.HasOption(party, CrawlerOptions.PartyUpgrades))
            {
                stateData.Actions.Add(new CrawlerStateAction("Upgrades", 'U', ECrawlerStates.UpgradeParty));
            }
            stateData.Actions.Add(new CrawlerStateAction("Party Order", 'P', ECrawlerStates.PartyOrder,
                () =>
                {
                    _crawlerService.ChangeState(ECrawlerStates.PartyOrder, token, ECrawlerStates.GuildMain);
                }));
            stateData.Actions.Add(new CrawlerStateAction("Info", 'I', ECrawlerStates.GuildMain, onClickAction:
                () =>
                {
                    _screenService.Open(ScreenNames.CrawlerInfo);
                }));
            if (party.GetActiveParty().Count > 0)
            {
                stateData.Actions.Add(new CrawlerStateAction("Enter Map", 'E', ECrawlerStates.ExploreWorld));
            }

            if (!party.HasFlag(PartyFlags.InGuildHall))
            {
                await _timeService.UpdateTime(party, ECrawlerTimeUpdateTypes.GuildHall);
            }
            party.AddFlags(PartyFlags.InGuildHall);

            stateData.Actions.Add(new CrawlerStateAction("", CharCodes.Escape, ECrawlerStates.DoNotChangeState,
                () =>
                {
                    if (_screenService.GetScreen(ScreenNames.CrawlerMainMenu) == null)
                    {
                        _screenService.Open(ScreenNames.CrawlerMainMenu);
                    }
                }, hideText: true));

            stateData.Actions.Add(new CrawlerStateAction(_buffService.GetMissingBuffsString(party), CharCodes.None, ECrawlerStates.DoNotChangeState, null,
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

            _screenService.Close(ScreenNames.Loading);

            return stateData;

        }
    }
}
