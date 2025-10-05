using Assets.Scripts.Crawler.ClientEvents.CombatEvents;
using Assets.Scripts.Crawler.ClientEvents.StatusPanelEvents;
using Genrpg.Shared.Client.Core;
using Genrpg.Shared.Client.GameEvents;
using Genrpg.Shared.Crawler.Constants;
using Genrpg.Shared.Crawler.Currencies.Constants;
using Genrpg.Shared.Crawler.GameEvents;
using Genrpg.Shared.Crawler.Maps.Services;
using Genrpg.Shared.Crawler.Options.Constants;
using Genrpg.Shared.Crawler.Options.Services;
using Genrpg.Shared.Crawler.Parties.PlayerData;
using Genrpg.Shared.Crawler.Settings;
using Genrpg.Shared.Crawler.States.Constants;
using Genrpg.Shared.Crawler.States.Entities;
using Genrpg.Shared.Crawler.States.Services;
using Genrpg.Shared.Crawler.States.StateHelpers.Exploring;
using Genrpg.Shared.Crawler.Training.Services;
using Genrpg.Shared.Entities.Constants;
using Genrpg.Shared.GameSettings;
using Genrpg.Shared.Interfaces;
using Genrpg.Shared.Inventory.PlayerData;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Genrpg.Shared.Crawler.Party.Services
{

    public interface IPartyService : IInjectable
    {
        long GetMaxPartySize(PartyData party);
        void AddPartyMember(PartyData party, PartyMember member);
        void RemovePartyMember(PartyData party, PartyMember member);
        void DeletePartyMember(PartyData party, PartyMember member);
        void FullReset(PartyData party);
        void ResetMaps(PartyData party);
        void OnEnterMap(PartyData party);
        Task<bool> CheckIfPartyIsDead(PartyData party, CancellationToken token);
        void RearrangePartySlots(PartyData party, List<PartyMember> newPartyArrangement);
        void UpdateItemBuffs(PartyData party);
        bool HasPartyBuff(PartyData party, long entityTypeId, long entityId);
        void AddGold(PartyData party, long quantity);
        void AddClickPartyMemberButtons(CrawlerStateData stateData, PartyData party);
        void AddExp(PartyData party, PartyMember member, long quantity);

    }

    public class PartyService : IPartyService
    {
        private IGameData _gameData = null;
        private IClientGameState _gs = null;
        private IClientRandom _rand = null;
        private IDispatcher _dispatcher = null;
        private ICrawlerWorldService _crawlerWorldService = null;
        private ICrawlerService _crawlerService = null;
        private ICrawlerOptionsService _optionsService = null;
        private ITrainingService _trainingService = null;

        public long GetMaxPartySize(PartyData party)
        {

            if (_optionsService.HasOption(party, CrawlerOptions.OneCharacter))
            {
                return 1;
            }

            CrawlerSettings settings = _gameData.Get<CrawlerSettings>(_gs.ch);

            return settings.MaxPartySize;
        }

        public void AddPartyMember(PartyData party, PartyMember member)
        {
            bool didAdd = false;
            for (int i = 1; i <= GetMaxPartySize(party); i++)
            {
                if (party.GetMemberInSlot(i) == null)
                {
                    member.PartySlot = i;
                    didAdd = true;
                    break;
                }
            }


            FixPartySlots(party);

            if (!didAdd)
            {
                _dispatcher.Dispatch(new ShowFloatingText("Party is limited to " + GetMaxPartySize(party) + " members!", EFloatingTextArt.Error));
            }

        }

        public void RemovePartyMember(PartyData party, PartyMember member)
        {
            member.PartySlot = 0;
            FixPartySlots(party);
        }

        public void DeletePartyMember(PartyData party, PartyMember member)
        {
            if (member.PartySlot > 0)
            {
                return;
            }
            party.Members.Remove(member);
            FixPartySlots(party);
        }

        public void FixPartySlots(PartyData party)
        {
            List<PartyMember> currentMembers = party.Members.Where(x => x.PartySlot > 0).OrderBy(x => x.PartySlot).ToList();

            for (int i = 0; i < currentMembers.Count; i++)
            {
                if (i < GetMaxPartySize(party))
                {
                    currentMembers[i].PartySlot = i + 1;
                }
                else
                {
                    currentMembers[i].PartySlot = 0;
                }
            }
            List<PartyMember> inParty = party.Members.Where(x => x.PartySlot > 0).OrderBy(x => x.PartySlot).ToList();
            List<PartyMember> outOfParty = party.Members.Where(x => x.PartySlot == 0).ToList();
            party.Members = inParty.Concat(outOfParty).ToList();
            _dispatcher.Dispatch(new RefreshPartyStatus());
        }

        /// <summary>
        /// Use this when generating a new world to preserve the party data.
        /// But don't reset the points you get from exploring maps this run.
        /// </summary>
        /// <param name="party"></param>
        public void ResetMaps(PartyData party)
        {
            if (party.WorldId == 0)
            {
                party.WorldId = _rand.Next() % 100000000;
            }
            party.Maps = new List<CrawlerMapStatus>();
            party.CurrentMap = new CurrentMapStatus();
            party.CompletedMaps.Clear();
            party.RiddlesCompleted.Clear();
            party.QuestItems.Clear();
            party.CurrPos = new MapPosition();
            party.RecallPos = new MapPosition();
            party.CompletedQuests.Clear();
            party.Quests.Clear();
            OnEnterMap(party);
            party.AddFlags(PartyFlags.InGuildHall);
        }

        public void OnEnterMap(PartyData party)
        {
            party.FailedKillQuestTimes = 0;
            party.FailedItemQuestTimes = 0;
            party.RemoveFlags(PartyFlags.InGuildHall);
        }

        public void FullReset(PartyData party)
        {
            ResetMaps(party);
            party.LastVendorRefresh = DateTime.UtcNow.AddDays(-1);
            party.Inventory = new List<Item>();
            party.VendorBuyback = new List<Item>();
            party.VendorItems = new List<Item>();

            party.RemoveFlags(-1);
            party.DaysPlayed = 0;
            party.Members.Clear();
            foreach (UpgradeStatus status in party.UpgradeStatuses)
            {
                status.RunLevel = 0;
            }

            AddGold(party, -party.Currencies.Get(CrawlerCurrencyTypes.Gold));
            party.HourOfDay = 0;
            party.Combat = null;
            party.InitialCombat = null;

            party.AddFlags(PartyFlags.HasRecall);
        }

        public async Task<bool> CheckIfPartyIsDead(PartyData party, CancellationToken token)
        {
            if (!party.PartyIsDead())
            {
                return false;
            }
            if (_optionsService.HasOption(party, CrawlerOptions.Permadeath))
            {
                FullReset(party);
                await _crawlerWorldService.GenerateWorld(party);
            }
            _crawlerService.ChangeState(ECrawlerStates.GuildMain, token);
            _dispatcher.Dispatch(new RefreshPartyStatus());
            _dispatcher.Dispatch(new UpdateCombatGroups());

            return true;
        }

        public void RearrangePartySlots(PartyData party, List<PartyMember> newPartyArrangement)
        {
            List<PartyMember> activeMembers = party.GetActiveParty();

            List<PartyMember> missingActives = activeMembers.Except(newPartyArrangement).ToList();

            List<PartyMember> addedActives = newPartyArrangement.Except(activeMembers).ToList();

            if (missingActives.Count > 0 || addedActives.Count > 0)
            {
                return;
            }

            for (int i = 0; i < newPartyArrangement.Count; i++)
            {
                newPartyArrangement[i].PartySlot = i + 1;
            }
            FixPartySlots(party);
        }

        public void UpdateItemBuffs(PartyData party)
        {
            party.ItemBuffs.Clear();

            foreach (PartyMember member in party.GetActiveParty())
            {
                foreach (Item item in member.Equipment)
                {
                    foreach (ItemEffect eff in item.Effects)
                    {
                        if (eff.EntityTypeId != EntityTypes.Stat &&
                            eff.EntityTypeId != EntityTypes.StatPct)
                        {
                            party.AddItemBuff(eff.EntityTypeId, eff.EntityId);
                        }
                    }
                }

            }
        }

        public bool HasPartyBuff(PartyData party, long entityTypeId, long entityId)
        {
            if (party.HasItemBuff(entityTypeId, entityId))
            {
                return true;
            }
            return false;
        }

        public void AddGold(PartyData party, long quantity)
        {
            party.Currencies.Add(CrawlerCurrencyTypes.Gold, quantity);
            _dispatcher.Dispatch(new UpdateCrawlerUI());
        }


        public void AddClickPartyMemberButtons(CrawlerStateData stateData, PartyData party)
        {

            long maxPartySize = GetMaxPartySize(party);

            for (int m = 1; m <= maxPartySize; m++)
            {
                AddClickPartyMemberButton(stateData, party, m);
            }
        }

        private void AddClickPartyMemberButton(CrawlerStateData stateData, PartyData party, int index)
        {

            stateData.Actions.Add(new CrawlerStateAction("", (char)(index + '0'),
                ECrawlerStates.None,
                onClickAction: () =>
                {
                    PartyMember member = party.GetMemberInSlot(index);
                    if (member != null)
                    {
                        _dispatcher.Dispatch(new CrawlerCharacterScreenData() { Unit = member });
                    }
                }));
        }

        public void AddExp(PartyData party, PartyMember member, long quantity)
        {
            member.Exp += quantity;

            if (_optionsService.HasOption(party, CrawlerOptions.AutoLevelUp))
            {
                int levelTimes = 0;
                do
                {
                    TrainingInfo info = _trainingService.GetTrainingInfo(party, member);

                    if (info.CanLevelUp())
                    {
                        _trainingService.TrainPartyMemberLevels(party, member, 0, null);

                    }
                    else
                    {
                        break;
                    }
                }
                while (++levelTimes < 100);
            }
        }
    }
}
