using System.Text.Json;
using System.Text.Json.Serialization;
using Microsoft.AspNetCore.Mvc.Testing;
using OrdersDemo.Features.Reconciliation;

namespace OrdersDemo.Tests.Reconciliation;

public sealed class ReconciliationEndpointsTests(WebApplicationFactory<Program> factory) : IClassFixture<WebApplicationFactory<Program>>
{
    private static readonly JsonSerializerOptions JsonOptions = new(JsonSerializerDefaults.Web)
    {
        Converters = { new JsonStringEnumConverter() },
    };

    private readonly HttpClient _client = factory.CreateClient();

    [Fact]
    public async Task Reconciliation_reports_all_four_states()
    {
        var rows = await _client.GetFromJsonAsync<List<ReconciliationRow>>(
            "/reconciliation", JsonOptions, TestContext.Current.CancellationToken);

        Assert.NotNull(rows);
        Assert.Equal(
            [
                ReconciliationStatus.Ok,
                ReconciliationStatus.QuantityMismatch,
                ReconciliationStatus.MissingInErp,
                ReconciliationStatus.MissingInWarehouse,
            ],
            rows.Select(r => r.Status));
    }
}
