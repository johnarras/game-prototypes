using OpenTelemetry;
using System.Diagnostics;

public class AzureMetricExtractorSanitizer : BaseProcessor<Activity>
{
    private readonly IEnumerable<string> _dropPaths;

    public AzureMetricExtractorSanitizer(IEnumerable<string> dropPaths)
    {
        _dropPaths = dropPaths;
    }

    public override void OnEnd(Activity activity)
    {
        // Target incoming web server spans
        if (activity.Kind == ActivityKind.Server)
        {
            string? httpTarget = activity.GetTagItem("http.target") as string
                              ?? activity.GetTagItem("url.path") as string;

            if (httpTarget != null)
            {
                foreach (string dropPath in _dropPaths)
                {
                    if (httpTarget.Contains(dropPath, StringComparison.OrdinalIgnoreCase))
                    {
                        // Strip the recorded bit flag so the trace exporter ignores the item
                        activity.ActivityTraceFlags &= ~ActivityTraceFlags.Recorded;
                        return;
                    }
                }
            }
        }
    }
}