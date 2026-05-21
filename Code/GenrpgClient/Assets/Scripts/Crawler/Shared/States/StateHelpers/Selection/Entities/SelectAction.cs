using OxDb.SharedGame.Crawler.Parties.PlayerData;
using OxDb.SharedGame.Crawler.Spells.Entities;
using OxDb.SharedGame.Crawler.States.Constants;

namespace OxDb.SharedGame.Crawler.States.StateHelpers.Selection.Entities
{
    public class SelectAction
    {
        public PartyMember Member { get; set; }
        public UnitAction Action { get; set; }
        public ECrawlerStates ReturnState { get; set; }
        public ECrawlerStates NextState { get; set; }
    }
}


