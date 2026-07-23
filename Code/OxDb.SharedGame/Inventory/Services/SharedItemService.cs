using OxDb.SharedCore.Effects.Entities;
using OxDb.SharedCore.Entities.Constants;
using OxDb.SharedCore.GameSettings;
using OxDb.SharedCore.Interfaces;
using OxDb.SharedCore.PlayerFiltering.Interfaces;
using OxDb.SharedGame.Crafting.Settings.Recipes;
using OxDb.SharedGame.Crawler.Roles.Settings;
using OxDb.SharedGame.Inventory.PlayerData;
using OxDb.SharedGame.Inventory.Settings.ItemTypes;
using OxDb.SharedGame.Inventory.Settings.Qualities;
using OxDb.SharedGame.Inventory.Settings.Ranks;
using OxDb.SharedGame.RpgLevels.Settings;
using OxDb.SharedGame.Stats.Settings.Scaling;
using System;
using System.Linq;

namespace OxDb.SharedGame.Inventory.Services
{

    public class BuyCostArgs
    {
        public long ItemTypeId { get; set; }
        public long QualityTypeId { get; set; }
        public long ScalingTypeId { get; set; }
        public long Level { get; set; }

        public double ExtraScaling { get; set; }
    }

    public class WeaponRoleDamage
    {
        public long ItemTypeId { get; set; }

        public long RoleScalingTypeId { get; set; }

        public double RawMinDam { get; set; }

        public double RawMaxDam { get; set; }

        public int MinDam => (int)RawMinDam;
        public int MaxDam => (int)RawMaxDam;

        public long LootRankId { get; set; }

        public string DamageName { get; set; } = "";

    }

    public interface ISharedItemService : IInjectable
    {
        string GetName(IFilteredObject obj, Item item);
        string GetIcon(IFilteredObject obj, Item item);
        string GetBasicInfo(IFilteredObject obj, Item item);
        string GetMapArt(IFilteredObject obj, Item item);
        long CalcBuyCost(IFilteredObject obj, BuyCostArgs args);
        void CopyStatsFrom(Item fromItem, Item toItem);

        WeaponRoleDamage GetRoleDamage(IFilteredObject obj, long itemTypeId, long roleScalingTypeId, long lootRankId = 0);
    }

    public class SharedItemService : ISharedItemService
    {
        protected IGameData _gameData = null;
        public string GetName(IFilteredObject unit, Item item)
        {
            if (!string.IsNullOrEmpty(item.Name))
            {
                return item.Name;
            }
            ItemType itype = _gameData.Get<ItemTypeSettings>(unit).Get(item.ItemTypeId);
            if (itype == null)
            {
                return "Item";
            }

            item.Name = itype.Name;
            if (item.Name == RecipeType.RecipeItemName)
            {
                Effect firstSet = item.Effects.FirstOrDefault(X => X.EntityTypeId == EntityTypes.Set);
                if (firstSet != null)
                {
                    RecipeType rtype = _gameData.Get<RecipeSettings>(unit).Get(firstSet.EntityId);
                    if (rtype != null)
                    {
                        item.Name = "Recipe: L " + item.Level + " " + rtype.Name;
                    }
                }
            }
            return item.Name;
        }

        public string GetIcon(IFilteredObject unit, Item item)
        {
            string mainIconName = "";

            ItemType itype = _gameData.Get<ItemTypeSettings>(unit).Get(item.ItemTypeId);
            if (string.IsNullOrEmpty(itype.Icon))
            {
                mainIconName = "";
            }
            else
            {
                mainIconName = itype.Icon;
            }

            if (item.IconIndex < 1)
            {

                int maxIconIndex = Math.Max(1, itype.IconCount);

                int IdHash = 1;

                if (!string.IsNullOrEmpty(item.Id))
                {
                    for (int c = 0; c < Math.Min(3, item.Id.Length); c++)
                    {
                        IdHash += item.Id[c] * (c + 1) * (c + 1) * 17;
                    }
                }

                item.IconIndex = ((IdHash * 131 + 29) % maxIconIndex) + 1;
            }

            return mainIconName + "_" + item.IconIndex.ToString("D3");
        }

        public string GetBasicInfo(IFilteredObject unit, Item item)
        {
            if (!string.IsNullOrEmpty(item.GetBasicInfo()))
            {
                return item.GetBasicInfo();
            }

            ItemType itype = _gameData.Get<ItemTypeSettings>(unit).Get(item.ItemTypeId);

            string basicInfo = "Lv. " + item.Level;

            if (itype != null)
            {
                basicInfo += " " + itype.Name;
            }

            item.SetBasicInfo(basicInfo);

            return item.GetBasicInfo();

        }

        public string GetMapArt(IFilteredObject obj, Item item)
        {
            if (!string.IsNullOrEmpty(item.GetArt()))
            {
                return item.GetArt();
            }

            ItemType itype = _gameData.Get<ItemTypeSettings>(obj).Get(item.ItemTypeId);
            if (itype == null || string.IsNullOrEmpty(itype.Art))
            {
                item.SetArt("");
            }
            else
            {
                item.SetArt(itype.Art);
            }
            return item.GetArt();
        }

        public long CalcBuyCost(IFilteredObject obj, BuyCostArgs args)
        {
            long buyPrice = 0;
            int minBuyPrice = 8;
            if (buyPrice < 1)
            {
                long itemValue = minBuyPrice;
                RpgLevel levelData = _gameData.Get<RpgLevelSettings>(obj).Get(args.Level);
                if (levelData != null)
                {
                    itemValue = levelData.KillMoney * 5;
                }

                if (itemValue < buyPrice)
                {
                    itemValue = buyPrice;
                }

                QualityType quality = _gameData.Get<QualityTypeSettings>(obj).Get(args.QualityTypeId);
                if (quality != null && quality.ItemCostPct > 0)
                {
                    itemValue = itemValue * quality.ItemCostPct / 100;
                }
                else
                {
                    itemValue *= 100;
                }

                ScalingType scaling = _gameData.Get<ScalingTypeSettings>(obj).Get(args.ScalingTypeId);
                if (scaling != null)
                {
                    itemValue *= scaling.CostPct;
                }
                else
                {
                    itemValue *= 100;
                }

                itemValue /= 10000;

                if (itemValue < minBuyPrice)
                {
                    itemValue = minBuyPrice;
                }

                buyPrice = itemValue;

            }

            return buyPrice;
        }

        public void CopyStatsFrom(Item fromItem, Item toItem)
        {
            toItem.SetArt(fromItem.GetArt());
            toItem.Effects = fromItem.Effects;

        }

        public WeaponRoleDamage GetRoleDamage(IFilteredObject obj, long itemTypeId, long roleScalingTypeId, long lootRankId = 0)
        {
            WeaponRoleDamage roleDamage = new WeaponRoleDamage()
            {
                ItemTypeId = itemTypeId,
                RoleScalingTypeId = roleScalingTypeId,
                LootRankId = lootRankId,
            };

            ItemType itype = _gameData.Get<ItemTypeSettings>(obj).Get(itemTypeId);

            if (itype == null)
            {
                return roleDamage;
            }

            Effect damEffect = itype.Effects.FirstOrDefault(x => x.EntityTypeId == EntityTypes.RoleScaling && x.EntityId ==
             roleScalingTypeId);

            if (damEffect == null)
            {
                return roleDamage;
            }

            RoleScalingType scalingType = _gameData.Get<RoleScalingTypeSettings>(obj).Get(roleScalingTypeId);

            if (scalingType == null)
            {
                return roleDamage;
            }

            roleDamage.DamageName = scalingType.Name;

            double minDam = itype.MinDam * damEffect.Quantity / 100.0f;
            double maxDam = itype.MaxDam * damEffect.Quantity / 100.0f;

            LootRank rank = _gameData.Get<LootRankSettings>(obj).Get(lootRankId);

            if (rank != null)
            {
                minDam *= rank.DamageScale;
                maxDam *= rank.DamageScale;
            }

            roleDamage.RawMinDam = minDam;
            roleDamage.RawMaxDam = maxDam;

            return roleDamage;

        }
    }
}


