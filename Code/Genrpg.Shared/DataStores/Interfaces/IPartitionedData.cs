using Genrpg.Shared.Interfaces;
using Genrpg.Shared.Serialization.Attributes;
using System.Collections.Generic;
using System.Text.Json.Serialization;
using System.Text.Json.Serialization.Metadata;

namespace Genrpg.Shared.DataStores.Interfaces
{
    [SystemTextJsonInterface]
    [JsonPolymorphic]
    [JsonDerivedType(typeof(Genrpg.Shared.Trader.Stats.PlayerData.TraderStatData), nameof(Genrpg.Shared.Trader.Stats.PlayerData.TraderStatData))]
    [JsonDerivedType(typeof(Genrpg.Shared.Trader.Holdings.PlayerData.HoldingsData), nameof(Genrpg.Shared.Trader.Holdings.PlayerData.HoldingsData))]
    [JsonDerivedType(typeof(Genrpg.Shared.Trader.Caravans.PlayerData.CaravanData), nameof(Genrpg.Shared.Trader.Caravans.PlayerData.CaravanData))]
    [JsonDerivedType(typeof(Genrpg.Shared.Purchasing.PlayerData.CurrentPurchaseData), nameof(Genrpg.Shared.Purchasing.PlayerData.CurrentPurchaseData))]
    [JsonDerivedType(typeof(Genrpg.Shared.Purchasing.PlayerData.PlayerStoreOfferData), nameof(Genrpg.Shared.Purchasing.PlayerData.PlayerStoreOfferData))]
    [JsonDerivedType(typeof(Genrpg.Shared.Purchasing.PlayerData.PurchaseHistoryData), nameof(Genrpg.Shared.Purchasing.PlayerData.PurchaseHistoryData))]
    [JsonDerivedType(typeof(Genrpg.Shared.LoadSave.PlayerData.SaveSlotData), nameof(Genrpg.Shared.LoadSave.PlayerData.SaveSlotData))]
    [JsonDerivedType(typeof(Genrpg.Shared.Ftue.PlayerData.FtueData), nameof(Genrpg.Shared.Ftue.PlayerData.FtueData))]
    [JsonDerivedType(typeof(Genrpg.Shared.Crawler.Parties.PlayerData.PartyData), nameof(Genrpg.Shared.Crawler.Parties.PlayerData.PartyData))]
    [JsonDerivedType(typeof(Genrpg.Shared.Core.PlayerData.CoreData), nameof(Genrpg.Shared.Core.PlayerData.CoreData))]
    public interface IPartitionedData : IStringId, IVersionedData
    {
        string pk { get; }
    }
    #region SourceGen
    [JsonSourceGenerationOptions(GenerationMode = JsonSourceGenerationMode.Metadata)]
    [JsonSerializable(typeof(IPartitionedData))]
    [JsonSerializable(typeof(List<IPartitionedData>))]
    public partial class IPartitionedDataJsonGenerationContext : JsonSerializerContext
    {
    }
    #endregion SourceGen
}
