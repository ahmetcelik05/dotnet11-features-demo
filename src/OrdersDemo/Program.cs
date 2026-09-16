using OrdersDemo.Features.Orders;
using OrdersDemo.Features.Products;
using OrdersDemo.Features.Reconciliation;
using OrdersDemo.Infrastructure.Telemetry;
using OrdersDemo.Infrastructure.Tracing;

var builder = WebApplication.CreateBuilder(args);

builder.Services
    .AddOrders()                                     // async DataAnnotations validation + traced order placement
    .AddProducts()                                   // MemoryCache with built-in OpenTelemetry metrics
    .AddReconciliation()                             // LINQ FullJoin
    .AddDemoTracing(builder.Configuration)           // declarative Activity tracing rules
    .AddDemoOpenTelemetry();                         // console exporters for metrics and traces

var app = builder.Build();

app.MapOrdersEndpoints();
app.MapProductsEndpoints();
app.MapReconciliationEndpoints();

app.Run();

/// <summary>Exposed so integration tests can host the application with <c>WebApplicationFactory</c>.</summary>
public partial class Program;
