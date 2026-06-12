using Newtonsoft.Json;
using OxDb.SharedCore.Website.Responses.Interfaces;
using OxDb.SharedGame.Characters.PlayerData;
using OxDb.SharedGame.DataStores.Categories.PlayerData.Units;
using OxDb.SharedGame.GameAuth.Interfaces;
using OxDb.SharedGame.MapServer.Entities;
using OxDb.SharedGame.Purchasing.PlayerData;
using System.Collections.Generic;

namespace OxDb.SharedGame.GameAuth.WebApi.Auth
{
    public class GameAuthResponse : IWebResponse, IGameSessionState
    {
        public string GameUserId { get; set; }
        public string FullToken { get; set; }
        public string GameSessionId { get; set; }
        public string RefreshToken { get; set; }
        public string ServerName { get; set; }
        public string ServerVersion { get; set; }
        public string ServerEnv { get; set; }
        public bool DidCreateAccount { get; set; }
        public List<CharacterStub> CharacterStubs { get; set; } = new List<CharacterStub>();
        public List<MapStub> MapStubs { get; set; } = new List<MapStub>();
        public PlayerStoreOfferData OfferData { get; set; }
        [JsonProperty(TypeNameHandling = TypeNameHandling.Auto)]
        public List<IUnitData> UserData { get; set; } = new List<IUnitData>();
    }
}


