using OxDb.SharedCore.GameSettings;
using OxDb.SharedCore.Interfaces;
using OxDb.SharedCore.Utils;
using OxDb.SharedGame.Crawler.Options.Settings;
using OxDb.SharedGame.Crawler.Parties.PlayerData;

namespace OxDb.SharedGame.Crawler.Options.Services
{
    public interface ICrawlerOptionsService : IInjectable
    {

        bool HasOption(PartyData party, long optionIndex);
    }


    public class CrawlerOptionService : ICrawlerOptionsService
    {

        private IGameData _gameData = null;
        private IClientGameState _gs = null;

        public bool HasOption(PartyData party, long optionIndex)
        {

            CrawlerOption crawlerOption = _gameData.Get<CrawlerOptionSettings>(_gs.ch).Get(optionIndex);

            if (crawlerOption == null)
            {
                return false;
            }
            if (crawlerOption.ForceDefault)
            {
                return crawlerOption.DefaultForNewGame;
            }
                

            return FlagUtils.MatchesAnyBits(party.Options, (1 << (int)optionIndex));
        }
    }
}


