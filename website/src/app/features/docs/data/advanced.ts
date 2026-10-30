import { DocPage } from '../domain/doc-page';
import { cs, json, list, note, p, section, table, tip, warn } from './builders';

export const GENERIC_SHORT_CIRCUITING: DocPage = {
  slug: 'generic-short-circuiting',
  title: 'Generic short-circuiting',
  icon: 'bi-cpu',
  badge: { label: 'Advanced', tone: 'muted' },
  summary: 'IResultFailureFactory<TSelf> and ResultFailureFactory: return failures from MediatR behaviors, decorators or any generic pipeline without throwing.',
  sections: [
    section(
      'problem',
      'The problem',
      p('A MediatR `IPipelineBehavior<TRequest, TResponse>` only knows `TResponse` as a type parameter. You cannot `new TResponse(...)`, so the usual workaround is throwing a `ValidationException` just to unwind the pipeline, and then catching it again in middleware.'),
    ),
    section(
      'crtp',
      'Static abstract factories (C# 11)',
      cs(`public interface IResultFailureFactory<TSelf> where TSelf : IOperationResult
{
    static abstract TSelf Failure(IReadOnlyList<string> errors);
    static abstract TSelf Failure(IReadOnlyDictionary<string, IReadOnlyList<string>> fieldErrors);
    static abstract TSelf Failure(string title, string detail, ResultStatus status);
}`),
      p('`OperationResult` and `OperationDataResult<T>` implement it, so any result type built on them can create its own failure, even inside fully generic code.'),
    ),
    section(
      'behaviors',
      'Pipeline behaviors that return instead of throw',
      cs(`public sealed class ValidationBehavior<TRequest, TResponse>(IEnumerable<IValidator<TRequest>> validators)
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

        return errors.Length > 0 ? TResponse.Failure(errors) : await next(ct);
    }
}

public sealed class AuthorizationBehavior<TRequest, TResponse>(ICurrentUser user)
    : IPipelineBehavior<TRequest, TResponse>
    where TRequest : IRequest<TResponse>, IRequireRole
    where TResponse : IOperationResult, IResultFailureFactory<TResponse>
{
    public async Task<TResponse> Handle(TRequest request, RequestHandlerDelegate<TResponse> next, CancellationToken ct)
    {
        if (!user.IsAuthenticated) return ResultFailureFactory.Unauthorized<TResponse>();
        if (!user.IsInRole(request.RequiredRole)) return ResultFailureFactory.Forbidden<TResponse>();
        return await next(ct);
    }
}`),
      tip('`ResultFailureFactory` exposes the same per-status vocabulary as `Result` (`NotFound<T>`, `Conflict<T>`, `TooManyRequests<T>`…) but for any `TSelf`. It delegates to `Result`, so the texts are identical.'),
    ),
    section(
      'handlers',
      'Requests and handlers',
      cs(`public sealed record CreateProductCommand(string Name, decimal Price) : IRequest<OperationDataResult<ProductDto>>;

public sealed class CreateProductHandler(IProductRepository repository)
    : IRequestHandler<CreateProductCommand, OperationDataResult<ProductDto>>
{
    public async Task<OperationDataResult<ProductDto>> Handle(CreateProductCommand request, CancellationToken ct)
    {
        if (await repository.ExistsByName(request.Name))
        {
            var duplicate = ResultFailureFactory.Conflict<OperationResult>($"A product named '{request.Name}' already exists.");
            return duplicate.ToErrorDataResult<ProductDto>();
        }

        var product = await repository.Add(request.Name, request.Price);
        return Result.Success(ToDto(product));
    }
}`),
      note('`ToErrorDataResult<T>()` re-projects an existing failure (decided elsewhere) into the `ErrorDataResult<T>` the handler must return. It is a conversion, not a new factory call.'),
      warn('The request must declare `OperationDataResult<T>` (or another `IResultFailureFactory` implementer) as its response. A bare `IOperationResult<T>` interface cannot satisfy the static-abstract constraint.'),
    ),
    section(
      'without-mediatr',
      'Without MediatR: a decorator',
      p('Nothing here is MediatR-specific. Any generic code that only knows `TResponse` can use the same constraint, for example a validation decorator around your own handler abstraction:'),
      cs(`public interface IHandler<TRequest, TResponse> where TResponse : IOperationResult
{
    Task<TResponse> HandleAsync(TRequest request, CancellationToken ct);
}

public interface IRequestValidator<in TRequest>
{
    IReadOnlyDictionary<string, IReadOnlyList<string>> Validate(TRequest request);
}

public sealed class ValidatingHandler<TRequest, TResponse>(IHandler<TRequest, TResponse> inner, IRequestValidator<TRequest> validator)
    : IHandler<TRequest, TResponse>
    where TResponse : IOperationResult, IResultFailureFactory<TResponse>
{
    public async Task<TResponse> HandleAsync(TRequest request, CancellationToken ct)
    {
        var fieldErrors = validator.Validate(request);
        return fieldErrors.Count > 0
            ? TResponse.Failure(fieldErrors)                  // 422 with fieldErrors, no exception
            : await inner.HandleAsync(request, ct);
    }
}`),
      p('Register it as a decorator (by hand, or with a DI helper such as Scrutor). The same pattern fits Wolverine or Brighter middleware, background job pipelines and anything else that is generic over the response type.'),
    ),
  ],
};

export const FIELD_ERRORS: DocPage = {
  slug: 'field-errors',
  title: 'Per-field validation errors',
  icon: 'bi-ui-checks-grid',
  badge: { label: 'New in 12.1', tone: 'new' },
  summary: 'IHasFieldErrors: messages keyed by property name, ready for form UIs.',
  sections: [
    section(
      'interface',
      'IHasFieldErrors',
      cs(`public interface IHasFieldErrors
{
    IReadOnlyDictionary<string, IReadOnlyList<string>> FieldErrors { get; }
}`),
      p('`OperationResult` and `OperationDataResult<T>` implement it, and therefore so do `ErrorResult` and `ErrorDataResult<T>`.'),
    ),
    section(
      'build',
      'Building one',
      cs(`using ResultHandler.Core.Base;

var fieldErrors = new Dictionary<string, IReadOnlyList<string>>
{
    ["Name"] = ["Name is required."],
    ["Price"] = ["Price must be greater than zero."],
};

OperationResult validation = OperationResult.Failure(fieldErrors); // 422 "Validation Failed"`),
      list(
        '`FieldErrors` and the flattened `Errors` are both populated from the same input.',
        '`Title` / `Status` default to `Validation Failed` / `422`.',
        'A `null` message list for a key is skipped instead of throwing.',
      ),
    ),
    section(
      'reading',
      'Reading from any result',
      cs(`IReadOnlyDictionary<string, IReadOnlyList<string>> errors = result.GetFieldErrors();`),
      tip('`GetFieldErrors()` works on **any** `IOperationResult`: it returns the field errors when the result implements `IHasFieldErrors`, otherwise an empty dictionary. No casting needed.'),
    ),
    section(
      'behavior',
      'FluentValidation behavior',
      cs(`public sealed class FieldValidationBehavior<TRequest, TResponse>(IEnumerable<IValidator<TRequest>> validators)
    : IPipelineBehavior<TRequest, TResponse>
    where TRequest : IRequest<TResponse>
    where TResponse : IOperationResult, IResultFailureFactory<TResponse>
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
}`),
    ),
    section(
      'propagation',
      'Propagation',
      table(
        ['API', 'What happens to FieldErrors'],
        ['`Map` / `Bind` / `ToErrorDataResult<T>()`', 'Carried through to the new result type'],
        ['`Result.Combine(...)`', 'Merged by key across every failure'],
        ['`ToProblemDetails()`', 'Emitted as the `fieldErrors` extension when non-empty'],
        ['`JsonSerializer`', 'Always emitted as `fieldErrors` (`{}` when empty)'],
      ),
    ),
  ],
};

export const SERIALIZATION: DocPage = {
  slug: 'serialization',
  title: 'Serialization & AOT',
  icon: 'bi-braces',
  summary: 'Stable JSON contract, numeric status codes, round-tripping and source-generated contexts.',
  sections: [
    section(
      'contract',
      'A stable JSON contract',
      cs(`JsonSerializer.Serialize(Result.NotFound<ProductDto>("Product 42 does not exist."));`),
      json(`{
  "resultData": null,
  "isSuccessful": false,
  "statusCode": 404,
  "statusMessage": "Not Found",
  "detail": "Product 42 does not exist.",
  "errors": [],
  "fieldErrors": {}
}`),
      list(
        'Property names are fixed by `[JsonPropertyName]`, so your naming policy cannot rename them.',
        '`statusCode` is always the numeric HTTP code, never the enum name.',
        '`detail` is omitted when null; `errors` and `fieldErrors` are always present.',
      ),
    ),
    section(
      'roundtrip',
      'Round-tripping',
      p('Results have `[JsonConstructor]`s, so another .NET process (a worker, a MAUI app, a Blazor client) can deserialize the envelope straight back into a result:'),
      cs(`var result = await http.GetFromJsonAsync<OperationDataResult<ProductDto>>("api/products/42/envelope");`),
      note('`statusCode` is read from a number **or** a numeric string (`"404"`), and unknown codes fall back to the generic status of their class.'),
    ),
    section(
      'converter',
      'Reuse ResultStatusJsonConverter in your own types',
      p('`ResultStatusJsonConverter` (`ResultHandler.Serialization`) is public. Put it on any of your own `ResultStatus` properties to get the same contract: written as the numeric HTTP code, read from a number or a numeric string.'),
      cs(`using System.Text.Json.Serialization;
using ResultHandler.Core.Enums;
using ResultHandler.Serialization;

public sealed record AuditEntry(
    string Operation,
    [property: JsonConverter(typeof(ResultStatusJsonConverter))] ResultStatus Outcome);

// {"operation":"product.delete","outcome":404}`),
      tip('Or register it once for every `ResultStatus` in your app: `options.Converters.Add(new ResultStatusJsonConverter());`.'),
    ),
    section(
      'aot',
      'Native AOT / trimming',
      p('The constructors that carry `errors` and `fieldErrors` are public precisely so a source-generated `JsonSerializerContext` can call them:'),
      cs(`[JsonSerializable(typeof(OperationResult))]
[JsonSerializable(typeof(OperationDataResult<ProductDto>))]
internal sealed partial class AppJsonContext : JsonSerializerContext;

builder.Services.ConfigureHttpJsonOptions(o => o.SerializerOptions.TypeInfoResolverChain.Insert(0, AppJsonContext.Default));`),
    ),
  ],
};

export const EQUALITY: DocPage = {
  slug: 'equality-debugging',
  title: 'Equality & debugging',
  icon: 'bi-bug',
  summary: 'Structural equality, stable hash codes and a readable ToString().',
  sections: [
    section(
      'equality',
      'Value equality',
      cs(`Result.NotFound("x") == Result.NotFound("x");       // false: reference types
Result.NotFound("x").Equals(Result.NotFound("x"));  // true: structural`),
      p('`Equals` compares status, success flag, title, detail, errors and field errors. `GetHashCode` hashes field errors **order-independently**, so dictionaries built in a different insertion order still agree.'),
      table(
        ['Rule', 'Consequence'],
        ['Runtime types must match', 'An `ErrorResult` never equals an `OperationResult` with the same values'],
        ['Data results also compare `Data`', 'Uses `EqualityComparer<T>.Default`, so make DTOs `record`s to get value semantics'],
        ['Errors compare by sequence', 'Order matters for `Errors`, but not for `FieldErrors` keys'],
      ),
      tip('Structural equality makes assertions trivial: `Assert.Equal<IOperationResult>(Result.NotFound("x"), actual)`.'),
    ),
    section(
      'tostring',
      'Readable ToString()',
      cs(`Result.NotFound("x").ToString(); // "NotFound (404): Not Found"`),
      note('Great in the debugger, in log messages and in test failure output.'),
    ),
  ],
};
