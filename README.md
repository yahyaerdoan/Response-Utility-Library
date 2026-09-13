# ResponseResultHandler

A Result-Pattern library for .NET (net8.0–net10.0). Wraps the outcome of an
operation — success or failure, an HTTP-mappable status, a title/detail, and optional data — in an
immutable object instead of throwing exceptions or returning bare booleans.

This repository is two NuGet packages sharing this README:

| Package | Project | Purpose |
|---|---|---|
| [`ResponseResultHandler`](https://www.nuget.org/packages/ResponseResultHandler) | `src/ResultHandler` | Core library — framework-agnostic |
| [`ResponseResultHandler.AspNetCore`](https://www.nuget.org/packages/ResponseResultHandler.AspNetCore) | `src/ResultHandler.AspNetCore` | Converts results to `IActionResult` / RFC 9457 Problem Details |

```
dotnet add package ResponseResultHandler
dotnet add package ResponseResultHandler.AspNetCore   # optional: IActionResult / RFC 9457 adapter
```

See [`CHANGELOG.md`](CHANGELOG.md) for what changed between versions.

Everything below is demonstrated through one running example: a `ProductService` that looks up a
product by id, and a `ProductsController` that exposes it over HTTP.

---
## 1. The core contract

Every result implements `IOperationResult` (`ResultHandler.Core.Abstractions`):

| Member | Meaning |
|---|---|
| `bool IsSuccessful` | did the operation succeed |
| `ResultStatus Status` | outcome status — an enum, not `HttpStatusCode`, see §2 |
| `string Title` | short summary, e.g. `"Not Found"` |
| `string? Detail` | optional extra context, e.g. `"Product 42 does not exist."` |
| `IReadOnlyList<string> Errors` | optional list of individual error messages (validation, etc.) |

`IOperationResult<T> : IOperationResult` adds `T Data` — guaranteed non-null when `IsSuccessful` is `true`
(the compiler enforces this via `[MemberNotNullWhen]`, so `result.Data` is safe to use right after
an `if (result.IsSuccessful)` check without a null-forgiving operator).

---
## 2. `ResultStatus`

An enum covering every standard 1xx–5xx HTTP status (`ResultHandler.Core.Enums`), kept independent
of `System.Net.HttpStatusCode` so the library has no hard dependency on ASP.NET Core. Convert either
direction with the extension methods in `ResultHandler.Mapping`:

```csharp
using ResultHandler.Core.Enums;
using ResultHandler.Mapping;

HttpStatusCode code = ResultStatus.NotFound.ToHttpStatusCode();   // 404
ResultStatus status = HttpStatusCode.Conflict.ToResultStatus();  // Conflict
```

---
## 3. Building results directly

`SuccessResult` / `SuccessDataResult<T>` and `ErrorResult` / `ErrorDataResult<T>`
(`ResultHandler.Implementations.Success` / `.Error`) pin `IsSuccessful` for you:

```csharp
using ResultHandler.Core.Enums;
using ResultHandler.Implementations.Error;
using ResultHandler.Implementations.Success;

public IOperationResult<ProductDto> GetById(int id)
{
    var product = _products.Find(id);

    if (product is null)
    {
        return new ErrorDataResult<ProductDto>(
            "Not found.", ResultStatus.NotFound, $"Product {id} does not exist.");
    }

    return new SuccessDataResult<ProductDto>(ToDto(product), "Product found.", ResultStatus.Ok);
}
```

Validation errors use the `IReadOnlyList<string> errors` overload instead of `detail`:

```csharp
if (request.Name is { Length: 0 })
{
    return new ErrorResult("Validation failed.", ResultStatus.UnprocessableContent,
        new[] { "Name is required.", "Price must be greater than zero." });
}
```

`OperationResult` / `OperationDataResult<T>` (`ResultHandler.Core.Base`) are the base classes both of
the above inherit from — construct them directly only for a custom result shape that isn't a plain
success/error; `SuccessResult`/`ErrorResult` cover the normal cases.

---
## 4. The `Result` facade — the recommended way

`ResultHandler.Facade.Result` is a static class with one factory pair per `ResultStatus`
(non-generic and `<T>`), named after the status, with sensible default titles baked in. It's what
the example above looks like using it instead:

```csharp
using ResultHandler.Facade; // Result

public IOperationResult<ProductDto> GetById(int id)
{
    var product = _products.Find(id);

    return product is null
        ? Result.NotFound<ProductDto>($"Product {id} does not exist.")
        : Result.Success(ToDto(product), "Product found.");
}

public ErrorResult? ValidateCreate(CreateProductRequest request)
{
    var errors = new List<string>();
    if (string.IsNullOrEmpty(request.Name)) errors.Add("Name is required.");
    if (request.Price <= 0) errors.Add("Price must be greater than zero.");

    return errors.Count > 0 ? Result.Invalid(errors.ToArray()) : null;
}

public SuccessResult MoveResource(int id, string newLocation)
    => Result.MovedPermanently(newLocation); // 3xx redirects — location gets interpolated into the title

// Escape hatch for anything not covered by a named factory:
public ErrorResult CustomFailure()
    => Result.Failure("Payment declined.", "The card was rejected by the issuer.", ResultStatus.PaymentRequired);
```

---
## 5. `ResultHandler.AspNetCore` — converting to `IActionResult`

Add the `ResponseResultHandler.AspNetCore` package and call one extension method
(`ResultHandler.AspNetCore.Extensions`) at the end of a controller action:

```csharp
using ResultHandler.AspNetCore.Extensions;

[ApiController]
[Route("api/products")]
public class ProductsController : ControllerBase
{
    private readonly ProductService _products;

    [HttpGet("{id}")]
    public IActionResult GetById(int id)
        => _products.GetById(id).ToActionResult(HttpContext);

    [HttpPost]
    public IActionResult Create(CreateProductRequest request)
    {
        var validation = _products.ValidateCreate(request);
        if (validation is not null)
        {
            return validation.ToActionResult(); // 422 Problem Details with the two validation errors
        }

        return Result.Created(_products.Create(request)).ToActionResult();
    }
}
```

* **`ToActionResult()`** — for non-generic `IOperationResult` (commands with no data to return).
  Success is always bodyless — a bare status code matching the result's actual `Status` (`201` for
  `Created`, `204` for `NoContent`, etc.), never a payload; failure returns an RFC 9457
  `ProblemDetails` body.
* **`ToActionResult<T>()`** — for `IOperationResult<T>`. Success returns the raw `T` payload with the
  result's actual status code (`201 { ... }` for `Created`, `202 { ... }` for `Accepted`, etc. — not
  hardcoded to `200`), or a bodyless status for `NoContent`/1xx/3xx; failure is the same
  `ProblemDetails` body.
* **`ToEnvelopedActionResult()`** — success returns the *whole* result object (status/title/data) as
  the body instead of just the payload, with the same status-code-preserving behavior — useful when
  clients want metadata alongside the data.
* **`ToProblemDetails(HttpContext? httpContext = null)`** — builds the `ProblemDetails` yourself;
  pass `HttpContext` and `Instance` gets set to the current request path (RFC 9457 §3.1.4).
* Every 4xx/5xx `ResultStatus` maps `ProblemDetails.Type` to the actual RFC section that defines it
  (RFC 9110, RFC 6585, RFC 4918, RFC 7725, RFC 8470); 1xx/2xx/3xx use `about:blank` per RFC 9457
  §4.2.1, since those aren't "problems".
* These `IActionResult`-returning methods are for **MVC controllers**. In a Minimal API endpoint
  delegate, use the `IResult`-returning siblings from §6 instead — returning `IActionResult` from a
  delegate triggers analyzer warning `ASP0004` and hides the response shape from OpenAPI/Swagger
  generation at compile time.

A failed `GetById(999)` call above produces:

```json
{
  "type": "https://tools.ietf.org/html/rfc9110#section-15.5.5",
  "title": "Not Found",
  "status": 404,
  "detail": "Product 999 does not exist.",
  "instance": "/api/products/999"
}
```

---
## 6. Minimal APIs

Minimal API endpoint delegates *can* return `IActionResult` — ASP.NET Core has run it through an MVC
compatibility shim since .NET 7 — but don't. That path triggers analyzer warning `ASP0004` and, more
importantly, `IActionResult` is opaque to the endpoint metadata pipeline: the compile-time
`IEndpointMetadataProvider` machinery that Swashbuckle/`Microsoft.AspNetCore.OpenApi` rely on to infer
response types and status codes can't see through it, so your OpenAPI/Swagger document ends up
missing or wrong for those endpoints.

Use the `IResult`-returning siblings instead — same shapes, same status-code-preserving behavior, but
native to Minimal APIs and fully visible to OpenAPI generation:

* **`ToResult(HttpContext? httpContext = null)`** — `IOperationResult` → `IResult`. Success returns a
  bodyless status (`204` for `NoContent`, `304` for `NotModified`, plain status code for 1xx/3xx/`Ok`);
  failure returns an RFC 9457 `ProblemDetails` JSON body via `ToProblemResult`.
* **`ToResult<T>(HttpContext? httpContext = null)`** — `IOperationResult<T>` → `IResult`. Success
  returns the raw payload as JSON with the result's actual status code (`201` for `Created`, `202` for
  `Accepted`, etc.); failure is the same `ProblemDetails` JSON body.
* **`ToEnvelopedResult(HttpContext? httpContext = null)`** — success returns the *whole* result object
  (status/title/data) as the JSON body instead of just the payload.
* **`ToProblemResult(HttpContext? httpContext = null)`** — maps a failed result straight to an
  `IResult` carrying RFC 9457 `ProblemDetails` JSON; the same building block `ToResult`/`ToResult<T>`/
  `ToEnvelopedResult` use internally for their failure branch.

```csharp
using ResultHandler.AspNetCore.Extensions;
using ResultHandler.Facade; // Result

var app = WebApplication.Create(args);
app.Services.GetRequiredService<IServiceCollection>(); // ...DI setup omitted

app.MapGet("/api/products/{id:int}", (int id, ProductService products, HttpContext httpContext)
    => products.GetById(id).ToResult(httpContext));

app.MapPost("/api/products", (CreateProductRequest request, ProductService products) =>
{
    var validation = products.ValidateCreate(request);
    return validation is not null
        ? validation.ToProblemResult() // 422 Problem Details with the two validation errors
        : Result.Created(products.Create(request)).ToResult();
});

app.Run();
```

---
## 7. Functional composition

`ResultHandler.Functional` adds chaining helpers so callers don't have to repeat
`if (!result.IsSuccessful) return ...` at every step:

```csharp
using ResultHandler.Functional;

string message = _products.GetById(id)
    .Map(p => p.Name.ToUpperInvariant())
    .Match(
        onSuccess: name => $"Found: {name}",
        onFailure: failure => $"Error: {failure.Title}");

_products.GetById(id)
    .OnSuccess(product => _logger.LogInformation("Fetched {Name}", product.Name)) // typed: product is ProductDto
    .OnFailure(failure => _logger.LogWarning("Lookup failed: {Title}", failure.Title));

IOperationResult<OrderDto> order = _products.GetById(id)
    .Bind(product => _orders.CreateDraftOrder(product)); // chains into another IOperationResult<T>-returning call

IOperationResult<ProductDto> validated = _products.GetById(id)
    .Ensure(product => product.Stock > 0, "Product is out of stock."); // guard clause: 422 if the predicate fails
```

`Map`/`Bind` short-circuit automatically: if the source result failed, the mapper/binder never runs
and the failure (title/status/detail/errors) is carried over into the new result type. `Ensure` turns
a still-*successful* result into a failure when a business-rule predicate rejects the data — the
shortcut overload above defaults to `"Validation Failed"` / `422 Unprocessable Content` (same shape as
`Result.Invalid`); pass your own `(title, detail, status)` when a different outcome fits better:

```csharp
_products.GetById(id)
    .Ensure(product => product.OwnerId == currentUserId, "Forbidden.", "You do not own this product.", ResultStatus.Forbidden);
```

---
## 8. Combining independent checks — `Result.Combine`

`Ensure` (and `Bind`/`Map`) stop at the *first* failure — the rest of the chain never runs. That's the
right behavior for a pipeline of dependent steps, but wrong for independent checks where you want to
report every problem at once (e.g. every invalid field on a form). `Result.Combine(...)` (`ResultHandler.Facade`)
covers that case instead — it runs every result to completion and merges their outcomes:

```csharp
using ResultHandler.Facade; // Result

ErrorResult? ValidateCreate(CreateProductRequest request)
{
    var nameCheck = string.IsNullOrEmpty(request.Name)
        ? Result.Invalid("Name is required.")
        : Result.Success();

    var priceCheck = request.Price <= 0
        ? Result.Invalid("Price must be greater than zero.")
        : Result.Success();

    var combined = Result.Combine(nameCheck, priceCheck);
    return combined.IsSuccessful ? null : (ErrorResult)combined;
}
```

If both checks fail, `combined.Errors` contains **both** messages — `["Name is required.", "Price must be greater than zero."]`
— not just the first one. Every failed result's `Errors` (or `Detail`/`Title` when it carries none) are
concatenated in order; if every result succeeds, `Combine` returns `Result.Success()`.

When the independent checks each carry data you actually need afterward, the 2–4 arity generic
overloads combine both the outcome *and* the payloads into a named tuple:

```csharp
IOperationResult<(CustomerDto Customer, ShippingAddressDto Address)> ValidateOrder(int customerId, int addressId)
    => Result.Combine(_customers.GetById(customerId), _addresses.GetById(addressId));
```

If both succeed, `Data` is `(CustomerDto Customer, ShippingAddressDto Address)`; if either (or both) fail,
you get the same aggregated `Result.Invalid(...)` as above, re-projected into `IOperationResult<(...)>`.

---
## 9. Async composition

Real handlers usually chain calls that are themselves `async` — a repository hit, an email send, a
downstream API call. `ResultHandler.Functional` mirrors every method from §7 with an `...Async`
counterpart so those chains read the same way, without an `await` breaking up the fluent chain at
every step:

```csharp
using ResultHandler.Functional;

public Task<IOperationResult<ProductDto>> ActivateAsync(int id)
    => _products.GetByIdAsync(id)                       // Task<IOperationResult<Product>>
        .BindAsync(product => ValidateCanActivateAsync(product)) // async business rule
        .MapAsync(product => ToDto(product))                     // sync mapper
        .OnSuccessAsync(dto => _email.SendActivationEmailAsync(dto.OwnerEmail)); // async side effect
```

If `GetByIdAsync` returns `NotFound`, or `ValidateCanActivateAsync` fails, the chain short-circuits
immediately — `MapAsync` and `OnSuccessAsync` never run, and the original failure (title/status/detail)
is what the caller gets back, exactly like the sync `Map`/`Bind` in §7.

Each method comes in three shapes so you can start the chain from either a `Task<IOperationResult<T>>`
or a plain `IOperationResult<T>`, and pass either a sync or an `async` delegate — mix and match freely
in the same chain:

| Shape | Example |
|---|---|
| `Task<IOperationResult<T>>` source, sync delegate | `.MapAsync(p => p.Name)` |
| `Task<IOperationResult<T>>` source, async delegate | `.BindAsync(p => _orders.CreateDraftOrderAsync(p))` |
| `IOperationResult<T>` source, async delegate | `existingResult.OnSuccessAsync(p => _email.SendAsync(p))` |

`MatchAsync`, `OnSuccessAsync`, `OnFailureAsync`, `MapAsync`, `BindAsync`, and `EnsureAsync` all follow
this pattern, for both `IOperationResult` and `IOperationResult<T>` (`EnsureAsync` only exists for
`IOperationResult<T>` — same reasoning as `Ensure` in §7, there's no data to check on the non-generic
form). The controller/endpoint at the edge (§5/§6)
doesn't change — just `await` the final result and call `.ToActionResult()` / `.ToResult()` as usual:

```csharp
[HttpPost("{id}/activate")]
public async Task<IActionResult> Activate(int id)
    => (await _products.ActivateAsync(id)).ToActionResult();
```

---
## 10. Generic short-circuiting — `IResultFailureFactory<TSelf>` / `ResultFailureFactory`

Everything above assumes the calling code knows the concrete result type (`IOperationResult<ProductDto>`,
`ErrorResult`, ...). Generic infrastructure often doesn't — a MediatR `IPipelineBehavior<TRequest, TResponse>`,
a gRPC interceptor, any short-circuiting middleware only has `TResponse` as a type parameter. Today that
usually gets solved by throwing an exception to unwind the pipeline, because you can't `new TResponse(...)`
without knowing what `TResponse` actually is.

`IResultFailureFactory<TSelf>` (`ResultHandler.Core.Abstractions`) solves this with a C# 11 static-abstract-interface
CRTP: it lets `TSelf` build its own failure instance. `OperationResult` and `OperationDataResult<T>` already
implement it, so **any** result type built on top of this library (concrete or still generic) gets it for free:

```csharp
public interface IResultFailureFactory<TSelf> where TSelf : IOperationResult
{
    static abstract TSelf Failure(IReadOnlyList<string> errors);               // validation message list
    static abstract TSelf Failure(string title, string detail, ResultStatus status); // everything else
}
```

`ResultFailureFactory` (`ResultHandler.Functional`) layers the same named, per-status vocabulary as `Result` on top
of these two primitives — `BadRequest`, `NotFound`, `Unauthorized`, `Forbidden`, and every other 4xx/5xx —
generically, for any `TSelf`. It delegates to the matching `Result.XXX(detail)` method internally, so titles
and default messages have exactly one source of truth (`Result`); nothing is duplicated.

A MediatR pipeline behavior that stops throwing and starts returning:

```csharp
using ResultHandler.Core.Abstractions;
using ResultHandler.Functional;

public class ValidationBehavior<TRequest, TResponse>(IEnumerable<IValidator<TRequest>> validators)
    : IPipelineBehavior<TRequest, TResponse>
    where TRequest : IRequest<TResponse>
    where TResponse : IOperationResult, IResultFailureFactory<TResponse>
{
    public async Task<TResponse> Handle(TRequest request, RequestHandlerDelegate<TResponse> next, CancellationToken ct)
    {
        var errors = validators
            .Select(v => v.Validate(request))
            .SelectMany(r => r.Errors)
            .Select(f => f.ErrorMessage)
            .ToArray();

        if (errors.Length > 0)
        {
            return TResponse.Failure(errors); // short-circuits the pipeline — no throw, no exception cost
        }

        return await next(ct);
    }
}

public class AuthorizationBehavior<TRequest, TResponse>(ICurrentUser user)
    : IPipelineBehavior<TRequest, TResponse>
    where TRequest : IRequest<TResponse>, IRequireRole
    where TResponse : IOperationResult, IResultFailureFactory<TResponse>
{
    public async Task<TResponse> Handle(TRequest request, RequestHandlerDelegate<TResponse> next, CancellationToken ct)
    {
        if (!user.IsAuthenticated)
        {
            return ResultFailureFactory.Unauthorized<TResponse>();
        }

        if (!user.IsInRole(request.RequiredRole))
        {
            return ResultFailureFactory.Forbidden<TResponse>();
        }

        return await next(ct);
    }
}
```

For this to type-check, the command/query itself declares its response as an `OperationDataResult<T>` (or
any other `IResultFailureFactory` implementer) instead of a bare DTO:

```csharp
public record CreateProductCommand(string Name, decimal Price) : IRequest<OperationDataResult<ProductDto>>;

public class CreateProductHandler(IProductRepository repository)
    : IRequestHandler<CreateProductCommand, OperationDataResult<ProductDto>>
{
    public async Task<OperationDataResult<ProductDto>> Handle(CreateProductCommand request, CancellationToken ct)
    {
        if (await repository.ExistsByName(request.Name))
        {
            // built by a business-rule check that only knows IOperationResult, not the final DTO shape:
            var duplicate = ResultFailureFactory.Conflict<OperationResult>($"A product named '{request.Name}' already exists.");
            return duplicate.ToErrorDataResult<ProductDto>(); // re-projects into the handler's actual return type
        }

        var product = await repository.Add(request.Name, request.Price);
        return Result.Success(ToDto(product));
    }
}
```

`ToErrorDataResult<T>()` (`ResultHandler.Functional`) is the companion piece: it re-projects an *existing*
failed `IOperationResult` (title/status/detail/errors already decided elsewhere, e.g. in a business-rule
method) into the `ErrorDataResult<T>` shape a handler must return — a conversion, not a new factory call,
which is why it reads as `failure.ToErrorDataResult<T>()` rather than `Result.ToErrorDataResult<T>(failure)`.

The controller/endpoint at the edge doesn't change at all — `result.ToActionResult()` still works, because
`OperationDataResult<T>` still implements `IOperationResult<T>`:

```csharp
[HttpPost]
public async Task<IActionResult> Create(CreateProductCommand command)
    => (await mediator.Send(command)).ToActionResult();
```

---
## 11. Per-field validation errors — `IHasFieldErrors` / `IFieldFailureFactory<TSelf>`

§3's plain `IReadOnlyList<string> Errors` is a flat list — fine for "here are the problems" messages,
but a form UI usually needs to know *which* input each message belongs to. `OperationResult`/
`OperationDataResult<T>` (and therefore `ErrorResult`/`ErrorDataResult<T>`) also implement
`IHasFieldErrors` (`ResultHandler.Core.Abstractions`):

```csharp
public interface IHasFieldErrors
{
    IReadOnlyDictionary<string, IReadOnlyList<string>> FieldErrors { get; }
}
```

Build one with `OperationResult.Failure(fieldErrors)` / `OperationDataResult<T>.Failure(fieldErrors)`,
or the generic `IFieldFailureFactory<TSelf>.Failure(fieldErrors)` (implemented by the same types, for
the same CRTP reason as §10) — keyed by property name, valued by that property's messages:

```csharp
using ResultHandler.Core.Base;

var fieldErrors = new Dictionary<string, IReadOnlyList<string>>
{
    ["Name"] = ["Name is required."],
    ["Price"] = ["Price must be greater than zero."],
};

OperationResult validation = OperationResult.Failure(fieldErrors);
```

`FieldErrors` and the flattened `Errors` list are both populated from the same input — pick whichever
shape a given caller needs; `Title`/`Status` default to `"Validation Failed"` /
`422 Unprocessable Content`, same as the flat-list `Failure(errors)` overload.

A generic pipeline behavior (mirrors §10's `ValidationBehavior`, keyed per field instead of a flat list):

```csharp
public class FieldValidationBehavior<TRequest, TResponse>(IEnumerable<IValidator<TRequest>> validators)
    : IPipelineBehavior<TRequest, TResponse>
    where TRequest : IRequest<TResponse>
    where TResponse : IOperationResult, IFieldFailureFactory<TResponse>
{
    public async Task<TResponse> Handle(TRequest request, RequestHandlerDelegate<TResponse> next, CancellationToken ct)
    {
        var fieldErrors = validators
            .Select(v => v.Validate(request))
            .SelectMany(r => r.Errors)
            .GroupBy(f => f.PropertyName)
            .ToDictionary(g => g.Key, IReadOnlyList<string> (g) => [.. g.Select(f => f.ErrorMessage)]);

        return fieldErrors.Count > 0 ? TResponse.Failure(fieldErrors) : await next(ct);
    }
}
```

`ToErrorDataResult<T>()`, `Map`, and `Bind` (§7/§10) carry `FieldErrors` through when re-projecting a
failure into a different result type, same as they carry `Title`/`Status`/`Detail`/`Errors`.

`ResultHandler.AspNetCore`'s `ToProblemDetails()` (§5/§6) adds a failed result's `FieldErrors` to the
Problem Details body as a `"fieldErrors"` extension when non-empty:

```json
{
  "type": "https://tools.ietf.org/html/rfc9110#section-15.5.21",
  "title": "Validation Failed",
  "status": 422,
  "errors": ["Name is required.", "Price must be greater than zero."],
  "fieldErrors": {
    "Name": ["Name is required."],
    "Price": ["Price must be greater than zero."]
  }
}
```

---
## 12. Serialization

`System.Text.Json` output uses fixed property names regardless of your `JsonSerializerOptions`
naming policy, and `ResultStatus` always serializes as its numeric HTTP code:

```csharp
JsonSerializer.Serialize(Result.NotFound<ProductDto>("Product 42 does not exist."));
```

```json
{
  "resultData": null,
  "isSuccessful": false,
  "statusCode": 404,
  "statusMessage": "Not Found",
  "detail": "Product 42 does not exist.",
  "errors": [],
  "fieldErrors": {}
}
```

`Detail` is omitted entirely when `null`; `errors` and `fieldErrors` are always present, empty
(`[]`/`{}`) when the failure doesn't carry any (see §11).

---
## 13. Equality & debugging

`OperationResult`/`OperationDataResult<T>` override `Equals`/`GetHashCode` (structural, by value) and `ToString()`:

```csharp
Result.NotFound("x") == Result.NotFound("x"); // false (reference types) — use .Equals()
Result.NotFound("x").Equals(Result.NotFound("x")); // true
Result.NotFound("x").ToString(); // "NotFound (404): Not Found"
```

---
## 14. Migrating to v12

v12 **removes** every member that v11 marked `[Obsolete]` — they no longer compile, there's no
forwarding shim. Replace them directly:

| Removed in v12 | Use instead |
| --- | --- |
| `StatusMessage` | `Title` |
| `StatusCode: HttpStatusCode` | `Status: ResultStatus` |
| `ResultData` | `Data` |
| `new ErrorResult(string statusMessage, HttpStatusCode statusCode)` and the equivalent `HttpStatusCode`-based constructors on `OperationResult`, `OperationDataResult<T>`, `SuccessResult`, `SuccessDataResult<T>`, `ErrorDataResult<T>` | The `ResultStatus`-based constructor, e.g. `new ErrorResult(title, status)`. If you only have an `HttpStatusCode` on hand, convert it first: `new ErrorResult(title, httpStatusCode.ToResultStatus())` (`HttpStatusCodeExtensions.ToResultStatus`). |

```csharp
// Before (v11, obsolete):
var legacy = new ErrorResult("Not found.", HttpStatusCode.NotFound);
Console.WriteLine(legacy.StatusMessage);

// After (v12):
var current = new ErrorResult("Not found.", HttpStatusCode.NotFound.ToResultStatus());
Console.WriteLine(current.Title);
```

**`ResultHandler.AspNetCore` behavior fix (carried over from v11):** `ToActionResult<T>()` and `ToEnvelopedActionResult()`
used to hardcode a `200` status code for any successful result that carried a body, silently
discarding the result's actual `Status` (so `Result.Created(...)` came back as `200`, not `201`).
Both now honor `Status` correctly. If you were relying on the old (incorrect) `200`-always behavior,
check call sites that use non-`Ok` success statuses with `ToActionResult<T>`/`ToEnvelopedActionResult`.
