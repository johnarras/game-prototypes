using OxDb.SharedGame.Units.Entities;

namespace OxDb.SharedGame.Spells.Casting
{
    public class TargetCastState
    {
        public Unit Target { get; set; }
        public TryCastState State { get; set; }
    }
}


