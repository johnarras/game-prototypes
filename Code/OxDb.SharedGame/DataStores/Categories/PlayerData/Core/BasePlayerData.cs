using MessagePack;
using Newtonsoft.Json;
using OxDb.SharedGame.DataStores.Categories.PlayerData.Units;

namespace OxDb.SharedGame.DataStores.Categories.PlayerData.Core
{
    public abstract class BasePlayerData : IUnitData
    {
        [IgnoreMember]
        [JsonProperty("id")]
        public abstract string Id { get; set; }

        public abstract IUnitData Unpack();

    }
}


