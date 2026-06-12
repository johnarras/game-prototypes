using OxDb.SharedCore.Website.Interfaces;
using System;

namespace OxDb.SharedGame.GameAuth.WebApi.Auth
{
    public class GameAuthRequest : IGameAuthRequest
    {
        public string AccountId { get; set; }
        public string GameUserId { get; set; }

        public string DisplayName { get; set; }
        public string AccountSessionId { get; set; }
        public string ClientVersion { get; set; }
        public DateTime ClientGameDataSaveTime { get; set; }
        public string ClientPlatformName { get; set; }
        public string ProductName { get; set; }
        public long DataBits { get; set; }
        public long ProductId { get; set; }
    }
}


