using System;
using System.Collections.Generic;
using System.Net.Sockets;
using System.Text;

namespace Genrpg.Shared.Stats.Constants
{
    public class StatTypes
    {
        public const long None = 0;
        public const long Health = 1;
        public const long Mana = 2; // slow drain, slow regen.
        public const long Energy = 3; // quick drain, quick regen. 
        public const long Combo = 4; // Build up with use of other skills

        public const long Strength = 11; // + melee dam
        public const long Intellect = 12; // + spell dam
        public const long Devotion = 13; // + heals
        public const long Agility = 14; // + ranged dam
        public const long Stamina = 15; // health
        public const long Luck = 16; // Extra attacks
        public const long Willpower = 17; // + Mana

        // For 20-60 make sure the offsets for +power, +defense, +powerMult , +defenseMult are all offset the same at 2x 3x 4x 5x
        // to simplify calculations.

        public const long DamagePower = 20; // All Attack
        public const long AttackPower = 21; // Physical Attack
        public const long SpellPower = 22; // Magical Attack

        public const long Defense = 30; // All Defense
        public const long Armor = 31; // Resist phys Armor
        public const long Resist = 32; // Resist magic Resist
        public const long HealthRegen = 33; // Adds to health regen for players
        public const long ManaRegen = 34; // Adds to mana regen for players
        public const long RoleScalingPercent = 35;
        public const long DetectHidden = 36;
        public const long SmartTarget = 37;

        public const long PowerMult = 40; // multiplier to all dam/healing
        public const long AttackMult = 41; // Multiplier to body dam/healing
        public const long SpellMult = 42; // Multiplier to magic dam/healing

        public const long DefenseMult = 50; // All Defense Mult
        public const long AttackDefMult = 51; // Physical Defense Mult
        public const long SpellDefMult = 52; // Magical Defense Mult


        public const long Crit = 61;
        public const long Haste = 62;
        public const long Speed = 63; // Move speed
        public const long Efficiency = 64; // Cost reduction
        public const long Cooldown = 65; // Cooldown reduction
        public const long CritDam = 66; // Crit damage
        
        public const long Hit = 70; // Chance to hit

        // Update StatConstants.MaxStatType to be the max value + 1 in this file.
    }
}
