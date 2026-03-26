using Genrpg.Shared.Units.Entities;

namespace Genrpg.Shared.Spells.Casting
{
    public class TargetCastState
    {
        public Unit Target { get; set; }
        public TryCastState State { get; set; }
    }
}


