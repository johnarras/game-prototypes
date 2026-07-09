using System.Text.Json;
using System;
using OxDb.SharedCore.DataStores.Interfaces;
using OxDb.SharedCore.Interfaces;
using OxDb.SharedCore.Serialization.Attributes;
using System.Collections.Generic;
using System.Text.Json.Serialization;
using System.Text.Json.Serialization.Metadata;

namespace OxDb.SharedGame.DataStores.Interfaces
{
    [SystemTextJsonInterface]
    [JsonPolymorphic]
    [JsonDerivedType(typeof(OxDb.SharedGame.Trader.Shipments.PlayerData.ShipmentData),nameof(OxDb.SharedGame.Trader.Shipments.PlayerData.ShipmentData))]
    [JsonDerivedType(typeof(OxDb.SharedGame.Trader.Holdings.PlayerData.HoldingsData),nameof(OxDb.SharedGame.Trader.Holdings.PlayerData.HoldingsData))]
    [JsonDerivedType(typeof(OxDb.SharedGame.Trader.Caravans.PlayerData.CaravanData),nameof(OxDb.SharedGame.Trader.Caravans.PlayerData.CaravanData))]
    [JsonDerivedType(typeof(OxDb.SharedGame.Resets.PlayerData.ResetData),nameof(OxDb.SharedGame.Resets.PlayerData.ResetData))]
    [JsonDerivedType(typeof(OxDb.SharedGame.Purchasing.PlayerData.CurrentPurchaseData),nameof(OxDb.SharedGame.Purchasing.PlayerData.CurrentPurchaseData))]
    [JsonDerivedType(typeof(OxDb.SharedGame.Purchasing.PlayerData.PlayerStoreOfferData),nameof(OxDb.SharedGame.Purchasing.PlayerData.PlayerStoreOfferData))]
    [JsonDerivedType(typeof(OxDb.SharedGame.Purchasing.PlayerData.PurchaseHistoryData),nameof(OxDb.SharedGame.Purchasing.PlayerData.PurchaseHistoryData))]
    [JsonDerivedType(typeof(OxDb.SharedGame.Ftue.PlayerData.FtueData),nameof(OxDb.SharedGame.Ftue.PlayerData.FtueData))]
    [JsonDerivedType(typeof(OxDb.SharedGame.Core.PlayerData.CoreData),nameof(OxDb.SharedGame.Core.PlayerData.CoreData))]
    [JsonDerivedType(typeof(OxDb.SharedGame.Core.PlayerData.GameAccount),nameof(OxDb.SharedGame.Core.PlayerData.GameAccount))]
    [JsonDerivedType(typeof(OxDb.SharedGame.Attributes.PlayerData.AttributesData),nameof(OxDb.SharedGame.Attributes.PlayerData.AttributesData))]
    [JsonDerivedType(typeof(OxDb.SharedGame.Ads.PlayerData.AdsData),nameof(OxDb.SharedGame.Ads.PlayerData.AdsData))]
    public interface IPartitionedData : IStringId
    {
        string pk { get; }
        string _etag { get; set; }
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
