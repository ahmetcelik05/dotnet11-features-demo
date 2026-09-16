namespace OrdersDemo.Tests.Tracing;

/// <summary>A throw-away JSON file holding a <c>Tracing</c> section, used to exercise configuration reload.</summary>
internal sealed class TemporaryTracingConfiguration : IDisposable
{
    public TemporaryTracingConfiguration(bool healthCheckEnabled)
    {
        Path = System.IO.Path.Combine(System.IO.Path.GetTempPath(), $"tracing-{Guid.NewGuid():N}.json");
        Write(healthCheckEnabled);
    }

    public string Path { get; }

    public void Write(bool healthCheckEnabled)
    {
        var json = $$"""
            {
              "Tracing": {
                "EnabledTracing": {
                  "OrdersDemo.Orders": {
                    "Default": true,
                    "HealthCheck": {{(healthCheckEnabled ? "true" : "false")}}
                  }
                }
              }
            }
            """;
        File.WriteAllText(Path, json);
    }

    public void Dispose() => File.Delete(Path);
}
