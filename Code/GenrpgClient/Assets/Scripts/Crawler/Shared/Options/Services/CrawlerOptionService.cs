using OxDb.SharedCore.Interfaces;
using OxDb.SharedCore.Utils;
using OxDb.SharedGame.Crawler.Parties.PlayerData;

namespace OxDb.SharedGame.Crawler.Options.Services
{
    public interface ICrawlerOptionsService : IInjectable
    {

        bool HasOption(PartyData party, long optionIndex);
    }


    public class CrawlerOptionService : ICrawlerOptionsService
    {

        public bool HasOption(PartyData party, long optionIndex)
        {
            return FlagUtils.MatchesAnyBits(party.Options, (1 << (int)optionIndex));
        }
    }
}


