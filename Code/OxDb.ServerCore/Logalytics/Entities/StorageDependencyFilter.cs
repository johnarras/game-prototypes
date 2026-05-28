using System;
using System.Diagnostics;
using OpenTelemetry;

namespace OxDb.ServerCore.Logalytics.Entities
{

    public class StorageDependencyFilterProcessor : BaseProcessor<Activity>
    {
        public override void OnEnd(Activity activity)
        {
            // Outbound HTTP and Azure SDK tracking use the "client" or "internal" kinds
            if (activity.Kind == ActivityKind.Client || activity.Kind == ActivityKind.Internal)
            {
                // OpenTelemetry populates the destination URI under standard attribute tags
                foreach (KeyValuePair<string, object> tag in activity.TagObjects)
                {
                    if (tag.Key == "url.full" || tag.Key == "http.url" || tag.Key == "peer.service")
                    {
                        string urlValue = tag.Value?.ToString();
                        if (urlValue != null)
                        {
                            // Setting ActivityTraceFlags to None tells the exporter to drop it
                            activity.ActivityTraceFlags &= ~ActivityTraceFlags.Recorded;
                            return;
                        }
                    }
                }
            }
        }
    }
}