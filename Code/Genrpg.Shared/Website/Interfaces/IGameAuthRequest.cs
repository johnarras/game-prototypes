using System;

namespace Genrpg.Shared.Website.Interfaces
{
    public interface IGameAuthRequest : IWebRequest
    {
        string AccountId { get; set; }
        string SessionId { get; set; }
        string ClientVersion { get; set; }
        string ClientPlatformName { get; set; }
        DateTime ClientGameDataSaveTime { get; set; }
    }
}
