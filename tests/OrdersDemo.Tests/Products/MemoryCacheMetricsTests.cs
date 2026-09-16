using System.Diagnostics.Metrics;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using OrdersDemo.Features.Products;

namespace OrdersDemo.Tests.Products;

public sealed class MemoryCacheMetricsTests
{
    [Fact]
    public async Task Cache_publishes_hit_miss_entry_and_eviction_metrics()
    {
        using var listener = new CacheMeterListener();

        // AddProducts binds ProductCacheOptions from configuration; an empty configuration yields the defaults.
        var services = new ServiceCollection()
            .AddSingleton<IConfiguration>(new ConfigurationBuilder().Build())
            .AddMetrics()
            .AddProducts()
            .BuildServiceProvider();
        var catalog = services.GetRequiredService<IProductCatalog>();
        var cancellationToken = TestContext.Current.CancellationToken;

        await catalog.GetAsync("LAPTOP-15", cancellationToken);                                   // miss
        for (var i = 0; i < 9; i++)
        {
            await catalog.GetAsync("LAPTOP-15", cancellationToken);                               // hits
        }

        await catalog.GetAsync("MOUSE-01", cancellationToken);                                    // miss
        await catalog.GetAsync("DESK-STD", cancellationToken);                                    // miss
        await catalog.GetAsync("CHAIR-ERG", cancellationToken);                                   // miss, over SizeLimit

        // Observable instruments are only read when a collector asks.
        var measurements = listener.Collect();

        Assert.Equal(9, measurements["dotnet.cache.requests:hit"]);
        Assert.Equal(4, measurements["dotnet.cache.requests:miss"]);
        Assert.True(measurements["dotnet.cache.entries"] <= 3);
        Assert.Contains("dotnet.cache.evictions", measurements.Keys);
        Assert.Contains("dotnet.cache.estimated_size", measurements.Keys);
    }

    [Fact]
    public void No_instruments_are_published_without_TrackStatistics()
    {
        using var listener = new CacheMeterListener();

        using var cache = new MemoryCache(new MemoryCacheOptions { TrackStatistics = false });
        cache.Set("key", "value");
        cache.TryGetValue("key", out _);

        Assert.Empty(listener.PublishedInstruments);
    }

    /// <summary>Collects the cache meter's long measurements, keyed by instrument name and hit/miss tag.</summary>
    private sealed class CacheMeterListener : IDisposable
    {
        private readonly MeterListener _listener = new();
        private readonly Dictionary<string, long> _measurements = [];

        public List<string> PublishedInstruments { get; } = [];

        public CacheMeterListener()
        {
            _listener.InstrumentPublished = (instrument, listener) =>
            {
                if (instrument.Meter.Name != ProductsServiceCollectionExtensions.CacheMeterName)
                {
                    return;
                }

                PublishedInstruments.Add(instrument.Name);
                listener.EnableMeasurementEvents(instrument);
            };
            _listener.SetMeasurementEventCallback<long>(OnMeasurement);
            _listener.Start();
        }

        public Dictionary<string, long> Collect()
        {
            _listener.RecordObservableInstruments();
            return _measurements;
        }

        public void Dispose() => _listener.Dispose();

        private void OnMeasurement(Instrument instrument, long value, ReadOnlySpan<KeyValuePair<string, object?>> tags, object? state)
        {
            var key = instrument.Name;
            foreach (var tag in tags)
            {
                // RC1 uses "dotnet.cache.request.type"; the release/11.0 sources already rename it to "...request.result".
                if (tag.Key.EndsWith("request.type", StringComparison.Ordinal) || tag.Key.EndsWith("request.result", StringComparison.Ordinal))
                {
                    key = $"{key}:{tag.Value}";
                }
            }

            _measurements[key] = value;
        }
    }
}
