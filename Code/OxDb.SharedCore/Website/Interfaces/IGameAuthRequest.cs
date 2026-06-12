using OxDb.SharedCore.Website.Requests.Interfaces;
using System;

namespace OxDb.SharedCore.Website.Interfaces
{
    public interface IGameAuthRequest : IWebRequest
    {
        string AccountId { get; set; }
        string AccountSessionId { get; set; }
        string ClientVersion { get; set; }
        string ClientPlatformName { get; set; }
        DateTime ClientGameDataSaveTime { get; set; }
    }
}


