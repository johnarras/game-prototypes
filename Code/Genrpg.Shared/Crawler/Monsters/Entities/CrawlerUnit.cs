using MessagePack;
using Genrpg.Shared.Units.Entities;
using System;
using System.Collections.Generic;
using System.Text;
using Genrpg.Shared.Stats.Constants;
using Genrpg.Shared.Inventory.PlayerData;
using Genrpg.Shared.Crawler.Combat.Constants;
using Genrpg.Shared.Crawler.Spells.Entities;
using Newtonsoft.Json;
using Genrpg.Shared.DataStores.Entities;
using Genrpg.Shared.Units.Settings;

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
