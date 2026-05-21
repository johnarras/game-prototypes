using OxDb.SharedGame.Spells.Procs.Interfaces;
using System;

namespace OxDb.SharedGame.Spells.Procs.Entities
{
    public class SpellProc : IProc
    {
        public long EntityTypeId { get; set; }
        public long EntityId { get; set; }
        public long MinQuantity { get; set; }
        public long MaxQuantity { get; set; }
        public double Chance { get; set; }
        public long MaxCharges { get; set; }
        public long CurrCharges { get; set; }
        public long CooldownSeconds { get; set; }
        public DateTime LastUsedTime { get; set; }
        public long ElementTypeId { get; set; }
        public string Name { get; set; }
    }
}


