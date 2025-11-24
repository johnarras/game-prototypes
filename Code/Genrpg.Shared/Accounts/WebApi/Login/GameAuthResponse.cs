using Genrpg.Shared.Characters.PlayerData;
using Genrpg.Shared.Core.PlayerData;
using Genrpg.Shared.DataStores.Categories.PlayerData.Units;
using Genrpg.Shared.MapServer.Entities;
using Genrpg.Shared.Purchasing.PlayerData;
using Genrpg.Shared.Website.Interfaces;
using MessagePack;
using Newtonsoft.Json;
using System.Collections.Generic;

namespace Genrpg.Shared.Accounts.WebApi.Login
{
    [MessagePackObject]
    public class GameAuthResponse : IWebResponse
    {
        [Key(0)] public GameAccount GameAccount { get; set; }
        [Key(1)] public List<CharacterStub> CharacterStubs { get; set; } = new List<CharacterStub>();
        [Key(2)] public List<MapStub> MapStubs { get; set; } = new List<MapStub>();
        [JsonProperty(TypeNameHandling = TypeNameHandling.Auto)]
        [Key(3)] public string LoginToken { get; set; }
        [Key(4)] public List<IUnitData> UserData { get; set; } = new List<IUnitData>();
        [Key(5)] public PlayerStoreOfferData OfferData { get; set; }
    }
}
