using Genrpg.Shared.Client.Core;
using Genrpg.Shared.Crafting.Entities;
using Genrpg.Shared.Crawler.Crawlers.Services;
using Genrpg.Shared.Crawler.Loot.Constants;
using Genrpg.Shared.Crawler.Loot.Helpers;
using Genrpg.Shared.Crawler.Loot.Settings;
using Genrpg.Shared.Crawler.Maps.Entities;
using Genrpg.Shared.Crawler.Maps.Services;
using Genrpg.Shared.Crawler.Monsters.Entities;
using Genrpg.Shared.Crawler.Parties.PlayerData;
using Genrpg.Shared.Crawler.Party.Services;
using Genrpg.Shared.Crawler.Quests.Services;
using Genrpg.Shared.Crawler.States.Constants;
using Genrpg.Shared.Crawler.States.Services;
using Genrpg.Shared.Crawler.Training.Services;
using Genrpg.Shared.Crawler.Training.Settings;
using Genrpg.Shared.Crawler.Upgrades.Constants;
using Genrpg.Shared.Entities.Constants;
using Genrpg.Shared.GameSettings;
using Genrpg.Shared.HelperClasses;
using Genrpg.Shared.Interfaces;
using Genrpg.Shared.Inventory.Constants;
using Genrpg.Shared.Inventory.Entities;
using Genrpg.Shared.Inventory.PlayerData;
using Genrpg.Shared.Inventory.Services;
using Genrpg.Shared.Inventory.Settings.ItemTypes;
using Genrpg.Shared.Inventory.Settings.Ranks;
using Genrpg.Shared.Inventory.Settings.Slots;
using Genrpg.Shared.Logging.Interfaces;
using Genrpg.Shared.Spells.Settings.Elements;
using Genrpg.Shared.Stats.Constants;
using Genrpg.Shared.Stats.Settings.Scaling;
using Genrpg.Shared.Stats.Settings.Stats;
using Genrpg.Shared.Utils;
using Genrpg.Shared.Vendors.Settings;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Genrpg.Shared.Crawler.Loot.Services
{


    public interface ILootGenService : IInjectable
    {

        Item GenerateItem(ItemGenArgs lootGenData);
        Task<LootGenData> GenerateCombatLoot(PartyData party, CancellationToken token);
        Task<PartyLoot> GiveLoot(PartyData party, CrawlerMap map, LootGenData genData, CancellationToken token);
        List<ItemNameResult> GenerateItemNames(IRandom rand, int itemCount, int level);
        long GetPartyInventorySize(PartyData party);
        Task<LootGenData> CreateLootGenData(PartyData party, double expMult, double goldMult, double itemMult, string topMessage = null, ECrawlerStates nextState = ECrawlerStates.None, object nextStateData = null);
    }

    public class LootGenData
    {
        public double Exp { get; set; }
        public double Gold { get; set; }
        public int ItemCount { get; set; }
        public int Level { get; set; }
        public List<long> QuestItems { get; set; } = new List<long>();
        public ECrawlerStates NextState { get; set; } = ECrawlerStates.None;
        public object NextStateData { get; set; } = null;
        public List<string> TopMessages { get; set; } = new List<string>();
        public List<string> ExtraMessages { get; set; } = new List<string>();
    }

    public class PartyLoot
    {
        public long Gold { get; set; }
        public long Exp { get; set; }
        public List<Item> Items { get; set; } = new List<Item>();
        public List<long> NewQuestItems { get; set; } = new List<long>();
        public long TotalInventorySize { get; set; }
        public List<string> TopMessages { get; set; } = new List<string>();
        public List<string> ExtraMessages { get; set; } = new List<string>();
        public ECrawlerStates NextState { get; set; }
        public object NextStateData { get; set; }
    }

    public class LootGenService : ILootGenService
    {
        private IGameData _gameData = null;
        private IClientGameState _gs = null;
        private IClientRandom _rand = null;
        private IItemGenService _itemGenService = null;
        private ICrawlerUpgradeService _upgradeService = null;
        private ICrawlerService _crawlerService = null;
        private ILogService _logService = null;
        private ICrawlerQuestService _questService = null;
        private ITrainingService _trainingService = null;
        private ICrawlerWorldService _worldService = null;
        private IPartyService _partyService = null;

        private SetupDictionaryContainer<long, ICrawlerLootTypeHelper> _lootTypeHelpers = new SetupDictionaryContainer<long, ICrawlerLootTypeHelper>();

        public Item GenerateItem(ItemGenArgs itemGenArgs)
        {
            return GenerateEquipment(itemGenArgs);
        }

        public Item GenerateEquipment(ItemGenArgs itemGenArgs)
        {
            int level = itemGenArgs.Level;

            PartyData party = _crawlerService.GetParty();

            CrawlerLootSettings lootSettings = _gameData.Get<CrawlerLootSettings>(null);

            LootRankSettings rankSettings = _gameData.Get<LootRankSettings>(null);

            IReadOnlyList<LootRank> ranks = rankSettings.GetData();

            int expectedOffset = (int)(level / rankSettings.LevelsPerQuality + 1);

            expectedOffset = MathUtils.Clamp(1, expectedOffset, ranks.Count - 2);

            List<LootRank> okRanks = new List<LootRank>();

            while (expectedOffset < ranks.Count - 2 && _rand.NextDouble() < rankSettings.ExtraQualityChance)
            {
                expectedOffset++;
            }

            for (int index = expectedOffset - 1; index <= expectedOffset + 1; index++)
            {
                if (ranks[index].IdKey == 0)
                {
                    continue;
                }
                okRanks.Add(ranks[index]);
            }


            // Level 0 items have no tiers
            if (level < 1)
            {
                okRanks.Clear();
                okRanks.Add(ranks.FirstOrDefault(x => x.IdKey > 0));
            }

            // Allow some variance

            // Pick a quality...

            LootRank chosenRank = okRanks[0];

            int rankIndex = 0;
            while (rankIndex < okRanks.Count - 1 && _rand.NextDouble() < rankSettings.ExtraQualityChance)
            {
                rankIndex++;
            }

            chosenRank = okRanks[rankIndex];

            ItemType itemType = null;

            if (itemGenArgs.ItemTypeId > 0)
            {
                itemType = _gameData.Get<ItemTypeSettings>(_gs.ch).Get(itemGenArgs.ItemTypeId);
            }

            bool allItemSlotsOk = false;

            if (itemType == null)
            {
                List<EquipSlot> okEquipSlots = _gameData.Get<EquipSlotSettings>(null).GetData().Where(x => x.IsCrawlerSlot || allItemSlotsOk).ToList();

                List<long> okEquipSlotIds = okEquipSlots.Select(x => x.IdKey).ToList();

                IReadOnlyList<ItemType> allLootItems = _gameData.Get<ItemTypeSettings>(null).GetData();

                List<ItemType> okLootItems = allLootItems.Where(x => okEquipSlotIds.Contains(x.EquipSlotId)).ToList();

                List<ItemType> weaponItems = okLootItems.Where(x => EquipSlots.IsWeapon(x.EquipSlotId)).ToList();


                List<ItemType> rangedWeapons = weaponItems.Where(x => x.EquipSlotId == EquipSlots.Ranged).ToList();

                foreach (ItemType rangedWeaponType in rangedWeapons)
                {
                    for (int i = 0; i < 4; i++)
                    {
                        weaponItems.Add(rangedWeaponType);
                    }
                }

                List<ItemType> armorItems = okLootItems.Where(x => EquipSlots.IsArmor(x.EquipSlotId)).ToList();

                bool armorItem = _rand.NextDouble() < rankSettings.ArmorChance;

                List<ItemType> finalList = (armorItem ? armorItems : weaponItems);

                if (finalList.Count < 1)
                {
                    return null;
                }

                itemType = finalList[_rand.Next() % finalList.Count];
            }

            bool isArmor = EquipSlots.IsArmor(itemType.EquipSlotId);

            ScalingType scalingType = null;
            long scalingTypeId = 0;

            if (isArmor || lootSettings.AllowAllWeaponTypes)
            {
                scalingTypeId = MathUtils.IntRange(1, LootConstants.MaxArmorScalingType, _rand);
                scalingType = _gameData.Get<ScalingTypeSettings>(null).Get(scalingTypeId);
            }

            Item item = new Item() { Id = HashUtils.NewUUId().ToString() };

            item.ItemTypeId = itemType.IdKey;

            if (isArmor || lootSettings.AllowAllWeaponTypes)
            {
                item.ScalingTypeId = scalingTypeId;
            }
            item.LootRankId = chosenRank.IdKey;
            item.QualityTypeId = 0;

            if (isArmor)
            {
                EquipSlot equipSlot = _gameData.Get<EquipSlotSettings>(null).Get(itemType.EquipSlotId);

                if (equipSlot == null || equipSlot.BaseBonusStatTypeId < 1)
                {
                    item.ScalingTypeId = 0;
                }
                else
                {
                    if (equipSlot.BaseBonusStatTypeId != StatTypes.Armor)
                    {
                        item.ScalingTypeId = 0;
                    }
                    long bonusStat = itemType.MinVal;
                    if (scalingType != null)
                    {
                        bonusStat = Math.Max(1, (bonusStat * scalingType.ArmorPct) / 100);
                    }
                    item.Effects.Add(new ItemEffect() { EntityTypeId = EntityTypes.Stat, EntityId = equipSlot.BaseBonusStatTypeId, Quantity = bonusStat });
                }
            }

            string baseItemName = itemType.Name;
            if (itemType.Names.Count > 0)
            {
                baseItemName = RandomUtils.GetRandomElement(itemType.Names, _rand).Name;
            }

            // Weapon damage is calculated dynamically as needed.

            if (itemType.EquipSlotId == EquipSlots.Quiver || itemType.EquipSlotId == EquipSlots.PoisonVial)
            {
                List<ElementType> okElements = _gameData.Get<ElementTypeSettings>(null).GetData().Where(x => x.IdKey > 1).ToList();

                ElementType okElement = okElements[_rand.Next() % okElements.Count];

                ItemProc iproc = new ItemProc()
                {
                    EntityTypeId = EntityTypes.Damage,
                    EntityId = 0,
                    ElementTypeId = okElement.IdKey,
                    Chance = 0.5,
                    MinQuantity = level / 5,
                    MaxQuantity = level / 2,
                };
                item.Procs.Add(iproc);
                if (itemType.EquipSlotId == EquipSlots.Quiver)
                {
                    item.Name = chosenRank.Name + " " + okElement.Name + " Quiver";
                }
                else if (itemType.EquipSlotId == EquipSlots.PoisonVial)
                {
                    item.Name = chosenRank.Name + " Vial of " + okElement.Name;
                }
            }
            else
            {
                if (level > 0)
                {
                    List<long> usedStatTypeIds = new List<long>();

                    if (scalingType.MainStatTypeId > 0)
                    {
                        usedStatTypeIds.Add(scalingType.MainStatTypeId);
                    }
                    usedStatTypeIds.Add(StatTypes.Stamina);

                    List<StatType> okStats = _gameData.Get<StatSettings>(null).GetData()
                        .Where(x => x.IdKey >= StatConstants.PrimaryStatStart &&
                    x.IdKey <= StatConstants.PrimaryStatEnd && !usedStatTypeIds.Contains(x.IdKey)).ToList();

                    int statQuantity = (int)chosenRank.IdKey / 8;
                    if (_rand.NextDouble() < chosenRank.IdKey * 0.1f)
                    {
                        statQuantity++;
                    }
                    for (int i = 0; i < statQuantity && okStats.Count > 0; i++)
                    {

                        StatType okStat = okStats[_rand.Next() % okStats.Count];
                        usedStatTypeIds.Add(okStat.IdKey);
                        okStats.Remove(okStat);
                    }

                    usedStatTypeIds = usedStatTypeIds.OrderBy(x => x).ToList();

                    foreach (long statTypeId in usedStatTypeIds)
                    {
                        ItemEffect itemEffect = new ItemEffect()
                        {
                            EntityTypeId = EntityTypes.Stat,
                            EntityId = statTypeId,
                            Quantity = 2 + level / 7,
                        };

                        item.Effects.Add(itemEffect);
                    }


                    if (itemGenArgs.ExtraItems > 0)
                    {
                        double extraStatQuantity = lootSettings.StatPointsPerExtraItem * itemGenArgs.ExtraItems;
                        foreach (ItemEffect effect in item.Effects)
                        {
                            if (effect.EntityTypeId == EntityTypes.Stat)
                            {
                                effect.Quantity +=
                                    (long)extraStatQuantity +
                                    _rand.NextDouble() < (extraStatQuantity - (long)extraStatQuantity) ? 1 : 0;
                            }
                        }

                        if (_rand.NextDouble() < lootSettings.ItemEnchantChance * itemGenArgs.ExtraItems)
                        {
                            CrawlerLootType enchantType = RandomUtils.GetRandomEnchant(lootSettings.GetData(), _rand);

                            if (enchantType != null)
                            {
                                if (_lootTypeHelpers.TryGetValue(enchantType.EntityTypeId, out ICrawlerLootTypeHelper helper))
                                {
                                    helper.AddEnchantToItem(party, item, itemGenArgs);
                                }
                            }
                        }
                    }
                }

                if (!isArmor)
                {
                    item.ScalingTypeId = 0;
                }
                item.Name = chosenRank.Name + " " + _itemGenService.GenerateItemName(_rand, itemType.IdKey, level, QualityTypes.Uncommon, null).SingularName;
                item.ScalingTypeId = scalingTypeId;
                item.Level = Math.Max(1, level);

            }

            double cost = lootSettings.BaseLootCost;

            cost = cost * (1 + (itemType.MinVal + itemType.MaxVal) / 2.0f);

            if (itemType.EquipSlotId == EquipSlots.MainHand)
            {
                cost *= lootSettings.WeaponMult;
                if (itemType.HasFlag(ItemFlags.FlagTwoHandedItem))
                {
                    cost *= lootSettings.TwoHandWeaponMult;
                }
            }

            if (item.Procs.Count > 0)
            {
                cost *= lootSettings.ProcMult;
            }
            if (item.Effects.Count > 0)
            {
                cost *= lootSettings.EffectMult;
            }

            if (isArmor)
            {
                cost = cost * scalingType.CostPct / 100.0f;
            }

            cost = cost * chosenRank.CostPct / 100.0f;

            item.BuyCost = (long)cost;
            item.SellValue = (long)(cost * _gameData.Get<VendorSettings>(_gs.ch).SellToVendorPriceMult);
            item.Level = Math.Max(1, item.Level);
            item.Name = item.Name.Trim();
            return item;
        }

        public async Task<LootGenData> GenerateCombatLoot(PartyData party, CancellationToken token)
        {
            if (party.Combat == null || party.GetActiveParty().Count < 1)
            {
                return new LootGenData();
            }


            CrawlerTrainingSettings trainingSettings = _gameData.Get<CrawlerTrainingSettings>(null);

            CrawlerLootSettings lootSettings = _gameData.Get<CrawlerLootSettings>(null);

            double extraScalePerBonus = lootSettings.ExtraLootScalePerMonsterBonus;

            double itemChance = lootSettings.ItemChancePerMonster;

            double exp = 0;
            double gold = 0;

            int itemCount = 0;

            long minGold = (long)(party.Combat.Level * lootSettings.MinGoldPerLevel);
            long maxGold = (long)(party.Combat.Level * lootSettings.MaxGoldPerLevel);

            long expPerMonster = trainingSettings.GetMonsterExp(party.Combat.Level);

            foreach (CrawlerUnit crawlerUnit in party.Combat.EnemiesKilled)
            {
                double lootScale = (1 + crawlerUnit.BonusCount * extraScalePerBonus);
                exp += expPerMonster * lootScale;
                gold += MathUtils.LongRange(minGold, maxGold, _rand) * lootScale;

                if (_rand.NextDouble() < itemChance * lootScale)
                {
                    itemCount++;
                }
            }

            long maxLevel = party.GetActiveParty().Max(x => x.Level);

            long levelDifference = Math.Max(0, (maxLevel - party.Combat.Level) - lootSettings.LevelDiffBeforeLootLoss);

            double levelLootScale = 1.0f;
            string lootLossMessage = null;
            if (levelDifference > 0)
            {
                levelLootScale -= levelDifference * lootSettings.LootLossPerLevelDiff;

                if (levelLootScale < lootSettings.MinLootPercent)
                {
                    levelLootScale = lootSettings.MinLootPercent;
                }

                lootLossMessage = $"Loot scaled down to {levelLootScale}% of normal since your max level is so far above the monsters. ";


                gold *= levelLootScale;
                exp *= levelLootScale;
                itemCount = (int)(itemCount * levelLootScale);
            }

            LootGenData allLootGenData = new LootGenData()
            {
                Gold = gold,
                Exp = exp,
                Level = party.Combat.Level,
                ItemCount = itemCount,
                ExtraMessages = await _questService.UpdateAfterCombat(party, party.Combat.EnemiesKilled, token),
                NextState = ECrawlerStates.ExploreWorld,
                NextStateData = null,
            };

            allLootGenData.TopMessages.Add("You are Victorious!");
            if (!string.IsNullOrEmpty(lootLossMessage))
            {
                allLootGenData.TopMessages.Add(lootLossMessage);
            }

            return allLootGenData;
        }

        public async Task<PartyLoot> GiveLoot(PartyData party, CrawlerMap map, LootGenData genData, CancellationToken token)
        {
            if (genData == null)
            {
                return new PartyLoot();
            }

            PartyLoot loot = new PartyLoot()
            {
                ExtraMessages = genData.ExtraMessages.ToList(),
                TopMessages = genData.TopMessages.ToList(),
                NextState = genData.NextState,
                NextStateData = genData.NextStateData,
            };

            CrawlerLootSettings lootSettings = _gameData.Get<CrawlerLootSettings>(_gs.ch);

            List<Item> items = new List<Item>();

            double lootQualityBonus = _upgradeService.GetPartyBonus(party, PartyUpgrades.LootQuality);


            long extraItems = Math.Max(0, genData.ItemCount - lootSettings.MaxLootItems);

            for (int i = 0; i < Math.Min(lootSettings.MaxLootItems, genData.ItemCount); i++)
            {
                ItemGenArgs itemGenArgs = new ItemGenArgs()
                {
                    Level = genData.Level,
                    QualityTypeId = (long)(_rand.NextDouble() * (lootQualityBonus * 2 + 0.5f)),
                    ExtraItems = extraItems,
                };

                Item item = GenerateItem(itemGenArgs);
                if (item != null)
                {
                    items.Add(item);
                }
            }

            try
            {

                long questItemId = map.GetEntityId(party.CurrPos.X, party.CurrPos.Z, EntityTypes.QuestItem);

                if (questItemId > 0)
                {
                    loot.NewQuestItems.Add(questItemId);

                    party.QuestItems.SetBit(questItemId);
                }

                while (items.Count > lootSettings.MaxLootItems)
                {
                    Item lastItem = items.Last();
                    items.Remove(lastItem);

                    genData.Gold += lastItem.BuyCost;
                }


                loot.Items = items;

                loot.Gold = (long)(genData.Gold * (1 + _upgradeService.GetPartyBonus(party, PartyUpgrades.GoldPercent) / 100.0f));


                _partyService.AddGold(party, loot.Gold);

                genData.Exp = (long)(genData.Exp * (1 + _upgradeService.GetPartyBonus(party, PartyUpgrades.ExpPercent) / 100.0f));
                loot.Exp = (long)genData.Exp / party.GetActiveParty().Count;

                foreach (PartyMember member in party.GetActiveParty())
                {
                    member.Exp += loot.Exp;
                }

                party.Inventory.AddRange(loot.Items);

                loot.TotalInventorySize = GetPartyInventorySize(party);

            }
            catch (Exception ee)
            {
                _logService.Exception(ee, "GiveLoot");
            }

            await Task.CompletedTask;
            return loot;
        }

        List<long> okEquipSlotIds = new List<long>() { EquipSlots.Necklace, EquipSlots.Ring1, EquipSlots.Jewelry1, EquipSlots.OffHand };

        public List<ItemNameResult> GenerateItemNames(IRandom rand, int itemCount, int level)
        {
            List<ItemType> okItemTypes = _gameData.Get<ItemTypeSettings>(null).GetData().Where(x => okEquipSlotIds.Contains(x.EquipSlotId)).ToList();

            okItemTypes = okItemTypes.Where(x => x.Name != "Shield").ToList();

            List<ItemNameResult> retval = new List<ItemNameResult>();

            for (int i = 0; i < itemCount; i++)
            {
                long lootQualityId = QualityTypes.Legendary;

                long itemTypeId = okItemTypes[rand.Next() % okItemTypes.Count].IdKey;

                retval.Add(_itemGenService.GenerateItemName(rand, itemTypeId, level, lootQualityId, new List<FullReagent>()));
            }

            return retval;
        }

        public long GetPartyInventorySize(PartyData party)
        {
            CrawlerLootSettings lootSettings = _gameData.Get<CrawlerLootSettings>(_gs.ch);
            long inventoryPerPlayer = lootSettings.InventoryPerPartyMember + (long)_upgradeService.GetPartyBonus(party, PartyUpgrades.InventorySize);
            return party.GetActiveParty().Count * inventoryPerPlayer;
        }

        public async Task<LootGenData> CreateLootGenData(PartyData party, double expMult, double goldMult, double itemMult, string topMessage = null, ECrawlerStates nextState = ECrawlerStates.None, object nextStateData = null)
        {
            CrawlerLootSettings settings = _gameData.Get<CrawlerLootSettings>(_gs.ch);

            int level = await _worldService.GetMapLevelAtParty(party);

            int itemCount = 1;

            double itemChance = settings.ItemChanceDefault * itemMult;

            while (_rand.NextDouble() < itemChance && itemCount < settings.MaxLootItems)
            {
                itemCount++;
            }

            LootGenData genData = new LootGenData()
            {
                Exp = _trainingService.GetBaseExpForNextLevel(level) * expMult * MathUtils.FloatRange(settings.MinLevelExpMultDefault, settings.MaxLevelExpMultDefault, _rand),
                Gold = _trainingService.GetBaseTrainingCostForNextLevel(level) * goldMult * MathUtils.FloatRange(settings.MinLevelGoldMultDefault, settings.MaxLevelGoldMultDefault, _rand),
                ItemCount = itemCount,
                NextState = nextState,
                NextStateData = nextStateData,
                Level = level,
            };
            if (!string.IsNullOrEmpty(topMessage))
            {
                genData.TopMessages.Add(topMessage);
            }

            return genData;
        }
    }
}
