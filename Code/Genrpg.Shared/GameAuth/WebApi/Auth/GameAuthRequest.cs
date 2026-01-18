using Genrpg.Shared.Website.Interfaces;
using System;

namespace Genrpg.Shared.GameAuth.WebApi.Auth
{
    public class GameAuthRequest : IGameAuthRequest
    {
        public string AccountId { get; set; }
        public string GameUserId { get; set; }
        public string SessionToken { get; set; }
        public string ClientVersion { get; set; }
        public DateTime ClientGameDataSaveTime { get; set; }
        public string ClientPlatformName { get; set; }
        public string GameName { get; set; }
    }
}


