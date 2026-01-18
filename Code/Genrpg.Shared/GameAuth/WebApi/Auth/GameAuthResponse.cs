using Genrpg.Shared.Characters.PlayerData;
using Genrpg.Shared.DataStores.Categories.PlayerData.Units;
using Genrpg.Shared.GameAuth.Interfaces;
using Genrpg.Shared.MapServer.Entities;
using Genrpg.Shared.Purchasing.PlayerData;
using Genrpg.Shared.Website.Interfaces;
using Newtonsoft.Json;
using System.Collections.Generic;

namespace Genrpg.Shared.GameAuth.WebApi.Auth
{
    public class GameAuthResponse : IWebResponse, IGameSessionState
    {
        public string GameUserId { get; set; }
        public string SessionToken { get; set; }
        public string RefreshToken { get; set; }
        public List<CharacterStub> CharacterStubs { get; set; } = new List<CharacterStub>();
        public List<MapStub> MapStubs { get; set; } = new List<MapStub>();
        public PlayerStoreOfferData OfferData { get; set; }
        [JsonProperty(TypeNameHandling = TypeNameHandling.Auto)]
        public List<IUnitData> UserData { get; set; } = new List<IUnitData>();
    }
}


