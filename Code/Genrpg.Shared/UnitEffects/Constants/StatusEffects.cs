using Genrpg.Shared.Characters.PlayerData;
using System;
using System.Collections.Generic;
using System.Text;

namespace Genrpg.Shared.UnitEffects.Constants
{
    public class StatusEffects
    {
        public const long Poisoned = 1; // Lose Health during regen
        public const long Diseased = 2; // Lose mana during regen
        public const long Weak = 3; // Small reduction in combat effectiveness

        public const long Slowed = 4; // Less physical dam
        public const long Befuddled = 5; // Less magical dam
        public const long Nearsighted = 6; // Less ranged dam

        public const long Cursed = 7; // Lower effective level
        public const long Withered = 8; // Less bonus value from stats
        public const long Confused = 9; // Chance to target wrong thing TPDP
        public const long Clumsy = 10; // Chance to fail to perform skill in combat
        public const long Berserk = 11; // Chance to use random skill TODO

        public const long Rooted = 12; // Cannot melee
        public const long Silenced = 13; // Cannot cast spells
        public const long Blind = 14; // Cannot shoot

        public const long Possessed = 15; // Attacks allies.
        public const long Stunned = 16; // Cannot do anything
        public const long Dead = 17;

    }
}
