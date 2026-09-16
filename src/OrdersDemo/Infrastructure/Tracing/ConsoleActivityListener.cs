using System.Diagnostics;
using System.Globalization;
using Microsoft.Extensions.Diagnostics.Tracing;

namespace OrdersDemo.Infrastructure.Tracing;

/// <summary>
/// A named listener registered through <c>AddTracing</c>. Tracing rules decide which activities reach it.
/// </summary>
internal static class ConsoleActivityListener
{
    /// <summary>Rules can target this listener by name.</summary>
    public const string Name = "console";

    public static void Configure(ActivityListenerBuilder listener)
    {
        // The rules API makes no sampling decision for you: without Sample, StartActivity returns null.
        listener.Sample = static (ref _) => ActivitySamplingResult.AllDataAndRecorded;
        listener.ActivityStopped = Print;
    }

    private static void Print(Activity activity)
    {
        // Activity.Tags only yields string-valued tags; TagObjects includes numbers such as order.lines.
        var tags = string.Join(' ', activity.TagObjects.Select(tag => $"{tag.Key}={tag.Value}"));
        var duration = activity.Duration.TotalMilliseconds.ToString("F1", CultureInfo.InvariantCulture);

        Console.WriteLine($"[trace] {activity.Source.Name}/{activity.OperationName} {duration} ms {tags}");
    }
}
