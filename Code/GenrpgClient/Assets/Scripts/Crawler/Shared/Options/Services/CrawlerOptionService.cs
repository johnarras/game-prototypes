using Genrpg.Shared.Crawler.Parties.PlayerData;
using Genrpg.Shared.Interfaces;
using Genrpg.Shared.Utils;

namespace Genrpg.Shared.Crawler.Options.Services
{
    public interface ICrawlerOptionsService : IInjectable
    {

        bool HasOption(PartyData party, long optionIndex);
    }


    public class CrawlerOptionService : ICrawlerOptionsService
    {

        public bool HasOption(PartyData party, long optionIndex)
        {
            return FlagUtils.IsSet(party.Options, (1 << (int)optionIndex));
        }
    }
}
