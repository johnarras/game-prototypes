using Genrpg.Shared.Characters.PlayerData;
using Genrpg.Shared.DataStores.Categories.PlayerData.Units;
using Genrpg.Shared.MapServer.Entities;
using Genrpg.Shared.Networking.Constants;
using Genrpg.Shared.Purchasing.PlayerData;
using Genrpg.Shared.Website.Interfaces;
using Newtonsoft.Json;
using System.Collections.Generic;

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


