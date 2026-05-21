using OxDb.SharedGame.Crawler.Buffs.Settings;
using OxDb.SharedGame.Crawler.Spells.Services;
using System.Collections.Generic;

namespace OxDb.SharedGame.Spells.Entities
{
    public class ApplyEffectArgs
    {
        public bool IsEnemyTarget { get; set; }
        public float DelayTime { get; set; }
        public float AfterInitialTextTime { get; set; }
        public long TotalDamage { get; set; }
        public long TotalHealing { get; set; }
        public int CurrHitTimes { get; set; }
        public long NewQuantity { get; set; }
        public string FullAction { get; set; }
        public double CritChanceScaling { get; set; } = 1.0f;
        public long ExtraMessageBits { get; set; }
        public PartyBuffSettings BuffSettings { get; set; }
        public bool DidParry { get; set; }
        public bool DidKill { get; set; }
        public bool IsDead { get; set; }
        public Dictionary<string, ActionListItem> ActionList = new Dictionary<string, ActionListItem>();
    }
}


