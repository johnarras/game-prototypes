using OxDb.SharedCore.Logalytics.Interfaces;
using OxDb.SharedCore.Rewards.Entities;
using System;
using System.Collections.Generic;
using System.Text;

namespace OxDb.ServerCore.AzureImpl.Logalytics
{
    public class AzureStubAnalyticsService : IAnalyticsService
    {
        public async Task Initialize(CancellationToken token)
        {
            await Task.CompletedTask;
        }

        public void TrackAccumulatedRewards(AccumulatedRewards rewards, Dictionary<string, string> properties = null, Dictionary<string, double> measurements = null)
        {
        }

        public void TrackEconomyEvent(string eventName, long entityTypeId, long entityId, long quantity, long rewardSourceId, Dictionary<string, string> properties = null, Dictionary<string, double> measurements = null)
        {
        }

        public void TrackEvent(string eventName, Dictionary<string, string> properties = null, Dictionary<string, double> measurements = null)
        {
        }

        public void TrackUIEvent(string eventName, string screenName, string buttonName = null, Dictionary<string, string> properties = null, Dictionary<string, double> measurements = null)
        {
        }
    }
}
