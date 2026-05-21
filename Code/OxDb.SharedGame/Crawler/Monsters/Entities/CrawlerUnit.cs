using Newtonsoft.Json;
using OxDb.SharedGame.Crawler.Combat.Constants;
using OxDb.SharedGame.Crawler.Spells.Entities;
using OxDb.SharedGame.Inventory.PlayerData;
using OxDb.SharedGame.Units.Entities;
using OxDb.SharedGame.Units.Settings;
using System.Collections.Generic;

namespace OxDb.SharedGame.Crawler.Monsters.Entities
{

    public class CrawlerUnit : Unit
    {
        public long UnitTypeId { get; set; }

        public EDefendRanks DefendRank { get; set; }
        public long HideExtraRange { get; set; }
        public string PortraitName { get; set; }


        [JsonIgnore]
        public List<UnitAction> CombatActions { get; set; } = new List<UnitAction>();

        public void AddAction(UnitAction action)
        {
            if (!CombatActions.Contains(action))
            {
                CombatActions.Add(action);
            }
        }

        [JsonIgnore]
        public double CombatPriority { get; set; }

        public string CombatGroupId { get; set; }

        public bool IsGuardian { get; set; }

        public long VulnBits { get; set; }

        public long ResistBits { get; set; }

        [JsonIgnore]
        public long ActionsThisRound { get; set; }

        [JsonIgnore]
        public long DoTDamage { get; set; }

        public List<UnitKeyword> ExtraKeywords { get; set; } = new List<UnitKeyword>();

        public int BonusCount { get; set; }

        public virtual Item GetEquipmentInSlot(long equipSlotId)
        {
            return null;
        }
    }
}


