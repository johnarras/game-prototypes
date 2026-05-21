using OxDb.SharedGame.Crawler.Maps.Entities;
using OxDb.SharedGame.Crawler.States.Constants;

namespace OxDb.SharedGame.Crawler.States.Entities
{
    public class ForcedNextState
    {
        public ECrawlerStates NextState { get; set; }
        public MapCellDetail Detail { get; set; }
    }
}


