using Assets.Scripts.Crawler.Maps.Services;
using Assets.Scripts.UI.Constants;
using Assets.Scripts.UI.Interfaces;
using OxDb.SharedCore.Effects.Entities;
using OxDb.SharedCore.Entities.Constants;
using OxDb.SharedCore.GameSettings;
using OxDb.SharedCore.HelperClasses;
using OxDb.SharedCore.Interfaces;
using OxDb.SharedCore.Logalytics.Interfaces;
using OxDb.SharedCore.Utils;
using OxDb.SharedCore.Utils.Data;
using OxDb.SharedGame.Crafting.Entities;
using OxDb.SharedGame.Crawler.Crafting.Settings;
using OxDb.SharedGame.Crawler.Crawlers.Services;
using OxDb.SharedGame.Crawler.Loot.Constants;
using OxDb.SharedGame.Crawler.Loot.Helpers;
using OxDb.SharedGame.Crawler.Loot.Settings;
using OxDb.SharedGame.Crawler.Maps.Entities;
using OxDb.SharedGame.Crawler.Monsters.Entities;
using OxDb.SharedGame.Crawler.Options.Constants;
using OxDb.SharedGame.Crawler.Options.Services;
using OxDb.SharedGame.Crawler.Parties.PlayerData;
using OxDb.SharedGame.Crawler.Party.Services;
using OxDb.SharedGame.Crawler.Quests.Services;
using OxDb.SharedGame.Crawler.States.Constants;
using OxDb.SharedGame.Crawler.States.Services;
using OxDb.SharedGame.Crawler.Training.Services;
using OxDb.SharedGame.Crawler.Training.Settings;
using OxDb.SharedGame.Crawler.Upgrades.Constants;
using OxDb.SharedGame.Currencies.Constants;
using OxDb.SharedGame.Currencies.Settings;
using OxDb.SharedGame.Inventory.Constants;
using OxDb.SharedGame.Inventory.Entities;
using OxDb.SharedGame.Inventory.PlayerData;
using OxDb.SharedGame.Inventory.Services;
using OxDb.SharedGame.Inventory.Settings.ItemTypes;
using OxDb.SharedGame.Inventory.Settings.Ranks;
using OxDb.SharedGame.Inventory.Settings.Slots;
using OxDb.SharedGame.Stats.Constants;
using OxDb.SharedGame.Stats.Settings.Scaling;
using OxDb.SharedGame.Stats.Settings.Stats;
using OxDb.SharedGame.Vendors.Settings;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace OxDb.SharedGame.Crawler.Loot.Services
{


    public interface ILootGenService : IInjectable
    {

        Item GenerateItem(ItemGenArgs lootGenData);
        Task<LootGenData> GenerateCombatLoot(PartyData party, CancellationToken token);
        Task<PartyLoot> GiveLoot(PartyData party, CrawlerMap map, LootGenData genData, CancellationToken token);
        List<ItemNameResult> GenerateItemNames(IRandom rand, int itemCount, int level, string forcedItemName = null);
        long GetPartyInventorySize(PartyData party);
        Task<LootGenData> CreateLootGenData(PartyData party, double expMult, double goldMult, double itemMult, string topMessage = null, ECrawlerStates nextState = ECrawlerStates.None, object nextStateData = null);
    }

    public class LootGenData
    {
        public double Exp { get; set; }
        public SmallIdLongCollection Currencies { get; set; } = new SmallIdLongCollection();
        public int ItemCount { get; set; }
        public long Level { get; set; }
        public List<long> QuestItems { get; set; } = new List<long>();
        public ECrawlerStates NextState { get; set; } = ECrawlerStates.None;
        public object NextStateData { get; set; } = null;
        public List<string> TopMessages { get; set; } = new List<string>();
        public List<string> ExtraMessages { get; set; } = new List<string>();
    }

    public class PartyLoot
    {
        public SmallIdLongCollection Currencies { get; set; } = new SmallIdLongCollection();
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
        private IItemGenService _itemGenService = null;
        private ICrawlerUpgradeService _upgradeService = null;
        private ICrawlerService _crawlerService = null;
        private ILogService _logService = null;
        private ICrawlerQuestService _questService = null;
        private ITrainingService _trainingService = null;
        private ICrawlerWorldService _worldService = null;
        private IPartyService _partyService = null;
        private ICrawlerOptionsService _optionsService = null;
        private ITextService _textService = null;


        private SetupDictionaryContainer<long, ICrawlerLootTypeHelper> _lootTypeHelpers = new SetupDictionaryContainer<long, ICrawlerLootTypeHelper>();

        public Item GenerateItem(ItemGenArgs itemGenArgs)
        {
            return GenerateEquipment(itemGenArgs);
        }

        public Item GenerateEquipment(ItemGenArgs itemGenArgs)
        {
            if (itemGenArgs == null)
            {
                itemGenArgs = new ItemGenArgs();
            }

            long level = itemGenArgs.Level;

            PartyData party = _crawlerService.GetParty();

            CrawlerLootSettings lootSettings = _gameData.Get<CrawlerLootSettings>(null);

            LootRankSettings rankSettings = _gameData.Get<LootRankSettings>(null);

            IReadOnlyList<LootRank> ranks = rankSettings.GetData();

            int expectedOffset = (int)(level / Math.Max(1, rankSettings.LevelsPerQuality));

            expectedOffset = MathUtil.Clamp(1, expectedOffset, ranks.Count - 2);

            List<LootRank> okRanks = new List<LootRank>();

            while (expectedOffset < ranks.Count - 2 && _gs.Rand.NextDouble() < rankSettings.ExtraQualityChance)
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

            if (okRanks.Count < 1)
            {
                return null;
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
            while (rankIndex < okRanks.Count - 1 && _gs.Rand.NextDouble() < rankSettings.ExtraQualityChance)
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

            if (_optionsService.HasOption(party, CrawlerOptions.AllEquipmentSlots))
            {
                allItemSlotsOk = true;
            }

            if (itemType == null)
            {
                List<EquipSlot> okEquipSlots = _gameData.Get<EquipSlotSettings>(null).GetData().Where(x => x.IsCrawlerSlot || allItemSlotsOk).ToList();

                List<EquipSlot> weaponSlots = _gameData.Get<EquipSlotSettings>(null).GetData().Where(x => x.IsWeaponSlot).ToList();

                List<long> weaponSlotIds = weaponSlots.Select(x => x.IdKey).ToList();

                List<long> okEquipSlotIds = okEquipSlots.Select(x => x.IdKey).ToList();

                IReadOnlyList<ItemType> allLootItems = _gameData.Get<ItemTypeSettings>(null).GetData();

                List<ItemType> okLootItems = allLootItems.Where(x => okEquipSlotIds.Contains(x.EquipSlotId)).ToList();

                List<ItemType> weaponItems = okLootItems.Where(x => weaponSlotIds.Contains(x.EquipSlotId)).ToList();

                List<ItemType> rangedWeapons = weaponItems.Where(x => x.EquipSlotId == EquipSlots.Ranged).ToList();

                foreach (ItemType rangedWeaponType in rangedWeapons)
                {
                    for (int i = 0; i < 2; i++)
                    {
                        weaponItems.Add(rangedWeaponType);
                    }
                }

                List<ItemType> armorItems = okLootItems.Where(x => x.EquipSlotId > 0 && !weaponSlotIds.Contains(x.IdKey)).ToList();

                bool armorItem = _gs.Rand.NextDouble() < rankSettings.ArmorChance;

                List<ItemType> finalList = (armorItem ? armorItems : weaponItems);

                if (finalList.Count < 1)
                {
                    return null;
                }

                itemType = finalList[_gs.Rand.Next() % finalList.Count];
            }

            EquipSlot finalSlot = _gameData.Get<EquipSlotSettings>(null).Get(itemType.EquipSlotId);

            bool isArmor = finalSlot != null && !finalSlot.IsWeaponSlot;

            ScalingType scalingType = null;
            long scalingTypeId = 0;

            if (itemType == null)
            {
                return null;
            }

            scalingTypeId = RandUtils.IntRange(1, LootConstants.MaxArmorScalingType, _gs.Rand);
            scalingType = _gameData.Get<ScalingTypeSettings>(null).Get(scalingTypeId);

            if (scalingType == null)
            {
                return null;
            }

            Item item = new Item() { Id = HashUtils.NewGuid().ToString() };

            item.ItemTypeId = itemType.IdKey;

            item.LootRankId = chosenRank.IdKey;

            EquipSlot equipSlot = _gameData.Get<EquipSlotSettings>(null).Get(itemType.EquipSlotId);

            if (itemType.Armor > 0)
            {

                long baseArmor = (long)(itemType.Armor * scalingType.ArmorPct * chosenRank.DefenseScale / 100.0f);

                if (baseArmor > 0)
                {
                    item.Effects.Add(new Effect() { EntityTypeId = EntityTypes.Stat, EntityId = StatTypes.Armor, Quantity = baseArmor });
                }
            }

            if (itemType.Resist > 0)
            {
                long baseResist = (long)(itemType.Resist * scalingType.ArmorPct * chosenRank.DefenseScale / 100.0f);

                if (baseResist > 0)
                {
                    item.Effects.Add(new Effect() { EntityTypeId = EntityTypes.Stat, EntityId = StatTypes.Resist, Quantity = baseResist });
                }
            }


            string baseItemName = RandUtils.GetRandomElement(itemType.GetNames(), _gs.Rand)?.Name ?? "Armor";

            // Weapon damage is calculated dynamically as needed.

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
                if (_gs.Rand.NextDouble() < chosenRank.IdKey * 0.2f)
                {
                    statQuantity++;
                }
                for (int i = 0; i < statQuantity && okStats.Count > 0; i++)
                {

                    StatType okStat = okStats[_gs.Rand.Next() % okStats.Count];
                    usedStatTypeIds.Add(okStat.IdKey);
                    okStats.Remove(okStat);
                }

                usedStatTypeIds = usedStatTypeIds.OrderBy(x => x).ToList();

                double midStatAmount = lootSettings.StartStatBonusAmount + level * lootSettings.StatBonusPerLevel;

                double bonusStatScale = equipSlot.BonusStatScale;

                if (equipSlot.IdKey == EquipSlots.MainHand && itemType.HasFlag(ItemFlags.FlagTwoHandedItem))
                {
                    bonusStatScale += _gameData.Get<EquipSlotSettings>(_gs.ch).Get(EquipSlots.OffHand).BonusStatScale;
                }

                midStatAmount *= bonusStatScale;

                foreach (long statTypeId in usedStatTypeIds)
                {

                    double finalStatAmount = Math.Max(1, Math.Round(midStatAmount * RandUtils.DeltaScale(lootSettings.StatBonusVariance, _gs.Rand)));


                    Effect itemEffect = new Effect()
                    {
                        EntityTypeId = EntityTypes.Stat,
                        EntityId = statTypeId,
                        Quantity = (int)finalStatAmount,
                    };

                    item.Effects.Add(itemEffect);
                }

                if (itemGenArgs.PowerIncrease > 0)
                {
                    double extraStatQuantity = lootSettings.StatPointsPerExtraItem * itemGenArgs.PowerIncrease;
                    foreach (Effect effect in item.Effects)
                    {
                        if (effect.EntityTypeId == EntityTypes.Stat)
                        {
                            effect.Quantity +=
                                (long)extraStatQuantity +
                                _gs.Rand.NextDouble() < (extraStatQuantity - (long)extraStatQuantity) ? 1 : 0;
                        }
                    }
                }

                if (_gs.Rand.NextDouble() < lootSettings.BaseEnchantChance + lootSettings.EnchantChancePerPowerIncrease * itemGenArgs.PowerIncrease)
                {
                    CrawlerLootType enchantType = RandUtils.GetRandomEnchant(lootSettings.GetData(), _gs.Rand);

                    if (enchantType != null)
                    {
                        if (_lootTypeHelpers.TryGetValue(enchantType.EntityTypeId, out ICrawlerLootTypeHelper helper))
                        {
                            helper.AddEnchantToItem(party, item, itemGenArgs);
                        }
                    }
                }
            }

            item.Name = chosenRank.Name + " " + _itemGenService.GenerateItemName(_gs.Rand, itemType.IdKey, level, QualityTypes.Uncommon, null).SingularName;
            item.Level = Math.Max(1, level);

            double cost = lootSettings.BaseLootCost;

            cost = cost * (1 + (itemType.MinDam + itemType.MaxDam) / 2.0f);

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

            cost = cost * chosenRank.CostScale;

            item.BuyCost = (long)cost;
            item.SellValue = (long)(cost * _gameData.Get<VendorSettings>(_gs.ch).SellToVendorPriceMult);
            item.Level = Math.Max(1, item.Level);
            item.Name = item.Name.Trim();
            return item;
        }

        public async Task<LootGenData> GenerateCombatLoot(PartyData party, CancellationToken token)
        {
            if (party.Combat == null || party.ActiveParty.Count < 1)
            {
                return new LootGenData();
            }


            CrawlerTrainingSettings trainingSettings = _gameData.Get<CrawlerTrainingSettings>(null);

            CrawlerLootSettings lootSettings = _gameData.Get<CrawlerLootSettings>(null);

            CrawlerCraftingSettings craftingSettings = _gameData.Get<CrawlerCraftingSettings>(null);

            double extraScalePerBonus = lootSettings.ExtraLootScalePerMonsterBonus;

            double itemChance = lootSettings.ItemChancePerMonster;

            double exp = 0;
            double gold = 0;

            int itemCount = 0;
            int reagentCount = 0;

            long minGold = (long)(party.Combat.Level * lootSettings.MinGoldPerLevel);
            long maxGold = (long)(party.Combat.Level * lootSettings.MaxGoldPerLevel);

            long expPerMonster = trainingSettings.GetMonsterExp(party.Combat.Level);

            foreach (CrawlerUnit crawlerUnit in party.Combat.EnemiesKilled)
            {
                double lootScale = (1 + crawlerUnit.BonusCount * extraScalePerBonus);
                exp += expPerMonster * lootScale;
                gold += RandUtils.LongRange(minGold, maxGold, _gs.Rand) * lootScale;

                if (_gs.Rand.NextDouble() < itemChance * lootScale)
                {
                    itemCount++;
                }

                if (_gs.Rand.NextDouble() < craftingSettings.MonsterDropReagentChance)
                {
                    reagentCount++;
                }
            }

            if (_gs.Rand.NextDouble() < lootSettings.FirstMonsterItemDropChance)
            {
                itemCount++;
            }

            long maxLevel = party.ActiveParty.Max(x => x.Level);

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
                Exp = exp,
                Level = party.Combat.Level,
                ItemCount = itemCount,
                ExtraMessages = await _questService.UpdateAfterCombat(party, party.Combat, token),
                NextState = ECrawlerStates.ExploreWorld,
                NextStateData = null,
            };

            allLootGenData.Currencies[CoreCurrencyTypes.Coins] = (long)gold;

            if (reagentCount > 0)
            {
                IReadOnlyList<CoreCurrencyType> ctypes = _gameData.Get<CoreCurrencyTypeSettings>(null).GetData();

                for (int i = 0; i < reagentCount; i++)
                {
                    allLootGenData.Currencies.Add(ctypes[_gs.Rand.Next() % ctypes.Count].IdKey, 1);
                }
            }


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

            bool harderMonsters = _optionsService.HasOption(party, CrawlerOptions.HarderMonsters);

            CrawlerLootSettings lootSettings = _gameData.Get<CrawlerLootSettings>(_gs.ch);

            List<Item> items = new List<Item>();

            double lootQualityBonus = _upgradeService.GetPartyBonus(party, PartyUpgrades.LootQuality);

            long extraItems = Math.Max(0, genData.ItemCount - lootSettings.MaxLootItems);

            if (harderMonsters)
            {
                extraItems++;
            }

            for (int i = 0; i < Math.Min(lootSettings.MaxLootItems, genData.ItemCount); i++)
            {
                ItemGenArgs itemGenArgs = new ItemGenArgs()
                {
                    Level = genData.Level,
                    QualityTypeId = (long)(_gs.Rand.NextDouble() * (lootQualityBonus * 2 + 0.5f)),
                    PowerIncrease = extraItems,
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

                    party.QuestItems.SetBitIndex(questItemId);
                }

                while (items.Count > lootSettings.MaxLootItems)
                {
                    Item lastItem = items.Last();
                    items.Remove(lastItem);

                    genData.Currencies.Add(CoreCurrencyTypes.Coins, lastItem.BuyCost);
                }


                loot.Items = items;

                loot.Currencies[CoreCurrencyTypes.Coins] = (long)(genData.Currencies[CoreCurrencyTypes.Coins] *
                    (1 + _upgradeService.GetPartyBonus(party, PartyUpgrades.GoldPercent) / 100.0f));


                IReadOnlyList<CoreCurrencyType> ctypes = _gameData.Get<CoreCurrencyTypeSettings>(_gs.ch).GetData();

                foreach (CoreCurrencyType ctype in ctypes)
                {

                    if (loot.Currencies[ctype.IdKey] > 0)
                    {
                        _partyService.AddCurrency(party, ctype.IdKey, loot.Currencies[ctype.IdKey]);
                    }
                }

                genData.Exp = (long)(genData.Exp * (1 + _upgradeService.GetPartyBonus(party, PartyUpgrades.ExpPercent) / 100.0f));
                loot.Exp = (long)genData.Exp / party.ActiveParty.Count;

                foreach (PartyMember member in party.ActiveParty)
                {
                    long oldLevel = member.Level;
                    _partyService.AddExp(party, member, loot.Exp);
                    if (member.Level > oldLevel)
                    {
                        loot.ExtraMessages.Add(_textService.HighlightText(member.Name + " Levelled up to level " + member.Level + "!", TextColors.ColorGold));
                    }
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

        public List<ItemNameResult> GenerateItemNames(IRandom rand, int itemCount, int level, string forcedItemName = null)
        {
            List<ItemType> okItemTypes = _gameData.Get<ItemTypeSettings>(null).GetData().Where(x => okEquipSlotIds.Contains(x.EquipSlotId)).ToList();

            okItemTypes = okItemTypes.Where(x => x.Name != "Shield").ToList();

            List<ItemNameResult> retval = new List<ItemNameResult>();

            for (int i = 0; i < itemCount; i++)
            {
                long lootQualityId = QualityTypes.Legendary;

                long itemTypeId = okItemTypes[rand.Next() % okItemTypes.Count].IdKey;

                retval.Add(_itemGenService.GenerateItemName(rand, itemTypeId, level, lootQualityId, new List<FullReagent>(), forcedItemName));
            }

            return retval;
        }

        public long GetPartyInventorySize(PartyData party)
        {

            CrawlerLootSettings lootSettings = _gameData.Get<CrawlerLootSettings>(_gs.ch);
            long inventoryPerPlayer = lootSettings.InventoryPerPartyMember + (long)_upgradeService.GetPartyBonus(party, PartyUpgrades.InventorySize);

            long count = party.ActiveParty.Count;
            if (!_optionsService.HasOption(party, CrawlerOptions.WholeParty))
            {
                count = 5;
            }
            return count * inventoryPerPlayer;
        }

        public async Task<LootGenData> CreateLootGenData(PartyData party, double expMult, double goldMult, double itemMult, string topMessage = null, ECrawlerStates nextState = ECrawlerStates.None, object nextStateData = null)
        {
            CrawlerLootSettings settings = _gameData.Get<CrawlerLootSettings>(_gs.ch);

            long level = await _worldService.GetMapLevelAtParty(party);

            int itemCount = 1;

            double itemChance = settings.ItemChanceDefault * itemMult;

            while (_gs.Rand.NextDouble() < itemChance && itemCount < settings.MaxLootItems)
            {
                itemCount++;
            }

            LootGenData genData = new LootGenData()
            {
                Exp = _trainingService.GetBaseExpForNextLevel(level) * expMult * RandUtils.FloatRange(settings.MinLevelExpMultDefault, settings.MaxLevelExpMultDefault, _gs.Rand),
                ItemCount = itemCount,
                NextState = nextState,
                NextStateData = nextStateData,
                Level = level,
            };

            genData.Currencies[CoreCurrencyTypes.Coins] = (long)(_trainingService.GetBaseTrainingCostForNextLevel(level) * goldMult *
                RandUtils.FloatRange(settings.MinLevelGoldMultDefault, settings.MaxLevelGoldMultDefault, _gs.Rand));

            if (!string.IsNullOrEmpty(topMessage))
            {
                genData.TopMessages.Add(topMessage);
            }

            return genData;
        }
    }
}


