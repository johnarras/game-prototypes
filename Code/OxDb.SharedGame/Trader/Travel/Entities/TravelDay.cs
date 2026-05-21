using OxDb.SharedCore.Client.Interfaces;
using OxDb.SharedCore.Utils.Data;
using OxDb.SharedGame.LevelTracks.WebApi;
using OxDb.SharedGame.Trader.Encounters.Entities;

namespace OxDb.SharedGame.Trader.Travel.Entities
{

    public class DayVars
    {
        public const int Day = 1;
        public const int BonusDistance = 2;
        public const int TotalDistance = 3;
        public const int EndDistance = 4;
        public const int EndFlags = 5;
        public const int Exp = 6;
        public const int DiceCount = 7;
    }

    public class TravelDay : IClientEvent
    {
        public SmallIdLongCollection Currencies { get; set; } = new SmallIdLongCollection();
        public SmallIdIntCollection Vars { get; set; } = new SmallIdIntCollection();

        public EncounterResult EncounterResult { get; set; } = null!;
        public GainExpResponse ExpResponse { get; set; } = null!;
    }
}
