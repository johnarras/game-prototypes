using OpenTelemetry;
using OpenTelemetry.Logs;

namespace OxDb.ServerCore.Logalytics.Entities
{
    public class LogalyticsEnricher : BaseProcessor<LogRecord>
    {

        List<KeyValuePair<string, object?>> _customAttributes = new List<KeyValuePair<string, object?>>();
        public LogalyticsEnricher(Dictionary<string, object> dict)
        {
            foreach (string key in dict.Keys)
            {
                _customAttributes.Add(new KeyValuePair<string, object?>(key, dict[key]));
            }
        }

        public override void OnEnd(LogRecord record)
        {
            List<KeyValuePair<string, object?>> currAttributes = new List<KeyValuePair<string, object?>>(_customAttributes);

            // Grab the live request activity running on this thread
            System.Diagnostics.Activity? currentActivity = System.Diagnostics.Activity.Current;
            if (currentActivity != null)
            {
                // Pull the request tags you set at the top of your handler
                foreach (KeyValuePair<string, string?> tag in currentActivity.Tags)
                {
                    currAttributes.Add(new KeyValuePair<string, object?>(tag.Key, tag.Value));
                }
            }

            if (record.Attributes != null)
            {
                currAttributes.AddRange(record.Attributes);
            }

            record.Attributes = currAttributes;
        }
    }
}
