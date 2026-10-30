import { TAB_GROUPS } from '@core/domain/tab-groups';
import { DocPage } from '../domain/doc-page';
import { code, cs, dir, file, good, list, minimalApi, mvcController, note, p, section, steps, syncedTabs, tab, table, tabs, tip, tree, warn } from './builders';

export const VERTICAL_SLICE: DocPage = {
  slug: 'vertical-slice',
  title: 'Recipe: vertical slice + MediatR',
  icon: 'bi-layers',
  summary: 'A complete feature folder (command, validator, handler and endpoint) wired with results end-to-end.',
  sections: [
    section(
      'layout',
      'Folder layout',
      tree(
        dir(
          'Features',
          'one folder per business capability',
          dir(
            'Products',
            null,
            dir(
              'CreateProduct',
              'one folder per use case',
              file('CreateProductCommand.cs', 'request and its result type', true),
              file('CreateProductValidator.cs', 'FluentValidation rules'),
              file('CreateProductHandler.cs', 'business rules, returns a result'),
              file('CreateProductEndpoint.cs', 'HTTP mapping with ToResult'),
            ),
            dir('GetProduct', null, file('GetProductQuery.cs'), file('GetProductHandler.cs'), file('GetProductEndpoint.cs')),
          ),
        ),
        dir('Common', null, dir('Behaviors', null, file('FieldValidationBehavior.cs', 'short-circuits with TResponse.Failure(...)'))),
        file('Program.cs', 'registers MediatR, validators and behaviors'),
      ),
      p('Each slice owns everything it needs. Cross-cutting concerns (validation, authorization, logging) live in generic pipeline behaviors that short-circuit with results instead of exceptions.'),
    ),
    section(
      'slice',
      'The slice',
      tabs(
        tab('Command', 'csharp', `public sealed record CreateProductCommand(string Name, decimal Price)
    : IRequest<OperationDataResult<ProductDto>>;`),
        tab('Validator', 'csharp', `public sealed class CreateProductValidator : AbstractValidator<CreateProductCommand>
{
    public CreateProductValidator()
    {
        RuleFor(x => x.Name).NotEmpty().MaximumLength(120);
        RuleFor(x => x.Price).GreaterThan(0);
    }
}`),
        tab('Handler', 'csharp', `public sealed class CreateProductHandler(AppDbContext db)
    : IRequestHandler<CreateProductCommand, OperationDataResult<ProductDto>>
{
    public async Task<OperationDataResult<ProductDto>> Handle(CreateProductCommand command, CancellationToken ct)
    {
        if (await db.Products.AnyAsync(p => p.Name == command.Name, ct))
        {
            return ResultFailureFactory.Conflict<OperationDataResult<ProductDto>>(
                $"A product named '{command.Name}' already exists.");
        }

        var product = new Product(command.Name, command.Price);
        db.Products.Add(product);
        await db.SaveChangesAsync(ct);

        return Result.Created(product.ToDto());
    }
}`),
        tab('Endpoint', 'csharp', `public static class CreateProductEndpoint
{
    public static RouteGroupBuilder MapCreateProduct(this RouteGroupBuilder group)
    {
        group.MapPost("/", async (CreateProductCommand command, ISender sender, HttpContext http, CancellationToken ct)
                => (await sender.Send(command, ct)).ToResult(http))
            .Produces<ProductDto>(StatusCodes.Status201Created)
            .ProducesProblem(StatusCodes.Status409Conflict)
            .ProducesProblem(StatusCodes.Status422UnprocessableEntity);

        return group;
    }
}`),
        tab('Behavior', 'csharp', `public sealed class FieldValidationBehavior<TRequest, TResponse>(IEnumerable<IValidator<TRequest>> validators)
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
    ),
    section(
      'why',
      'Why this works well',
      list(
        '**Single Responsibility:** the handler only contains business rules; validation and HTTP mapping live elsewhere.',
        '**Open/Closed:** add an `AuthorizationBehavior` or a `LoggingBehavior` without touching a single handler.',
        '**Dependency Inversion:** the application layer depends on `IOperationResult`, not on ASP.NET Core.',
        '**Explicit outcomes:** `201`, `409` and `422` are all visible in the endpoint metadata and in the code.',
      ),
      good('No exception is thrown for an expected outcome anywhere in this slice.'),
    ),
    section(
      'expose',
      'Exposing the slice',
      p('The handler does not care how it is called. Pick your API style; the choice is remembered across the site.'),
      syncedTabs(
        TAB_GROUPS.apiStyle,
        minimalApi(`var products = app.MapGroup("/api/products").WithTags("Products");
products.MapCreateProduct();`),
        mvcController(`[ApiController]
[Route("api/products")]
public sealed class ProductsController(ISender sender) : ControllerBase
{
    [HttpPost]
    [ProducesResponseType<ProductDto>(StatusCodes.Status201Created)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status409Conflict)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status422UnprocessableEntity)]
    public async Task<IActionResult> Create(CreateProductCommand command, CancellationToken ct)
        => (await sender.Send(command, ct)).ToActionResult(HttpContext);
}`),
      ),
    ),
  ],
};

export const CLIENT_CONSUMPTION: DocPage = {
  slug: 'client-consumption',
  title: 'Recipe: consuming from a client',
  icon: 'bi-window-stack',
  summary: 'Typed Problem Details in TypeScript / Angular and mapping fieldErrors onto form controls.',
  sections: [
    section(
      'types',
      'Type the contract once',
      code('typescript', `export interface ProblemDetails {
  type: string;
  title: string;
  status: number;
  detail?: string;
  instance?: string;
  errors?: string[];
  fieldErrors?: Record<string, string[]>;
}

export function isProblemDetails(value: unknown): value is ProblemDetails {
  return typeof value === 'object' && value !== null && 'status' in value && 'title' in value;
}`, 'problem-details.ts'),
    ),
    section(
      'forms',
      'Map fieldErrors to Angular form controls',
      code('typescript', `import { HttpErrorResponse } from '@angular/common/http';
import { FormGroup } from '@angular/forms';

export function applyServerErrors(form: FormGroup, error: HttpErrorResponse): string[] {
  if (!isProblemDetails(error.error)) {
    return ['Unexpected error.'];
  }
  for (const [field, messages] of Object.entries(error.error.fieldErrors ?? {})) {
    const control = form.get(field.charAt(0).toLowerCase() + field.slice(1));
    control?.setErrors({ server: messages });
    control?.markAsTouched();
  }
  return error.error.errors ?? [error.error.detail ?? error.error.title];
}`, 'apply-server-errors.ts'),
      note('Field keys are the C# property names (`Name`, `Price`). The helper camel-cases the first letter so they match typical form control names.'),
      tip('Because the validation rules live only on the server, the client never duplicates them; it simply renders what the API returns.'),
    ),
    section(
      'dotnet',
      '.NET clients (MAUI, Blazor, workers)',
      p('Call an enveloped endpoint and deserialize straight back into a result object, then keep composing with `Map`/`Bind` on the client:'),
      cs(`var result = await http.GetFromJsonAsync<OperationDataResult<ProductDto>>("api/products/42/envelope");

var label = result.Match(
    onSuccess: p => $"{p.Name}: {p.Price:C}",
    onFailure: f => f.Detail ?? f.Title);`),
    ),
  ],
};

export const TESTING: DocPage = {
  slug: 'testing',
  title: 'Recipe: testing',
  icon: 'bi-check2-square',
  summary: 'Assert outcomes, not exceptions, in unit and integration tests.',
  sections: [
    section(
      'unit',
      'Unit tests',
      cs(`[Fact]
public async Task Returns_NotFound_for_unknown_id()
{
    var result = await _service.GetByIdAsync(999, CancellationToken.None);

    Assert.False(result.IsSuccessful);
    Assert.Equal(ResultStatus.NotFound, result.Status);
    Assert.Equal("Product 999 does not exist.", result.Detail);
}

[Fact]
public void Reports_every_invalid_field()
{
    var result = _validator.ValidateCreate(new CreateProductRequest("", 0));

    Assert.Equal(["Name", "Price"], result.GetFieldErrors().Keys.Order());
}`),
      tip('The [playground](/playground) generates a starter test for any result you build.'),
    ),
    section(
      'integration',
      'Integration tests',
      cs(`[Fact]
public async Task Get_unknown_product_returns_problem_details()
{
    var response = await _client.GetAsync("/api/products/999");

    Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    Assert.Equal("application/problem+json", response.Content.Headers.ContentType?.MediaType);

    var problem = await response.Content.ReadFromJsonAsync<ProblemDetails>();
    Assert.Equal("https://tools.ietf.org/html/rfc9110#section-15.5.5", problem?.Type);
    Assert.Equal("/api/products/999", problem?.Instance);
}`),
    ),
    section(
      'why',
      'Why it is easier than exceptions',
      list(
        'No `Assert.ThrowsAsync<…>` with exception types that mirror HTTP statuses.',
        'Structural equality lets you compare whole results.',
        '`ToString()` produces readable failure output: `NotFound (404): Not Found`.',
      ),
    ),
  ],
};

export const BEST_PRACTICES: DocPage = {
  slug: 'best-practices',
  title: 'Best practices',
  icon: 'bi-award',
  summary: 'Guidelines that keep a result-based codebase clean and predictable.',
  sections: [
    section(
      'dos',
      'Do / Don’t',
      table(
        ['Do', 'Don’t'],
        ['Return results for **expected** outcomes (not found, forbidden, invalid, conflict)', 'Use results to hide bugs; let genuine exceptions reach your global handler'],
        ['Pick the most specific factory (`NotFound`, `Conflict`, `Gone`)', 'Return `BadRequest` for everything'],
        ['Write user-safe `detail` text', 'Put `exception.Message` or stack traces in `detail`'],
        ['Convert to HTTP only at the edge (`ToResult` / `ToActionResult`)', 'Reference ASP.NET Core from domain or application projects'],
        ['Use non-generic results for commands, generic for queries', 'Return `IOperationResult<bool>` just to say “it worked”'],
        ['Use `Result.Combine` for independent validations', 'Chain `Ensure` for rules that should all be reported'],
        ['Use `ToResult` in Minimal APIs', 'Return `IActionResult` from Minimal API delegates (ASP0004)'],
      ),
    ),
    section(
      'layers',
      'Where results belong in Clean Architecture',
      table(
        ['Layer', 'Package', 'Role'],
        ['Domain', '`ResponseResultHandler` (optional)', 'Invariants can return results instead of throwing'],
        ['Application', '`ResponseResultHandler`', 'Handlers/services return `IOperationResult(<T>)`; behaviors short-circuit'],
        ['Infrastructure', '`ResponseResultHandler`', 'Wrap downstream calls: `(int)response.StatusCode` → `ToResultStatus()`'],
        ['Presentation / API', '`ResponseResultHandler.AspNetCore`', 'One-line conversion to HTTP'],
      ),
    ),
    section(
      'status',
      'Choosing a status',
      list(
        'Resource does not exist → `NotFound`. Existed but was deleted permanently → `Gone`.',
        'Duplicate / state clash / concurrency token mismatch → `Conflict`; failed `If-Match` → `PreconditionFailed`.',
        'Well-formed but invalid input → `Invalid(...)` / `UnprocessableContent`. Malformed request → `BadRequest`.',
        'Not logged in → `Unauthorized`. Logged in but not allowed → `Forbidden`.',
        'Plan limit or payment problem → `PaymentRequired`. Throttled → `TooManyRequests`.',
        'Queued background work → `Accepted`. Nothing to return → `NoContent`.',
      ),
      warn('Returning `NotFound` instead of `Forbidden` for resources the caller must not know about is a legitimate security choice. Just do it deliberately.'),
    ),
  ],
};

export const PITFALLS: DocPage = {
  slug: 'pitfalls',
  title: 'Common pitfalls',
  icon: 'bi-exclamation-triangle',
  badge: { label: 'From real projects', tone: 'accent' },
  summary: 'Mistakes that only show up once results flow through caching, transactions, pipelines and payments, and how to avoid them.',
  sections: [
    section(
      'caching',
      'Caching a failure',
      p('A caching layer that stores whatever the handler returned will happily cache `404 Not Found`, and keep serving it after the resource is created.'),
      cs(`var response = await next(ct);

if (response is IOperationResult { IsSuccessful: false })
{
    return response; // never cache a failure
}

await cache.SetAsync(key, Serialize(response), options, ct);`),
      tip('When reading from the cache, ignore an entry that deserializes to a failure too, so an old bad entry cannot outlive the fix.'),
    ),
    section(
      'transactions',
      'Committing a failed result',
      p('A failure is **returned**, not thrown, so a transaction behavior that only rolls back on exceptions commits half-done work.'),
      cs(`using var scope = new TransactionScope(TransactionScopeAsyncFlowOption.Enabled);

var response = await next(ct);

if (response is not IOperationResult { IsSuccessful: false })
{
    scope.Complete(); // commit only when the result succeeded
}

return response;`),
    ),
    section(
      'order',
      'Validating before authorizing',
      p('If validation runs first, an anonymous caller gets `422` with your validation rules instead of `401`. Register authorization (and tenant or subscription checks) **before** validation:'),
      cs(`services.AddTransient(typeof(IPipelineBehavior<,>), typeof(AuthorizationBehavior<,>));
services.AddTransient(typeof(IPipelineBehavior<,>), typeof(ValidationBehavior<,>));
services.AddTransient(typeof(IPipelineBehavior<,>), typeof(LoggingBehavior<,>));`, 'Registration order = execution order'),
    ),
    section(
      'bad-request',
      'Returning 400 for everything',
      p('`400 Bad Request` means the request could not be understood. Most business failures deserve a more precise status, which clients can act on:'),
      table(
        ['Situation', 'Status', 'Facade'],
        ['The resource does not exist', '404', '`Result.NotFound(...)`'],
        ['The current state does not allow it (already cancelled, slot taken, duplicate name)', '409', '`Result.Conflict(...)`'],
        ['The request content is invalid on its own (bad range, unknown key)', '422', '`Result.UnprocessableContent(...)` / `Result.Invalid(...)`'],
        ['A link or invite has expired', '410', '`Result.Gone(...)`'],
        ['A payment or other upstream provider failed', '502', '`Result.BadGateway(...)`'],
        ['Your own server-side step failed', '500', '`Result.InternalServerError()`'],
      ),
      note('Keep `400` for authentication flows (bad credentials, invalid refresh or reset token): that is what OAuth 2 specifies, and it keeps clients from treating a wrong password as an expired session.'),
      tip('Not sure? The [status wizard](/status-wizard) asks a few questions and suggests one.'),
    ),
    section(
      'side-effects',
      'A second side effect after an external charge',
      p('A card charge cannot be rolled back with your database. If a second step (spending account credit, redeeming a coupon) runs **after** the charge and fails, the customer keeps a discount they never paid for.'),
      steps(
        '**Reserve first**: debit the credit before calling the payment provider, keyed by the purchase id so a retry never debits twice.',
        '**Charge**.',
        '**Compensate on failure**: if the charge fails, append a reversing entry. An append-only ledger makes this natural.',
      ),
      warn('Never discard the result of a follow-up step (`_ = await sender.Send(...)`). If it can fail, its failure has to change what you return.'),
    ),
    section(
      'logging',
      'Failures that never reach the logs',
      p('Exceptions get logged by middleware; returned failures do not. Log them where every request passes through, and pick the level from the status so 4xx outcomes do not page anyone:'),
      cs(`public sealed class LoggingBehavior<TRequest, TResponse>(ILogger<LoggingBehavior<TRequest, TResponse>> logger)
    : IPipelineBehavior<TRequest, TResponse>
    where TRequest : notnull
    where TResponse : IOperationResult
{
    public async Task<TResponse> Handle(TRequest request, RequestHandlerDelegate<TResponse> next, CancellationToken ct)
    {
        var response = await next(ct);

        if (!response.IsSuccessful)
        {
            var level = (int)response.Status.ToHttpStatusCode() >= 500 ? LogLevel.Error : LogLevel.Information;
            logger.Log(level, "{Request} failed with {Status}: {Title} {Detail}", typeof(TRequest).Name, response.Status, response.Title, response.Detail);
        }

        return response;
    }
}`),
      p('Without MediatR, the same thing is one call at the end of a chain: `.OnFailure(f => logger.LogInformation("{Status}: {Detail}", f.Status, f.Detail))`.'),
    ),
    section(
      'success-string',
      '`Result.Success("text")` has no data',
      p('A single string argument binds to the **title** overload, so the result carries no data. To return a string as data, name the type:'),
      cs(`Result.Success("Saved.");          // title only, no data
Result.Success<string>("AB-1234"); // data = "AB-1234"`),
    ),
  ],
};

export const CASE_STUDY: DocPage = {
  slug: 'case-study',
  title: 'Case study: a modular monolith',
  icon: 'bi-building',
  badge: { label: 'Real project', tone: 'accent' },
  summary: 'What adopting results end-to-end changed in a production field-service platform: less code, and bugs found along the way.',
  sections: [
    section(
      'context',
      'The project',
      table(
        ['', ''],
        ['Domain', 'Field-service company (duct cleaning, maintenance, installation, repair): bookings, memberships, payments, referrals'],
        ['Architecture', 'Modular monolith, 13 modules, about 250 endpoints, MediatR + EF Core, a shared in-house core package'],
        ['Clients', 'Web API consumed by a .NET MAUI app'],
      ),
    ),
    section(
      'before-after',
      'One endpoint, before and after',
      p('About 30 endpoints started the same way: load the current user, check it, send a command, envelope the result.'),
      cs(`private static async Task<IResult> Handle(Guid id, ISender sender, HttpContext httpContext)
{
    var currentUser = await sender.Send(new GetCurrentUserQuery());
    if (!currentUser.IsSuccessful || currentUser.Data is null)
    {
        return currentUser.ToEnvelopedResult(httpContext);
    }

    var result = await sender.Send(new DeleteUserAddressCommand(id, currentUser.Data.Id));
    return result.ToEnvelopedResult(httpContext);
}`, 'Before'),
      cs(`private static async Task<IResult> Handle(Guid id, ISender sender, HttpContext httpContext)
    => (await sender.Send(new GetCurrentUserQuery())
            .BindAsync(user => sender.Send(new DeleteUserAddressCommand(id, user.Id))))
        .ToEnvelopedResult(httpContext);`, 'After (13.1)'),
      p('Across those endpoints the handlers shrank from about 350 lines to about 140, with identical behavior: a failed user lookup is still returned unchanged and the command is never sent.'),
    ),
    section(
      'bugs',
      'Bugs found while adopting it',
      p('Treating failures as values forced every layer to decide what a failure means. That surfaced real defects, each now covered in [common pitfalls](/docs/pitfalls):'),
      table(
        ['Found', 'Effect', 'Fix'],
        ['The caching behavior stored failed results', 'a `404` could be served after the item existed', 'skip caching failures; ignore cached failures on read'],
        ['The transaction behavior only rolled back on exceptions', 'returned failures committed half-done work', 'complete the scope only on success'],
        ['Validation ran before authorization', 'anonymous callers saw validation rules', 'register authorization first'],
        ['Account credit was spent after the card charge, result discarded', 'a failed or repeated spend went unnoticed; a retry could debit twice', 'reserve before charging, reverse on failure, idempotent per purchase'],
        ['77 business failures returned `400`', 'clients could not tell "not found" from "conflict"', '404 / 409 / 422 / 410 / 502 / 500 by meaning'],
        ['Identity errors were a flat list', 'forms could not highlight the password field', 'map error codes to field errors'],
        ['The mobile client treated `204 No Content` as a failure', 'successful deletes showed an error', 'read the status before the body'],
      ),
    ),
    section(
      'takeaways',
      'Takeaways',
      steps(
        'Adopt results at the edges first (endpoints), then let them flow inward; the shape of each chain shows where logic is duplicated.',
        'Audit every cross-cutting behavior (caching, transactions, logging) for how it treats a *returned* failure.',
        'Pick statuses by meaning; the [status wizard](/status-wizard) makes the rule explicit for the team.',
      ),
    ),
  ],
};
