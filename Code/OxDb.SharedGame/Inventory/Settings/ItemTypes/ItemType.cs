using OxDb.SharedCore.Effects.Entities;
using OxDb.SharedCore.Entities.Constants;
using OxDb.SharedCore.GameSettings;
using OxDb.SharedCore.GameSettings.BaseDataStores;
using OxDb.SharedCore.Interfaces;
using OxDb.SharedGame.Inventory.Constants;
using OxDb.SharedGame.Inventory.Settings.Qualities;
using OxDb.SharedGame.Inventory.Settings.Slots;
using OxDb.SharedGame.MapObjects.Entities;
using OxDb.SharedGame.Names.Settings;
using OxDb.SharedGame.RpgLevels.Settings;
using OxDb.SharedGame.Units.Entities;
using System.Collections.Generic;
using System.Linq;

namespace OxDb.SharedGame.Inventory.Settings.ItemTypes
{
    public class ItemType : ChildSettings, IIndexedGameItem
    {

        public const int MinRangedItemLevel = 5;
        public const int LevelGap = 2 * MinRangedItemLevel;
        public override string Id { get; set; }
        public override string ParentId { get; set; }
        public long IdKey { get; set; }
        public override string Name { get; set; }
        public string Desc { get; set; }
        public string AtlasPrefix { get; set; }
        public string Icon { get; set; }
        public string Art { get; set; }

        public int MinVal { get; set; }
        public int MaxVal { get; set; }
        // Probably want to use bitfields but bleh. IDK.
        public long EquipSlotId { get; set; }

        public int IconCount { get; set; }

        public int Flags { get; set; }
        public bool HasFlag(int flagBits) { return (Flags & flagBits) != 0; }
        public void AddFlags(int flagBits) { Flags |= flagBits; }
        public void RemoveFlags(int flagBits) { Flags &= ~flagBits; }


        public List<Effect> Effects { get; set; } = new List<Effect>();

        public List<WeightedName> Names { get; set; }

        public ItemType()
        {
            Effects = new List<Effect>();
            Names = new List<WeightedName>();
        }


        public string GetIcon(int level)
        {
            return Icon;
        }
        public string GetNamePrefix(int level, long quality)
        {
            return "";
        }

        public Dictionary<long, long> GetCraftingStatPercents(IGameData gameData, Unit crafter, long level, long quality)
        {
            Dictionary<long, long> dict = new Dictionary<long, long>();

            if (Effects == null)
            {
                return dict;
            }

            RpgLevel ldata = gameData.Get<RpgLevelSettings>(crafter).Get(level);
            QualityType qtype = gameData.Get<QualityTypeSettings>(crafter).Get(quality);

            int baseStat = 10;
            int qualityPercent = 100;
            if (ldata != null)
            {
                baseStat = ldata.StatAmount;
            }

            if (qtype != null)
            {
                qualityPercent = qtype.ItemStatPct;
            }

            int globalScaling = gameData.Get<ItemTypeSettings>(crafter).GenGlobalScalingPercent;


            foreach (Effect eff in Effects)
            {
                if (eff.EntityTypeId != EntityTypes.Stat)
                {
                    continue;
                }

                // amt    basestat globalscale qualScale example
                // 125   *   75    *    20    *   150 =  
                long value = eff.Quantity * baseStat * globalScaling * qualityPercent / (100L * 100);
                dict[eff.EntityId] = value;
            }

            return dict;
        }

        /// <summary>
        /// Get any slots related by parent/child relationships to this slot. Go only one level deep.
        /// </summary>
        /// <param name="gs"></param>
        /// <returns></returns>
        public List<long> GetRelatedEquipSlots(IGameData gameData, Unit unit)
        {
            List<long> retval = new List<long>();
            if (EquipSlotId < 1)
            {
                return retval;
            }


            EquipSlot eqSlot = gameData.Get<EquipSlotSettings>(unit).Get(EquipSlotId);
            if (eqSlot == null)
            {
                return retval;
            }
            retval.Add(EquipSlotId);

            if (eqSlot.ParentEquipSlotId > 0 && !retval.Contains(eqSlot.ParentEquipSlotId))
            {
                retval.Add(eqSlot.ParentEquipSlotId);
            }

            List<EquipSlot> childSlots = gameData.Get<EquipSlotSettings>(unit).GetData().Where(x => x.ParentEquipSlotId == EquipSlotId).ToList();
            foreach (EquipSlot childSlot in childSlots)
            {
                if (!retval.Contains(childSlot.IdKey))
                {
                    retval.Add(childSlot.IdKey);
                }
            }
            return retval;

        }

        /// <summary>
        /// Get all slots where we could place this item.
        /// </summary>
        /// <param name="gs"></param>
        /// <returns></returns>
        public List<long> GetCompatibleEquipSlots(IGameData gameData, MapObject unit)
        {
            List<long> retval = new List<long>();
            if (gameData.Get<EquipSlotSettings>(unit).GetData() == null || EquipSlotId < 1)
            {
                return retval;
            }

            EquipSlot eqSlot = gameData.Get<EquipSlotSettings>(unit).Get(EquipSlotId);
            if (eqSlot == null)
            {
                return retval;
            }
            retval.Add(EquipSlotId);

            if (EquipSlotId == EquipSlots.OffHand)
            {
                return retval;
            }

            if (HasFlag(ItemFlags.FlagTwoHandedItem))
            {
                return retval;
            }


            long mainSlotId = eqSlot.ParentEquipSlotId > 0 ? eqSlot.ParentEquipSlotId : eqSlot.IdKey;

            foreach (EquipSlot slot in gameData.Get<EquipSlotSettings>(unit).GetData())
            {
                if (slot.IdKey < 1 || retval.Contains(slot.IdKey))
                {
                    continue;
                }
                if (slot.IdKey == mainSlotId || slot.ParentEquipSlotId == mainSlotId)
                {
                    retval.Add(slot.IdKey);
                }

            }
            return retval;
        }
    }

    public class LevelRangeName
    {
        public int MinLevel { get; set; }
        public int MaxLevel { get; set; }
        public string Name { get; set; }

        public string Icon { get; set; }
    }
}


