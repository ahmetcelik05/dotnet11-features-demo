# .NET 11 features demo

A small minimal API that exercises four developer-facing .NET 11 features together:

| Endpoint / component | Feature |
|---|---|
| `POST /orders` | Asynchronous DataAnnotations validation (`AsyncValidationAttribute`, `IAsyncValidatableObject`) via `AddValidation()` |
| `GET /products/{sku}` | `MemoryCache` with built-in OpenTelemetry metrics (`TrackStatistics = true`) |
| `GET /reconciliation` | LINQ `FullJoin` (tuple-returning overload) |
| `OrderService` + the `Tracing` section in `appsettings.json` | Declarative `Activity` tracing rules (`AddTracing`, `AddListener`, `AddConfiguration`) |

Built and tested against **.NET 11 RC1** (`11.0.100-rc.1.26425.128`). RC1 has a go-live license, but API details can still change before GA (see the notes at the end).

## Project layout

```
src/OrdersDemo
  Program.cs                      composition root only
  Features/
    Orders/                       request model, async validation attribute, order service, endpoints
    Products/                     product store, cached catalog, cache options, endpoints
    Reconciliation/               stock sources, FullJoin reconciliation, endpoints
  Infrastructure/
    Tracing/                      tracing rules registration, console listener
    Telemetry/                    OpenTelemetry console exporters
tests/OrdersDemo.Tests            xUnit v3 on Microsoft.Testing.Platform, one folder per feature
Directory.Build.props             shared build settings (nullable, analyzers, warnings as errors)
.editorconfig                     code style enforced at build time
```

Each feature registers its services through an `Add...` extension and its routes through a `Map...Endpoints` extension, so `Program.cs` stays a thin composition root. Services sit behind interfaces; the in-memory implementations stand in for a database or a remote system.

## Prerequisites

The repository pins the SDK in `global.json`. To install RC1 without touching an existing .NET installation:

```powershell
Invoke-WebRequest https://dot.net/v1/dotnet-install.ps1 -OutFile dotnet-install.ps1
.\dotnet-install.ps1 -Version 11.0.100-rc.1.26425.128 -InstallDir D:\dotnet11 -NoPath
D:\dotnet11\dotnet.exe --version   # 11.0.100-rc.1.26425.128
```

(Or use the regular installer from https://dotnet.microsoft.com/download/dotnet/11.0.)

## Run

```bash
dotnet run --project src/OrdersDemo
```

```bash
# async validation -> 400 with ValidationProblemDetails
curl -X POST http://localhost:5028/orders -H "Content-Type: application/json" \
     -d '{"customerEmail":"nobody@example.com","lines":[{"sku":"MOUSE-01","quantity":2}]}'

# FullJoin reconciliation
curl http://localhost:5028/reconciliation

# cache metrics (printed by the OpenTelemetry console exporter every 10 s)
curl http://localhost:5028/products/LAPTOP-15

# tracing rules: HealthCheck is disabled in appsettings.json, PlaceOrder is enabled
curl http://localhost:5028/health
```

Flip `"HealthCheck": false` to `true` in `src/OrdersDemo/appsettings.json` while the app is running and call `/health` again: the `[trace]` line appears without a restart.

## Test

```bash
dotnet test
```

28 tests cover join semantics, the async validation pipeline, cache instruments and tracing rules, including runtime configuration reload.

## Observed on RC1

- `MemoryCache` meter `Microsoft.Extensions.Caching.Memory.MemoryCache` publishes `dotnet.cache.requests` (tag `dotnet.cache.request.type` = `hit`/`miss`), `dotnet.cache.evictions`, `dotnet.cache.entries`, `dotnet.cache.estimated_size` (unit `By`). The `release/11.0` sources already rename the tag to `dotnet.cache.request.result` and the size unit to `1`, so expect a change at GA.
- Tracing rules only govern listeners registered through `AddTracing`. OpenTelemetry's own listener is unaffected. Listeners become active when the host starts (or when `ActivitySourceFactory` is first resolved), and a listener without a `Sample` callback never records anything.
- The selector-less `GroupJoin` overload returns `IGrouping<TOuter, TInner>` (outer element as `Key`); only the selector-less `Join` returns tuples.
- Calling the synchronous `Validator.TryValidateObject` on a model with an async-only attribute throws.
