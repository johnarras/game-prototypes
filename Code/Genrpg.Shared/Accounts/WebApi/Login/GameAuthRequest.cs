using Genrpg.Shared.Website.Interfaces;
using MessagePack;
using System;

namespace Genrpg.Shared.Accounts.WebApi.Login
{
    [MessagePackObject]
    public class GameAuthRequest : IGameAuthRequest
    {
        [Key(0)] public string AccountId { get; set; }
        [Key(1)] public string ProductAccountId { get; set; }
        [Key(2)] public string SessionId { get; set; }
        [Key(3)] public string ClientVersion { get; set; }
        [Key(4)] public DateTime ClientGameDataSaveTime { get; set; }
    }
}
