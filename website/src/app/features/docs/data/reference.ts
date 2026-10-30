import { DocPage } from '../domain/doc-page';
import { cs, good, list, note, p, section, table, tip, warn } from './builders';

export const MIGRATION_V12: DocPage = {
  slug: 'migration-v12',
  title: 'Migrating to v12',
  icon: 'bi-arrow-up-circle',
  summary: 'Members removed in v12 and what to use instead.',
  sections: [
    section(
      'removed',
      'Removed members',
      p('v12 **removes** every member v11 marked `[Obsolete]`. There is no forwarding shim.'),
      table(
        ['Removed in v12', 'Use instead'],
        ['`StatusMessage`', '`Title`'],
        ['`StatusCode: HttpStatusCode`', '`Status: ResultStatus`'],
        ['`ResultData`', '`Data`'],
        ['`HttpStatusCode`-based constructors', 'The `ResultStatus`-based constructor; convert with `httpStatusCode.ToResultStatus()`'],
      ),
      cs(`// Before (v11)
var legacy = new ErrorResult("Not found.", HttpStatusCode.NotFound);
Console.WriteLine(legacy.StatusMessage);

// After (v12)
var current = new ErrorResult("Not found.", HttpStatusCode.NotFound.ToResultStatus());
Console.WriteLine(current.Title);`),
    ),
    section(
      'behaviour',
      'Behaviour fix carried over from v11',
      warn('`ToActionResult<T>()` and `ToEnvelopedActionResult()` used to hard-code `200` for any success with a body. They now honour `Status`, so `Result.Created(...)` returns **201**. Check call sites that relied on the old behaviour.'),
    ),
    section(
      'net7',
      'net7.0 dropped in 12.1',
      note('12.1 targets `net8.0`, `net9.0` and `net10.0`. Upgrade your app’s target framework before taking this version.'),
    ),
  ],
};

export const MIGRATION_V13: DocPage = {
  slug: 'migration-v13',
  title: 'Migrating to v13',
  icon: 'bi-arrow-up-circle',
  badge: { label: 'New', tone: 'new' },
  summary: 'Facade factories return the base types. What breaks, how to fix it, and why everything must be rebuilt.',
  sections: [
    section(
      'what-changed',
      'What changed',
      p('Every `Result` facade factory now returns the common base type. The objects created are the same as before; only the **declared** return type changed.'),
      table(
        ['Factory', 'v12 return type', 'v13 return type'],
        ['`Result.Success()`, `Result.NoContent()`, `Result.Created()`…', '`SuccessResult`', '`OperationResult`'],
        ['`Result.Success<T>(data)`, `Result.Created<T>(data)`…', '`SuccessDataResult<T>`', '`OperationDataResult<T>`'],
        ['`Result.NotFound(detail)`, `Result.Conflict(detail)`, `Result.Invalid(...)`…', '`ErrorResult`', '`OperationResult`'],
        ['`Result.NotFound<T>(detail)`, `Result.Invalid<T>(...)`…', '`ErrorDataResult<T>`', '`OperationDataResult<T>`'],
      ),
      good('Success and failure branches now share one type, so `cond ? Result.Success(x) : Result.NotFound<T>("...")` and lambdas returning both compile without casts. See [the facade](/docs/result-facade#return-types).'),
    ),
    section(
      'source',
      'Source changes',
      p('Only code that **declares** a facade result as a concrete subclass breaks. Everything that uses `var`, the interfaces, or passes the result straight on keeps compiling.'),
      table(
        ['Breaks in v13', 'Fix'],
        ['`ErrorResult e = Result.Conflict("...");`', '`OperationResult e = ...;` or `var e = ...;`'],
        ['`SuccessDataResult<T> s = Result.Success(x);`', '`OperationDataResult<T> s = ...;`'],
        ['`public ErrorResult? Validate(...) => ... Result.Invalid(...)`', 'return `OperationResult?` or `IOperationResult?`'],
        ['`Func<string, ErrorResult> f = Result.Conflict;`', '`Func<string, OperationResult> f = Result.Conflict;`'],
        ['`(ErrorResult)Result.Combine(a, b)`', 'drop the cast; `Combine` already returns `IOperationResult`'],
      ),
      cs(`// v12
ErrorResult? Validate(CreateProductRequest request)
    => string.IsNullOrEmpty(request.Name) ? Result.Invalid("Name is required.") : null;

// v13
OperationResult? Validate(CreateProductRequest request)
    => string.IsNullOrEmpty(request.Name) ? Result.Invalid("Name is required.") : null;`),
      tip('Search your solution for the four concrete names next to `= Result.` and inside `Func<`: `ErrorResult`, `ErrorDataResult<`, `SuccessResult`, `SuccessDataResult<`. The [v13 upgrade checker](/migrate) does this for pasted code.'),
    ),
    section(
      'binary',
      'Rebuild everything that references the library',
      warn('The return type is part of a method signature, so v13 is **binary** breaking. An assembly compiled against v12 (your own shared library, or another NuGet package built on this one) fails at runtime with `MissingMethodException` until it is rebuilt against v13. Upgrade and republish those packages together.'),
    ),
    section(
      'runtime',
      'What does not change',
      list(
        'The runtime objects: `Result.Success(x)` is still a `SuccessDataResult<T>`, so `is` checks and pattern matching behave as before.',
        'JSON shape, equality, `ToString()`.',
        'ASP.NET Core mapping: status codes, Problem Details and envelopes are identical.',
        '`ResultFailureFactory` and `IResultFailureFactory<TSelf>`.',
      ),
    ),
    section(
      'overloads',
      'Created and Accepted',
      p('`Result.Created` and `Result.Accepted` became explicit overload pairs instead of an optional `title` parameter. Every existing call compiles unchanged, including `title:` named arguments, and the parameterless forms can now be used as method groups (`Func<OperationResult> f = Result.Created;`).'),
    ),
    section(
      'new',
      'New in 13.x',
      list(
        'Async operators for concrete `Task<OperationDataResult<T>>` / `Task<OperationResult>` sources, e.g. MediatR `Send` (13.0). See [async composition](/docs/async#services).',
        '`Bind` / `BindAsync` into a result with no data, e.g. a delete command (13.1). See [binding into commands](/docs/async#commands).',
      ),
    ),
  ],
};

export const COMPARISON: DocPage = {
  slug: 'comparison',
  title: 'Compared to alternatives',
  icon: 'bi-columns-gap',
  summary: 'How ResponseResultHandler compares with throwing exceptions and with other .NET result libraries, including where it is not the best fit.',
  sections: [
    section(
      'exceptions',
      'Results vs. exceptions',
      table(
        ['', 'Throwing exceptions', 'Returning results'],
        ['Expected failures (not found, conflict, invalid input)', 'invisible in the signature; callers must know what to catch', 'part of the return type; the compiler reminds callers'],
        ['Cost (see below)', 'stack capture and unwinding on every failure', 'an ordinary object allocation'],
        ['HTTP mapping', 'exception filter or middleware per exception type', '`ToResult()` / `ToActionResult()`, one line'],
        ['Collecting every validation error', 'awkward; the first throw ends the method', '`Result.Combine`, field errors'],
        ['Real bugs and infrastructure failures', 'the right tool', 'still throw these; results are for expected outcomes'],
      ),
      tip('The rule of thumb: return a result when a caller could reasonably handle the outcome, throw when it indicates a bug. See [best practices](/docs/best-practices).'),
    ),
    section(
      'performance',
      'What a failure costs',
      p('A BenchmarkDotNet run of the same "product not found" path, once as throw + catch and once as a returned `Result.NotFound`:'),
      table(
        ['Failure path', 'Median time', 'Allocated'],
        ['`throw` + `catch` a custom exception', '~1,200 ns', '400 B'],
        ['`return Result.NotFound<Product>(...)` + `IsSuccessful` check', '~20 ns', '144 B'],
      ),
      cs(`[MemoryDiagnoser]
public class NotFoundBenchmarks
{
    [Benchmark(Baseline = true)]
    public string Exception()
    {
        try { return FindOrThrow(42).Name; }
        catch (ProductNotFoundException ex) { return ex.Message; }
    }

    [Benchmark]
    public string ResultPattern()
    {
        var result = Find(42); // Result.NotFound<Product>($"Product {id} does not exist.")
        return result.IsSuccessful ? result.Data.Name : result.Detail ?? result.Title;
    }
}`, 'Benchmark'),
      note('Measured with BenchmarkDotNet 0.15 on .NET 10, x64, Windows 11. The exception timings were noisy (multimodal), so the median is shown; expect a different absolute number on your machine, but the same order of magnitude.'),
      tip('Keep it in proportion: a microsecond does not matter next to a database round-trip. The real gains are that expected failures stay visible in the signature, and that a burst of invalid requests does not turn into a burst of exceptions.'),
    ),
    section(
      'libraries',
      'Other result libraries',
      p('All of these are solid choices. The table focuses on what differs, as far as each project documents it at the time of writing; check their docs for the current state.'),
      table(
        ['', 'ResponseResultHandler', 'FluentResults', 'ErrorOr', 'Ardalis.Result', 'CSharpFunctionalExtensions'],
        ['Focus', 'HTTP-shaped outcomes', 'rich error/reason trees', 'discriminated union of value or errors', 'HTTP-shaped outcomes', 'functional programming toolkit'],
        ['Status model', '62 statuses (every standard HTTP code) as `ResultStatus`', 'none built in', 'a handful of error types', 'a curated set of statuses', 'none built in'],
        ['RFC 9457 Problem Details', 'built in, per-status `type` URIs', 'you map it', 'you map it', 'via its ASP.NET Core package', 'you map it'],
        ['Per-field validation errors', 'built in (`fieldErrors`)', 'via metadata', 'via error metadata', 'built in', 'you model it'],
        ['Generic pipeline failures without exceptions', '`IResultFailureFactory<TSelf>` (static abstract)', 'you write it', 'implicit conversions help', 'you write it', 'you write it'],
        ['Composition (`Map`, `Bind`, `Ensure`, async)', 'yes, including concrete `Task` sources', 'yes', 'yes (`Then`, `Else`, `Match`)', 'limited', 'the most extensive'],
        ['Result type', 'class', 'class', 'struct', 'class', 'struct and class variants'],
        ['Dependencies', 'none (core); ASP.NET Core only for the web package', 'none', 'none', 'none (core)', 'none'],
      ),
    ),
    section(
      'when-not',
      'When another library may fit better',
      list(
        'You want a **struct** result for hot paths with zero allocation on success: look at ErrorOr or CSharpFunctionalExtensions.',
        'You need **nested reasons** and error hierarchies: FluentResults models that directly.',
        'You want a full **functional toolkit** (`Maybe`, value objects, many combinators): CSharpFunctionalExtensions.',
      ),
      p('Pick ResponseResultHandler when your results end up as HTTP responses and you want the status, the Problem Details body and per-field errors to come for free.'),
    ),
  ],
};

export const API_REFERENCE: DocPage = {
  slug: 'api-reference',
  title: 'API reference',
  icon: 'bi-list-columns-reverse',
  badge: { label: 'New', tone: 'new' },
  summary: 'Every public type and member of both packages, grouped by namespace.',
  sections: [
    section(
      'abstractions',
      'ResultHandler.Core.Abstractions',
      table(
        ['Type', 'Members'],
        ['`IOperationResult`', '`IsSuccessful`, `Status`, `Title`, `Detail`, `Errors`'],
        ['`IOperationResult<T>`', '`Data` (non-null when `IsSuccessful`)'],
        ['`IHasFieldErrors`', '`FieldErrors`'],
        ['`IResultFailureFactory<TSelf>`', 'static abstract `Failure(errors)`, `Failure(fieldErrors)`, `Failure(title, detail, status)`'],
        ['`FieldErrorsExtensions`', '`GetFieldErrors(this IOperationResult)`'],
      ),
      p('See [the core contract](/docs/core-contract), [field errors](/docs/field-errors) and [generic short-circuiting](/docs/generic-short-circuiting).'),
    ),
    section(
      'base',
      'ResultHandler.Core.Base',
      table(
        ['Type', 'Members'],
        ['`OperationResult`', 'constructors `(isSuccessful, status, title, detail?, errors?)` and `(…, errors, fieldErrors)`; static `Failure(errors)`, `Failure(fieldErrors)`, `Failure(title, detail, status)`; structural `Equals`, `GetHashCode`, `ToString`'],
        ['`OperationDataResult<T>`', 'same, with a leading `data` argument; adds `Data`'],
      ),
    ),
    section(
      'implementations',
      'ResultHandler.Implementations.Success / .Error',
      table(
        ['Type', 'Constructors'],
        ['`SuccessResult`', '`()`, `(title)`, `(title, status)`'],
        ['`SuccessDataResult<T>`', '`(data)`, `(data, title)`, `(data, title, status)`'],
        ['`ErrorResult`', '`()`, `(title, status)`, `(title, status, detail)`, `(title, status, errors)`, `(title, status, fieldErrors)`'],
        ['`ErrorDataResult<T>`', '`()`, `(data)`, `(title, status)`, `(title, status, detail)`, `(title, status, errors)`, `(title, status, fieldErrors)`, `(data, title, status)`, `(data, title, status, detail)`'],
      ),
      p('See [building results directly](/docs/building-results).'),
    ),
    section(
      'facade',
      'ResultHandler.Facade',
      table(
        ['Member', 'Notes'],
        ['`Result.<Status>(…)` / `Result.<Status><T>(…)`', 'one pair per status; browse them in the [status explorer](/statuses)'],
        ['return types', 'every factory returns `OperationResult` or `OperationDataResult<T>` (since 13.0)'],
        ['`Result.Success()`, `Success(title)`, `Success<T>(data)`, `Success<T>(data, title)`', '200'],
        ['`Result.Created()`, `Created(title)`, `Created<T>(data)`, `Created<T>(data, title)`; same four for `Accepted`', '201 / 202'],
        ['`Result.Invalid(params string[])`, `Invalid(IReadOnlyList<string>)` and `<T>` twins', '422 "Validation Failed"'],
        ['`Result.Failure(title, detail, status)`, `Failure<T>(…)`, `Failure<T>(data, title, detail, status)`', 'escape hatch'],
        ['`Result.Combine(params IOperationResult[])`, `Combine(IEnumerable<IOperationResult>)`', 'aggregate outcomes'],
        ['`Result.Combine<T1, T2>` … `Combine<T1, T2, T3, T4>`', 'aggregate outcomes and data into a tuple'],
      ),
    ),
    section(
      'functional',
      'ResultHandler.Functional',
      table(
        ['Type', 'Members'],
        ['`ResultExtensions` (sync)', '`Match`, `OnSuccess`, `OnFailure`, `Map`, `Bind` (data and no-data binders), `Ensure` (2 overloads), `ToErrorDataResult<T>`'],
        ['`ResultExtensions` (async)', '`MatchAsync`, `OnSuccessAsync`, `OnFailureAsync`, `MapAsync`, `BindAsync`, `EnsureAsync`, each for `Task<IOperationResult…>`, concrete `Task<OperationDataResult<T>>` / `Task<OperationResult>` and plain sources; `BindAsync` also binds into no-data results'],
        ['`ResultFailureFactory`', 'one `X<TSelf>(detail)` per 4xx/5xx status, e.g. `NotFound<TSelf>`, `TooManyRequests<TSelf>`'],
      ),
      p('See [functional composition](/docs/functional) and [async composition](/docs/async).'),
    ),
    section(
      'mapping',
      'ResultHandler.Mapping and .Serialization',
      table(
        ['Type', 'Members'],
        ['`ResultStatusExtensions`', '`ToHttpStatusCode(this ResultStatus)`'],
        ['`HttpStatusCodeExtensions`', '`ToResultStatus(this HttpStatusCode)`, `ToResultStatus(this int)`'],
        ['`ResultStatusJsonConverter`', '`JsonConverter<ResultStatus>` writing the numeric code'],
      ),
    ),
    section(
      'aspnetcore',
      'ResultHandler.AspNetCore.Extensions',
      table(
        ['Member', 'Returns'],
        ['`ToActionResult()`, `ToActionResult<T>()`, `ToEnvelopedActionResult()`', '`IActionResult` (MVC)'],
        ['`ToResult()`, `ToResult<T>()`, `ToEnvelopedResult()`, `ToProblemResult()`', '`IResult` (Minimal APIs)'],
        ['`ToProblemDetails()`', '`ProblemDetails`'],
      ),
      note('All of them live in the static class `AspNetCoreResultExtensions` and take an optional `HttpContext` that fills the Problem Details `instance`.'),
    ),
  ],
};

export const FAQ: DocPage = {
  slug: 'faq',
  title: 'FAQ',
  icon: 'bi-question-circle',
  summary: 'Short answers to the questions people ask most.',
  sections: [
    section('aspnet', 'Does the core package depend on ASP.NET Core?', p('No. `ResponseResultHandler` depends only on the BCL. The ASP.NET Core integration lives in the separate `ResponseResultHandler.AspNetCore` package.')),
    section('exceptions', 'Should I stop throwing exceptions entirely?', p('No. Throw for bugs and infrastructure failures; return results for expected business outcomes. See [best practices](/docs/best-practices).')),
    section('equals', 'Why is `==` false for two identical results?', p('Results are classes, so `==` compares references. Use `.Equals()` for structural comparison. See [equality](/docs/equality-debugging).')),
    section('mediatr-required', 'Do I need MediatR?', p('No. Neither package references MediatR, FluentValidation or any other library: results, composition and the async chains work in plain services, repositories, controllers and background jobs. MediatR is one integration the docs show because it is common, not a requirement. See [async composition without MediatR](/docs/async#services).')),
    section('mediatr', 'Can I use it with MediatR?', p('Yes. Declare `OperationDataResult<T>` as the response type and your behaviors can return failures generically through `IResultFailureFactory<TSelf>`; the same interface serves any generic pipeline, such as your own decorators. See [generic short-circuiting](/docs/generic-short-circuiting).')),
    section('aot', 'Is it Native AOT friendly?', p('The serialization constructors are public so a source-generated `JsonSerializerContext` can use them. See [serialization](/docs/serialization).')),
    section('status-200', 'Why does my Created result return 201 now?', p('Since v11 the ASP.NET Core adapters honour the result’s `Status`. That is the correct behaviour; see [migration](/docs/migration-v12).')),
    section('license', 'What is the license?', p('MIT. Both packages ship with SourceLink and symbol packages (`.snupkg`), so you can step into the library while debugging.')),
  ],
};
