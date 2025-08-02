using MessagePack;

using System.Collections.Generic;
using Newtonsoft.Json;
using Genrpg.Shared.MapServer.Entities;
using Genrpg.Shared.Networking.Constants;
using Genrpg.Shared.DataStores.Categories.GameSettings;
using Genrpg.Shared.GameSettings.Interfaces;
using Genrpg.Shared.GameSettings.PlayerData;
using Genrpg.Shared.Characters.PlayerData;
using Genrpg.Shared.Purchasing.PlayerData;
using Genrpg.Shared.Website.Interfaces;
using Genrpg.Shared.DataStores.Categories.PlayerData.Units;

namespace Genrpg.Shared.MapServer.WebApi.LoadIntoMap
{
    [MessagePackObject]
    public class LoadIntoMapResponse : IWebResponse
    {
        [Key(0)] public Map Map { get; set; }
        [Key(1)] public CoreCharacter Char { get; set; }
        [Key(2)] public bool Generating { get; set; }
        [Key(3)] public string Host { get; set; }
        [Key(4)] public long Port { get; set; }
        [JsonProperty(TypeNameHandling = TypeNameHandling.Auto)]
        [Key(5)] public List<IUnitData> CharData { get; set; } = new List<IUnitData>();

        [Key(6)] public EMapApiSerializers Serializer { get; set; }

        [Key(7)] public string WorldDataEnv { get; set; }

        [Key(8)] public PlayerStoreOfferData Stores { get; set; }

        public LoadIntoMapResponse()
        {
        }
    }
}
