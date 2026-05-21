using OxDb.SharedCore.Logalytics.Interfaces;

namespace OxDb.ServerCore.AzureImpl.Analytics.Services
{
    public class AzureAnalyticsService : IAnalyticsService
    {
        public async Task Initialize(CancellationToken toke)
        {
            await Task.CompletedTask;
        }

        public void TrackEvent(string eventId, string eventType, string eventSubtype, Dictionary<string, string> extraData = null)
        {
        }
    }
}


