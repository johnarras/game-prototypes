using Genrpg.Shared.Crawler.Combat.Constants;
using Genrpg.Shared.Crawler.Spells.Entities;
using Genrpg.Shared.Inventory.PlayerData;
using Genrpg.Shared.Units.Entities;
using Genrpg.Shared.Units.Settings;
using Newtonsoft.Json;
using System.Collections.Generic;

namespace Genrpg.Shared.Crawler.Monsters.Entities
{

    // MessagePackIgnore
    public class CrawlerUnit : Unit
    {
        public long UnitTypeId { get; set; }

        public EDefendRanks DefendRank { get; set; }
        public long HideExtraRange { get; set; }
        public string PortraitName { get; set; }


        [JsonIgnore]
        public UnitAction Action { get; set; }

        [JsonIgnore]
        public double CombatPriority { get; set; }

        public string CombatGroupId { get; set; }

        public bool IsGuardian { get; set; }

        public long VulnBits { get; set; }

        public long ResistBits { get; set; }

        public List<UnitKeyword> ExtraKeywords { get; set; } = new List<UnitKeyword>();

        public int BonusCount { get; set; }

        public virtual Item GetEquipmentInSlot(long equipSlotId)
        {
            return null;
        }
    }
}
