using Genrpg.Shared.Attributes.PlayerData;
using Genrpg.Shared.Interfaces;
using Genrpg.Shared.Serialization.Attributes;
using Genrpg.Shared.Website.Interfaces;
using MessagePack;
using System.Threading.Tasks;

namespace Genrpg.Shared.DataStores.Categories.PlayerData.Units
{
    [MessagePackInterface]

    [Union(0, typeof(Genrpg.Shared.UserMail.PlayerData.UserLetter))]
    [Union(1, typeof(Genrpg.Shared.UserMail.PlayerData.UserMailData))]
    [Union(2, typeof(Genrpg.Shared.UserMail.PlayerData.UserMailDto))]
    [Union(3, typeof(Genrpg.Shared.Trader.Holdings.PlayerData.HoldingsData))]
    [Union(4, typeof(Genrpg.Shared.Trader.Holdings.PlayerData.HoldingsDto))]
    [Union(5, typeof(Genrpg.Shared.Trader.Caravans.PlayerData.CaravanData))]
    [Union(6, typeof(Genrpg.Shared.Trader.Caravans.PlayerData.CaravanDto))]
    [Union(7, typeof(Genrpg.Shared.Spells.PlayerData.CombatAbilityRank))]
    [Union(8, typeof(Genrpg.Shared.Spells.PlayerData.CombatAbilityData))]
    [Union(9, typeof(Genrpg.Shared.Spells.PlayerData.CombatAbilityDto))]
    [Union(10, typeof(Genrpg.Shared.Spells.PlayerData.Spells.Spell))]
    [Union(11, typeof(Genrpg.Shared.Spells.PlayerData.Spells.SpellData))]
    [Union(12, typeof(Genrpg.Shared.Spells.PlayerData.Spells.SpellDto))]
    [Union(13, typeof(Genrpg.Shared.Quests.PlayerData.QuestStatus))]
    [Union(14, typeof(Genrpg.Shared.Quests.PlayerData.QuestData))]
    [Union(15, typeof(Genrpg.Shared.Quests.PlayerData.QuestDto))]
    [Union(16, typeof(Genrpg.Shared.Purchasing.PlayerData.CompletedPurchaseData))]
    [Union(17, typeof(Genrpg.Shared.Purchasing.PlayerData.CurrentPurchaseData))]
    [Union(18, typeof(Genrpg.Shared.Purchasing.PlayerData.PlayerStoreOfferData))]
    [Union(19, typeof(Genrpg.Shared.Purchasing.PlayerData.PlayerStoreOfferDto))]
    [Union(20, typeof(Genrpg.Shared.Purchasing.PlayerData.PurchaseHistoryDto))]
    [Union(21, typeof(Genrpg.Shared.Purchasing.PlayerData.PurchaseHistoryData))]
    [Union(22, typeof(Genrpg.Shared.LoadSave.PlayerData.SaveSlotData))]
    [Union(23, typeof(Genrpg.Shared.Inventory.PlayerData.InventoryData))]
    [Union(24, typeof(Genrpg.Shared.Inventory.PlayerData.InventoryDto))]
    [Union(25, typeof(Genrpg.Shared.Inventory.PlayerData.Item))]
    [Union(26, typeof(Genrpg.Shared.Input.PlayerData.ActionInput))]
    [Union(27, typeof(Genrpg.Shared.Input.PlayerData.ActionInputData))]
    [Union(28, typeof(Genrpg.Shared.Input.PlayerData.ActionInputDto))]
    [Union(29, typeof(Genrpg.Shared.Input.PlayerData.KeyComm))]
    [Union(30, typeof(Genrpg.Shared.Input.PlayerData.KeyCommData))]
    [Union(31, typeof(Genrpg.Shared.Input.PlayerData.KeyCommDto))]
    [Union(32, typeof(Genrpg.Shared.Ftue.PlayerData.FtueData))]
    [Union(33, typeof(Genrpg.Shared.Ftue.PlayerData.FtueDto))]
    [Union(34, typeof(Genrpg.Shared.Factions.PlayerData.ReputationData))]
    [Union(35, typeof(Genrpg.Shared.Factions.PlayerData.ReputationDto))]
    [Union(36, typeof(Genrpg.Shared.Currencies.PlayerData.CharCurrencyData))]
    [Union(37, typeof(Genrpg.Shared.Currencies.PlayerData.CurrencyDto))]
    [Union(38, typeof(Genrpg.Shared.Crawler.Parties.PlayerData.PartyDto))]
    [Union(39, typeof(Genrpg.Shared.Crafting.PlayerData.Recipes.RecipeStatus))]
    [Union(40, typeof(Genrpg.Shared.Crafting.PlayerData.Recipes.RecipeData))]
    [Union(41, typeof(Genrpg.Shared.Crafting.PlayerData.Recipes.RecipeDataDto))]
    [Union(42, typeof(Genrpg.Shared.Crafting.PlayerData.Crafting.CraftingStatus))]
    [Union(43, typeof(Genrpg.Shared.Crafting.PlayerData.Crafting.CraftingData))]
    [Union(44, typeof(Genrpg.Shared.Crafting.PlayerData.Crafting.CraftingDto))]
    [Union(45, typeof(Genrpg.Shared.Core.PlayerData.CoreData))]
    [Union(46, typeof(Genrpg.Shared.Core.PlayerData.CoreDataDto))]
    [Union(47, typeof(Genrpg.Shared.Core.PlayerData.GameAccount))]
    [Union(48, typeof(Genrpg.Shared.Chests.PlayerData.ChestData))]
    [Union(49, typeof(Genrpg.Shared.Chests.PlayerData.ChestStatus))]
    [Union(50, typeof(Genrpg.Shared.Chests.PlayerData.ChestDto))]
    [Union(51, typeof(Genrpg.Shared.Charms.PlayerData.PlayerCharm))]
    [Union(52, typeof(Genrpg.Shared.Charms.PlayerData.PlayerCharmData))]
    [Union(53, typeof(Genrpg.Shared.Charms.PlayerData.PlayerCharmDto))]
    [Union(54, typeof(Genrpg.Shared.CharMail.PlayerData.CharLetter))]
    [Union(55, typeof(Genrpg.Shared.CharMail.PlayerData.CharMailData))]
    [Union(56, typeof(Genrpg.Shared.CharMail.PlayerData.CharMailDto))]
    [Union(57, typeof(Genrpg.Shared.Characters.PlayerData.CoreCharacter))]
    [Union(58, typeof(AttributeData))]
    [Union(59, typeof(GameplayStatDto))]
    [Union(60, typeof(Genrpg.Shared.Achievements.PlayerData.AchievementData))]
    [Union(61, typeof(Genrpg.Shared.Achievements.PlayerData.AchievementDto))]
    public interface IUnitData : IStringId
    {
        IUnitData Unpack();
    }


    public interface IUnitDataLookup
    {
        Task<T> GetAsync<T>() where T : class, IUnitData, new();
        void AddResponse(IWebResponse response);
    }
}


