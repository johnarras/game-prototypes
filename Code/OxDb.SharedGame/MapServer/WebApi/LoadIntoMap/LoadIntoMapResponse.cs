using Newtonsoft.Json;
using OxDb.SharedCore.Website.Responses.Interfaces;
using OxDb.SharedGame.Characters.PlayerData;
using OxDb.SharedGame.DataStores.Categories.PlayerData.Units;
using OxDb.SharedGame.MapServer.Entities;
using OxDb.SharedGame.Networking.Constants;
using OxDb.SharedGame.Purchasing.PlayerData;
using System.Collections.Generic;

namespace OxDb.SharedGame.MapServer.WebApi.LoadIntoMap
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


