using OxDb.SharedCore.GameSettings;
using OxDb.SharedCore.Interfaces;
using OxDb.SharedGame.Crawler.ManaRegen.Settings;
using OxDb.SharedGame.Crawler.Parties.PlayerData;
using OxDb.SharedGame.Crawler.Party.Services;
using OxDb.SharedGame.Currencies.Constants;
using OxDb.SharedGame.Stats.Constants;

namespace OxDb.SharedGame.Crawler.ManaRegen.Services
{


    public class ManaRegenResult
    {
        public string Message;
        public bool Success;
        public long Cost;
        public PartyMember Member;
    }

    public interface IManaRegenService : IInjectable
    {
        long GetRegenCostForMember(PartyData party, PartyMember member);
        void RegenPartyMember(PartyData party, PartyMember member, ManaRegenResult result);
    }

    public class ManaRegenService : IManaRegenService
    {

        private IGameData _gameData = null;
        private IClientGameState _gs = null;
        private IPartyService _partyService = null;

        public long GetRegenCostForMember(PartyData party, PartyMember member)
        {

            ManaRegenSettings settings = _gameData.Get<ManaRegenSettings>(_gs.ch);

            long cost = 0;

            long missingMana = member.Stats.Max(StatTypes.Mana) - member.Stats.Curr(StatTypes.Mana);

            if (missingMana > 0)
            {
                cost += settings.CostPerMana * missingMana;
            }
            return cost;
        }

        public void RegenPartyMember(PartyData party, PartyMember member, ManaRegenResult result)
        {
            result.Member = member;
            result.Cost = GetRegenCostForMember(party, member);
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

            member.Stats.SetCurr(StatTypes.Mana, member.Stats.Max(StatTypes.Mana));

            result.Success = true;
            result.Message = member.Name + " is fully healed.";
            return;
        }
    }
}


