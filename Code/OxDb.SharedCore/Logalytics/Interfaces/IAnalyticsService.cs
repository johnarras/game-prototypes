using OxDb.SharedCore.Interfaces;
using System.Collections.Generic;

namespace OxDb.SharedCore.Logalytics.Interfaces
{
    public interface IAnalyticsService : IInitializable
    {
        void TrackEvent(string eventType, string eventId, string eventSubtype = null, Dictionary<string, string> extraData = null);
    }
}


