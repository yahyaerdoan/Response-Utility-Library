import { DocPage } from '../domain/doc-page';
import { cs, list, note, p, section, table, tip, warn } from './builders';

export const CORE_CONTRACT: DocPage = {
  slug: 'core-contract',
  title: 'The core contract',
  icon: 'bi-bounding-box',
  summary: 'IOperationResult and IOperationResult<T>: the shape every result shares.',
  sections: [
    section(
      'members',
      'IOperationResult',
      p('Every result, success or failure, with or without data, implements `IOperationResult` from `ResultHandler.Core.Abstractions`:'),
      table(
        ['Member', 'Meaning'],
        ['`bool IsSuccessful`', 'Did the operation succeed?'],
        ['`ResultStatus Status`', 'Outcome status: an enum covering every standard HTTP status'],
        ['`string Title`', 'Short summary, e.g. `"Not Found"`'],
        ['`string? Detail`', 'Optional context, e.g. `"Product 42 does not exist."`'],
        ['`IReadOnlyList<string> Errors`', 'Individual messages (validation etc.); never null, empty when none'],
      ),
    ),
    section(
      'generic',
      'IOperationResult<T>',
      p('`IOperationResult<T> : IOperationResult` adds `T Data`. Its `IsSuccessful` is annotated with `[MemberNotNullWhen(true, nameof(Data))]`, so the compiler **knows** `Data` is non-null right after a success check:'),
      cs(`var result = await _products.GetByIdAsync(id, ct);

if (result.IsSuccessful)
{
    Console.WriteLine(result.Data.Name); // no warning, no "!" needed
}`),
      tip('Queries return `IOperationResult<T>`; commands that have nothing to return use the non-generic `IOperationResult`. This split mirrors CQRS naturally.'),
    ),
    section(
      'immutability',
      'Immutable by design',
      list(
        'Every property is get-only; a result never changes after it is created.',
        'Composition methods (`Map`, `Bind`, `Ensure`…) always return a **new** result.',
        'Collections are exposed as `IReadOnlyList` / `IReadOnlyDictionary`.',
      ),
      note('Immutability makes results safe to cache, log, pass across threads and compare by value. See [Equality & debugging](/docs/equality-debugging).'),
    ),
  ],
};

export const RESULT_STATUS_DOC: DocPage = {
  slug: 'result-status',
  title: 'ResultStatus',
  icon: 'bi-123',
  summary: 'An HTTP-shaped status enum that does not depend on ASP.NET Core.',
  sections: [
    section(
      'enum',
      'Why an enum of its own?',
      p('`ResultStatus` (`ResultHandler.Core.Enums`) lists every standard status from `100 Continue` to `511 Network Authentication Required`: 62 values in total. It is deliberately **not** `System.Net.HttpStatusCode`: that type misses modern codes (`422`, `425`, `451`…) under their current RFC names, and using it in the domain would suggest a transport dependency.'),
      p('Browse all of them interactively in the [status explorer](/statuses).'),
    ),
    section(
      'mapping',
      'Converting both ways',
      cs(`using ResultHandler.Mapping;

HttpStatusCode code   = ResultStatus.NotFound.ToHttpStatusCode();   // 404
ResultStatus   status = HttpStatusCode.Conflict.ToResultStatus();   // ResultStatus.Conflict
ResultStatus   fromInt = 429.ToResultStatus();                          // ResultStatus.TooManyRequests`),
      p('Both directions are total; they never throw:'),
      table(
        ['Input', 'Fallback'],
        ['An unmapped `ResultStatus` → `ToHttpStatusCode()`', '`500 Internal Server Error`'],
        ['An unknown code such as `499` → `ToResultStatus()`', 'The generic status of its class: `1xx → Processing`, `2xx → Ok`, `3xx → Found`, `4xx → BadRequest`, `5xx+ → InternalServerError`'],
      ),
      tip('`int.ToResultStatus()` is handy when wrapping a downstream `HttpClient` call: `(int)response.StatusCode` becomes a proper `ResultStatus` in one call.'),
    ),
    section(
      'registry',
      'One table, two directions',
      p('Internally a single registry maps `ResultStatus → HttpStatusCode`. The reverse map is **derived** from it at start-up and throws immediately if two statuses ever point at the same code, so the two directions can never silently drift apart.'),
      tip('This is a nice pattern to copy in your own code: keep one source of truth and derive the rest.'),
    ),
    section(
      'names',
      'Modern RFC 9110 names',
      table(
        ['ResultStatus', 'Code', 'Older name you may know'],
        ['`ContentTooLarge`', '413', '`RequestEntityTooLarge`'],
        ['`UnprocessableContent`', '422', '`UnprocessableEntity`'],
        ['`UriTooLong`', '414', '`RequestUriTooLong`'],
        ['`RangeNotSatisfiable`', '416', '`RequestedRangeNotSatisfiable`'],
      ),
    ),
  ],
};

export const BUILDING_RESULTS: DocPage = {
  slug: 'building-results',
  title: 'Building results directly',
  icon: 'bi-hammer',
  summary: 'SuccessResult, ErrorResult and their generic twins: the concrete types behind the facade.',
  sections: [
    section(
      'types',
      'The four concrete types',
      table(
        ['Type', 'IsSuccessful', 'Carries data'],
        ['`SuccessResult`', '`true`', 'no'],
        ['`SuccessDataResult<T>`', '`true`', 'yes'],
        ['`ErrorResult`', '`false`', 'no'],
        ['`ErrorDataResult<T>`', '`false`', 'no (typed as `T` for the signature)'],
      ),
      p('They pin `IsSuccessful` for you, so you cannot accidentally build a “successful 404”.'),
    ),
    section(
      'examples',
      'Constructing them',
      cs(`using ResultHandler.Core.Enums;
using ResultHandler.Implementations.Error;
using ResultHandler.Implementations.Success;

public IOperationResult<ProductDto> GetById(int id)
{
    var product = _products.Find(id);

    if (product is null)
    {
        return new ErrorDataResult<ProductDto>("Not found.", ResultStatus.NotFound, $"Product {id} does not exist.");
    }

    return new SuccessDataResult<ProductDto>(ToDto(product), "Product found.", ResultStatus.Ok);
}`),
      p('Validation messages use the `IReadOnlyList<string> errors` overload instead of `detail`:'),
      cs(`return new ErrorResult("Validation failed.", ResultStatus.UnprocessableContent,
    new[] { "Name is required.", "Price must be greater than zero." });`),
    ),
    section(
      'defaults',
      'Parameterless and short constructors',
      table(
        ['Constructor', 'IsSuccessful', 'Status', 'Title'],
        ['`new SuccessResult()`', '`true`', '`200 Ok`', '"Operation completed successfully."'],
        ['`new SuccessResult(title)`', '`true`', '`200 Ok`', 'your title'],
        ['`new SuccessDataResult<T>(data)`', '`true`', '`200 Ok`', '"Operation completed successfully."'],
        ['`new ErrorResult()`', '`false`', '`500 InternalServerError`', '"An error occurred."'],
        ['`new ErrorDataResult<T>()`', '`false`', '`500 InternalServerError`', '"An error occurred."'],
      ),
      tip('Defaults are handy in tests and prototypes. In production code prefer a named factory, so the status says what actually happened.'),
    ),
    section(
      'failure-data',
      'Failures that carry data',
      p('`ErrorDataResult<T>` has two constructors that keep a payload on a **failed** result: `(data, title, status)` and `(data, title, status, detail)`. Use them when the caller needs context about the failure, for example the current server version on an optimistic-concurrency conflict:'),
      cs(`IOperationResult<ProductDto> Update(UpdateProduct cmd)
{
    var current = _products.Find(cmd.Id);
    if (current.Version != cmd.Version)
    {
        return new ErrorDataResult<ProductDto>(ToDto(current), "Conflict", ResultStatus.Conflict,
            "The product was changed by someone else.");
    }

    // ...
}`),
      p('The facade has the same shape: `Result.Failure<ProductDto>(ToDto(current), "Conflict", "The product was changed by someone else.", ResultStatus.Conflict)`.'),
      warn('The payload is only visible in process and in `JsonSerializer` output (`resultData`). The ASP.NET Core adapters always turn a failure into Problem Details, so the data is **not** sent over HTTP. `[MemberNotNullWhen]` only guarantees `Data` on success, so check it for null on a failure.'),
    ),
    section(
      'base',
      'Base classes',
      p('`OperationResult` and `OperationDataResult<T>` (`ResultHandler.Core.Base`) are what the four types inherit from. They also implement `IHasFieldErrors` and `IResultFailureFactory<TSelf>`. Construct them directly only for unusual shapes, or use them as the response type of MediatR requests (see [generic short-circuiting](/docs/generic-short-circuiting)).'),
      warn('Prefer the [Result facade](/docs/result-facade) in application code. It gives you correct default titles and a discoverable, status-named API.'),
    ),
  ],
};

export const RESULT_FACADE: DocPage = {
  slug: 'result-facade',
  title: 'Result facade',
  icon: 'bi-magic',
  badge: { label: 'Recommended', tone: 'accent' },
  summary: 'One named factory per status, with sensible default titles and messages.',
  sections: [
    section(
      'overview',
      'Overview',
      p('`ResultHandler.Facade.Result` is a static class with a factory pair (non-generic and `<T>`) for every `ResultStatus`, named after the status. Type `Result.` and IntelliSense shows you the whole HTTP vocabulary.'),
      cs(`using ResultHandler.Facade;

Result.Success();                          // 200, "Operation completed successfully."
Result.Success(dto);                       // 200 + data
Result.Created(dto);                       // 201 + data, "Resource created successfully."
Result.Success("Saved.");                  // 200 with your own title
Result.Success(dto, "Product found.");     // 200 + data + your title
Result.Accepted();                         // 202
Result.NoContent();                        // 204

Result.NotFound<ProductDto>("Product 42 does not exist.");
Result.Conflict("A product with this name already exists.");
Result.Forbidden();                        // detail defaults to "You do not have permission to access this resource."
Result.TooManyRequests();                  // detail defaults to "Too many requests. Please slow down and try again later."`),
    ),
    section(
      'return-types',
      'One return type for success and failure',
      p('Every factory returns the common base type: `OperationResult` for the non-generic form and `OperationDataResult<T>` for `<T>` (since 13.0). Success and failure branches share one static type, so these compile without casts or type arguments:'),
      cs(`OperationDataResult<ProductDto> Find(int id)
    => _products.Find(id) is { } product
        ? Result.Success(ToDto(product))
        : Result.NotFound<ProductDto>($"Product {id} does not exist.");

OperationResult Delete(bool exists)
    => exists ? Result.NoContent() : Result.NotFound("Missing.");`),
      note('The objects are still `SuccessDataResult<T>`, `ErrorResult` and so on at runtime, so `is` checks, serialization and ASP.NET Core mapping behave as before.'),
      warn('`Result.Success("text")` binds to the **title** overload and returns a result with no data. To carry a string as data, write `Result.Success<string>("text")`.'),
    ),
    section(
      'defaults',
      'Required vs. optional detail',
      p('Statuses whose meaning is always the same (`401`, `403`, `429`, `503`…) have a **default detail**. Statuses that only make sense with context (`400`, `404`, `409`, `422`…) **require** one, so the compiler forces you to explain *what* was not found.'),
      tip('The [status explorer](/statuses) shows, for each status, whether the detail is required and what the default is.'),
    ),
    section(
      'validation',
      'Validation: Result.Invalid',
      cs(`public OperationResult? ValidateCreate(CreateProductRequest request)
{
    var errors = new List<string>();
    if (string.IsNullOrEmpty(request.Name)) errors.Add("Name is required.");
    if (request.Price <= 0) errors.Add("Price must be greater than zero.");

    return errors.Count > 0 ? Result.Invalid(errors) : null; // 422 "Validation Failed"
}`),
      p('The typed variant keeps the signature of a query: `return Result.Invalid<ProductDto>("Name is required.");`'),
    ),
    section(
      'redirects',
      'Redirects',
      p('3xx factories take the target and interpolate it into the title:'),
      cs(`Result.MovedPermanently("/api/v2/products/42"); // "Resource moved permanently to: /api/v2/products/42"
Result.SeeOther("/orders/1001");                   // "See other resource at: /orders/1001"
Result.NotModified();                              // 304, always bodyless`),
    ),
    section(
      'escape',
      'Escape hatch: Result.Failure',
      p('When no named factory fits, specify title, detail and status yourself:'),
      cs(`Result.Failure("Payment declined.", "The card was rejected by the issuer.", ResultStatus.PaymentRequired);`),
    ),
    section(
      'single-source',
      'One source of truth for texts',
      note('All default titles and messages live in exactly one place inside the library. `ResultFailureFactory` (for generic code) delegates to `Result`, so the wording is identical no matter which API produced the failure.'),
    ),
  ],
};
