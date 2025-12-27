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
    public class LoadIntoMapResponse : IWebResponse
    {
        public Map Map { get; set; }
        public CoreCharacter Char { get; set; }
        public bool Generating { get; set; }
        public string Host { get; set; }
        public long Port { get; set; }
        [JsonProperty(TypeNameHandling = TypeNameHandling.Auto)]
        public List<IUnitData> CharData { get; set; } = new List<IUnitData>();

        public EMapApiSerializers Serializer { get; set; }

        public string WorldDataEnv { get; set; }

        public PlayerStoreOfferData Stores { get; set; }

        public LoadIntoMapResponse()
        {
        }
    }
}


