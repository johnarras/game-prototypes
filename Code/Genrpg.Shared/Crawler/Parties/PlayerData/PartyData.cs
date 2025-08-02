using Genrpg.Shared.Crawler.Combat.Constants;
using Genrpg.Shared.Crawler.Combat.Entities;
using Genrpg.Shared.Crawler.Items.Entities;
using Genrpg.Shared.DataStores.Categories.PlayerData.NoChild;
using Genrpg.Shared.DataStores.Categories.PlayerData.Users;
using Genrpg.Shared.Interfaces;
using Genrpg.Shared.Inventory.PlayerData;
using Genrpg.Shared.UnitEffects.Constants;
using Genrpg.Shared.Units.Loaders;
using Genrpg.Shared.Units.Mappers;
using Genrpg.Shared.Utils.Data;
using MessagePack;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Genrpg.Shared.Crawler.Parties.PlayerData
{
    /// <summary>
    /// Used to contain a list of party members
    /// </summary>

    // MessagePackIgnore
    public class PartyData : NoChildPlayerData, IUserData, INamedUpdateData
    {
        public override string Id { get; set; }
        public string MainMessage => "Yes, it's pretty print JSON. You're welcome.";
        public string Name { get; set; }

        public int Flags { get; set; }
        public bool HasFlag(int flagBits) { return (Flags & flagBits) != 0; }
        public void AddFlags(int flagBits) { Flags |= flagBits; }
        public void RemoveFlags(int flagBits) { Flags &= ~flagBits; }

        [JsonIgnore]
        public List<Item> Inventory { get; set; } = new List<Item>();

        public List<CrawlerSaveItem> SaveInventory { get; set; } = new List<CrawlerSaveItem>();

        [JsonIgnore]
        public List<Item> VendorBuyback { get; set; } = new List<Item>();

        [JsonIgnore]
        public List<Item> VendorItems { get; set; } = new List<Item>();

        public SmallIndexBitList QuestItems { get; set; } = new SmallIndexBitList();

        public DateTime LastVendorRefresh { get; set; }

        public long Gold { get; set; } = 0;

        public long Seed { get; set; }

        public long WorldId { get; set; }

        public MapPosition CurrPos { get; set; } = new MapPosition();

        public MapPosition RecallPos { get; set; } = new MapPosition();

        public long NextId { get; set; }

        public long RiddleStatus { get; set; }

        public List<CrawlerMapStatus> Maps { get; set; } = new List<CrawlerMapStatus>();

        public CurrentMapStatus CurrentMap { get; set; } = new CurrentMapStatus();

        public SmallIndexBitList CompletedMaps { get; set; } = new SmallIndexBitList();

        public SmallIndexBitList RiddlesCompleted { get; set; } = new SmallIndexBitList();

        public float HourOfDay { get; set; } = 0;

        public long DaysPlayed { get; set; } = 0;

        public long UpgradePoints { get; set; }

        public long TotalUpgradePoints { get; set; }

        public List<UpgradeStatus> UpgradeStatuses { get; set; } = new List<UpgradeStatus>();

        public SmallIdIntCollection Upgrades { get; set; } = new SmallIdIntCollection();

        public long SaveSlotId { get; set; }

        public int ScrollFramesIndex { get; set; } = CrawlerCombatConstants.StartScrollFramesIndex;

        public SmallIdFloatCollection Buffs { get; set; } = new SmallIdFloatCollection();

        public SmallIndexBitList CompletedQuests { get; set; } = new SmallIndexBitList();

        public List<PartyQuest> Quests { get; set; } = new List<PartyQuest>();

        public int FailedKillQuestTimes { get; set; }

        public int FailedItemQuestTimes { get; set; }

        public InitialCombatState InitialCombat { get; set; }

        public List<PartyMember> Members { get; set; } = new List<PartyMember>();

        [JsonIgnore][IgnoreMember] public CrawlerCombatState Combat = null;

        public string GetNextId(string prefix)
        {
            return prefix + (++NextId).ToString();
        }

        public PartyMember GetMemberInSlot(int slot)
        {
            return Members.FirstOrDefault(x => x.PartySlot == slot);
        }

        public List<PartyMember> GetActiveParty()
        {

            return Members.Where(x => x.PartySlot > 0).ToList();
        }

        public EActionCategories GetActionCategory()
        {
            if (Combat == null)
            {
                return EActionCategories.NonCombat;
            }
            if (Combat.PartyGroup.CombatGroupAction == ECombatGroupActions.Prepare)
            {
                return EActionCategories.Preparing;
            }
            return EActionCategories.Combat;
        }

        public bool PartyIsDead()
        {

            if (GetActiveParty().Count < 1)
            {
                return false;
            }

            return !(GetActiveParty().Any(x => !x.StatusEffects.HasBit(StatusEffects.Dead)));

        }

        public int GetUpgradePointsLevel(long upgradeReasonId, bool gameUpgrade)
        {
            UpgradeStatus status = UpgradeStatuses.FirstOrDefault(x => x.UpgradeReasonId == upgradeReasonId);
            if (status != null)
            {
                return (gameUpgrade ? status.GameLevel : status.RunLevel);
            }
            return 0;
        }

        public CrawlerMapStatus GetMapStatus(long mapId, bool createIfNotExist)
        {
            CrawlerMapStatus status = Maps.FirstOrDefault(x => x.MapId == mapId);
            if (status == null && createIfNotExist)
            {
                status = new CrawlerMapStatus() { MapId = mapId };
                Maps.Add(status);
            }
            return status;
        }
    }

    [MessagePackObject]
    public class PartyDataLoader : UnitDataLoader<PartyData>
    {
    }

    public class PartyDto : NoChildPlayerDataDto<PartyData> { }


    [MessagePackObject]
    public class PartyDataMapper : NoChildUnitDataMapper<PartyData, PartyDto> { }
}
