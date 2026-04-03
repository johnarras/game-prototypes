using Assets.Scripts.Core;
using Assets.Scripts.Crawler.ClientEvents.CombatEvents;
using Assets.Scripts.Crawler.ClientEvents.StatusPanelEvents;
using Assets.Scripts.Crawler.Services.CrawlerMaps;
using Assets.Scripts.DynamicUI.Services;
using Assets.Scripts.FloatingText.ClientEvents;
using Genrpg.Shared.Crawler.Combat.Services;
using Genrpg.Shared.Crawler.Constants;
using Genrpg.Shared.Crawler.Crawlers.Services;
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
using Genrpg.Shared.Crawler.Upgrades.Constants;
using Genrpg.Shared.Currencies.Constants;
using Genrpg.Shared.Effects.Entities;
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
        void AddActivePartyMember(PartyData party, PartyMember member);
        void RemoveActivePartyMember(PartyData party, PartyMember member);
        void AddPartyMemberToGuild(PartyData party, PartyMember member);
        void DeletePartyMemberFromGuild(PartyData party, PartyMember member);
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
        void AddCurrency(PartyData party, long entityId, long quantity);

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
        private IInputService _inputService = null;
        private ICrawlerMapService _mapService = null;
        private ICrawlerUpgradeService _upgradeService = null;
        private ICrawlerCombatService _combatService = null;
        private IDynamicUIService _dynamicUIService = null;

        public long GetMaxPartySize(PartyData party)
        {

            long upgradeBonus = (long)_upgradeService.GetPartyBonus(party, PartyUpgrades.PartySize);

            if (!_optionsService.HasOption(party, CrawlerOptions.WholeParty))
            {
                return 1 + upgradeBonus;
            }

            CrawlerSettings settings = _gameData.Get<CrawlerSettings>(_gs.ch);


            return settings.MaxPartySize + upgradeBonus;
        }

        public void AddPartyMemberToGuild(PartyData party, PartyMember member)
        {
            party.InGuild.Add(member);
        }

        public void AddActivePartyMember(PartyData party, PartyMember member)
        {
            if (party.ActiveParty.Contains(member))
            {
                return;
            }

            if (party.ActiveParty.Count < GetMaxPartySize(party))
            {
                party.ActiveParty.Add(member);
                party.InGuild.Remove(member);
            }
            else
            {
                _dispatcher.Dispatch(new ShowFloatingText("Party is limited to " + GetMaxPartySize(party) + " members!", EFloatingTextArt.Error));
            }

            FixPartySlots(party);
        }

        public void RemoveActivePartyMember(PartyData party, PartyMember member)
        {
            member.PartySlot = 0;
            party.ActiveParty.Remove(member);
            party.InGuild.Add(member);
            FixPartySlots(party);
        }

        public void DeletePartyMemberFromGuild(PartyData party, PartyMember member)
        {
            if (party.ActiveParty.Contains(member))
            {
                return;
            }
            party.InGuild.Remove(member);
            FixPartySlots(party);
        }

        public void FixPartySlots(PartyData party)
        {

            for (int i = 0; i < party.ActiveParty.Count; i++)
            {
                party.ActiveParty[i].PartySlot = i + 1;
            }

            foreach (PartyMember member in party.InGuild)
            {
                member.PartySlot = 0;
            }
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
            party.CompletedMaps.Clear();
            party.RiddlesCompleted.Clear();
            party.QuestItems.Clear();
            party.RecallPos = new MapPosition();
            party.CompletedQuests.Clear();
            party.Quests.Clear();
            OnEnterMap(party);
            ResetToFirstCity(party);
        }

        protected void ResetToFirstCity(PartyData party)
        {
            party.CurrentMap = new CurrentMapStatus();
            party.AddFlags(PartyFlags.InGuildHall);
            party.CurrPos = new MapPosition();
        }

        public void OnEnterMap(PartyData party)
        {
            party.FailedKillQuestTimes = 0;
            party.FailedItemQuestTimes = 0;
            party.RemoveFlags(PartyFlags.InGuildHall);
            if (party.LastAutoCompleteLevel != party.CurrPos.MapId)
            {
                party.LastAutoCompleteLevel = 0;
            }
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
            party.ActiveParty.Clear();
            party.InGuild.Clear();
            foreach (UpgradeStatus status in party.UpgradeStatuses)
            {
                status.RunLevel = 0;
            }

            AddGold(party, -party.Currencies[CoreCurrencyTypes.Coins]);
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
            else
            {
                ResetToFirstCity(party);
            }


            _crawlerService.ClearAllStates();
            _combatService.EndCombat(party);
            _crawlerService.ChangeState(ECrawlerStates.GuildMain, token);
            _mapService.CleanMap();

            _dispatcher.Dispatch(new RefreshPartyStatus());
            _dispatcher.Dispatch(new UpdateCombatGroups());

            return true;
        }

        public void RearrangePartySlots(PartyData party, List<PartyMember> newPartyArrangement)
        {

            List<PartyMember> addedMembers = newPartyArrangement.Except(party.ActiveParty).ToList();

            List<PartyMember> missingMembers = party.ActiveParty.Except(newPartyArrangement).ToList();

            if (addedMembers.Count > 0 || missingMembers.Count > 0)
            {
                return;
            }

            party.ActiveParty = newPartyArrangement.ToList();

            FixPartySlots(party);
        }

        public void UpdateItemBuffs(PartyData party)
        {
            party.ItemBuffs.Clear();

            foreach (PartyMember member in party.ActiveParty)
            {
                foreach (Item item in member.Equipment)
                {
                    foreach (Effect eff in item.Effects)
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
            AddCurrency(party, CoreCurrencyTypes.Coins, quantity);
        }

        public void AddCurrency(PartyData party, long entityId, long quantity)
        {
            party.Currencies.Add(entityId, quantity);
            _dynamicUIService.AddEntityQuantityVisual(EntityTypes.CoreCurrency, entityId, quantity, false);
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

            stateData.Actions.Add(new CrawlerStateAction("", _inputService.FromChar((char)(index + '0')),
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


