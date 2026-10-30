import { installSnippets } from '@core/domain/install-commands';
import { TAB_GROUPS } from '@core/domain/tab-groups';
import { DocPage } from '../domain/doc-page';
import { cards, code, cs, list, minimalApi, mvcController, note, p, procedure, section, step, steps, syncedTabs, table, tip } from './builders';

export const INTRODUCTION: DocPage = {
  slug: 'introduction',
  title: 'Introduction',
  icon: 'bi-house',
  summary: 'What ResponseResultHandler is, the problem it solves and how its pieces fit together.',
  sections: [
    section(
      'what',
      'What is it?',
      p('**ResponseResultHandler** is a Result-pattern library for .NET 8, 9 and 10. Instead of throwing exceptions or returning bare booleans, every operation returns an immutable object that tells you **whether it succeeded**, **what status** it ended with, a human-readable **title / detail**, optional **error lists**, optional **per-field errors** and, for queries, the **data**.'),
      p('At the edge of your app a single extension method turns that object into the right HTTP response: the payload with the correct status code on success, or an RFC 9457 **Problem Details** body on failure.'),
      cs(`public IOperationResult<ProductDto> GetById(int id)
{
    var product = _products.Find(id);

    return product is null
        ? Result.NotFound<ProductDto>($"Product {id} does not exist.")
        : Result.Success(ToDto(product));
}

// Controller or Minimal API, in one line:
app.MapGet("/api/products/{id:int}", (int id, ProductService s, HttpContext ctx) => s.GetById(id).ToResult(ctx));`, 'The whole idea in 10 lines'),
    ),
    section(
      'why',
      'Why not just throw?',
      table(
        ['Concern', 'Exceptions for flow control', 'Results'],
        ['Visibility', 'Hidden: the signature says nothing about failure', 'Explicit: `IOperationResult<T>` is in the signature'],
        ['Cost', 'Stack-trace capture on every “not found”', 'A small immutable object'],
        ['Composition', 'try/catch pyramids', '`Map`, `Bind`, `Ensure`, `Combine`, `Match`'],
        ['HTTP mapping', 'Custom middleware + exception types per status', 'Built-in, RFC-accurate, 62 statuses'],
        ['Validation', 'First error thrown, rest lost', 'Every error collected, keyed per field'],
      ),
      tip('Exceptions are still the right tool for **bugs and infrastructure outages** (a dropped DB connection). Results are for **expected** outcomes: not found, forbidden, invalid, conflict…', 'Rule of thumb'),
    ),
    section(
      'packages',
      'Two packages',
      table(
        ['Package', 'Depends on', 'Use it in'],
        ['`ResponseResultHandler`', 'nothing but the BCL', 'Domain, application and infrastructure layers, class libraries, workers, MAUI/Blazor clients'],
        ['`ResponseResultHandler.AspNetCore`', 'the core package + ASP.NET Core', 'Only the web/API project (controllers and Minimal API endpoints)'],
      ),
      note('Because the core package does not reference ASP.NET Core, your domain and application layers stay framework-free, which is exactly what Clean Architecture asks for.'),
    ),
    section(
      'map',
      'The mental model',
      cards(
        { icon: 'bi-bounding-box', title: 'Core contract', text: '`IOperationResult` / `IOperationResult<T>`: the shape every result has.', link: '/docs/core-contract' },
        { icon: 'bi-magic', title: 'Result facade', text: 'One named factory per HTTP status: `Result.NotFound(...)`, `Result.Created(...)`.', link: '/docs/result-facade' },
        { icon: 'bi-link-45deg', title: 'Composition', text: '`Map`, `Bind`, `Ensure`, `Match`, `Combine` and async twins.', link: '/docs/functional' },
        { icon: 'bi-globe2', title: 'ASP.NET Core adapter', text: '`ToActionResult()` / `ToResult()` → status codes + Problem Details.', link: '/docs/mvc-controllers' },
        { icon: 'bi-cpu', title: 'Generic infrastructure', text: 'Short-circuit MediatR behaviors without throwing.', link: '/docs/generic-short-circuiting' },
        { icon: 'bi-ui-checks-grid', title: 'Field errors', text: 'Per-property validation messages that map straight onto form inputs.', link: '/docs/field-errors' },
      ),
    ),
  ],
};

export const INSTALLATION: DocPage = {
  slug: 'installation',
  title: 'Installation',
  icon: 'bi-download',
  summary: 'Add the packages, pick your target framework and learn which namespace holds what.',
  sections: [
    section(
      'install',
      'Install from NuGet',
      syncedTabs(TAB_GROUPS.install, ...installSnippets({ channel: 'stable', includeAspNetCore: true, version: null })),
      tip('Only the web project needs `ResponseResultHandler.AspNetCore`. For beta builds or the exact latest version, use the **Install** button (download icon) in the header.'),
    ),
    section(
      'requirements',
      'Requirements',
      list(
        'Target frameworks: `net8.0`, `net9.0`, `net10.0` (net7.0 was dropped in 12.1 because it is end-of-life).',
        'C# 11 or later if you want the static-abstract `IResultFailureFactory<TSelf>` features (the default for net8+).',
        'Nullable reference types are recommended; the library is fully annotated.',
        'Dependencies: `ResponseResultHandler` uses only the BCL; `ResponseResultHandler.AspNetCore` adds only the ASP.NET Core shared framework. MediatR, FluentValidation and similar libraries are optional integrations, never required.',
      ),
    ),
    section(
      'namespaces',
      'Where things live',
      table(
        ['Namespace', 'Contains'],
        ['`ResultHandler.Facade`', '`Result`, the static factory you will use 90% of the time, plus `Result.Combine`'],
        ['`ResultHandler.Core.Abstractions`', '`IOperationResult`, `IOperationResult<T>`, `IHasFieldErrors`, `IResultFailureFactory<TSelf>`, `GetFieldErrors()`'],
        ['`ResultHandler.Core.Base`', '`OperationResult`, `OperationDataResult<T>` base classes'],
        ['`ResultHandler.Core.Enums`', '`ResultStatus`'],
        ['`ResultHandler.Implementations.Success` / `.Error`', '`SuccessResult`, `SuccessDataResult<T>`, `ErrorResult`, `ErrorDataResult<T>`'],
        ['`ResultHandler.Functional`', '`Map`, `Bind`, `Ensure`, `Match`, `OnSuccess`, `OnFailure`, async twins, `ResultFailureFactory`, `ToErrorDataResult<T>()`'],
        ['`ResultHandler.Mapping`', '`ToHttpStatusCode()` / `ToResultStatus()`'],
        ['`ResultHandler.AspNetCore.Extensions`', '`ToActionResult`, `ToResult`, `ToEnvelopedResult`, `ToProblemDetails`, `ToProblemResult`…'],
      ),
      tip('Put the common namespaces in a `GlobalUsings.cs` so every handler gets them for free.'),
      cs(`global using ResultHandler.Core.Abstractions;
global using ResultHandler.Core.Enums;
global using ResultHandler.Facade;
global using ResultHandler.Functional;`, 'GlobalUsings.cs'),
    ),
  ],
};

export const QUICK_START: DocPage = {
  slug: 'quick-start',
  title: 'Quick start',
  icon: 'bi-lightning-charge',
  badge: { label: 'Start here', tone: 'accent' },
  summary: 'From zero to a Problem-Details-returning endpoint in five minutes.',
  sections: [
    section(
      'build',
      'Build it in four steps',
      procedure(
        step('Install the packages', syncedTabs(TAB_GROUPS.install, ...installSnippets({ channel: 'stable', includeAspNetCore: true, version: null }))),
        step(
          'Return results from your service',
          cs(`public sealed class ProductService(AppDbContext db)
{
    public async Task<IOperationResult<ProductDto>> GetByIdAsync(int id, CancellationToken ct)
    {
        var product = await db.Products.FindAsync([id], ct);

        return product is null
            ? Result.NotFound<ProductDto>($"Product {id} does not exist.")
            : Result.Success(new ProductDto(product.Id, product.Name, product.Price));
    }

    public async Task<IOperationResult> DeleteAsync(int id, CancellationToken ct)
    {
        var deleted = await db.Products.Where(p => p.Id == id).ExecuteDeleteAsync(ct);
        return deleted == 0 ? Result.NotFound($"Product {id} does not exist.") : Result.NoContent();
    }
}`),
        ),
        step(
          'Convert at the edge',
          syncedTabs(
            TAB_GROUPS.apiStyle,
            minimalApi(`using ResultHandler.AspNetCore.Extensions;

var products = app.MapGroup("/api/products");

products.MapGet("/{id:int}", async (int id, ProductService service, HttpContext http, CancellationToken ct)
    => (await service.GetByIdAsync(id, ct)).ToResult(http));

products.MapDelete("/{id:int}", async (int id, ProductService service, CancellationToken ct)
    => (await service.DeleteAsync(id, ct)).ToResult());`),
            mvcController(`using ResultHandler.AspNetCore.Extensions;

[ApiController]
[Route("api/products")]
public sealed class ProductsController(ProductService service) : ControllerBase
{
    [HttpGet("{id:int}")]
    public async Task<IActionResult> GetById(int id, CancellationToken ct)
        => (await service.GetByIdAsync(id, ct)).ToActionResult(HttpContext);

    [HttpDelete("{id:int}")]
    public async Task<IActionResult> Delete(int id, CancellationToken ct)
        => (await service.DeleteAsync(id, ct)).ToActionResult();
}`),
          ),
        ),
        step(
          'Call it',
          table(
            ['Call', 'Response'],
            ['`GET /api/products/42` (exists)', '`200` + `{ "id": 42, "name": "…", "price": 129.9 }`'],
            ['`GET /api/products/999`', '`404` + `application/problem+json` with `type`, `title`, `status`, `detail`, `instance`'],
            ['`DELETE /api/products/42`', '`204`, no body'],
          ),
          p('Want to see every combination live? Open the [playground](/playground).'),
        ),
      ),
    ),
    section(
      'next',
      'Where to go next',
      steps(
        'Learn the [Result facade](/docs/result-facade): every status has a named factory.',
        'Replace `if (!result.IsSuccessful) return …` ladders with [functional composition](/docs/functional).',
        'Return all validation errors at once with [Result.Combine](/docs/combine) and [field errors](/docs/field-errors).',
        'Use the same service in a console app, Blazor or a worker: see [use it anywhere](/docs/use-anywhere).',
        'Wire it into MediatR with the [generic short-circuiting](/docs/generic-short-circuiting) guide.',
      ),
    ),
  ],
};

export const USE_ANYWHERE: DocPage = {
  slug: 'use-anywhere',
  title: 'Use it anywhere',
  icon: 'bi-grid-3x3-gap',
  summary: 'The same result-returning service in a console app, a Blazor component, a worker and an API, with no extra infrastructure.',
  sections: [
    section(
      'overview',
      'One service, any host',
      p('Nothing about results is web-specific. Write the service once with the core package, then consume it wherever you need it. Only a web API needs the second package.'),
      table(
        ['Host', 'Packages', 'How you consume a result'],
        ['Class library', '`ResponseResultHandler`', 'return it'],
        ['Console app', '`ResponseResultHandler`', '`Match` into text, pick an exit code'],
        ['Blazor (Server or interactive)', '`ResponseResultHandler`', 'render `Data` or `Detail`'],
        ['Worker / background service', '`ResponseResultHandler`', '`OnSuccess` / `OnFailure` into logs'],
        ['Web API', '+ `ResponseResultHandler.AspNetCore`', '`ToResult()` / `ToActionResult()`'],
      ),
    ),
    section(
      'service',
      'The shared service (class library)',
      code('bash', 'dotnet add package ResponseResultHandler', 'Terminal'),
      cs(`using ResultHandler.Core.Abstractions;
using ResultHandler.Core.Enums;
using ResultHandler.Facade;
using ResultHandler.Functional;

public sealed record Product(int Id, string Name, decimal Price, int Stock);

public sealed class ProductCatalog
{
    private readonly Dictionary<int, Product> _products = new()
    {
        [1] = new(1, "Air filter", 24.90m, 12),
        [2] = new(2, "Duct brush", 59.00m, 0),
    };

    public IOperationResult<Product> Find(int id)
        => _products.TryGetValue(id, out var product)
            ? Result.Success(product)
            : Result.NotFound<Product>($"Product {id} does not exist.");

    public IOperationResult<Product> Reserve(int id, int quantity)
        => Find(id)
            .Ensure(_ => quantity > 0, "Quantity must be at least 1.")
            .Ensure(p => p.Stock >= quantity, "Out of stock", "Not enough units left for this reservation.", ResultStatus.Conflict)
            .Map(p => p with { Stock = p.Stock - quantity })
            .OnSuccess(p => _products[p.Id] = p);
}`, 'ProductCatalog.cs'),
      note('No base classes, no DI requirements, no web types. The examples below all use this exact class.'),
    ),
    section(
      'console',
      'Console app',
      cs(`using ResultHandler.Functional;

var catalog = new ProductCatalog();

var message = catalog.Reserve(id: 1, quantity: 2).Match(
    onSuccess: p => $"Reserved 2 x {p.Name}. {p.Stock} left.",
    onFailure: f => $"{f.Title}: {f.Detail}");

Console.WriteLine(message);

var missing = catalog.Find(99);
if (!missing.IsSuccessful)
{
    Console.Error.WriteLine(missing.Detail);
    return 1;
}

Console.WriteLine(missing.Data.Name); // no "!" needed: IsSuccessful proves Data is not null
return 0;`, 'Program.cs'),
      code('plaintext', `Reserved 2 x Air filter. 10 left.
Product 99 does not exist.`, 'Output (exit code 1)'),
    ),
    section(
      'blazor',
      'Blazor (Server or interactive)',
      p('Register the service (`builder.Services.AddSingleton<ProductCatalog>();`) and inject it into a component. The component renders the data on success and the message on failure, with no HTTP round-trip:'),
      code('xml', `@page "/products/{Id:int}"
@using ResultHandler.Core.Abstractions
@inject ProductCatalog Catalog

@if (_result is null)
{
    <p>Loading...</p>
}
else if (_result.IsSuccessful)
{
    <h1>@_result.Data.Name</h1>
    <p>@_result.Data.Price.ToString("C") (@_result.Data.Stock in stock)</p>
    <button class="btn btn-primary" @onclick="Reserve">Reserve one</button>
}
else
{
    <div class="alert alert-warning">@(_result.Detail ?? _result.Title)</div>
}

@code {
    [Parameter] public int Id { get; set; }

    private IOperationResult<Product>? _result;

    protected override void OnParametersSet() => _result = Catalog.Find(Id);

    private void Reserve() => _result = Catalog.Reserve(Id, 1);
}`, 'ProductDetails.razor'),
      tip('For forms, `_result.GetFieldErrors()` gives the messages per property name, so each input can show its own error. See [field errors](/docs/field-errors).'),
      note('A Blazor WebAssembly app that calls a remote API instead deserializes the response back into a result; see [consuming from clients](/docs/client-consumption#dotnet).'),
    ),
    section(
      'worker',
      'Worker / background service',
      cs(`using ResultHandler.Core.Enums;
using ResultHandler.Functional;

public sealed class RestockWorker(ProductCatalog catalog, ILogger<RestockWorker> logger) : BackgroundService
{
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        using var timer = new PeriodicTimer(TimeSpan.FromMinutes(5));

        while (await timer.WaitForNextTickAsync(stoppingToken))
        {
            catalog.Find(2)
                .Ensure(p => p.Stock > 0, "Out of stock", "Needs restocking.", ResultStatus.Conflict)
                .OnSuccess(p => logger.LogInformation("{Product} has {Stock} in stock", p.Name, p.Stock))
                .OnFailure(f => logger.LogWarning("Stock check failed: {Title} {Detail}", f.Title, f.Detail));
        }
    }
}`, 'RestockWorker.cs'),
      p('An expected failure is a logged outcome, not an exception, so one bad item never stops the loop.'),
    ),
    section(
      'api',
      'The same service in an API',
      p('Add `ResponseResultHandler.AspNetCore` and convert at the edge. The service does not change:'),
      cs(`app.MapGet("/api/products/{id:int}", (int id, ProductCatalog catalog, HttpContext http)
    => catalog.Find(id).ToResult(http)); // 200 + product, or 404 Problem Details`),
      p('The full walkthrough is in the [quick start](/docs/quick-start).'),
    ),
  ],
};
