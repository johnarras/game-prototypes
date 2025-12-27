using Genrpg.Shared.Editors.Interfaces;
using Genrpg.Shared.Interfaces;
using Genrpg.Shared.Serialization.Attributes;
using MessagePack;

namespace Genrpg.Shared.DataStores.Categories.PlayerData.Units
{
    [MessagePackInterface]

    [Union(0 ,typeof(Genrpg.Shared.UserMail.PlayerData.UserLetter))]
    [Union(1 ,typeof(Genrpg.Shared.UserMail.PlayerData.UserMailData))]
    [Union(2 ,typeof(Genrpg.Shared.UserMail.PlayerData.UserMailDto))]
    [Union(3 ,typeof(Genrpg.Shared.Trader.Stats.PlayerData.TraderStatData))]
    [Union(4 ,typeof(Genrpg.Shared.Trader.Stats.PlayerData.TraderStatDto))]
    [Union(5 ,typeof(Genrpg.Shared.Trader.Holdings.PlayerData.HoldingsData))]
    [Union(6 ,typeof(Genrpg.Shared.Trader.Caravans.PlayerData.CaravanData))]
    [Union(7 ,typeof(Genrpg.Shared.Spells.PlayerData.CombatAbilityRank))]
    [Union(8 ,typeof(Genrpg.Shared.Spells.PlayerData.CombatAbilityData))]
    [Union(9 ,typeof(Genrpg.Shared.Spells.PlayerData.CombatAbilityDto))]
    [Union(10 ,typeof(Genrpg.Shared.Spells.PlayerData.Spells.Spell))]
    [Union(11 ,typeof(Genrpg.Shared.Spells.PlayerData.Spells.SpellData))]
    [Union(12 ,typeof(Genrpg.Shared.Spells.PlayerData.Spells.SpellDto))]
    [Union(13 ,typeof(Genrpg.Shared.Quests.PlayerData.QuestStatus))]
    [Union(14 ,typeof(Genrpg.Shared.Quests.PlayerData.QuestData))]
    [Union(15 ,typeof(Genrpg.Shared.Quests.PlayerData.QuestDto))]
    [Union(16 ,typeof(Genrpg.Shared.Purchasing.PlayerData.CompletedPurchaseData))]
    [Union(17 ,typeof(Genrpg.Shared.Purchasing.PlayerData.CurrentPurchaseData))]
    [Union(18 ,typeof(Genrpg.Shared.Purchasing.PlayerData.PlayerStoreOfferData))]
    [Union(19 ,typeof(Genrpg.Shared.Purchasing.PlayerData.PlayerStoreOfferDto))]
    [Union(20 ,typeof(Genrpg.Shared.Purchasing.PlayerData.PurchaseHistoryDto))]
    [Union(21 ,typeof(Genrpg.Shared.Purchasing.PlayerData.PurchaseHistoryData))]
    [Union(22 ,typeof(Genrpg.Shared.LoadSave.PlayerData.SaveSlotData))]
    [Union(23 ,typeof(Genrpg.Shared.Inventory.PlayerData.InventoryData))]
    [Union(24 ,typeof(Genrpg.Shared.Inventory.PlayerData.InventoryDto))]
    [Union(25 ,typeof(Genrpg.Shared.Inventory.PlayerData.Item))]
    [Union(26 ,typeof(Genrpg.Shared.Input.PlayerData.ActionInput))]
    [Union(27 ,typeof(Genrpg.Shared.Input.PlayerData.ActionInputData))]
    [Union(28 ,typeof(Genrpg.Shared.Input.PlayerData.ActionInputDto))]
    [Union(29 ,typeof(Genrpg.Shared.Input.PlayerData.KeyComm))]
    [Union(30 ,typeof(Genrpg.Shared.Input.PlayerData.KeyCommData))]
    [Union(31 ,typeof(Genrpg.Shared.Input.PlayerData.KeyCommDto))]
    [Union(32 ,typeof(Genrpg.Shared.Ftue.PlayerData.FtueData))]
    [Union(33 ,typeof(Genrpg.Shared.Ftue.PlayerData.FtueDto))]
    [Union(34 ,typeof(Genrpg.Shared.Factions.PlayerData.ReputationStatus))]
    [Union(35 ,typeof(Genrpg.Shared.Factions.PlayerData.ReputationData))]
    [Union(36 ,typeof(Genrpg.Shared.Factions.PlayerData.ReputationDto))]
    [Union(37 ,typeof(Genrpg.Shared.Currencies.PlayerData.CurrencyData))]
    [Union(38 ,typeof(Genrpg.Shared.Currencies.PlayerData.CurrencyStatus))]
    [Union(39 ,typeof(Genrpg.Shared.Currencies.PlayerData.CurrencyDto))]
    [Union(40 ,typeof(Genrpg.Shared.Crawler.Parties.PlayerData.PartyDto))]
    [Union(41 ,typeof(Genrpg.Shared.Crafting.PlayerData.Recipes.RecipeStatus))]
    [Union(42 ,typeof(Genrpg.Shared.Crafting.PlayerData.Recipes.RecipeData))]
    [Union(43 ,typeof(Genrpg.Shared.Crafting.PlayerData.Recipes.RecipeDataDto))]
    [Union(44 ,typeof(Genrpg.Shared.Crafting.PlayerData.Crafting.CraftingStatus))]
    [Union(45 ,typeof(Genrpg.Shared.Crafting.PlayerData.Crafting.CraftingData))]
    [Union(46 ,typeof(Genrpg.Shared.Crafting.PlayerData.Crafting.CraftingDto))]
    [Union(47 ,typeof(Genrpg.Shared.Core.PlayerData.CoreUserData))]
    [Union(48 ,typeof(Genrpg.Shared.Core.PlayerData.CoreUserDto))]
    [Union(49 ,typeof(Genrpg.Shared.Core.PlayerData.GameAccount))]
    [Union(50 ,typeof(Genrpg.Shared.Chests.PlayerData.ChestData))]
    [Union(51 ,typeof(Genrpg.Shared.Chests.PlayerData.ChestStatus))]
    [Union(52 ,typeof(Genrpg.Shared.Chests.PlayerData.ChestDto))]
    [Union(53 ,typeof(Genrpg.Shared.Charms.PlayerData.PlayerCharm))]
    [Union(54 ,typeof(Genrpg.Shared.Charms.PlayerData.PlayerCharmData))]
    [Union(55 ,typeof(Genrpg.Shared.Charms.PlayerData.PlayerCharmDto))]
    [Union(56 ,typeof(Genrpg.Shared.CharMail.PlayerData.CharLetter))]
    [Union(57 ,typeof(Genrpg.Shared.CharMail.PlayerData.CharMailData))]
    [Union(58 ,typeof(Genrpg.Shared.CharMail.PlayerData.CharMailDto))]
    [Union(59 ,typeof(Genrpg.Shared.Characters.PlayerData.CoreCharacter))]
    [Union(60 ,typeof(Genrpg.Shared.Achievements.PlayerData.AchievementData))]
    [Union(61 ,typeof(Genrpg.Shared.Achievements.PlayerData.AchievementStatus))]
    [Union(62 ,typeof(Genrpg.Shared.Achievements.PlayerData.AchievementDto))]
    public interface IUnitData : IStringId, IEditorMetaDataTarget
    {
        IUnitData Unpack();
    }
}


