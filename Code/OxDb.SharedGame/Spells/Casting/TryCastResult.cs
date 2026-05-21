using OxDb.SharedGame.Spells.PlayerData.Spells;
using OxDb.SharedGame.Spells.Settings.Elements;
using OxDb.SharedGame.Units.Entities;

namespace OxDb.SharedGame.Spells.Casting
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


