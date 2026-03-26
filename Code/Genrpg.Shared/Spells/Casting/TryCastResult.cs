using Genrpg.Shared.Spells.PlayerData.Spells;
using Genrpg.Shared.Spells.Settings.Elements;
using Genrpg.Shared.Units.Entities;

namespace Genrpg.Shared.Spells.Casting
{
    public class TryCastResult
    {
        public TryCastState State;
        public Unit Target { get; set; }
        public Spell Spell { get; set; }
        public ElementType ElementType { get; set; }
        public string StateText { get; set; }
    }
}


