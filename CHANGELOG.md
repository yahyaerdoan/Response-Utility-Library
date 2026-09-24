# Changelog

All notable changes to `ResponseResultHandler` and `ResponseResultHandler.AspNetCore` are documented
here. Format follows [Keep a Changelog](https://keepachangelog.com/en/1.1.0/); this project does not
yet commit to strict [SemVer](https://semver.org/) pre-1.0-style guarantees, but breaking changes are
always called out explicitly below.

## [Unreleased]

### Changed
- Package validation compares against `13.0.0` again (`PackageValidationBaselineVersion` in
  `Directory.Build.props`), so a publish build now fails on any breaking change to the 13.0 public API.
  No library code changed.

### Documentation
- NuGet package release notes (`PackageReleaseNotes` in `ResultHandler.csproj`) now describe 13.0; the
  13.0.0 package still showed the 12.1 notes.

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
