using Assets.Scripts.Assets;
using Assets.Scripts.Crawler.Services.CrawlerMaps;
using Assets.Scripts.UI.Interfaces;
using Genrpg.Shared.Buildings.Constants;
using Genrpg.Shared.Crawler.Constants;
using Genrpg.Shared.Crawler.Maps.Constants;
using Genrpg.Shared.Crawler.Maps.Entities;
using Genrpg.Shared.Crawler.Parties.PlayerData;
using Genrpg.Shared.Crawler.States.Constants;
using Genrpg.Shared.Crawler.States.Entities;
using Genrpg.Shared.Crawler.States.StateHelpers.Buildings;
using Genrpg.Shared.Crawler.TimeOfDay.Constants;
using Genrpg.Shared.Crawler.TimeOfDay.Services;
using Genrpg.Shared.Stats.Constants;
using Genrpg.Shared.UI.Constants;
using Genrpg.Shared.Utils;
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

        public override ECrawlerStates Key => ECrawlerStates.GuildMain;
        public override long TriggerBuildingId() { return BuildingTypes.Guild; }

        public override async Task<CrawlerStateData> Init(CrawlerStateData currentState, CrawlerStateAction action, CancellationToken token)
        {
            CrawlerStateData stateData = CreateStateData();
            stateData.BGSpriteName = CrawlerClientConstants.TavernImage;

            PartyData party = _crawlerService.GetParty();

            foreach (PartyMember member in party.Members)
            {
                member.Stats.SetCurr(StatTypes.Health, member.Stats.Max(StatTypes.Health));
                member.Stats.SetCurr(StatTypes.Mana, member.Stats.Max(StatTypes.Mana));
                member.StatusEffects.Clear();
            }

            party.Buffs.Clear();

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

            stateData.Actions.Add(new CrawlerStateAction("Add Char", 'A', ECrawlerStates.AddMember));
            stateData.Actions.Add(new CrawlerStateAction("Remove Char", 'R', ECrawlerStates.RemoveMember));
            stateData.Actions.Add(new CrawlerStateAction("Delete Char", 'D', ECrawlerStates.DeleteMember));
            stateData.Actions.Add(new CrawlerStateAction("Create Char", 'C', ECrawlerStates.ChooseRace));
            stateData.Actions.Add(new CrawlerStateAction("New Maps", 'N', ECrawlerStates.GuildMain, null, "GenerateWorld"));
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
            stateData.Actions.Add(new CrawlerStateAction("Upgrades", 'U', ECrawlerStates.UpgradeParty));

            if (!party.HasFlag(PartyFlags.InGuildHall))
            {
                await _timeService.UpdateTime(party, ECrawlerTimeUpdateTypes.Tavern);
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


            while (_assetService.IsDownloading())
            {
                await Awaitable.NextFrameAsync(token);
            }

            _screenService.Close(ScreenNames.Loading);

            return stateData;

        }
    }
}
