export type ChangeKind = 'Added' | 'Changed' | 'Fixed' | 'Deprecated' | 'Removed';

export interface ChangeGroup {
  readonly kind: ChangeKind;
  readonly breaking?: boolean;
  readonly items: readonly string[];
}

export interface Release {
  readonly version: string;
  readonly headline: string;
  readonly summary: string;
  readonly upgrade: readonly string[];
  readonly groups: readonly ChangeGroup[];
}

export const CHANGE_ICONS: Readonly<Record<ChangeKind, string>> = {
  Added: 'bi-plus-circle',
  Changed: 'bi-arrow-repeat',
  Fixed: 'bi-wrench-adjustable',
  Deprecated: 'bi-hourglass-split',
  Removed: 'bi-dash-circle',
};

export function findRelease(version: string): Release | undefined {
  return RELEASES.find(release => release.version === version);
}

export const RELEASES: readonly Release[] = [
  {
    version: '13.2',
    headline: 'Document every error response in one call',
    summary: 'A route-group convention declares the Problem Details responses a result-returning endpoint can produce, so OpenAPI documents stop hiding failures.',
    upgrade: [
      'Update both packages to 13.2. No code changes needed.',
      'Optional: add `.ProducesResultProblems()` to your API route group. See [Minimal APIs](/docs/minimal-apis#problems-convention).',
    ],
    groups: [
      {
        kind: 'Added',
        items: [
          '`ProducesResultProblems()` endpoint convention: `500` always, `401` / `403` for endpoints that require authorization, `422` for endpoints that read a body, all as `application/problem+json` with the `ProblemDetails` schema.',
          'Statuses an endpoint already declares are not duplicated; no new package dependency; works with `Microsoft.AspNetCore.OpenApi` and Swashbuckle.',
        ],
      },
    ],
  },
  {
    version: '13.1',
    headline: 'Bind into commands that return no data',
    summary: 'A data result can chain straight into a command such as a delete or a status change, so a whole MediatR endpoint fits in one expression.',
    upgrade: [
      'Update both packages to 13.1. Nothing else to change: existing calls resolve exactly as before.',
      'Optional: replace hand-written "load, check, send" blocks with one [async chain](/docs/async#commands).',
    ],
    groups: [
      {
        kind: 'Added',
        items: [
          '`Bind` / `BindAsync` overloads whose binder returns a result with no data (`IOperationResult`, or `Task<OperationResult>` such as a MediatR command `Send`).',
          'A failed source is re-projected into an `ErrorResult` with its title, status, detail, errors and field errors intact.',
          '`Task<OperationDataResult<T>>` chained into `Task<OperationResult>` returns `Task<OperationResult>`; every other shape returns `IOperationResult`.',
        ],
      },
      {
        kind: 'Changed',
        items: ['Package validation compares against `13.0.0` again, so a publish build fails on any breaking change to the 13.0 API.'],
      },
    ],
  },
  {
    version: '13.0',
    headline: 'One return type per facade call, and async chains off MediatR',
    summary: 'Success and failure factories now share a static type, and every async operator chains directly off a concrete `Task<OperationDataResult<T>>` or `Task<OperationResult>`.',
    upgrade: [
      'Update both packages to 13.0 and rebuild every assembly that references them, including other packages built on this one (the change is binary breaking).',
      'Read [Migrating to v13](/docs/migration-v13) for the full list, or paste code into the [upgrade checker](/migrate).',
      'Change variables, return types or delegates declared as a concrete subclass to the base type or `var`, e.g. `ErrorResult e = Result.Conflict("...")` becomes `OperationResult e = ...`, and `Func<string, ErrorResult>` becomes `Func<string, OperationResult>`.',
      'Runtime objects are unchanged, so `is` checks, serialization and ASP.NET Core mapping need no changes.',
    ],
    groups: [
      {
        kind: 'Changed',
        breaking: true,
        items: [
          'Every `Result` facade factory returns `OperationResult` (was `SuccessResult` / `ErrorResult`) or `OperationDataResult<T>` (was `SuccessDataResult<T>` / `ErrorDataResult<T>`).',
          '`cond ? Result.Success(x) : Result.NotFound<T>(...)` and lambdas returning both a success and a failure now compile without casts or type arguments.',
          '`Result.Created` / `Result.Accepted` are explicit overload pairs instead of an optional `title` parameter; every existing call still compiles.',
        ],
      },
      {
        kind: 'Added',
        items: [
          '`MapAsync` / `BindAsync` overloads for `Task<OperationDataResult<T>>` sources and for binders returning `Task<OperationDataResult<TOut>>`, so two MediatR `Send` calls chain without a cast.',
          '`MatchAsync`, `OnSuccessAsync`, `OnFailureAsync` for `Task<OperationResult>` and `Task<OperationDataResult<T>>`; `EnsureAsync` for `Task<OperationDataResult<T>>`.',
        ],
      },
      {
        kind: 'Fixed',
        items: ['Docs: `Ensure(predicate, message)` puts the message in `Detail` and leaves `Errors` empty; it was described as the same shape as `Result.Invalid`.'],
      },
    ],
  },
  {
    version: '12.1',
    headline: 'Per-field validation errors',
    summary: 'Results can now carry validation messages keyed by property name, and every API that re-projects or combines results keeps them.',
    upgrade: [
      'Update both packages to 12.1 (use the **Install** button in the header for the exact version).',
      'Make sure every project targets `net8.0` or later; `net7.0` is no longer supported.',
      'If you implement `IResultFailureFactory<TSelf>` yourself, add the new static `Failure(IReadOnlyDictionary<string, IReadOnlyList<string>>)` member.',
      'Optional: move form validation to [field errors](/docs/field-errors) so clients can highlight individual inputs.',
    ],
    groups: [
      {
        kind: 'Added',
        items: [
          '`IHasFieldErrors`: per-field validation messages keyed by property name, implemented by `OperationResult` / `OperationDataResult<T>`.',
          '`IResultFailureFactory<TSelf>.Failure(fieldErrors)` and `OperationResult.Failure(fieldErrors)` factories (default `422 Validation Failed`).',
          '`ToProblemDetails()` emits a non-empty `FieldErrors` as the `fieldErrors` extension.',
          'Public constructors carrying `errors` and `fieldErrors`, usable from source-generated `JsonSerializerContext`s.',
          '`GetFieldErrors()` extension for any `IOperationResult`.',
        ],
      },
      {
        kind: 'Fixed',
        items: [
          '`Map` / `Bind` / `ToErrorDataResult<T>()` carry `FieldErrors` through instead of dropping them.',
          '`Result.Combine(...)` merges `FieldErrors` by key.',
          'A `null` message list inside a field-error dictionary is skipped instead of throwing.',
          '`GetHashCode()` hashes `FieldErrors` order-independently, matching `Equals()`.',
        ],
      },
      { kind: 'Removed', breaking: true, items: ['`net7.0` target. The library now targets `net8.0`, `net9.0` and `net10.0`.'] },
    ],
  },
  {
    version: '12.0',
    headline: 'Combine, Ensure and async composition',
    summary: 'Independent checks can report every failure at once, guard clauses read fluently, and every composition helper has an async twin.',
    upgrade: [
      'Replace `StatusMessage` with `Title`, `StatusCode` with `Status` and `ResultData` with `Data`.',
      'Replace `HttpStatusCode`-based constructors with the `ResultStatus` ones; convert with `.ToResultStatus()` where needed.',
      'Read the full table in [Migrating to v12](/docs/migration-v12).',
    ],
    groups: [
      {
        kind: 'Added',
        items: [
          '`Result.Combine(...)` merges independent checks and reports every failure; 2–4 arity generic overloads return tuples.',
          '`Ensure<T>` / `EnsureAsync<T>` guard clauses.',
          '`MatchAsync`, `OnSuccessAsync`, `OnFailureAsync`, `MapAsync`, `BindAsync` in three shapes each.',
        ],
      },
      {
        kind: 'Changed',
        items: [
          'StyleCop and SonarAnalyzer enforced; the codebase builds warning-free.',
          'Duplicated default texts centralized behind single named constants.',
        ],
      },
      {
        kind: 'Removed',
        breaking: true,
        items: ['`StatusMessage`, `StatusCode: HttpStatusCode`, `ResultData` and every `HttpStatusCode`-based constructor.'],
      },
    ],
  },
  {
    version: '11.0',
    headline: 'ResultStatus, the Result facade and Problem Details',
    summary: 'The library stopped depending on HttpStatusCode, gained one named factory per status and learned to speak RFC 9457.',
    upgrade: [
      'Start using `ResultStatus` and the `Result` facade; the old members still compile but are marked `[Obsolete]`.',
      'Check call sites that relied on `ToActionResult<T>()` always returning 200: `Created` now returns 201.',
    ],
    groups: [
      {
        kind: 'Added',
        items: [
          '`ResultStatus` enum, independent of `System.Net.HttpStatusCode`.',
          '`Result` static facade with one named factory pair per status.',
          'RFC 9457 Problem Details with per-status `type` URIs, plus Minimal API `IResult` adapters.',
          'Fixed-name `System.Text.Json` serialization.',
          '`IResultFailureFactory<TSelf>` / `ResultFailureFactory` for generic short-circuiting.',
        ],
      },
      { kind: 'Fixed', items: ['`ToActionResult<T>()` / `ToEnvelopedActionResult()` honour `Status` instead of hard-coding 200.'] },
      { kind: 'Deprecated', items: ['`StatusMessage`, `StatusCode`, `ResultData` (removed in 12.0).'] },
    ],
  },
];
