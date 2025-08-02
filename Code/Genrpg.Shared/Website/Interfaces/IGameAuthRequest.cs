using System;
using System.Collections.Generic;
using System.Text;

namespace Genrpg.Shared.Website.Interfaces
{
    public interface IGameAuthRequest : IWebRequest
    {
        string ProductAccountId { get; set; }
        string AccountId { get; set; }
        string SessionId { get; set; }
        string ClientVersion { get; set; }
        DateTime ClientGameDataSaveTime { get; set; }
    }
}
