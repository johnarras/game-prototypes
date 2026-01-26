using Assets.Scripts.Crawler.Maps;
using Assets.Scripts.UI.Constants;
using Genrpg.Shared.Crawler.Constants;
using Genrpg.Shared.Crawler.Maps.Constants;
using Genrpg.Shared.Crawler.Maps.Entities;
using Genrpg.Shared.Crawler.Options.Constants;
using Genrpg.Shared.Crawler.Parties.PlayerData;
using Genrpg.Shared.Crawler.Party.Services;
using Genrpg.Shared.Crawler.States.Constants;
using Genrpg.Shared.Crawler.States.Entities;
using Genrpg.Shared.Crawler.Worlds.Entities;
using Genrpg.Shared.Entities.Constants;
using Genrpg.Shared.Riddles.Settings;
using Genrpg.Shared.Stats.Constants;
using Genrpg.Shared.Utils;
using Genrpg.Shared.Zones.Constants;
using Genrpg.Shared.Zones.Settings;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using UnityEngine.InputSystem;

namespace Genrpg.Shared.Crawler.States.StateHelpers.Exploring
{
    public class EnterMapStateHelper : BaseStateHelper
    {

        private IPartyService _partyService = null;
        private ICrawlerMapGenService _mapGenService = null;

        public override ECrawlerStates HelperKey => ECrawlerStates.EnterMap;
        public override long TriggerDetailEntityTypeId() { return EntityTypes.Map; }
        protected override bool OnlyUseBGImage() { return true; }

        public override async Task<CrawlerStateData> Init(CrawlerStateData currentData, CrawlerStateAction action, CancellationToken token)
        {
            CrawlerStateData stateData = CreateStateData();

            string errorText = null;
            MapCellDetail detail = action.ExtraData as MapCellDetail;

            ErrorMapCellDetail errorDetail = action.ExtraData as ErrorMapCellDetail;

            if (errorDetail != null)
            {
                detail = errorDetail.Detail;
                errorText = errorDetail.ErrorText;
            }

            if (detail == null || detail.EntityTypeId != EntityTypes.Map)
            {
                return new CrawlerStateData(ECrawlerStates.Error, true) { ExtraData = "Missing map at this coordinate." };
            }

            PartyData party = _crawlerService.GetParty();

            CrawlerWorld world = await _worldService.GetWorld(party.WorldId);

            CrawlerMap currMap = world.GetMap(party.CurrPos.MapId);

            List<CrawlerMap> nextMaps = new List<CrawlerMap>();

            CrawlerMap currNextMap = world.GetMap(detail.EntityId);

            if (!_optionsService.HasOption(party, CrawlerOptions.FullWorld))
            {
                if (detail.EntityId > 1)
                {
                    world.Maps = world.Maps.Where(x => x.IdKey == 1 || x.IdKey == currMap.IdKey).ToList();
                    world.ClearCache();

                    List<long> mapIds = new List<long>();

                    mapIds.Add(detail.EntityId);

                    if (detail.EntityId == 2 && currMap.IdKey == 1)
                    {
                        if (party.MaxLevelEntered > detail.EntityId)
                        {
                            mapIds.Add(party.MaxLevelEntered);

                            if (!mapIds.Contains(party.MaxLevelEntered / 2))
                            {
                                mapIds.Add(party.MaxLevelEntered / 2);
                            }

                            if (!mapIds.Contains(party.MaxLevelEntered * 3 / 4))
                            {
                                mapIds.Add(party.MaxLevelEntered * 3 / 4);
                            }

                            if (!mapIds.Contains(party.MaxLevelEntered / 4))
                            {
                                mapIds.Add(party.MaxLevelEntered / 4);
                            }
                        }
                    }

                    mapIds = mapIds.Where(x => x > 1).OrderBy(x => x).ToList();

                    foreach (long mapId in mapIds)
                    {

                        world.Seed = _rand.Next();

                        int enterX = party.CurrPos.X;
                        int enterZ = party.CurrPos.Z;

                        if (party.CurrPos.MapId == 1)
                        {
                            enterX = detail.X;
                            enterZ = detail.Z;
                        }


                        currNextMap = await _mapGenService.GenerateRoguelikeDungeonLevel(party, world, mapId, enterX, enterZ, token);

                        nextMaps.Add(currNextMap);
                        currNextMap.IdKey = mapId;

                        if (party.MaxLevelEntered < currNextMap.Level)
                        {
                            party.MaxLevelEntered = currNextMap.Level;
                        }

                        if (currMap != null && currMap.IdKey > 1)
                        {
                            currNextMap.Name = currMap.Name;
                        }
                    }

                    MapCellDetail prevDetail = currNextMap.Details.FirstOrDefault(x => x.EntityTypeId == EntityTypes.Map && x.EntityId < currNextMap.IdKey);
                    if (prevDetail != null)
                    {
                        detail.ToX = prevDetail.X;
                        detail.ToZ = prevDetail.Z;
                    }

                    await _worldService.SaveWorld(world);
                }
                else
                {
                    MapCellDetail targetDetail = currNextMap.Details.FirstOrDefault(x => x.EntityTypeId == EntityTypes.Map && x.EntityId == 2);
                    if (targetDetail != null)
                    {
                        _mapGenService.OneWayLink(world, currMap.IdKey, detail.X, detail.Z, 1, targetDetail.X, targetDetail.Z);
                    }

                    MapCellDetail newDetail = currMap.Details.FirstOrDefault(x => x.EntityTypeId == EntityTypes.Map && x.EntityId == 1);

                    if (newDetail != null)
                    {
                        detail.ToX = newDetail.ToX;
                        detail.ToZ = newDetail.ToZ;
                        targetDetail.ToX = newDetail.ToX;
                        targetDetail.ToZ = newDetail.ToZ;
                    }

                    nextMaps.Add(currNextMap);
                }
            }
            else
            {
                CrawlerMap nmap = world.GetMap(detail.EntityId);
                if (nmap != null)
                {
                    nextMaps.Add(nmap);
                }
            }

            if (nextMaps.Count < 1)
            {
                return new CrawlerStateData(ECrawlerStates.Error, true) { ExtraData = "No such map exists." };
            }

            nextMaps = nextMaps.OrderBy(x => x.IdKey).ToList();

            stateData.BGImageOnly = true;

            ZoneType zoneType = _gameData.Get<ZoneTypeSettings>(_gs.ch).Get(nextMaps[0].ZoneTypeId);

            if (nextMaps[0].IdKey > 1 && !_optionsService.HasOption(party, CrawlerOptions.FullWorld))
            {
                zoneType = _gameData.Get<ZoneTypeSettings>(_gs.ch).Get(ZoneTypes.Dungeon);
            }
            if (zoneType != null && !string.IsNullOrEmpty(zoneType.Icon))
            {
                stateData.BGSpriteName = zoneType.Icon;
            }
            else
            {
                stateData.BGSpriteName = CrawlerClientConstants.DefaultWorldBG;
            }


            CrawlerMapStatus nextMapStatus = party.GetMapStatus(detail.EntityId, false);

            bool didComplete = party.CompletedMaps.HasBit(detail.EntityId);

            #region Riddles
            bool havePartyBuff = _partyService.HasPartyBuff(party, EntityTypes.Riddle, 0);
            if (nextMapStatus == null && !didComplete && _optionsService.HasOption(party, CrawlerOptions.FullWorld))
            {
                if (nextMaps[0].MapQuestItemId > 0)
                {
                    if (havePartyBuff)
                    {
                        stateData.AddText("The party can bypass Riddle.");
                    }
                    else
                    {
                        WorldQuestItem itemNeeded = null;

                        if (!party.QuestItems.HasBit(nextMaps[0].MapQuestItemId))
                        {
                            WorldQuestItem wqi = world.QuestItems.FirstOrDefault(x => x.IdKey == nextMaps[0].MapQuestItemId);
                            if (wqi != null)
                            {
                                itemNeeded = wqi;
                            }
                        }

                        if (itemNeeded != null)
                        {
                            stateData.AddText(nextMaps[0].Name + " requires the following to enter: ");

                            stateData.AddText(itemNeeded.Name);

                            AddSpaceAction(stateData);

                            stateData.Actions.Add(new CrawlerStateAction("", Key.Escape, ECrawlerStates.ExploreWorld));

                            return stateData;
                        }
                    }
                }

                if (nextMaps[0].EntranceRiddleRequired())
                {
                    if (havePartyBuff)
                    {
                        stateData.AddText("The party can bypass Riddles.");
                    }
                    else
                    {
                        string[] descLines = nextMaps[0].EntranceRiddle.Text.Split("\n");

                        stateData.AddText("Answer this to pass:\n");
                        stateData.AddBlankLine();

                        for (int d = 0; d < descLines.Length; d++)
                        {
                            if (!string.IsNullOrEmpty(descLines[d]))
                            {
                                if (!nextMaps[0].HasFlag(CrawlerMapFlags.ShowFullRiddleText))
                                {
                                    stateData.AddText(descLines[d].Substring(0, (int)MathUtil.Min(descLines[d].Length, 6)) + "...");
                                }
                                else
                                {
                                    stateData.AddText(descLines[d]);
                                }
                            }
                        }

                        if (string.IsNullOrEmpty(errorText))
                        {
                            stateData.AddBlankLine();

                        }
                        else
                        {
                            stateData.AddText(_textService.HighlightText(errorText, TextColors.ColorRed));
                        }

                        RiddleType riddleType = _gameData.Get<RiddleTypeSettings>(_gs.ch).Get(nextMaps[0].EntranceRiddle?.RiddleTypeId ?? 0);

                        if (riddleType == null || (!riddleType.IsToggle && !riddleType.IsObject))
                        {
                            stateData.AddInputField("Answer:", delegate (string text)
                            {
                                string normalizedRiddleName = nextMaps[0].EntranceRiddle.Answer.ToLower().Trim();

                                string normalizedText = text.ToLower().Trim();

                                normalizedText = new string(text.Where(char.IsLetterOrDigit).ToArray()).ToLower();

                                if (!string.IsNullOrEmpty(normalizedText) && normalizedText == normalizedRiddleName)
                                {
                                    EnterCrawlerMapData enterMapData = new EnterCrawlerMapData()
                                    {
                                        MapId = nextMaps[0].IdKey,
                                        MapX = detail.ToX,
                                        MapZ = detail.ToZ,
                                        MapRot = 0,
                                        World = world,
                                        Map = nextMaps[0],
                                    };

                                    party.RiddlesCompleted.SetBit(party.CurrPos.MapId);
                                    _crawlerService.ChangeState(ECrawlerStates.ExploreWorld, token, enterMapData);
                                }
                                else
                                {
                                    ErrorMapCellDetail newErrorDetail = new ErrorMapCellDetail()
                                    {
                                        Detail = detail,
                                        ErrorText = nextMaps[0].EntranceRiddle.Error,
                                    };

                                    foreach (PartyMember member in party.ActiveParty)
                                    {
                                        member.Stats.SetCurr(StatTypes.Health, member.Stats.Curr(StatTypes.Health) / 2);
                                    }
                                    _crawlerService.ChangeState(ECrawlerStates.EnterMap, token, newErrorDetail);
                                }
                            });
                        }
                        else if (riddleType.IsToggle)
                        {
                            int maxBitIndex = currMap.RiddleHints.Hints.DefaultIfEmpty().Max(h => h.Index);

                            bool togglesAreCorrect = true;

                            if (maxBitIndex > 0 && Int64.TryParse(nextMaps[0].EntranceRiddle.Answer, out long answerVal))
                            {
                                for (int i = 0; i < maxBitIndex; i++)
                                {
                                    if (party.HasRiddleBitIndex(i) !=
                                        (FlagUtils.IsSet(answerVal, (1 << i))))
                                    {
                                        togglesAreCorrect = false;
                                        break;
                                    }
                                }
                            }


                            Action onClickAction =
                                (togglesAreCorrect ?
                                () =>
                                {
                                    EnterCrawlerMapData enterMapData = new EnterCrawlerMapData()
                                    {
                                        MapId = nextMaps[0].IdKey,
                                        MapX = detail.ToX,
                                        MapZ = detail.ToZ,
                                        MapRot = 0,
                                        World = world,
                                        Map = nextMaps[0],
                                    };

                                    party.RiddlesCompleted.SetBit(party.CurrPos.MapId);
                                    _crawlerService.ChangeState(ECrawlerStates.ExploreWorld, token, enterMapData);
                                }
                            :
                                () =>
                                {
                                    ErrorMapCellDetail newErrorDetail = new ErrorMapCellDetail()
                                    {
                                        Detail = detail,
                                        ErrorText = nextMaps[0].EntranceRiddle.Error,
                                    };

                                    foreach (PartyMember member in party.ActiveParty)
                                    {
                                        member.Stats.SetCurr(StatTypes.Health, member.Stats.Curr(StatTypes.Health) / 2);
                                    }
                                    _crawlerService.ChangeState(ECrawlerStates.EnterMap, token, newErrorDetail);
                                });

                            stateData.AddText("Are you ready to continue?");
                            stateData.Actions.Add(new CrawlerStateAction("Yes, the Orbs are Set Correctly", Key.Y, ECrawlerStates.DoNotChangeState,
                                onClickAction));

                            stateData.Actions.Add(new CrawlerStateAction("No, let me check the Orbs again.", Key.N, ECrawlerStates.ExploreWorld));



                        }
                        else if (riddleType.IsObject)
                        {
                            int unclickedButtons = 0;

                            if (int.TryParse(nextMaps[0].EntranceRiddle.Answer, out int allBits))
                            {
                                for (int i = 0; i < 32; i++)
                                {
                                    if (!FlagUtils.IsSet(allBits, 1 << i))
                                    {
                                        break;
                                    }
                                    if (!party.HasRiddleBitIndex(i))
                                    {
                                        unclickedButtons++;
                                    }
                                }
                            }
                            if (unclickedButtons == 0)
                            {

                                stateData.Actions.Add(new CrawlerStateAction("The path is clear, do you wish to go?"));
                                stateData.Actions.Add(new CrawlerStateAction("Yes go to the next floor.", Key.Y,
                                    ECrawlerStates.ExploreWorld, () =>
                                    {
                                        EnterCrawlerMapData enterMapData = new EnterCrawlerMapData()
                                        {
                                            MapId = nextMaps[0].IdKey,
                                            MapX = detail.ToX,
                                            MapZ = detail.ToZ,
                                            MapRot = 0,
                                            World = world,
                                            Map = nextMaps[0],
                                        };

                                        party.RiddlesCompleted.SetBit(party.CurrPos.MapId);
                                        _crawlerService.ChangeState(ECrawlerStates.ExploreWorld, token, enterMapData);
                                    }));

                                stateData.Actions.Add(new CrawlerStateAction("No, stay on this flor..", Key.N, ECrawlerStates.ExploreWorld));


                            }
                            else
                            {
                                string barText = (unclickedButtons == 1 ? "There is one bar blocking the next floor" :
                                    "There are " + unclickedButtons + " bars still blocking the next floor.");
                                stateData.Actions.Add(new CrawlerStateAction(barText));
                                stateData.Actions.Add(new CrawlerStateAction("Ok", Key.O, ECrawlerStates.ExploreWorld));
                            }
                        }
                        stateData.Actions.Add(new CrawlerStateAction("", Key.Escape, ECrawlerStates.ExploreWorld));

                        return stateData;
                    }
                }
            }
            #endregion

            stateData.AddText("Enter: " + nextMaps[0].GetName(detail.ToX, detail.ToZ));
            stateData.AddBlankLine();
            for (int m = 0; m < nextMaps.Count; m++)
            {
                Key enterCode = FromChar((char)(m + 'A'));

                if (nextMaps.Count == 1)
                {
                    enterCode = Key.Y;
                }

                CrawlerMap nmap = nextMaps[m];

                AddNewMapButton(party, stateData, enterCode, nmap, detail, world);

            }
            stateData.AddBlankLine();


            stateData.Actions.Add(new CrawlerStateAction("No, let's stay here.", Key.N, ECrawlerStates.ExploreWorld));

            stateData.Actions.Add(new CrawlerStateAction("", Key.Escape, ECrawlerStates.ExploreWorld));
            await Task.CompletedTask;
            return stateData;
        }

        private void AddNewMapButton(PartyData party, CrawlerStateData stateData, Key enterCode, CrawlerMap nmap, MapCellDetail detail,
            CrawlerWorld world)
        {
            MapCellDetail prevDetail = (party.CurrPos.MapId < nmap.IdKey ? nmap.Details.FirstOrDefault(x => x.EntityTypeId == EntityTypes.Map && x.EntityId < nmap.IdKey) :
                nmap.Details.FirstOrDefault(x => x.EntityTypeId == EntityTypes.Map && x.EntityId > nmap.IdKey));

            if (!_optionsService.HasOption(party, CrawlerOptions.FullWorld))
            {
                if (prevDetail != null && nmap.IdKey > 1)
                {
                    detail.ToX = prevDetail.X;
                    detail.ToZ = prevDetail.Z;
                }
            }
            EnterCrawlerMapData newMapData = new EnterCrawlerMapData()
            {
                MapId = nmap.IdKey,
                MapX = detail.ToX,
                MapZ = detail.ToZ,
                MapRot = 0,
                World = world,
                Map = nmap,
            };

            stateData.Actions.Add(new CrawlerStateAction(enterCode + " Go to " + nmap.GetName(detail.ToX, detail.ToZ) + " (Level: " +
               nmap.Level + ")?\n\n", enterCode, ECrawlerStates.ExploreWorld, null,
                    newMapData
                ));
        }
    }
}


