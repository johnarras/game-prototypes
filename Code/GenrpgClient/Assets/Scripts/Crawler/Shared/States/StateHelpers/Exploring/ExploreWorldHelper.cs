using Assets.Scripts.Crawler.Services.CrawlerMaps;
using Assets.Scripts.UI.Constants;
using Assets.Scripts.UI.Interfaces;
using Genrpg.Shared.Crawler.Constants;
using Genrpg.Shared.Crawler.Maps.Constants;
using Genrpg.Shared.Crawler.Maps.Entities;
using Genrpg.Shared.Crawler.Parties.PlayerData;
using Genrpg.Shared.Crawler.States.Constants;
using Genrpg.Shared.Crawler.States.Entities;
using Genrpg.Shared.Crawler.Worlds.Entities;
using Genrpg.Shared.UI.Constants;
using Genrpg.Shared.Utils;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Genrpg.Shared.Crawler.States.StateHelpers.Exploring
{
    public class ExploreWorldHelper : BaseStateHelper
    {

        private IScreenService _screenService;
        private ICrawlerMoveService _moveService;
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

        public override ECrawlerStates Key => ECrawlerStates.ExploreWorld;
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


            List<PartyMember> members = party.GetActiveParty();

            int maxPartySlot = 0;
            if (members.Count > 0)
            {
                maxPartySlot = members.Max(x => x.PartySlot);
            }
            if (maxPartySlot > 0)
            {
                stateData.AddText("1-" + maxPartySlot + " to view a member");
                foreach (PartyMember member in members)
                {
                    stateData.Actions.Add(new CrawlerStateAction("", (char)(member.PartySlot + '0'),
                        ECrawlerStates.ExploreWorld, extraData: member,
                        onClickAction: () =>
                        {
                            _dispatcher.Dispatch(new CrawlerCharacterScreenData() { Unit = member });
                        }));

                }
            }

            stateData.Actions.Add(new CrawlerStateAction(null, rowFiller: true));

            CrawlerWorld world = await _worldService.GetWorld(party.WorldId);

            CrawlerMap map = _worldService.GetMap(party.CurrPos.MapId);

            stateData.AddText("Use WASDQE to move.");
            stateData.Actions.Add(new CrawlerStateAction("Cast", 'C'));
            stateData.Actions.Add(new CrawlerStateAction("Map", 'M'));
            stateData.Actions.Add(new CrawlerStateAction("Quest Log", 'L'));
            stateData.Actions.Add(new CrawlerStateAction("Info", 'I'));
            stateData.Actions.Add(new CrawlerStateAction("Recall", 'R'));
            stateData.Actions.Add(new CrawlerStateAction("Options", 'O'));
            stateData.Actions.Add(new CrawlerStateAction("Party Order", 'P'));
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
                        stateData.Actions.Add(new CrawlerStateAction("Return to " + recallMap.Name + "?", 'R', ECrawlerStates.ReturnToSafety));
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
            int moveKeysShown = 0;

            IReadOnlyList<MovementKeyCode> moveKeys = _moveService.GetMovementKeyCodes();

            foreach (MovementKeyCode nmk in moveKeys)
            {
                stateData.Actions.Add(new CrawlerStateAction(nmk.Name, nmk.Key, ECrawlerStates.DoNotChangeState, () =>
                {
                    // Don't need this here because we now have click listeners on the actual movement buttons.
                    // It's a bit janky but trying to move to that system for main UI pieces.
                    //_moveService.AddMovementKeyInput(nmk.Key, token);
                }, hideText: true));
                moveKeysShown++;

                if (moveKeysShown % 3 == 0)
                {
                    stateData.Actions.Add(new CrawlerStateAction(null, rowFiller: true));
                }
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

            stateData.Actions.Add(new CrawlerStateAction("", CharCodes.Escape, ECrawlerStates.ExploreWorld,
                () =>
                {
                    if (_screenService.GetLayerScreen(ScreenLayers.Screens) == null)
                    {
                        _screenService.Open(ScreenNames.CrawlerMainMenu);
                    }
                }, hideText: true));

            await _crawlerMapService.EnterMap(party, mapData, token);

            return stateData;
        }
    }
}
