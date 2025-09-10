using Assets.Scripts.UI.Constants;
using Genrpg.Shared.Crawler.Constants;
using Genrpg.Shared.Crawler.Maps.Constants;
using Genrpg.Shared.Crawler.Maps.Entities;
using Genrpg.Shared.Crawler.Parties.PlayerData;
using Genrpg.Shared.Crawler.Party.Services;
using Genrpg.Shared.Crawler.States.Constants;
using Genrpg.Shared.Crawler.States.Entities;
using Genrpg.Shared.Crawler.Worlds.Entities;
using Genrpg.Shared.Entities.Constants;
using Genrpg.Shared.Riddles.Settings;
using Genrpg.Shared.Stats.Constants;
using Genrpg.Shared.Utils;
using Genrpg.Shared.Zones.Settings;
using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Genrpg.Shared.Crawler.States.StateHelpers.Exploring
{
    public class EnterMapStateHelper : BaseStateHelper
    {

        private IPartyService _partyService = null;

        public override ECrawlerStates Key => ECrawlerStates.EnterMap;
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

            CrawlerMap nextMap = world.GetMap(detail.EntityId);

            if (nextMap == null)
            {
                return new CrawlerStateData(ECrawlerStates.Error, true) { ExtraData = "No such map exists." };
            }
            stateData.BGImageOnly = true;



            ZoneType zoneType = _gameData.Get<ZoneTypeSettings>(_gs.ch).Get(nextMap.ZoneTypeId);


            stateData.BGImageOnly = true;
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

            bool havePartyBuff = _partyService.HasPartyBuff(party, EntityTypes.Riddle, 0);
            if (nextMapStatus == null && !didComplete)
            {
                if (nextMap.MapQuestItemId > 0)
                {
                    if (havePartyBuff)
                    {
                        stateData.AddText("The party can bypass Riddle.");
                    }
                    else
                    {
                        WorldQuestItem itemNeeded = null;

                        if (!party.QuestItems.HasBit(nextMap.MapQuestItemId))
                        {
                            WorldQuestItem wqi = world.QuestItems.FirstOrDefault(x => x.IdKey == nextMap.MapQuestItemId);
                            if (wqi != null)
                            {
                                itemNeeded = wqi;
                            }
                        }

                        if (itemNeeded != null)
                        {
                            stateData.AddText(nextMap.Name + " requires the following to enter: ");

                            stateData.AddText(itemNeeded.Name);

                            AddSpaceAction(stateData);

                            stateData.Actions.Add(new CrawlerStateAction("", CharCodes.Escape, ECrawlerStates.ExploreWorld));

                            return stateData;
                        }
                    }
                }

                if (nextMap.EntranceRiddleRequired())
                {
                    if (havePartyBuff)
                    {
                        stateData.AddText("The party can bypass Riddles.");
                    }
                    else
                    {
                        string[] descLines = nextMap.EntranceRiddle.Text.Split("\n");

                        stateData.AddText("Answer this to pass:\n");
                        stateData.AddBlankLine();

                        for (int d = 0; d < descLines.Length; d++)
                        {
                            if (!string.IsNullOrEmpty(descLines[d]))
                            {
                                if (!nextMap.HasFlag(CrawlerMapFlags.ShowFullRiddleText))
                                {
                                    stateData.AddText(descLines[d].Substring(0, (int)MathUtils.Min(descLines[d].Length, 6)) + "...");
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

                        RiddleType riddleType = _gameData.Get<RiddleTypeSettings>(_gs.ch).Get(nextMap.EntranceRiddle?.RiddleTypeId ?? 0);

                        if (riddleType == null || (!riddleType.IsToggle && !riddleType.IsObject))
                        {
                            stateData.AddInputField("Answer:", delegate (string text)
                            {
                                string normalizedRiddleName = nextMap.EntranceRiddle.Answer.ToLower().Trim();

                                string normalizedText = text.ToLower().Trim();

                                normalizedText = new string(text.Where(char.IsLetterOrDigit).ToArray()).ToLower();

                                if (!string.IsNullOrEmpty(normalizedText) && normalizedText == normalizedRiddleName)
                                {
                                    EnterCrawlerMapData enterMapData = new EnterCrawlerMapData()
                                    {
                                        MapId = nextMap.IdKey,
                                        MapX = detail.ToX,
                                        MapZ = detail.ToZ,
                                        MapRot = 0,
                                        World = world,
                                        Map = nextMap,
                                    };

                                    party.RiddlesCompleted.SetBit(party.CurrPos.MapId);
                                    _crawlerService.ChangeState(ECrawlerStates.ExploreWorld, token, enterMapData);
                                }
                                else
                                {
                                    ErrorMapCellDetail newErrorDetail = new ErrorMapCellDetail()
                                    {
                                        Detail = detail,
                                        ErrorText = nextMap.EntranceRiddle.Error,
                                    };

                                    foreach (PartyMember member in party.GetActiveParty())
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

                            if (maxBitIndex > 0 && Int64.TryParse(nextMap.EntranceRiddle.Answer, out long answerVal))
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
                                        MapId = nextMap.IdKey,
                                        MapX = detail.ToX,
                                        MapZ = detail.ToZ,
                                        MapRot = 0,
                                        World = world,
                                        Map = nextMap,
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
                                        ErrorText = nextMap.EntranceRiddle.Error,
                                    };

                                    foreach (PartyMember member in party.GetActiveParty())
                                    {
                                        member.Stats.SetCurr(StatTypes.Health, member.Stats.Curr(StatTypes.Health) / 2);
                                    }
                                    _crawlerService.ChangeState(ECrawlerStates.EnterMap, token, newErrorDetail);
                                });

                            stateData.AddText("Are you ready to continue?");
                            stateData.Actions.Add(new CrawlerStateAction("Yes, the Orbs are Set Correctly", 'Y', ECrawlerStates.DoNotChangeState,
                                onClickAction));

                            stateData.Actions.Add(new CrawlerStateAction("No, let me check the Orbs again.", 'N', ECrawlerStates.ExploreWorld));



                        }
                        else if (riddleType.IsObject)
                        {
                            int unclickedButtons = 0;

                            if (int.TryParse(nextMap.EntranceRiddle.Answer, out int allBits))
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
                                stateData.Actions.Add(new CrawlerStateAction("Yes go to the next floor.", 'Y',
                                    ECrawlerStates.ExploreWorld, () =>
                                    {
                                        EnterCrawlerMapData enterMapData = new EnterCrawlerMapData()
                                        {
                                            MapId = nextMap.IdKey,
                                            MapX = detail.ToX,
                                            MapZ = detail.ToZ,
                                            MapRot = 0,
                                            World = world,
                                            Map = nextMap,
                                        };

                                        party.RiddlesCompleted.SetBit(party.CurrPos.MapId);
                                        _crawlerService.ChangeState(ECrawlerStates.ExploreWorld, token, enterMapData);
                                    }));

                                stateData.Actions.Add(new CrawlerStateAction("No, stay on this flor..", 'N', ECrawlerStates.ExploreWorld));


                            }
                            else
                            {
                                string barText = (unclickedButtons == 1 ? "There is one bar blocking the next floor" :
                                    "There are " + unclickedButtons + " bars still blocking the next floor.");
                                stateData.Actions.Add(new CrawlerStateAction(barText));
                                stateData.Actions.Add(new CrawlerStateAction("Ok", 'O', ECrawlerStates.ExploreWorld));
                            }
                        }
                        stateData.Actions.Add(new CrawlerStateAction("", CharCodes.Escape, ECrawlerStates.ExploreWorld));

                        return stateData;
                    }
                }
            }


            stateData.Actions.Add(new CrawlerStateAction("Go to " + nextMap.GetName(detail.ToX, detail.ToZ) + " (Level: " +
                nextMap.Level + ")?\n\n", CharCodes.None, ECrawlerStates.None, null, null));

            stateData.Actions.Add(new CrawlerStateAction("Yes", 'Y', ECrawlerStates.ExploreWorld, null,
               new EnterCrawlerMapData()
               {
                   MapId = nextMap.IdKey,
                   MapX = detail.ToX,
                   MapZ = detail.ToZ,
                   MapRot = 0,
                   World = world,
                   Map = nextMap,
               }));

            stateData.Actions.Add(new CrawlerStateAction("No", 'N', ECrawlerStates.ExploreWorld));

            await Task.CompletedTask;
            return stateData;
        }
    }
}
