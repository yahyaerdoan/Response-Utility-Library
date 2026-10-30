import { CodeSnippet } from '@core/domain/content.model';
import { installSnippets } from '@core/domain/install-commands';

export interface Feature {
  readonly icon: string;
  readonly title: string;
  readonly text: string;
  readonly link: string;
}

export interface FlowStep {
  readonly icon: string;
  readonly label: string;
  readonly caption: string;
  readonly snippet: CodeSnippet;
}

export interface Stat {
  readonly value: string;
  readonly label: string;
}

export const INSTALL_SNIPPETS: readonly CodeSnippet[] = installSnippets({ channel: 'stable', includeAspNetCore: true, version: null });

export const STATS: readonly Stat[] = [
  { value: '62', label: 'HTTP statuses with named factories' },
  { value: 'RFC 9457', label: 'Problem Details out of the box' },
  { value: 'net8 · 9 · 10', label: 'Target frameworks' },
  { value: 'MIT', label: 'Free for any use' },
];

export const FEATURES: readonly Feature[] = [
  { icon: 'bi-magic', title: 'A factory for every status', text: '`Result.NotFound`, `Result.Created`, `Result.TooManyRequests`… with sensible default titles and messages.', link: '/docs/result-facade' },
  { icon: 'bi-globe2', title: 'One-line HTTP mapping', text: '`ToResult()` / `ToActionResult()` return the real status code and RFC-accurate Problem Details.', link: '/docs/mvc-controllers' },
  { icon: 'bi-link-45deg', title: 'Railway composition', text: '`Map`, `Bind`, `Ensure`, `Match` and async twins replace `if (!ok) return` ladders.', link: '/docs/functional' },
  { icon: 'bi-collection', title: 'Report every error at once', text: '`Result.Combine` aggregates independent checks and returns tuples of their data.', link: '/docs/combine' },
  { icon: 'bi-ui-checks-grid', title: 'Per-field validation', text: '`FieldErrors` keyed by property name flow all the way to a `fieldErrors` Problem Details extension.', link: '/docs/field-errors' },
  { icon: 'bi-cpu', title: 'MediatR without exceptions', text: 'Static-abstract failure factories let generic pipeline behaviors return `TResponse.Failure(...)`.', link: '/docs/generic-short-circuiting' },
  { icon: 'bi-braces', title: 'Stable JSON & AOT', text: 'Fixed property names, numeric status codes, round-trippable, source-generator friendly.', link: '/docs/serialization' },
  { icon: 'bi-shield-check', title: 'Null-safe by construction', text: '`Data` is non-null after a success check, enforced by the compiler.', link: '/docs/core-contract' },
  { icon: 'bi-feather', title: 'Framework-free core', text: 'The core package depends only on the BCL. Keep ASP.NET Core out of your domain.', link: '/docs/introduction' },
];

export const FLOW: readonly FlowStep[] = [
  {
    icon: 'bi-database',
    label: 'Service / handler',
    caption: 'returns IOperationResult<T>',
    snippet: {
      label: 'ProductService.cs',
      language: 'csharp',
      code: `public IOperationResult<ProductDto> GetById(int id)
{
    var product = _products.Find(id);

    return product is null
        ? Result.NotFound<ProductDto>($"Product {id} does not exist.")
        : Result.Success(product.ToDto());
}`,
    },
  },
  {
    icon: 'bi-link-45deg',
    label: 'Compose',
    caption: 'Ensure · Map · Bind · Combine',
    snippet: {
      label: 'OrderService.cs',
      language: 'csharp',
      code: `public IOperationResult<OrderDto> Place(PlaceOrder cmd)
    => _products.GetById(cmd.ProductId)
        .Ensure(p => p.Stock >= cmd.Quantity, "Not enough stock.")
        .Bind(p => _orders.Create(p, cmd.Quantity))
        .Map(order => order.ToDto());`,
    },
  },
  {
    icon: 'bi-plug',
    label: 'Edge adapter',
    caption: 'ToResult() / ToActionResult()',
    snippet: {
      label: 'Program.cs',
      language: 'csharp',
      code: `app.MapPost("/api/orders", (PlaceOrder cmd, OrderService orders, HttpContext http)
    => orders.Place(cmd).ToResult(http));`,
    },
  },
  {
    icon: 'bi-globe2',
    label: 'HTTP',
    caption: '201 payload or 4xx Problem Details',
    snippet: {
      label: '422 Unprocessable Content',
      language: 'json',
      code: `{
  "type": "https://tools.ietf.org/html/rfc9110#section-15.5.21",
  "title": "Validation Failed",
  "status": 422,
  "errors": ["Not enough stock."],
  "instance": "/api/orders"
}`,
    },
  },
];

export const BEFORE = `[HttpGet("{id}")]
public async Task<IActionResult> Get(int id)
{
    try
    {
        var product = await _service.GetAsync(id);
        return Ok(product);
    }
    catch (NotFoundException ex)
    {
        return NotFound(new { error = ex.Message });
    }
    catch (ForbiddenException)
    {
        return Forbid();
    }
    catch (ValidationException ex)
    {
        return BadRequest(ex.Errors);
    }
}`;

export const AFTER = `[HttpGet("{id}")]
public async Task<IActionResult> Get(int id)
    => (await _service.GetAsync(id)).ToActionResult(HttpContext);

// in the service
return product is null
    ? Result.NotFound<ProductDto>($"Product {id} does not exist.")
    : Result.Success(product.ToDto());`;

export const SHOWCASE: readonly CodeSnippet[] = [
  {
    label: 'Service',
    language: 'csharp',
    code: `public Task<IOperationResult<OrderDto>> PlaceOrderAsync(PlaceOrder cmd, CancellationToken ct)
    => _products.GetByIdAsync(cmd.ProductId, ct)
        .EnsureAsync(p => p.Stock >= cmd.Quantity, "Not enough stock.")
        .BindAsync(p => _orders.CreateAsync(p, cmd.Quantity, ct))
        .OnSuccessAsync(o => _bus.PublishAsync(new OrderPlaced(o.Id), ct));`,
  },
  {
    label: 'Minimal API',
    language: 'csharp',
    code: `app.MapPost("/api/orders", async (PlaceOrder cmd, OrderService orders, HttpContext http, CancellationToken ct)
    => (await orders.PlaceOrderAsync(cmd, ct)).ToResult(http))
   .Produces<OrderDto>(StatusCodes.Status201Created)
   .ProducesProblem(StatusCodes.Status404NotFound)
   .ProducesProblem(StatusCodes.Status422UnprocessableEntity);`,
  },
  {
    label: '201 Created',
    language: 'json',
    code: `{
  "id": 1001,
  "productId": 42,
  "quantity": 2
}`,
  },
  {
    label: '422 Problem',
    language: 'json',
    code: `{
  "type": "https://tools.ietf.org/html/rfc9110#section-15.5.21",
  "title": "Validation Failed",
  "status": 422,
  "errors": ["Not enough stock."],
  "instance": "/api/orders"
}`,
  },
];

export interface EcosystemItem {
  readonly name: string;
  readonly icon: string;
  readonly query: string;
}

export const ECOSYSTEM: readonly EcosystemItem[] = [
  { name: 'MediatR', icon: 'bi-cpu', query: 'mediatr' },
  { name: 'FluentValidation', icon: 'bi-ui-checks-grid', query: 'fluentvalidation' },
  { name: 'OpenAPI', icon: 'bi-filetype-json', query: 'openapi' },
  { name: 'EF Core', icon: 'bi-database', query: 'ef core' },
  { name: 'Angular', icon: 'bi-window-stack', query: 'angular' },
  { name: 'MAUI & Blazor', icon: 'bi-phone', query: 'maui' },
  { name: 'xUnit', icon: 'bi-check2-square', query: 'xunit' },
  { name: 'Native AOT', icon: 'bi-rocket-takeoff', query: 'aot' },
];
