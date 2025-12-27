using Genrpg.Shared.Website.Interfaces;
using MessagePack;
using System;

namespace Genrpg.Shared.Accounts.WebApi.Login
{
    public class GameAuthRequest : IGameAuthRequest
    {
        public string AccountId { get; set; }
        public string SessionId { get; set; }
        public string ClientVersion { get; set; }
        public DateTime ClientGameDataSaveTime { get; set; }
        public string ClientPlatformName { get; set; }
    }
}


