using OpenTelemetry;
using OxDb.ServerCore.Logalytics.Utils;
using System.Diagnostics;

namespace OxDb.ServerCore.Logalytics.Entities
{
    public class GlobalRequestActivityEnricher : BaseProcessor<Activity>
    {
        private readonly List<KeyValuePair<string, object?>> _customAttributes = new List<KeyValuePair<string, object?>>();

        public GlobalRequestActivityEnricher(Dictionary<string, object> dict)
        {
            foreach (string key in dict.Keys)
            {
                _customAttributes.Add(new KeyValuePair<string, object?>(key, dict[key]));
            }
        }

        public override void OnEnd(Activity activity)
        {

            if (activity.Kind == ActivityKind.Internal)
            {
                activity.ActivityTraceFlags &= ~ActivityTraceFlags.Recorded;
                return;
            }

            // Only process server spans (incoming HTTP requests)
            if (activity.Kind == ActivityKind.Server)
            {
                for (int i = 0; i < _customAttributes.Count; i++)
                {
                    KeyValuePair<string, object?> attr = _customAttributes[i];

                    // SetTag populates the customDimensions block on the 'request' itemType
                    ActivityUtils.SafeAddTag(activity, attr.Key, attr.Value);
                }
            }
        }
    }
}
