using MessagePack;
using Genrpg.Shared.Characters.PlayerData;
using Genrpg.Shared.Crawler.Combat.Constants;
using Genrpg.Shared.Crawler.Combat.Entities;
using Genrpg.Shared.Units.Entities;
using System;
using System.Collections.Generic;
using System.Text;
using Genrpg.Shared.Units.Settings;

namespace Genrpg.Shared.Crawler.Monsters.Entities
{
    public class FullMonsterStats
    {
        public List<UnitEffect> Spells { get; set; } = new List<UnitEffect>();
        public List<FullEffect> ApplyEffects { get; set; } = new List<FullEffect>();
        public bool IsGuardian { get; set; }
        public long ResistBits { get; set; }
        public long VulnBits { get; set; }
        public long Range { get; set; } = CrawlerCombatConstants.MinRange;
        public UnitKeyword SuffixKeyword { get; set; }
        public List<UnitKeyword> ExtraKeywords { get; set; } = new List<UnitKeyword>();
        public int BonusCount { get; set; }
    }
}


