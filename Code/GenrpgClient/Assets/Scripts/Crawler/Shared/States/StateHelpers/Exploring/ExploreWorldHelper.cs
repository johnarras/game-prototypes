using Assets.Scripts.ClientEvents.UI;
using Assets.Scripts.Crawler.Services.CrawlerMaps;
using Genrpg.Shared.Crawler.Constants;
using Genrpg.Shared.Crawler.Maps.Constants;
using Genrpg.Shared.Crawler.Maps.Entities;
using Genrpg.Shared.Crawler.Parties.PlayerData;
using Genrpg.Shared.Crawler.Party.Services;
using Genrpg.Shared.Crawler.States.Constants;
using Genrpg.Shared.Crawler.States.Entities;
using Genrpg.Shared.Crawler.Worlds.Entities;
using Genrpg.Shared.UI.Constants;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using UnityEngine.InputSystem;

namespace Genrpg.Shared.Crawler.States.StateHelpers.Exploring
{
    public class ExploreWorldHelper : BaseStateHelper
    {

        private IScreenService _screenService = null;
        private ICrawlerMoveService _moveService = null;
        private IPartyService _partyService = null;
        public class NamedMoveKey
        {
            public char Key { get; private set; }
            public string Name { get; private set; }

            public NamedMoveKey(char key, string name)
            {
                Key = key;
                Name = name;
            }
        }

        private ICrawlerMapService _crawlerMapService = null;

        public override ECrawlerStates HelperKey => ECrawlerStates.ExploreWorld;
        public override bool IsTopLevelState() { return true; }
        public override bool HideBigPanels() { return true; }
        public override bool ShouldDispatchClickKeys() { return true; }

        public override async Task<CrawlerStateData> Init(CrawlerStateData currentData, CrawlerStateAction action, CancellationToken token)
        {
            EnterCrawlerMapData mapData = action.ExtraData as EnterCrawlerMapData;

            PartyData party = _crawlerService.GetParty();

            _combatService.EndCombat(party);

            if (mapData == null)
            {
                CrawlerStateData topLevelData = _crawlerService.GetTopLevelState();
                if (topLevelData != null && topLevelData.Id == ECrawlerStates.ExploreWorld)
                {
                    topLevelData.DoNotTransitionToThisState = true;
                    _dispatcher.Dispatch(topLevelData);
                    return topLevelData;
                }
            }

            CrawlerStateData stateData = CreateStateData();
            stateData.ClearBGImage = true;

            _partyService.AddClickPartyMemberButtons(stateData, party);

            stateData.Actions.Add(new CrawlerStateAction(null, rowFiller: true));

            CrawlerWorld world = await _worldService.GetWorld(party.WorldId);

            CrawlerMap map = _worldService.GetMap(party.CurrPos.MapId);

            stateData.AddText("Use WASDQE to move.");

            stateData.Actions.Add(new CrawlerStateAction("Cast", Key.C));
            stateData.Actions.Add(new CrawlerStateAction("Map", Key.M));
            stateData.Actions.Add(new CrawlerStateAction("Quest Log", Key.L));
            stateData.Actions.Add(new CrawlerStateAction("Info", Key.I));
            stateData.Actions.Add(new CrawlerStateAction("Recall", Key.R));
            stateData.Actions.Add(new CrawlerStateAction("Options", Key.O));
            stateData.Actions.Add(new CrawlerStateAction("Party Order", Key.P));
            stateData.Actions.Add(new CrawlerStateAction("Guild Hall", Key.G, ECrawlerStates.GuildMain));
            stateData.Actions.Add(new CrawlerStateAction("Buffs", Key.B));
            stateData.Actions.Add(new CrawlerStateAction("Use Item", Key.U));


            if (map != null)
            {
                if (party.HasFlag(PartyFlags.HasRecall) && map.CrawlerMapTypeId != CrawlerMapTypes.City)
                {
                }
                else if (map.CrawlerMapTypeId == CrawlerMapTypes.City && party.RecallPos.MapId > 0)
                {
                    CrawlerMap recallMap = _worldService.GetMap(party.RecallPos.MapId);

                    if (recallMap != null)
                    {
                        stateData.Actions.Add(new CrawlerStateAction("Return to " + recallMap.Name + "?", Key.R, ECrawlerStates.ReturnToSafety));
                    }
                }
            }

            CrawlerMap firstCity = world.Maps.FirstOrDefault(x => x.CrawlerMapTypeId == CrawlerMapTypes.City);

            EnterCrawlerMapData firstCityData = new EnterCrawlerMapData()
            {
                MapId = firstCity.IdKey,
                MapX = firstCity.Width / 2,
                MapZ = firstCity.Height / 2,
                MapRot = 0,
                World = world,
                Map = firstCity,
            };

            stateData.Actions.Add(new CrawlerStateAction(null, rowFiller: true));

            IReadOnlyList<MovementKeyCode> moveKeys = _moveService.GetMovementKeyCodes();

            stateData.AddText("USE WASD or Arrow Keys to Move");
            foreach (MovementKeyCode nmk in moveKeys)
            {
                stateData.Actions.Add(new CrawlerStateAction(nmk.Name, nmk.Key, ECrawlerStates.DoNotChangeState, () =>
                {
                    _moveService.AddMovementKeyInput(nmk.Key, token);
                }, hideText: true));
            }

            if (mapData == null)
            {
                if (world.GetMap(party.CurrPos.MapId) != null)
                {
                    mapData = new EnterCrawlerMapData()
                    {
                        MapId = party.CurrPos.MapId,
                        MapX = party.CurrPos.X,
                        MapZ = party.CurrPos.Z,
                        MapRot = party.CurrPos.Rot,
                        World = world,
                        Map = world.GetMap(party.CurrPos.MapId),
                    };
                }
                else
                {
                    mapData = firstCityData;
                }
            }
            else if (mapData.ReturnToSafety)
            {
                mapData = firstCityData;
            }

            stateData.Actions.Add(new CrawlerStateAction("", Key.Escape, ECrawlerStates.ExploreWorld,
                () =>
                {
                    if (!_screenService.ShowingLayerScreen(ScreenLayers.Screens))
                    {
                        _dispatcher.Dispatch(new OpenScreen(ScreenNames.CrawlerMainMenu));
                    }
                }, hideText: true));

            await _crawlerMapService.EnterMap(party, mapData, token);

            return stateData;
        }
    }
}


