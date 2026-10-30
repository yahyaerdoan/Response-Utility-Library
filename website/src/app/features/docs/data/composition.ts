import { TAB_GROUPS } from '@core/domain/tab-groups';
import { DocPage } from '../domain/doc-page';
import { cs, minimalApi, mvcController, note, p, section, syncedTabs, table, tip, warn } from './builders';

export const FUNCTIONAL: DocPage = {
  slug: 'functional',
  title: 'Functional composition',
  icon: 'bi-link-45deg',
  summary: 'Map, Bind, Ensure, Match, OnSuccess and OnFailure: railway-oriented programming for .NET.',
  sections: [
    section(
      'problem',
      'The problem',
      cs(`var product = _products.GetById(id);
if (!product.IsSuccessful) return product.ToErrorDataResult<OrderDto>();
if (product.Data.Stock <= 0) return Result.Invalid<OrderDto>("Product is out of stock.");
var order = _orders.CreateDraftOrder(ToDto(product.Data));
if (!order.IsSuccessful) return order;
_logger.LogInformation("Order {Id} drafted", order.Data.Id);
return order;`, 'Without composition'),
      cs(`return _products.GetById(id)
    .Ensure(p => p.Stock > 0, "Product is out of stock.")
    .Map(ToDto)
    .Bind(_orders.CreateDraftOrder)
    .OnSuccess(order => _logger.LogInformation("Order {Id} drafted", order.Id));`, 'With composition'),
      p('Watch this exact chain execute step by step in the [pipeline visualizer](/pipeline).'),
    ),
    section(
      'methods',
      'The toolbox',
      table(
        ['Method', 'Runs when', 'Does'],
        ['`Map(f)`', 'success', 'Transforms `T → TOut`; failure is re-projected into `IOperationResult<TOut>`'],
        ['`Bind(f)`', 'success', 'Chains into another `IOperationResult<TOut>`-returning call'],
        ['`Ensure(predicate, message)`', 'success', 'Turns success into `422 Validation Failed` if the predicate is false; the message goes to `detail`, not `errors`'],
        ['`Ensure(predicate, title, detail, status)`', 'success', 'Same, with your own outcome (e.g. `403`)'],
        ['`OnSuccess(action)`', 'success', 'Side effect; returns the same result'],
        ['`OnFailure(action)`', 'failure', 'Side effect; returns the result as non-generic `IOperationResult`'],
        ['`Match(onSuccess, onFailure)`', 'always', 'Collapses the result into any value'],
      ),
    ),
    section(
      'examples',
      'Examples',
      cs(`string message = _products.GetById(id)
    .Map(p => p.Name.ToUpperInvariant())
    .Match(
        onSuccess: name => $"Found: {name}",
        onFailure: failure => $"Error: {failure.Title}");

_products.GetById(id)
    .OnSuccess(product => _logger.LogInformation("Fetched {Name}", product.Name))
    .OnFailure(failure => _logger.LogWarning("Lookup failed: {Title}", failure.Title));

_products.GetById(id)
    .Ensure(p => p.OwnerId == currentUserId, "Forbidden.", "You do not own this product.", ResultStatus.Forbidden);`),
    ),
    section(
      'onfailure',
      'Where to put OnFailure',
      p('`OnFailure` is defined on the non-generic `IOperationResult` (every `IOperationResult<T>` is one), and it returns `IOperationResult`. Anything chained after it no longer knows `T`, so keep it at the end of a statement or call it separately:'),
      cs(`IOperationResult<OrderDto> order = _products.GetById(id)
    .Bind(_orders.CreateDraftOrder)
    .OnSuccess(o => _logger.LogInformation("Order {Id} drafted", o.Id));

order.OnFailure(f => _logger.LogWarning("Order failed: {Title}", f.Title));

return order.ToActionResult(); // typed: 201 + OrderDto on success`),
      warn('`IOperationResult<OrderDto> x = result.OnSuccess(...).OnFailure(...);` does not compile: the value after `OnFailure` is a plain `IOperationResult`.'),
    ),
    section(
      'commands',
      'Non-generic results (commands)',
      p('`Match`, `OnSuccess` and `OnFailure` also work on plain `IOperationResult`. The success callback receives the result itself, since there is no data:'),
      cs(`IOperationResult deleted = await _products.DeleteAsync(id, ct);

string message = deleted
    .OnSuccess(r => _audit.Record("product.deleted", id))
    .Match(
        onSuccess: r => "Deleted.",
        onFailure: f => f.Detail ?? f.Title);`),
    ),
    section(
      'preserved',
      'Nothing is lost on the way',
      note('When `Map`/`Bind` skip because of an earlier failure, they carry **everything** over into the new result type: title, status, detail, errors and field errors.'),
    ),
  ],
};

export const COMBINE: DocPage = {
  slug: 'combine',
  title: 'Result.Combine',
  icon: 'bi-collection',
  summary: 'Run independent checks to completion and report every failure at once.',
  sections: [
    section(
      'why',
      'First failure vs. all failures',
      p('`Ensure`, `Bind` and `Map` stop at the first failure, which is right for dependent steps. For **independent** checks, like every field on a form, you want all problems at once. `Result.Combine(...)` runs every result and merges their outcomes.'),
      cs(`IOperationResult? ValidateCreate(CreateProductRequest request)
{
    var nameCheck = string.IsNullOrEmpty(request.Name)
        ? Result.Invalid("Name is required.")
        : Result.Success();

    var priceCheck = request.Price <= 0
        ? Result.Invalid("Price must be greater than zero.")
        : Result.Success();

    var combined = Result.Combine(nameCheck, priceCheck);
    return combined.IsSuccessful ? null : combined;
}`),
    ),
    section(
      'rules',
      'Merge rules',
      table(
        ['Situation', 'Outcome'],
        ['All succeed', '`Result.Success()`'],
        ['Some fail', 'Aggregated `Result.Invalid(...)` with every message in order'],
        ['A failure has no `Errors`', 'Its `Detail` (or `Title`) is used as its message'],
        ['Failures carry `FieldErrors`', 'Merged by key, so `Name` and `Price` both survive'],
      ),
    ),
    section(
      'tuples',
      'Combining payloads into a tuple',
      p('The 2–4 arity generic overloads combine outcomes **and** data:'),
      cs(`IOperationResult<(CustomerDto First, AddressDto Second)> order =
    Result.Combine(_customers.GetById(customerId), _addresses.GetById(addressId));

if (order.IsSuccessful)
{
    var (customer, address) = order.Data;
}`),
      tip('Tuple element names are just labels. You can deconstruct into any names you like, or declare the return type as `IOperationResult<(CustomerDto Customer, AddressDto Address)>`.'),
    ),
    section(
      'enumerable',
      'Dynamic lists',
      cs(`IEnumerable<IOperationResult> checks = request.Lines.Select(line => _stock.EnsureAvailable(line));
IOperationResult all = Result.Combine(checks);`),
    ),
  ],
};

export const ASYNC: DocPage = {
  slug: 'async',
  title: 'Async composition',
  icon: 'bi-arrow-repeat',
  badge: { label: 'Updated in 13.1', tone: 'new' },
  summary: 'MapAsync, BindAsync, EnsureAsync, MatchAsync, OnSuccessAsync and OnFailureAsync, including MediatR Send chains.',
  sections: [
    section(
      'chain',
      'A fluent chain across awaits',
      cs(`public Task<IOperationResult<ProductDto>> ActivateAsync(int id)
    => _products.GetByIdAsync(id)                              // Task<IOperationResult<Product>>
        .BindAsync(product => ValidateCanActivateAsync(product)) // async business rule
        .MapAsync(product => ToDto(product))                     // sync mapper
        .OnSuccessAsync(dto => _email.SendActivationEmailAsync(dto.OwnerEmail));`),
      p('If `GetByIdAsync` returns `NotFound`, nothing else runs and the original failure is what the caller receives.'),
    ),
    section(
      'more',
      'EnsureAsync, MatchAsync and OnFailureAsync',
      cs(`IOperationResult<ProductDto> product = await _products.GetByIdAsync(id)
    .EnsureAsync(p => _stock.HasStockAsync(p.Id), "Product is out of stock.") // async predicate
    .MapAsync(p => ToDto(p));

string label = await _products.GetByIdAsync(id)
    .MatchAsync(
        onSuccess: p => p.Name,
        onFailure: f => $"Unavailable ({(int)f.Status.ToHttpStatusCode()})");

await _orders.CancelAsync(orderId)                          // Task<IOperationResult>
    .OnFailureAsync(f => _alerts.NotifyAsync(f.Title));`),
      note('`EnsureAsync` only has the message overload (default `422 Validation Failed`). When you need a custom title and status inside an async chain, `BindAsync` into a result you build yourself, e.g. `ResultFailureFactory.Forbidden<...>()` or `Result.Forbidden<T>()`.'),
    ),
    section(
      'shapes',
      'Three shapes, mix freely',
      table(
        ['Source', 'Delegate', 'Example'],
        ['`Task<IOperationResult<T>>`', 'sync', '`.MapAsync(p => p.Name)`'],
        ['`Task<IOperationResult<T>>`', 'async', '`.BindAsync(p => _orders.CreateDraftOrderAsync(p))`'],
        ['`IOperationResult<T>`', 'async', '`existing.OnSuccessAsync(p => _email.SendAsync(p))`'],
        ['`Task<OperationDataResult<T>>`', 'async', '`sender.Send(query).BindAsync(u => sender.Send(command))`'],
      ),
      note('`EnsureAsync` exists only for data results, because there is no data to check on the non-generic form.'),
    ),
    section(
      'services',
      'Concrete tasks from your own services',
      p('`Task<T>` is invariant, so a `Task<OperationDataResult<T>>` is not a `Task<IOperationResult<T>>`. Every async operator has overloads for the concrete task (since 13.0), so methods that declare the concrete type chain directly. No mediator is involved:'),
      cs(`public sealed class CheckoutService(IUserService users, IOrderService orders)
{
    public Task<OperationDataResult<OrderDto>> PlaceOrderAsync(Guid basketId)
        => users.GetCurrentAsync()                                               // Task<OperationDataResult<UserDto>>
            .EnsureAsync(user => user.IsActive, "Inactive users cannot order.")
            .BindAsync(user => orders.CreateFromBasketAsync(user.Id, basketId));  // Task<OperationDataResult<OrderDto>>

    public Task<OperationResult> CancelAsync(Guid orderId)
        => users.GetCurrentAsync()
            .BindAsync(user => orders.CancelAsync(orderId, user.Id));             // Task<OperationResult>
}`),
      tip('Declaring `OperationDataResult<T>` / `OperationResult` as the return type (instead of the interface) is what lets a caller return the chain directly, since the concrete type flows through.'),
    ),
    section(
      'mediatr',
      'Chaining MediatR Send calls',
      p('A MediatR handler returns the same concrete `OperationDataResult<T>`, so `Send` calls chain exactly like the service methods above:'),
      cs(`app.MapPut("/me/profile", async (UpdateProfileCommand command, ISender sender, HttpContext httpContext) =>
    (await sender.Send(new GetCurrentUserQuery())                           // Task<OperationDataResult<UserDto>>
        .BindAsync(user => sender.Send(command with { UserId = user.Id }))  // Task<OperationDataResult<ProfileDto>>
        .OnSuccessAsync(profile => _audit.Record(profile.Id)))
    .ToEnvelopedResult(httpContext));`),
      table(
        ['Chain', 'Result type'],
        ['concrete task, then concrete task or `MapAsync`', '`OperationDataResult<TOut>`: a handler can return it directly'],
        ['an `IOperationResult<T>` anywhere in the chain', '`IOperationResult<TOut>`'],
        ['`OnSuccessAsync` / `OnFailureAsync` / `EnsureAsync` on a concrete task', 'the same concrete type'],
      ),
    ),
    section(
      'commands',
      'Binding into a command that returns no data',
      p('When the next step returns a non-generic `OperationResult` (a delete, a status change, a password change), `Bind` / `BindAsync` return a non-generic result too (since 13.1):'),
      cs(`app.MapDelete("/addresses/{id:guid}", async (Guid id, ISender sender, HttpContext httpContext) =>
    (await sender.Send(new GetCurrentUserQuery())                                  // Task<OperationDataResult<UserDto>>
        .BindAsync(user => sender.Send(new DeleteUserAddressCommand(id, user.Id)))) // Task<OperationResult>
    .ToEnvelopedResult(httpContext));`),
      p('A failed source is re-projected into an `ErrorResult` with its title, status, detail, errors and field errors intact, exactly like the data-returning `Bind`. When the binder returns a data result, the data overloads still win.'),
    ),
    section(
      'mixed-lambdas',
      'Lambdas that return both a success and a failure',
      p('Facade factories share one return type (13.0), so an async lambda with a success branch and a failure branch infers `Task<OperationDataResult<T>>` and binds without type arguments:'),
      cs(`.BindAsync(async user =>
{
    if (!user.IsActive)
    {
        return Result.Forbidden<OrderDto>("Inactive user.");
    }

    return Result.Success(await _orders.CreateAsync(user.Id));
})`),
      warn('A lambda that returns **only** hand-built subclasses of different kinds (`new SuccessDataResult<T>(...)` in one branch, `new ErrorDataResult<T>(...)` in another) has no common data type. It still compiles, through the no-data overload, so the chain becomes `IOperationResult` and `.Data` disappears. Use the facade for at least one branch, or write `.BindAsync<UserDto, OrderDto>(...)`.'),
      tip('A lambda that only ever returns success is a mapping: use `MapAsync` with an async mapper.'),
    ),
    section(
      'edge',
      'At the edge',
      syncedTabs(
        TAB_GROUPS.apiStyle,
        minimalApi(`app.MapPost("/api/products/{id:int}/activate", async (int id, ProductService products)
    => (await products.ActivateAsync(id)).ToResult());`),
        mvcController(`[HttpPost("{id}/activate")]
public async Task<IActionResult> Activate(int id)
    => (await _products.ActivateAsync(id)).ToActionResult();`),
      ),
    ),
  ],
};
