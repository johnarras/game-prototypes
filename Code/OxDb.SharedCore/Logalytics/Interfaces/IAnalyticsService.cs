using OxDb.SharedCore.Interfaces;
using OxDb.SharedCore.Rewards.Entities;
using System.Collections.Generic;

namespace OxDb.SharedCore.Logalytics.Interfaces
{
    public interface IAnalyticsService : IInitializable
    {
        void TrackEvent(string eventName, Dictionary<string, string> properties = null, Dictionary<string,double> measurements = null);
        void TrackUIEvent(string eventName, string screenName, string buttonName = null, Dictionary<string, string> properties = null, Dictionary<string, double> measurements = null);
        void TrackEconomyEvent(string eventName, long entityTypeId, long entityId, long quantity, long rewardSourceId, Dictionary<string, string> properties = null, Dictionary<string, double> measurements = null);

        void TrackAccumulatedRewards(AccumulatedRewards rewards, Dictionary<string, string> properties = null, Dictionary<string, double> measurements = null);
    }
}


