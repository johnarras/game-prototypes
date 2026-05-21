using MessagePack;
using OxDb.SharedCore.Interfaces;
using OxDb.SharedCore.Serialization.Attributes;
using OxDb.SharedCore.Website.Responses.Interfaces;
using System.Threading.Tasks;

namespace OxDb.SharedGame.DataStores.Categories.PlayerData.Units
{
    [MessagePackInterface]
    [Union(0, typeof(OxDb.SharedGame.UserMail.PlayerData.UserLetter))]
    [Union(1, typeof(OxDb.SharedGame.UserMail.PlayerData.UserMailData))]
    [Union(2, typeof(OxDb.SharedGame.UserMail.PlayerData.UserMailDto))]
    [Union(3, typeof(OxDb.SharedGame.Trader.Shipments.PlayerData.ShipmentData))]
    [Union(4, typeof(OxDb.SharedGame.Trader.Shipments.PlayerData.ShipmentDto))]
    [Union(5, typeof(OxDb.SharedGame.Trader.Holdings.PlayerData.HoldingsData))]
    [Union(6, typeof(OxDb.SharedGame.Trader.Holdings.PlayerData.HoldingsDto))]
    [Union(7, typeof(OxDb.SharedGame.Trader.Caravans.PlayerData.CaravanData))]
    [Union(8, typeof(OxDb.SharedGame.Trader.Caravans.PlayerData.CaravanDto))]
    [Union(9, typeof(OxDb.SharedGame.Spells.PlayerData.CombatAbilityRank))]
    [Union(10, typeof(OxDb.SharedGame.Spells.PlayerData.CombatAbilityData))]
    [Union(11, typeof(OxDb.SharedGame.Spells.PlayerData.CombatAbilityDto))]
    [Union(12, typeof(OxDb.SharedGame.Spells.PlayerData.Spells.Spell))]
    [Union(13, typeof(OxDb.SharedGame.Spells.PlayerData.Spells.SpellData))]
    [Union(14, typeof(OxDb.SharedGame.Spells.PlayerData.Spells.SpellDto))]
    [Union(15, typeof(OxDb.SharedGame.Resets.PlayerData.ResetData))]
    [Union(16, typeof(OxDb.SharedGame.Resets.PlayerData.ResetDto))]
    [Union(17, typeof(OxDb.SharedGame.Quests.PlayerData.QuestStatus))]
    [Union(18, typeof(OxDb.SharedGame.Quests.PlayerData.QuestData))]
    [Union(19, typeof(OxDb.SharedGame.Quests.PlayerData.QuestDto))]
    [Union(20, typeof(OxDb.SharedGame.Purchasing.PlayerData.CompletedPurchaseData))]
    [Union(21, typeof(OxDb.SharedGame.Purchasing.PlayerData.CurrentPurchaseData))]
    [Union(22, typeof(OxDb.SharedGame.Purchasing.PlayerData.PlayerStoreOfferData))]
    [Union(23, typeof(OxDb.SharedGame.Purchasing.PlayerData.PlayerStoreOfferDto))]
    [Union(24, typeof(OxDb.SharedGame.Purchasing.PlayerData.PurchaseHistoryDto))]
    [Union(25, typeof(OxDb.SharedGame.Purchasing.PlayerData.PurchaseHistoryData))]
    [Union(26, typeof(OxDb.SharedGame.LoadSave.PlayerData.SaveSlotData))]
    [Union(27, typeof(OxDb.SharedGame.Inventory.PlayerData.InventoryData))]
    [Union(28, typeof(OxDb.SharedGame.Inventory.PlayerData.InventoryDto))]
    [Union(29, typeof(OxDb.SharedGame.Inventory.PlayerData.Item))]
    [Union(30, typeof(OxDb.SharedGame.Input.PlayerData.ActionInput))]
    [Union(31, typeof(OxDb.SharedGame.Input.PlayerData.ActionInputData))]
    [Union(32, typeof(OxDb.SharedGame.Input.PlayerData.ActionInputDto))]
    [Union(33, typeof(OxDb.SharedGame.Input.PlayerData.KeyComm))]
    [Union(34, typeof(OxDb.SharedGame.Input.PlayerData.KeyCommData))]
    [Union(35, typeof(OxDb.SharedGame.Input.PlayerData.KeyCommDto))]
    [Union(36, typeof(OxDb.SharedGame.Ftue.PlayerData.FtueData))]
    [Union(37, typeof(OxDb.SharedGame.Ftue.PlayerData.FtueDto))]
    [Union(38, typeof(OxDb.SharedGame.Factions.PlayerData.ReputationData))]
    [Union(39, typeof(OxDb.SharedGame.Factions.PlayerData.ReputationDto))]
    [Union(40, typeof(OxDb.SharedGame.Currencies.PlayerData.CharCurrencyData))]
    [Union(41, typeof(OxDb.SharedGame.Currencies.PlayerData.CurrencyDto))]
    [Union(42, typeof(OxDb.SharedGame.Crawler.Parties.PlayerData.PartyDto))]
    [Union(43, typeof(OxDb.SharedGame.Crafting.PlayerData.Recipes.RecipeStatus))]
    [Union(44, typeof(OxDb.SharedGame.Crafting.PlayerData.Recipes.RecipeData))]
    [Union(45, typeof(OxDb.SharedGame.Crafting.PlayerData.Recipes.RecipeDataDto))]
    [Union(46, typeof(OxDb.SharedGame.Crafting.PlayerData.Crafting.CraftingStatus))]
    [Union(47, typeof(OxDb.SharedGame.Crafting.PlayerData.Crafting.CraftingData))]
    [Union(48, typeof(OxDb.SharedGame.Crafting.PlayerData.Crafting.CraftingDto))]
    [Union(49, typeof(OxDb.SharedGame.Core.PlayerData.CoreData))]
    [Union(50, typeof(OxDb.SharedGame.Core.PlayerData.CoreDataDto))]
    [Union(51, typeof(OxDb.SharedGame.Core.PlayerData.GameAccount))]
    [Union(52, typeof(OxDb.SharedGame.Chests.PlayerData.ChestData))]
    [Union(53, typeof(OxDb.SharedGame.Chests.PlayerData.ChestStatus))]
    [Union(54, typeof(OxDb.SharedGame.Chests.PlayerData.ChestDto))]
    [Union(55, typeof(OxDb.SharedGame.Charms.PlayerData.PlayerCharm))]
    [Union(56, typeof(OxDb.SharedGame.Charms.PlayerData.PlayerCharmData))]
    [Union(57, typeof(OxDb.SharedGame.Charms.PlayerData.PlayerCharmDto))]
    [Union(58, typeof(OxDb.SharedGame.CharMail.PlayerData.CharLetter))]
    [Union(59, typeof(OxDb.SharedGame.CharMail.PlayerData.CharMailData))]
    [Union(60, typeof(OxDb.SharedGame.CharMail.PlayerData.CharMailDto))]
    [Union(61, typeof(OxDb.SharedGame.Characters.PlayerData.CoreCharacter))]
    [Union(62, typeof(OxDb.SharedGame.Attributes.PlayerData.AttributesData))]
    [Union(63, typeof(OxDb.SharedGame.Attributes.PlayerData.AttributesDataDto))]
    [Union(64, typeof(OxDb.SharedGame.Achievements.PlayerData.AchievementData))]
    [Union(65, typeof(OxDb.SharedGame.Achievements.PlayerData.AchievementDto))]
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


