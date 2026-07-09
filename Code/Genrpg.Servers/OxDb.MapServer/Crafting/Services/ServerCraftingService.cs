using OxDb.MapServer.Trades.Services;
using OxDb.SharedCore.Effects.Entities;
using OxDb.SharedCore.Entities.Constants;
using OxDb.SharedCore.GameSettings;
using OxDb.SharedCore.Interfaces;
using OxDb.SharedCore.Utils;
using OxDb.SharedGame.Characters.PlayerData;
using OxDb.SharedGame.Crafting.Constants;
using OxDb.SharedGame.Crafting.Entities;
using OxDb.SharedGame.Crafting.Messages;
using OxDb.SharedGame.Crafting.PlayerData.Crafting;
using OxDb.SharedGame.Crafting.PlayerData.Recipes;
using OxDb.SharedGame.Crafting.Services;
using OxDb.SharedGame.Crafting.Settings.Crafters;
using OxDb.SharedGame.Inventory.Entities;
using OxDb.SharedGame.Inventory.PlayerData;
using OxDb.SharedGame.Inventory.Services;
using OxDb.SharedGame.Inventory.Settings.ItemTypes;
using System;
using System.Collections.Generic;
using System.Linq;

namespace OxDb.MapServer.Crafting.Services
{

    public interface IServerCraftingService : IInjectable
    {
        CraftingResult CraftItem(Character ch, CraftingItemData data, bool sendUpdates = false);
        UseItemResult LearnRecipe(Character ch, Item recipeItem);
        Item GenerateRecipeReward(Character ch, long level);
    }

    public class ServerCraftingService : IServerCraftingService
    {
        private IGameData _gameData = null;
        private IInventoryService _inventoryService = null;
        private ITradeService _tradeService = null;
        private ISharedCraftingService _sharedCraftingService = null;
        private IItemGenService _itemGenService = null;

        public CraftingResult CraftItem(Character ch, CraftingItemData data, bool sendUpdates = false)
        {
            return _tradeService.SafeModifyObject(ch, delegate { return CraftItemInternal(ch, data, sendUpdates); },
                new CraftingResult());
        }

        private CraftingResult CraftItemInternal(Character ch, CraftingItemData data, bool sendUpdates = false)
        {
            CraftingResult result = new CraftingResult();
            CraftingStats stats = _sharedCraftingService.CalculateStatsFromReagents(ch, data);

            if (stats == null)
            {
                result.Message = "Failed to calculate stats";
                return result;
            }

            if (!stats.IsValid)
            {
                result.Message = "Stat calcs were not valid: " + stats.Message;
                return result;
            }

            result.Message = "This crafting is currently disabled, sorry.";
            return result;

            ValidityResult validResult = _sharedCraftingService.HasValidReagents(ch, data, ch);

            if (validResult == null)
            {
                result.Message = "Failed to create validity result from checking reagents";
                return result;
            }

            if (!validResult.IsValid)
            {
                result.Message = "Reagent validation failed: " + validResult.Message;
                return result;
            }


            long crafterTypeId = _sharedCraftingService.GetCrafterTypeFromRecipe(ch, data.RecipeTypeId, data.ScalingTypeId);

            CraftingData crafterData = ch.Get<CraftingData>();

            CraftingStatus crafterStatus = crafterData.Get(crafterTypeId);

            if (crafterStatus == null)
            {
                result.Message = "Unknown crafter type";
                return result;
            }

            int crafterLevel = crafterStatus.Get(CraftingConstants.CraftingSkill);

            RecipeData recipeData = ch.Get<RecipeData>();

            RecipeStatus recipeStatus = recipeData.Get(data.RecipeTypeId);

            if (recipeStatus == null)
            {
                result.Message = "Unknown recipe";
                return result;
            }

            int recipeSkillLevel = recipeStatus.Get();

            int maxCraftableLevel = Math.Min(crafterLevel, recipeSkillLevel) + CraftingConstants.ExtraCraftingLevelAllowed;

            int levelDiff = maxCraftableLevel - recipeSkillLevel;

            long recipeSkillGainChance = GetGainPercentChanceFromLevelDiff(recipeSkillLevel - stats.Level);

            if (ch.Rand.NextDouble() * 100 < recipeSkillGainChance && recipeStatus.Get() < recipeStatus.GetMaxLevel())
            {
                recipeStatus.AddLevel(1);
            }

            long crafterSkillGainChance = GetGainPercentChanceFromLevelDiff(crafterLevel - stats.Level);

            if (ch.Rand.NextDouble() * 100 < crafterSkillGainChance)
            {
                crafterStatus.AddSkillPoints(CraftingConstants.CraftingSkill, 1);
            }

            if (stats.Level > maxCraftableLevel)
            {
                result.Message = "Item level is too high for your skill";
                return result;
            }

            // Create the new item using the level and quality determined above. Name is generated.


            Item item = new Item()
            {
                Id = HashUtils.NewGuid(),
                Level = stats.Level,
                ItemTypeId = stats.EntityId,
                Name = _itemGenService.GenerateItemName(ch.Rand, stats.EntityId, stats.Level, stats.QualityTypeId, new List<FullReagent>()).SingularName,
            };

            // Now add the stats that were determined above.
            item.Effects = new List<Effect>();
            if (stats.Stats != null)
            {
                foreach (CraftingStat stat in stats.Stats)
                {
                    Effect ieff = new Effect() { EntityTypeId = EntityTypes.Stat, EntityId = stat.Id, Quantity = stat.Val };
                    item.Effects.Add(ieff);
                }
            }

            result.Succeeded = true;
            result.CraftedItem = item;
            _inventoryService.AddItem(ch, item, true);
            return result;

        }

        /// <summary>
        /// Get the chance to get a skill gain, levelDiff is mySkillLevel-tarGet
        /// </summary>
        /// <param name="levelDiff"></param>
        /// <returns></returns>
        protected long GetGainPercentChanceFromLevelDiff(long levelDiff)
        {
            long gainPercent = 0;
            if (levelDiff >= 20)
            {
                gainPercent = 0;
            }
            else if (levelDiff >= 10)
            {
                gainPercent = 25;
            }
            else if (levelDiff >= 5)
            {
                gainPercent = 75;
            }
            else
            {
                gainPercent = 100;
            }

            return gainPercent;
        }

        /// <summary>
        /// Need a better way to do these recipes. Need scaling and recipe rewards.
        /// </summary>
        /// <param name="gs"></param>
        /// <param name="ps"></param>
        /// <param name="level"></param>
        /// <returns></returns>
        public Item GenerateRecipeReward(Character ch, long level)
        {

            return null;
        }


        public UseItemResult LearnRecipe(Character ch, Item recipeItem)
        {
            UseItemResult res = new UseItemResult() { ItemUsed = recipeItem, Success = false };

            ItemProc proc = recipeItem.Procs.FirstOrDefault(x => x.EntityTypeId == EntityTypes.Recipe);

            if (proc == null)
            {
                res.Message = "This is not a recipe item.";
                return res;
            }

            ItemType itype = _gameData.Get<ItemTypeSettings>(ch).Get(recipeItem.ItemTypeId);
            if (itype == null)
            {
                res.Message = "Incorrect recipe item";
                return res;
            }

            RecipeData recipeData = ch.Get<RecipeData>();

            RecipeStatus status = recipeData.Get(recipeItem.ItemTypeId);

            if (status == null)
            {
                res.Message = "You don't know this recipe";
                return res;
            }
            if (_gameData.Get<CraftingSettings>(ch) == null)
            {
                res.Message = "Missing basic crafting info";
                return res;
            }

            if (status.Get() < recipeItem.Level - _gameData.Get<CraftingSettings>(ch).LootLevelIncrement)
            {
                res.Message = "You need to have " + (recipeItem.Level - _gameData.Get<CraftingSettings>(ch).LootLevelIncrement) +
                    " points to learn this recipe.";
                return res;
            }

            status.SetMaxLevel((int)recipeItem.Level);

            res.Success = true;
            res.Message = "Success!";
            return res;
        }
    }
}


