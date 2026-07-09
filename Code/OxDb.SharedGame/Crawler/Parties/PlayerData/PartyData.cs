using MessagePack;
using Newtonsoft.Json;
using OxDb.SharedCore.Serialization.Attributes;
using OxDb.SharedCore.Utils;
using OxDb.SharedCore.Utils.Data;
using OxDb.SharedGame.Crawler.Combat.Constants;
using OxDb.SharedGame.Crawler.Combat.Entities;
using OxDb.SharedGame.Crawler.Items.Entities;
using OxDb.SharedGame.DataStores.Categories.PlayerData.NoChild;
using OxDb.SharedGame.DataStores.Categories.PlayerData.Users;
using OxDb.SharedGame.Interfaces;
using OxDb.SharedGame.Inventory.PlayerData;
using OxDb.SharedGame.LoadSave.Constants;
using OxDb.SharedGame.UnitEffects.Constants;
using OxDb.SharedGame.Units.Loaders;
using OxDb.SharedGame.Units.Mappers;
using System;
using System.Collections.Generic;
using System.Linq;

namespace OxDb.SharedGame.Crawler.Parties.PlayerData
{
    /// <summary>
    /// Used to contain a list of party members
    /// </summary>

    [MessagePackIgnoreType]
    public class PartyData : NoChildIndexedUserData, IUserData, INamedUpdateData
    {
        public override string Id { get; set; }
        public string Name { get; set; }
        protected string _analyticsName = null;
        public override string VersionTag { get; set; }
        public string GetAnalyticsName()
        {
            if (string.IsNullOrEmpty(_analyticsName))
            {
                if (!string.IsNullOrEmpty(Name))
                {
                    _analyticsName = StrUtils.ToSnakeCase(Name);
                }

                if (string.IsNullOrEmpty(_analyticsName))
                {
                    _analyticsName = StrUtils.ToSnakeCase(GetType().Name);
                }
            }
            return _analyticsName;
        }

        public int Flags { get; set; }
        public bool HasFlag(int flagBits) { return (Flags & flagBits) != 0; }
        public void AddFlags(int flagBits) { Flags |= flagBits; }
        public void RemoveFlags(int flagBits) { Flags &= ~flagBits; }

        // Options set at the start of the game.
        public int Options { get; set; }

        [JsonIgnore]
        public List<Item> Inventory { get; set; } = new List<Item>();

        public List<CrawlerSaveItem> SaveInventory { get; set; } = new List<CrawlerSaveItem>();

        [JsonIgnore]
        public List<Item> VendorBuyback { get; set; } = new List<Item>();

        [JsonIgnore]
        public List<Item> VendorItems { get; set; } = new List<Item>();

        public SmallIndexBitList QuestItems { get; set; } = new SmallIndexBitList();

        public DateTime LastVendorRefresh { get; set; }


        public SmallIdLongCollection Currencies { get; set; } = new SmallIdLongCollection();

        public long Seed { get; set; }

        public long WorldId { get; set; }

        public MapPosition CurrPos { get; set; } = new MapPosition();

        public MapPosition RecallPos { get; set; } = new MapPosition();

        public long NextId { get; set; }

        public List<CrawlerMapStatus> Maps { get; set; } = new List<CrawlerMapStatus>();

        public CurrentMapStatus CurrentMap { get; set; } = new CurrentMapStatus();

        public SmallIndexBitList CompletedMaps { get; set; } = new SmallIndexBitList();

        public SmallIndexBitList RiddlesCompleted { get; set; } = new SmallIndexBitList();

        public float HourOfDay { get; set; } = 0;

        public long DaysPlayed { get; set; } = 0;

        public long UpgradePoints { get; set; }

        public long TotalUpgradePoints { get; set; }

        public long MaxLevelEntered { get; set; }

        public long MaxMapIdEntered { get; set; }

        public long LastAutoCompleteLevel { get; set; }

        public string RoguelikeDungeonName { get; set; }

        public List<UpgradeStatus> UpgradeStatuses { get; set; } = new List<UpgradeStatus>();

        public SmallIdIntCollection Upgrades { get; set; } = new SmallIdIntCollection();

        public long SaveSlotId { get; set; } = LoadSaveConstants.MinSlot;

        public int ScrollFramesIndex { get; set; } = CrawlerCombatConstants.StartScrollFramesIndex;

        public SmallIdFloatCollection Buffs { get; set; } = new SmallIdFloatCollection();

        public SmallIndexBitList CompletedQuests { get; set; } = new SmallIndexBitList();

        public List<PartyQuest> Quests { get; set; } = new List<PartyQuest>();

        public int FailedKillQuestTimes { get; set; }

        public int FailedItemQuestTimes { get; set; }

        public InitialCombatState InitialCombat { get; set; }

        public List<PartyMember> ActiveParty { get; set; } = new List<PartyMember>();

        public List<PartyMember> InGuild { get; set; } = new List<PartyMember>();

        public List<string> ItemsUsed { get; set; } = new List<string>();

        [JsonIgnore][IgnoreMember] public CrawlerCombatState Combat = null;

        public Dictionary<long, SmallIndexBitList> ItemBuffs { get; set; } = new Dictionary<long, SmallIndexBitList>();

        public bool HasItemBuff(long entityTypeId, long entityId)
        {
            if (ItemBuffs.TryGetValue(entityTypeId, out SmallIndexBitList bitList))
            {
                if (entityId == 0 || bitList.HasBitIndex(entityId))
                {
                    return true;
                }
            }

            return false;
        }

        public void AddItemBuff(long entityTypeId, long entityId)
        {
            if (!ItemBuffs.ContainsKey(entityTypeId))
            {
                ItemBuffs[entityTypeId] = new SmallIndexBitList();
            }
            ItemBuffs[entityTypeId].SetBitIndex(entityId);
        }

        public void RemoveItemBuff(long entityTypeId, long entityId)
        {

            if (ItemBuffs.ContainsKey(entityTypeId))
            {
                ItemBuffs[entityTypeId].RemoveBitIndex(entityId);
            }
        }

        public void ClearItemBuffs()
        {
            ItemBuffs.Clear();
        }

        public string GetNextId(string prefix)
        {
            return prefix + (++NextId).ToString();
        }

        public PartyMember GetMemberInSlot(int slot)
        {
            return ActiveParty.FirstOrDefault(x => x.PartySlot == slot);
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

            if (ActiveParty.Count < 1)
            {
                return false;
            }

            return !(ActiveParty.Any(x => !x.StatusEffects.HasBitIndex(StatusEffects.Dead)));
        }

        public List<PartyMember> GetAllMembers()
        {
            return ActiveParty.Concat(InGuild).ToList();
        }

        public long GetUpgradePointsLevel(long upgradeReasonId, bool gameUpgrade)
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
        public long GetRiddleStatus()
        {
            CrawlerMapStatus currStatus = GetMapStatus(CurrPos.MapId, true);

            return currStatus.RiddleStatus;
        }

        public void AddRiddleBitIndex(int bitIndex)
        {
            CrawlerMapStatus currStatus = GetMapStatus(CurrPos.MapId, true);

            currStatus.RiddleStatus |= (long)(1 << bitIndex);
        }

        public void RemoveRiddleBitIndex(int bitIndex)
        {
            CrawlerMapStatus currStatus = GetMapStatus(CurrPos.MapId, true);

            currStatus.RiddleStatus &= (long)(~(1 << bitIndex));
        }

        public bool HasRiddleBitIndex(long bitIndex)
        {
            return FlagUtils.MatchesAnyBits(GetRiddleStatus(), (1 << (int)bitIndex));
        }
    }

    public class PartyDataLoader : UnitDataLoader<PartyData>
    {
        public override bool IsClientOnlyData()
        {
            return true;
        }
    }

    [MessagePackObject]
    public class PartyDto : NoChildPlayerDataDto<PartyData>
    {
        [Key(0)] public override PartyData Parent { get; set; }
        [Key(1)] public override string Id { get; set; }
    }


    public class PartyDataMapper : NoChildUnitDataMapper<PartyData, PartyDto> { }

}


