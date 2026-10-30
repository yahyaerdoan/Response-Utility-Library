export type GemArea = 'Safety' | 'HTTP' | 'Serialization' | 'Composition' | 'Quality';

export interface Gem {
  readonly id: string;
  readonly area: GemArea;
  readonly icon: string;
  readonly title: string;
  readonly text: string;
  readonly snippet?: string;
  readonly link?: string;
}

export interface RoadmapIdea {
  readonly icon: string;
  readonly title: string;
  readonly text: string;
}

export const GEM_AREAS: readonly GemArea[] = ['Safety', 'HTTP', 'Serialization', 'Composition', 'Quality'];

export const GEMS: readonly Gem[] = [
  {
    id: 'nullability',
    area: 'Safety',
    icon: 'bi-shield-check',
    title: 'The compiler knows Data is not null',
    text: '`IOperationResult<T>.Data` is annotated with `[MemberNotNullWhen]`. After `if (result.IsSuccessful)` there is no nullable warning and no need for `!`.',
    snippet: 'if (result.IsSuccessful) Console.WriteLine(result.Data.Name);',
    link: '/docs/core-contract',
  },
  {
    id: 'total-mapping',
    area: 'Safety',
    icon: 'bi-infinity',
    title: 'Status mapping never throws',
    text: 'An unknown code like `499` maps to the generic status of its class (`BadRequest`); an unmapped status maps to `500`. Perfect for wrapping `HttpClient` responses.',
    snippet: 'ResultStatus status = ((int)response.StatusCode).ToResultStatus();',
    link: '/docs/result-status',
  },
  {
    id: 'registry',
    area: 'Safety',
    icon: 'bi-arrow-left-right',
    title: 'Self-checking status registry',
    text: 'The reverse map (code → status) is derived from the forward map and throws at start-up if two statuses ever share a code. The directions cannot drift apart.',
    link: '/docs/result-status',
  },
  {
    id: 'null-field-errors',
    area: 'Safety',
    icon: 'bi-funnel',
    title: 'Null-tolerant field errors',
    text: 'A `null` message list inside a field-error dictionary is skipped instead of crashing the request.',
    link: '/docs/field-errors',
  },
  {
    id: 'real-status',
    area: 'HTTP',
    icon: 'bi-bullseye',
    title: 'The real status code, every time',
    text: '`Result.Created(dto).ToResult()` returns **201**, `Accepted` returns **202**. Nothing is silently flattened to 200.',
    link: '/docs/mvc-controllers',
  },
  {
    id: 'rfc-types',
    area: 'HTTP',
    icon: 'bi-link',
    title: 'Problem types link to the defining RFC',
    text: '`type` points to the exact section of RFC 9110, 4918, 6585, 7725, 8470… for each status, so clients and humans can follow it.',
    link: '/docs/problem-details',
  },
  {
    id: 'instance',
    area: 'HTTP',
    icon: 'bi-geo-alt',
    title: '`instance` for free',
    text: 'Pass `HttpContext` and Problem Details gets the request path as `instance`, which makes log correlation and support tickets easier.',
    snippet: 'return result.ToResult(httpContext);',
    link: '/docs/mvc-controllers',
  },
  {
    id: 'openapi',
    area: 'HTTP',
    icon: 'bi-filetype-json',
    title: 'OpenAPI-friendly Minimal API adapters',
    text: '`ToResult()` returns `IResult`, avoiding analyzer ASP0004 and keeping endpoints visible to Swagger / `Microsoft.AspNetCore.OpenApi`.',
    link: '/docs/minimal-apis',
  },
  {
    id: 'envelope',
    area: 'HTTP',
    icon: 'bi-envelope-paper',
    title: 'Payload or envelope: your choice per endpoint',
    text: 'Return just the DTO, or the whole result with status and title via `ToEnvelopedResult()`. Handy for clients that want metadata.',
    link: '/playground',
  },
  {
    id: 'teapot',
    area: 'HTTP',
    icon: 'bi-cup-hot',
    title: "Yes, there's Result.ImATeapot()",
    text: 'All 62 standard statuses have named factories, including 418, 451 (legal reasons) and 511 (captive portals).',
    snippet: 'return Result.ImATeapot(); // 418',
    link: '/statuses?s=ImATeapot',
  },
  {
    id: 'fixed-names',
    area: 'Serialization',
    icon: 'bi-lock',
    title: 'JSON names ignore your naming policy',
    text: 'Properties are pinned with `[JsonPropertyName]`, so snake_case or PascalCase options can never break the client contract.',
    link: '/docs/serialization',
  },
  {
    id: 'numeric-strings',
    area: 'Serialization',
    icon: 'bi-123',
    title: 'Lenient status reading',
    text: '`statusCode` deserializes from `404` **or** `"404"`, so hand-written or proxied payloads still round-trip.',
    link: '/docs/serialization',
  },
  {
    id: 'aot',
    area: 'Serialization',
    icon: 'bi-rocket-takeoff',
    title: 'Native AOT ready',
    text: 'The full constructors are public on purpose, so a source-generated `JsonSerializerContext` can deserialize results including `fieldErrors`.',
    link: '/docs/serialization',
  },
  {
    id: 'converter',
    area: 'Serialization',
    icon: 'bi-plug',
    title: 'Reuse the status converter',
    text: '`ResultStatusJsonConverter` is public: put it on your own `ResultStatus` properties (audit logs, events) and they serialize as `404`, not `"NotFound"`.',
    link: '/docs/serialization',
  },
  {
    id: 'failure-data',
    area: 'Composition',
    icon: 'bi-box-arrow-in-down',
    title: 'Failures can carry data',
    text: '`Result.Failure<T>(data, title, detail, status)` keeps a payload on a failed result, e.g. the current version on a 409. It stays in process; HTTP still gets Problem Details.',
    link: '/docs/building-results',
  },
  {
    id: 'crtp',
    area: 'Composition',
    icon: 'bi-cpu',
    title: 'Fail generically without throwing',
    text: '`IResultFailureFactory<TSelf>` uses C# 11 static abstract members, so a MediatR behavior can return `TResponse.Failure(...)` for any response type.',
    snippet: 'return errors.Length > 0 ? TResponse.Failure(errors) : await next(ct);',
    link: '/docs/generic-short-circuiting',
  },
  {
    id: 'reprojection',
    area: 'Composition',
    icon: 'bi-arrow-repeat',
    title: 'Re-project a failure into any type',
    text: '`failure.ToErrorDataResult<T>()` keeps title, status, detail, errors and field errors while changing the data type.',
    link: '/docs/generic-short-circuiting',
  },
  {
    id: 'combine-tuples',
    area: 'Composition',
    icon: 'bi-collection',
    title: 'Combine returns tuples',
    text: '`Result.Combine(a, b, c)` gives you `(T1, T2, T3)` on success and every error on failure: parallel lookups in one expression.',
    snippet: 'IOperationResult<(CustomerDto, AddressDto)> both = Result.Combine(getCustomer, getAddress);',
    link: '/docs/combine',
  },
  {
    id: 'field-merge',
    area: 'Composition',
    icon: 'bi-ui-checks-grid',
    title: 'Field errors survive everything',
    text: '`Map`, `Bind`, `ToErrorDataResult` carry them; `Combine` merges them by key; `GetFieldErrors()` reads them from any result.',
    link: '/docs/field-errors',
  },
  {
    id: 'equality',
    area: 'Quality',
    icon: 'bi-hash',
    title: 'Order-independent hash codes',
    text: 'Two results whose field-error dictionaries differ only in insertion order are equal **and** hash the same, so they are safe as dictionary keys or in sets.',
    link: '/docs/equality-debugging',
  },
  {
    id: 'tostring',
    area: 'Quality',
    icon: 'bi-terminal',
    title: 'Readable ToString()',
    text: '`NotFound (404): Not Found` shows up in the debugger, in logs and in test failure messages.',
    link: '/docs/equality-debugging',
  },
  {
    id: 'sourcelink',
    area: 'Quality',
    icon: 'bi-bug',
    title: 'Step into the library while debugging',
    text: 'Packages ship with SourceLink, symbol packages (`.snupkg`) and deterministic builds.',
  },
  {
    id: 'api-guard',
    area: 'Quality',
    icon: 'bi-shield-lock',
    title: 'Guarded public API',
    text: 'Public API analyzers track every shipped member and package validation compares each release to the last stable baseline, so breaking changes cannot sneak in.',
  },
  {
    id: 'single-source',
    area: 'Quality',
    icon: 'bi-bookmark-check',
    title: 'One source of truth for messages',
    text: 'Default titles and details live in one place; `ResultFailureFactory` delegates to `Result`, so wording never diverges.',
    link: '/docs/result-facade',
  },
];

export const ROADMAP_IDEAS: readonly RoadmapIdea[] = [
  { icon: 'bi-translate', title: 'Localized default messages', text: 'Resolve default titles and details through `IStringLocalizer` so APIs can answer in the caller’s language.' },
  { icon: 'bi-filetype-json', title: 'OpenAPI schema transformer', text: 'Describe the `errors` / `fieldErrors` Problem Details extensions in generated OpenAPI documents.' },
  { icon: 'bi-check2-all', title: 'FluentValidation adapter package', text: 'Ship the field-validation pipeline behavior as a ready-made package.' },
  { icon: 'bi-exclamation-diamond', title: 'Discarded-result analyzer', text: 'A Roslyn analyzer that warns when a returned `IOperationResult` is ignored.' },
  { icon: 'bi-shield-exclamation', title: 'Result.Try', text: 'Wrap a throwing third-party call and convert known exceptions into results at the boundary.' },
  { icon: 'bi-filetype-tsx', title: 'npm types package', text: 'Publish the Problem Details + envelope TypeScript types for front-end consumers.' },
];
