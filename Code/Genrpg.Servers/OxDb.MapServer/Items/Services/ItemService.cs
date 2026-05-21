using OxDb.MapServer.Crafting.Services;
using OxDb.MapServer.Maps;
using OxDb.MapServer.Spawns.Services;
using OxDb.MapServer.Trades.Services;
using OxDb.SharedCore.Entities.Constants;
using OxDb.SharedCore.Interfaces;
using OxDb.SharedCore.Rewards.Entities;
using OxDb.SharedCore.Utils;
using OxDb.SharedGame.Characters.PlayerData;
using OxDb.SharedGame.Inventory.Entities;
using OxDb.SharedGame.Inventory.PlayerData;
using OxDb.SharedGame.Inventory.Services;
using OxDb.SharedGame.Rewards.Constants;
using OxDb.SharedGame.Rewards.Services;
using OxDb.SharedGame.Spawns.Entities;
using OxDb.SharedGame.Spells.Casting;
using OxDb.SharedGame.Units.Entities;
using System;
using System.Collections.Generic;
using System.Linq;

namespace OxDb.MapServer.Items.Services
{
    public interface IItemService : IInjectable
    {
        UseItemResult UseItem(IRandom rand, Character ch, Item item);

    }


    public class ItemService : IItemService
    {
        private ISpawnService _spawnService = null;
        private IServerCraftingService _craftingService = null;
        private IRewardService _rewardService = null;
        private IMapObjectManager _objectManager = null;
        private IInventoryService _inventoryService = null;
        private ITradeService _tradeService = null;

        // This should call out to different functions in different parts of the code.
        // Eventually split these cases into separate functions.
        public UseItemResult UseItem(IRandom rand, Character ch, Item item)
        {
            return _tradeService.SafeModifyObject(ch, delegate
            {
                return UseItemInternal(rand, ch, item);
            },
            new UseItemResult() { ItemUsed = item, Success = false });
        }

        private UseItemResult UseItemInternal(IRandom rand, Character ch, Item item)
        {
            UseItemResult res = new UseItemResult() { ItemUsed = item, Success = false };
            if (item == null)
            {
                res.Message = "Missing item";
                return res;
            }
            bool shouldRemoveItem = false;

            ItemProc theProc = null;

            ItemProc recipeProc = item.Procs.FirstOrDefault(x => x.EntityTypeId == EntityTypes.Recipe);

            if (recipeProc != null)
            {
                theProc = recipeProc;
                shouldRemoveItem = true;
                res = _craftingService.LearnRecipe(rand, ch, item);
            }

            if (theProc == null)
            {
                ItemProc spawnProc = item.Procs.FirstOrDefault(x => x.EntityTypeId == EntityTypes.Spawn);

                if (spawnProc != null)
                {
                    shouldRemoveItem = true;
                    theProc = spawnProc;
                    RollLootArgs rollLootArgs = new RollLootArgs()
                    {
                        Level = ch.Level,
                        Times = 1
                    };
                    List<RewardList> newItems = _spawnService.Roll(rand, spawnProc.EntityId, RewardSources.UseItem, rollLootArgs);
                    if (newItems != null)
                    {
                        _rewardService.GiveRewards(ch, newItems, null).Wait();
                    }

                    res.Success = true;
                }
            }

            if (theProc == null)
            {
                ItemProc spellProc = item.Procs.FirstOrDefault(x => x.EntityTypeId == EntityTypes.Spell);
                theProc = spellProc;
                if (spellProc != null)
                {
                    if (spellProc.MaxCharges > 0 && spellProc.CurrCharges < 1)
                    {
                        res.Message = "Out of Charges";
                        return res;
                    }

                    if (spellProc.CooldownSeconds > 0 && (DateTime.UtcNow - spellProc.LastUsedTime).TotalSeconds < spellProc.CooldownSeconds)
                    {
                        res.Message = "Item is on cooldown";
                        return res;
                    }

                    if (_objectManager.GetUnit(ch.TargetId, out Unit targUnit))
                    {
                        CastResult cr = new CastResult();
                        res.Success = true;
                    }
                    spellProc.CurrCharges--;
                    spellProc.LastUsedTime = DateTime.UtcNow;
                }
            }

            if (theProc != null && res.Success)
            {
                if (shouldRemoveItem)
                {
                    _inventoryService.RemoveItem(ch, item.Id, true);
                }
            }

            return res;
        }
    }
}


