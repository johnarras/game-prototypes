using Genrpg.Shared.Interfaces;
using System.Collections.Generic;

namespace Genrpg.Shared.Analytics.Services
{
    public interface IAnalyticsService : IInitializable
    {
        void Send(string eventId, string eventType, string eventSubtype = null, Dictionary<string, string> extraData = null);
    }
}


