using OxDb.SharedCore.Entities.Constants;
using OxDb.SharedCore.GameSettings;
using OxDb.SharedCore.Interfaces;
using OxDb.SharedGame.Crawler.Parties.PlayerData;
using OxDb.SharedGame.Crawler.Party.Services;
using OxDb.SharedGame.Crawler.Temples.Settings;
using OxDb.SharedGame.Currencies.Constants;
using OxDb.SharedGame.Spells.Interfaces;
using OxDb.SharedGame.Stats.Constants;
using System;
using System.Collections.Generic;
using System.Linq;

namespace OxDb.SharedGame.Crawler.Temples.Services
{


    public class TempleResult
    {
        public string Message;
        public bool Success;
        public long Cost;
        public PartyMember Member;
    }

    public interface ITempleService : IInjectable
    {
        long GetHealingCostForMember(PartyData party, PartyMember member);
        void HealPartyMember(PartyData party, PartyMember member, TempleResult result);
    }

    public class TempleService : ITempleService
    {

        private IGameData _gameData = null;
        private IClientGameState _gs = null;
        private IPartyService _partyService = null;

        public long GetHealingCostForMember(PartyData party, PartyMember member)
        {
            TempleSettings settings = _gameData.Get<TempleSettings>(_gs.ch);

            long cost = 0;

            long missingHealth = member.Stats.Max(StatTypes.Health) - member.Stats.Curr(StatTypes.Health);

            if (missingHealth > 0)
            {
                cost += settings.CostPerMissingHealth * missingHealth;
            }

            List<IDisplayEffect> statusEffects = member.Effects.Where(x => x.EntityTypeId == EntityTypes.StatusEffect).ToList();

            if (statusEffects.Count > 0)
            {
                long maxStatusIndex = statusEffects.Max(x => x.EntityId);

                cost += settings.StatusEffectCostPerLevel * Math.Min(member.LastCombatCrawlerSpellId, settings.MaxCostLevel);
            }

            return cost;
        }

        public void HealPartyMember(PartyData party, PartyMember member, TempleResult result)
        {
            result.Member = member;
            result.Cost = GetHealingCostForMember(party, member);
            result.Success = false;

            if (result.Cost == 0)
            {
                result.Message = member.Name + " is already fine.";
                return;
            }

            if (result.Cost > party.Currencies[CoreCurrencyTypes.Coins])
            {
                result.Message = "You need " + result.Cost + " Gold to heal " + member.Name;
                return;
            }

            _partyService.AddGold(party, -result.Cost);

            member.Stats.SetCurr(StatTypes.Health, member.Stats.Max(StatTypes.Health));

            member.StatusEffects.Clear();
            List<IDisplayEffect> statusEffects = member.Effects.Where(x => x.EntityTypeId == EntityTypes.StatusEffect).ToList();


            foreach (IDisplayEffect effect in statusEffects)
            {
                member.RemoveEffect(effect);
            }

            result.Success = true;
            result.Message = member.Name + " is fully healed.";
            return;
        }
    }
}


