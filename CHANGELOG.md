# Changelog

All notable changes to `ResponseResultHandler` and `ResponseResultHandler.AspNetCore` are documented
here. Format follows [Keep a Changelog](https://keepachangelog.com/en/1.1.0/); this project does not
yet commit to strict [SemVer](https://semver.org/) pre-1.0-style guarantees, but breaking changes are
always called out explicitly below.

## [15.0.0]

Everything an ASP.NET Core API needs around results, without MediatR: exceptions, async endpoints,
request validation and accurate OpenAPI documents.

### Changed
- **Breaking (wire):** the `ToEnvelopedResult`/`ToEnvelopedActionResult` success body no longer contains
  `errors` and `fieldErrors`. They were always empty on success. The body is now
  `{ "resultData", "isSuccessful", "statusCode", "statusMessage" }`, plus `detail` and `code` when set,
  written as `ResultEnvelope<T>`/`ResultEnvelope` (`ResultHandler.AspNetCore.Envelopes`). Failures are
  unchanged (Problem Details). Serializing a result object yourself is unchanged.
- `ProducesEnveloped<T>()`/`ProducesEnveloped()` declare `ResultEnvelope<T>`/`ResultEnvelope`, the type the
  body is written as, so `statusCode` is documented as an integer. The schema transformer the README used
  to suggest for it is no longer needed.
- `AddResultProblemDetails()` adds its customization as a post-configure step: it wraps any
  `CustomizeProblemDetails`, wherever it is set, and is safe to call more than once.

### Added
- `AddResultExceptionHandler(options => options.Map<TException>(...))`: an `IExceptionHandler` for
  `app.UseExceptionHandler()` that writes exceptions as the Problem Details of the failed result they map
  to. The closest mapped base type wins; `BadHttpRequestException` keeps its status code; everything else
  is a 500 whose message reaches the client only in Development (`ExposeExceptionMessages`). A mapping that
  returns a success is rejected.
- `ToResultAsync()`, `ToEnvelopedResultAsync()`, `ToActionResultAsync()` and `ToEnvelopedActionResultAsync()`
  for `Task<IOperationResult>`, `Task<OperationResult>`, `Task<IOperationResult<T>>` and
  `Task<OperationDataResult<T>>`.
- New package `ResponseResultHandler.FluentValidation`: `ValidationResult.ToFieldErrors()`,
  `ToOperationResult()` and `ToFailure<TResult>()`; `WithResultValidation()` validates Minimal API
  arguments for an endpoint or a route group; `AddResultValidation()` does the same for MVC actions. Invalid
  requests get 422 validation Problem Details. Depends on FluentValidation 12.0.0 or later.

### Migration
- Clients that read `errors`/`fieldErrors` from a success envelope can drop them; they were always empty.
- Tests that assert `JsonHttpResult<IOperationResult>` or `Assert.Same(result, objectResult.Value)` for an
  enveloped success should assert `JsonHttpResult<ResultEnvelope>` and read `ResultEnvelope<T>.ResultData`.
- If you added a schema transformer for `ResultStatus` only for the envelope, you can remove it.
- Package validation compares against 14.0.1; no public API was removed.

## [14.0.1]

### Fixed
- MVC: a validation failure returned with `ToActionResult`/`ToEnvelopedActionResult` from an
  `[ApiController]` lost its `"errors"` map when Problem Details services were registered
  (`AddProblemDetails`/`AddResultProblemDetails`), because MVC's `IProblemDetailsService` writer rebuilds
  the body as a plain `ProblemDetails`. MVC failures now apply `CustomizeProblemDetails` and are written
  through normal content negotiation, the same path as MVC's own `ValidationProblem()`. Minimal API
  results are unchanged.

## [14.0.0]

Failed responses now follow ASP.NET Core's own Problem Details conventions end to end, so clients,
Scalar/Swagger UI and generated code see one shape for every error.

### Changed
- **Breaking (wire):** validation failures are written as `HttpValidationProblemDetails`. `"errors"` is
  now a map from field name to messages, the same shape ASP.NET Core uses for `ValidationProblem`, and
  the `"fieldErrors"` extension is gone. Messages in `Errors` that don't belong to a field go under the
  `""` key (ASP.NET Core's model-level key); field messages are no longer written twice.
- Failures from `ToResult`/`ToProblemResult`/`ToEnvelopedResult` and the `ToActionResult` family are
  written through `IProblemDetailsService` when it is registered, so `CustomizeProblemDetails` applies
  to them. Previously they were serialized directly and skipped it.
- Every failed response gets `instance` and a `traceId` extension
  (`Activity.Current?.Id ?? HttpContext.TraceIdentifier`) when it is written, on every target framework,
  whether or not an `HttpContext` was passed in.
- `ProducesResultProblems()` also documents `400` for endpoints that read a body, and describes the
  responses with `ResultProblemDetails` / `ResultValidationProblemDetails` instead of `ProblemDetails`,
  so OpenAPI documents show `code`, `traceId` and the `errors` map.
- **Breaking (API):** `IOperationResult` has a new `Code` member. Only code that implements the
  interface itself is affected.
- The `[JsonConstructor]` of `OperationResult` / `OperationDataResult<T>` is now the overload with a
  trailing `code` parameter; the previous overloads remain as ordinary constructors.

### Added
- `IOperationResult.Code` and `WithCode(code)` (`ResultHandler.Facade`): a machine-readable error code,
  written as the `"code"` Problem Details extension and as `code` in the result JSON, carried through
  `Map`/`Bind`/`ToErrorDataResult`. `Result.Combine` does not merge codes.
- `AddResultProblemDetails()`: `AddProblemDetails()` plus a customization that gives framework-produced
  problems (401/403, unknown routes, 405, 415, 429, unhandled exceptions) the same `type`, `title`,
  `instance` and `traceId` as failed results. Use with `app.UseStatusCodePages()`.
- `ProducesResultProblem(params int[] statusCodes)` to document endpoint-specific failures (404, 409, ...)
  with the same schemas.
- `ResultProblemDetails` and `ResultValidationProblemDetails` (`ResultHandler.AspNetCore.OpenApi`):
  OpenAPI schemas for the failure bodies.

### Migration
- Clients that read `"fieldErrors"` should read `"errors"` (same map). Clients that read `"errors"` as a
  list should read the `""` key of the map, or flatten its values.
- Tests that assert `JsonHttpResult<ProblemDetails>` or an exact `ObjectResult` for a failure should use
  `IValueHttpResult<ProblemDetails>` / `IStatusCodeHttpResult`, or `Assert.IsAssignableFrom<ObjectResult>`.
- Middleware that writes `ToProblemDetails(...)` with `WriteAsJsonAsync<ProblemDetails>` loses the
  `errors` map (it serializes the declared base type); write it with
  `await result.ToProblemResult().ExecuteAsync(httpContext)` instead.
- Binary: assemblies compiled against 13.x must be recompiled against 14.0. Package validation has no
  baseline during 14.0; set `PackageValidationBaselineVersion` to `14.0.0` once it is published.

## [13.3.0]

### Added
- `ProducesEnveloped<T>()` and `ProducesEnveloped()` endpoint extensions (`ResultHandler.AspNetCore.Extensions`):
  declare the success body that `ToEnvelopedResult` writes, `OperationDataResult<T>` for data results and
  `OperationResult` for results without data, as `application/json` with the given status (200 by default,
  pass 201 for creates). OpenAPI documents and generated clients then show the envelope (`resultData`,
  `isSuccessful`, `statusCode`, `statusMessage`, `errors`, `fieldErrors`) and the data type inside it.
  Together with `ProducesResultProblems()` an endpoint's success and failure responses are both documented.
  Bodyless statuses (204) are still declared with `Produces(204)`.

## [13.2.0]

### Added
- `ProducesResultProblems()` endpoint convention (`ResultHandler.AspNetCore.Extensions`): declares
  `application/problem+json` responses for a route group or endpoint so OpenAPI documents show the
  failures a result-returning endpoint can produce: `500` always, `401`/`403` when authorization is
  required (and not `AllowAnonymous`), `422` when the endpoint reads a request body. Statuses an
  endpoint already declares are not duplicated. Uses only standard endpoint metadata, so it adds no
  package dependency and works with `Microsoft.AspNetCore.OpenApi` and Swashbuckle.

## [13.1.0]

### Added
- `Bind`/`BindAsync` overloads whose binder returns a result with no data (`IOperationResult`, or a
  concrete `Task<OperationResult>` such as a MediatR command `Send`). A data result can now chain
  into a command in one expression; previously there was no overload for it, so callers had to await,
  check `IsSuccessful` and branch by hand. A failed source is re-projected into an `ErrorResult` with
  title, status, detail, errors and field errors intact. Existing overloads are unchanged and still
  win whenever the binder returns a data result. `Task<OperationDataResult<T>>` chained into
  `Task<OperationResult>` returns `Task<OperationResult>`; every other shape returns `IOperationResult`.

### Changed
- Package validation compares against `13.0.0` again (`PackageValidationBaselineVersion` in
  `Directory.Build.props`), so a publish build now fails on any breaking change to the 13.0 public API.
  No library code changed.

### Documentation
- NuGet package release notes (`PackageReleaseNotes` in `ResultHandler.csproj`) now describe 13.0; the
  13.0.0 package still showed the 12.1 notes.
- README §9: binding into a command that returns no data, and why a lambda returning only hand-built
  subclasses now resolves to the no-data overload.

## [13.0.0]

### Changed
- **Breaking:** every `Result` facade factory (`ResultHandler.Facade`) now returns the common base type:
  `OperationResult` instead of `SuccessResult`/`ErrorResult`, and `OperationDataResult<T>` instead of
  `SuccessDataResult<T>`/`ErrorDataResult<T>`. Success and failure branches now share one static type,
  so these compile without casts or explicit type arguments:
  - `cond ? Result.Success(x) : Result.NotFound<T>("...")` (previously CS0173);
  - lambdas that return both a success and a failure, e.g. in `Bind` (previously CS0411).

  The objects created are unchanged (`Result.Success(x)` is still a `SuccessDataResult<T>` at runtime),
  so `is`/pattern checks, serialization and ASP.NET Core mapping behave exactly as before.
  `ResultFailureFactory` is unaffected.
- `Result.Created`/`Result.Accepted` (and their `<T>` forms) no longer use an optional `title`
  parameter; each is now an explicit pair, `Created()`/`Created(title)` and
  `Created<T>(data)`/`Created<T>(data, title)`, matching `Result.Success`. Every existing call,
  including `title:` named arguments, compiles unchanged, and the parameterless forms can now be used
  as method groups (`Func<OperationResult> f = Result.Created;`).

### Added
- `MapAsync`/`BindAsync` overloads for `Task<OperationDataResult<T>>` sources and for binders returning
  `Task<OperationDataResult<TOut>>`. `Task<T>` is invariant, so chains that start from, or bind into, a
  MediatR-style `Send` (which returns the concrete `OperationDataResult<T>`) previously did not compile
  without a cast. The result type follows the source: concrete in, concrete out; an interface anywhere
  in the chain keeps it an interface. Existing overloads and their return types are unchanged.
- `MatchAsync`/`OnSuccessAsync`/`OnFailureAsync` overloads for `Task<OperationResult>` and
  `Task<OperationDataResult<T>>` sources, and `EnsureAsync` overloads for `Task<OperationDataResult<T>>`,
  so every async operator chains directly off a MediatR `Send`. Side-effect and guard overloads return
  the concrete source type.

### Migration
- Source: only code that stores a facade result in a variable, field, return type or delegate typed
  as a concrete subclass breaks, e.g. `ErrorResult e = Result.Conflict("...")` or
  `Func<string, ErrorResult> f = Result.Conflict`. Change the declared type to `OperationResult` /
  `OperationDataResult<T>` (or `IOperationResult` / `IOperationResult<T>`), or use `var`.
- Binary: the return types are part of the method signatures, so assemblies compiled against 12.x
  (for example another NuGet package built on this one) must be recompiled against 13.0.
- Package validation no longer compares against the 12.x baseline; set
  `PackageValidationBaselineVersion` to `13.0.0` once it is published.

### Documentation
- README §7: `Ensure(predicate, message)` puts the message in `Detail` and leaves `Errors` empty; the
  previous "same shape as `Result.Invalid`" wording was wrong. Behavior is unchanged.
- README §9: async lambdas that return both a success and a failure now bind without type arguments
  (facade change above plus the new concrete-task overloads).

## [12.1.33]

### Added
- `IHasFieldErrors` (`ResultHandler.Core.Abstractions`) — optional companion to `IOperationResult` for
  results carrying per-field validation errors, keyed by property name. `OperationResult` and
  `OperationDataResult<T>` (and therefore `ErrorResult`/`ErrorDataResult<T>`) implement it.
- `IResultFailureFactory<TSelf>.Failure(IReadOnlyDictionary<string, IReadOnlyList<string>>)` — lets
  generic infrastructure that only knows `TSelf` build a per-field failure without knowing the concrete
  result type; implemented by the same types as the rest of `IResultFailureFactory<TSelf>`. **Breaking**
  for any existing external implementer of `IResultFailureFactory<TSelf>` — see `CompatibilitySuppressions.xml`.
- `OperationResult.Failure(IReadOnlyDictionary<string, IReadOnlyList<string>>)` /
  `OperationDataResult<T>.Failure(...)` static factories, and matching `ErrorResult`/`ErrorDataResult<T>`
  constructors — build a failure from per-field messages; `Title`/`Status` default to
  `"Validation Failed"` / `422 Unprocessable Content`, and `Errors` is populated with the flattened
  messages alongside `FieldErrors` (see README §11).
- `ResponseResultHandler.AspNetCore`'s `ToProblemDetails()` adds a non-empty `FieldErrors` to the
  Problem Details body as a `"fieldErrors"` extension.
- `OperationResult`/`OperationDataResult<T>` constructor overloads that carry `errors` and
  `fieldErrors` independently — public (rather than internal) so a System.Text.Json
  source-generated `JsonSerializerContext` (the pattern Native AOT/trimmed consumers use) can still
  call them to round-trip `FieldErrors`.
- `FieldErrorsExtensions.GetFieldErrors(this IOperationResult)` (`ResultHandler.Core.Abstractions`) —
  returns a result's `FieldErrors` if it implements `IHasFieldErrors` and has any, otherwise empty.

### Fixed
- `Map`/`Bind`/`ToErrorDataResult<T>()` (`ResultHandler.Functional`) now carry `FieldErrors` through
  when re-projecting a failure into a different result type, instead of silently dropping them, and
  preserve the original `Errors` list when it differs from the flattened `FieldErrors` values.
- `Result.Combine(...)` (`ResultHandler.Facade`) now merges every combined failure's `FieldErrors` by
  key, instead of only merging the flat `Errors` list and dropping field attribution.
- `OperationResult.Failure(fieldErrors)` / `OperationDataResult<T>.Failure(fieldErrors)` and the
  matching `ErrorResult`/`ErrorDataResult<T>` constructors no longer throw when a `fieldErrors`
  dictionary contains a `null` value — that field's messages are skipped instead.
- `OperationResult.GetHashCode()` now hashes `FieldErrors` order-independently, matching `Equals()` —
  two results built from field-error dictionaries with the same content in a different key insertion
  order previously could compare equal while returning different hash codes.

### Removed (breaking for net7.0 consumers)
- `net7.0` support — net7.0 reached end-of-life and current tooling (test SDK, xunit v3) has dropped
  it too. The library now targets `net8.0`/`net9.0`/`net10.0` only; consumers still on net7.0 must
  upgrade their app's target framework before taking this version.

## [12.0.0]

### Added
- `Result.Combine(params IOperationResult[])` / `Result.Combine(IEnumerable<IOperationResult>)`
  (`ResultHandler.Facade`) — merges independent checks into one outcome, collecting *every* failing
  result's messages instead of stopping at the first (see README §8).
- `Result.Combine<T1, T2>` / `<T1, T2, T3>` / `<T1, T2, T3, T4>` overloads — same merge behavior as
  above, but for `IOperationResult<T>`s: on success, returns every payload as a named tuple.
- `ResultExtensions.Ensure<T>` / `EnsureAsync<T>` (`ResultHandler.Functional`) — guard-clause-style
  helper that turns an already-successful result into a failure when a business-rule predicate
  rejects the data (see README §7).
- Async composition: `MatchAsync`, `OnSuccessAsync`, `OnFailureAsync`, `MapAsync`, `BindAsync`
  (`ResultHandler.Functional`) — `Task`-aware counterparts of the sync composition helpers, each in
  the three shapes needed to chain across `await` without an intermediate one (see README §9).

### Changed
- Enforced `StyleCop.Analyzers` and `SonarAnalyzer.CSharp` across all three projects; the codebase
  builds warning-free under both. Suppressed rules are limited to ones that conflict with an existing,
  deliberate convention (documented per-rule in `.editorconfig`) — nothing is silenced to hide a real
  issue.
- Centralized previously-duplicated literals (`"Operation completed successfully."`,
  `"An error occurred."`, `"Validation Failed"`, and the 3xx redirect message templates) behind single
  named constants (`OperationResultDefaults`, `ResultTitles`) so the concrete/generic overload pairs
  and legacy constructors can never drift apart.

### Removed (breaking)
- `StatusMessage`, `StatusCode: HttpStatusCode`, and `ResultData` — the members deprecated in
  `[11.0.0]` are gone, no forwarding shim. Use `Title`, `Status: ResultStatus`, and `Data`.
- Every `HttpStatusCode`-based constructor on `OperationResult`, `OperationDataResult<T>`,
  `SuccessResult`, `SuccessDataResult<T>`, `ErrorResult`, and `ErrorDataResult<T>` — use the
  `ResultStatus`-based constructor instead (convert an `HttpStatusCode` via
  `HttpStatusCodeExtensions.ToResultStatus()` if needed). See README §14 "Migrating to v12" for the
  full replacement table.

## [11.0.0]

### Added
- `ResultStatus` enum (`ResultHandler.Core.Enums`) — decouples the library from
  `System.Net.HttpStatusCode`; convert either direction via `HttpStatusCodeExtensions`/`ResultStatusExtensions`.
- `Result` static factory facade (`ResultHandler.Facade`) — one named factory pair per `ResultStatus`.
- RFC 9457 Problem Details support in `ResponseResultHandler.AspNetCore`, including per-status
  `ProblemDetails.Type` URIs and Minimal API `IResult` adapters alongside the existing MVC
  `IActionResult` ones.
- Custom `System.Text.Json` serialization with fixed property names, independent of the consumer's
  `JsonSerializerOptions` naming policy.
- `IResultFailureFactory<TSelf>` / `ResultFailureFactory` (`ResultHandler.Functional`) — lets generic
  infrastructure (MediatR pipeline behaviors, gRPC interceptors) short-circuit without knowing the
  concrete result type.

### Fixed
- `ToActionResult<T>()` / `ToEnvelopedActionResult()` used to hardcode HTTP `200` for any successful
  result carrying a body, discarding the result's actual `Status` (so `Result.Created(...)` came back
  as `200`, not `201`). Both now honor `Status` correctly — see README §14 if you relied on the old
  behavior.

### Deprecated
- `StatusMessage`, `StatusCode: HttpStatusCode`, and `ResultData` — marked `[Obsolete]`, forward into
  the new `Title`/`Status: ResultStatus`/`Data` members, and are kept indefinitely for backward
  compatibility (see README §14).
