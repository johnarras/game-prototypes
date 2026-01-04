using Genrpg.Shared.Characters.PlayerData;
using Genrpg.Shared.DataStores.Categories.PlayerData.Units;
using Genrpg.Shared.MapServer.Entities;
using Genrpg.Shared.Purchasing.PlayerData;
using Genrpg.Shared.Website.Interfaces;
using Newtonsoft.Json;
using System.Collections.Generic;

namespace Genrpg.Shared.Accounts.WebApi.Login
{
    public class GameAuthResponse : IWebResponse
    {
        public string GameUserId { get; set; }
        public string SessionId { get; set; }
        public List<CharacterStub> CharacterStubs { get; set; } = new List<CharacterStub>();
        public List<MapStub> MapStubs { get; set; } = new List<MapStub>();
        public string LoginToken { get; set; }
        [JsonProperty(TypeNameHandling = TypeNameHandling.Auto)]
        public List<IUnitData> UserData { get; set; } = new List<IUnitData>();
        public PlayerStoreOfferData OfferData { get; set; }
    }
}


