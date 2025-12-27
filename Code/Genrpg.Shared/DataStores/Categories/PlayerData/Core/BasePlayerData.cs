using Genrpg.Shared.DataStores.Categories.PlayerData.Units;
using MessagePack;
using Newtonsoft.Json;

namespace Genrpg.Shared.DataStores.Categories.PlayerData.Core
{
    public abstract class BasePlayerData : IUnitData
    {
        [IgnoreMember]
        [JsonProperty("id")]
        public abstract string Id { get; set; }

        public abstract IUnitData Unpack();

    }
}


