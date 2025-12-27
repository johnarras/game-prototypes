using Genrpg.Shared.Effects.Interfaces;
using MessagePack;
using System;
using System.Collections.Generic;
using System.Text;

namespace Genrpg.Shared.Spells.Settings.Effects
{ 

    /// <summary>
    /// These are used for passive bonuses that skills and elements give players.
    /// </summary>
    public class AbilityEffect : IEffect
    {
        public long EntityTypeId { get; set; }
        public long Quantity { get; set; }
        public long EntityId { get; set; }
        public string Name { get; set; }
    }
}


