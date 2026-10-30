import { TAB_GROUPS } from '@core/domain/tab-groups';
import { DocPage } from '../domain/doc-page';
import { cs, json, list, minimalApi, mvcController, note, p, section, syncedTabs, table, tip, warn } from './builders';

export const MVC_CONTROLLERS: DocPage = {
  slug: 'mvc-controllers',
  title: 'MVC controllers',
  icon: 'bi-diagram-2',
  summary: 'ToActionResult, ToActionResult<T> and ToEnvelopedActionResult for controller actions.',
  sections: [
    section(
      'usage',
      'One call at the end of the action',
      cs(`using ResultHandler.AspNetCore.Extensions;

[ApiController]
[Route("api/products")]
public sealed class ProductsController(ProductService products) : ControllerBase
{
    [HttpGet("{id:int}")]
    public IActionResult GetById(int id)
        => products.GetById(id).ToActionResult(HttpContext);

    [HttpPost]
    public IActionResult Create(CreateProductRequest request)
    {
        var validation = products.ValidateCreate(request);
        if (validation is not null)
        {
            return validation.ToActionResult(); // 422 Problem Details
        }

        return Result.Created(products.Create(request)).ToActionResult(); // 201 + body
    }
}`),
    ),
    section(
      'methods',
      'The three shapes',
      table(
        ['Method', 'On success', 'On failure'],
        ['`ToActionResult()`', 'Bodyless status that matches `Status` (`201`, `204`…)', 'Problem Details'],
        ['`ToActionResult<T>()`', 'Raw `T` with the real status (`201 { … }`), bodyless for 1xx/3xx/204/304', 'Problem Details'],
        ['`ToEnvelopedActionResult()`', 'The whole result object (status, title, data…) as the body', 'Problem Details'],
      ),
      note('Status codes are honoured: `Result.Created(dto)` really returns **201**, `Result.Accepted(dto)` **202**. Nothing is hard-coded to 200.'),
    ),
    section(
      'envelope',
      'Returning the envelope',
      p('Use the enveloped variant when a client wants status and title next to the data, for example a mobile app that shows the server message:'),
      cs(`[HttpGet("{id:int}/envelope")]
public IActionResult GetEnvelope(int id)
    => products.GetById(id).ToEnvelopedActionResult(HttpContext);`),
      json(`{
  "resultData": { "id": 42, "name": "Duct cleaning kit", "price": 129.9 },
  "isSuccessful": true,
  "statusCode": 200,
  "statusMessage": "Operation completed successfully.",
  "errors": [],
  "fieldErrors": {}
}`),
      note('Failures are still Problem Details. Only the success body changes.'),
    ),
    section(
      'httpcontext',
      'Why pass HttpContext?',
      p('Every method takes an optional `HttpContext`. When you pass it, the Problem Details `instance` member is set to the current request path (RFC 9457 §3.1.4), which makes logs and support tickets far easier to correlate.'),
    ),
    section(
      'minimal-warning',
      'Not for Minimal APIs',
      warn('Returning `IActionResult` from a Minimal API delegate triggers analyzer **ASP0004** and hides the response shape from OpenAPI. Use the `IResult` siblings from [Minimal APIs](/docs/minimal-apis) instead.'),
    ),
  ],
};

export const MINIMAL_APIS: DocPage = {
  slug: 'minimal-apis',
  title: 'Minimal APIs',
  icon: 'bi-lightning',
  summary: 'IResult-returning adapters that are native to Minimal APIs and visible to OpenAPI.',
  sections: [
    section(
      'methods',
      'IResult adapters',
      table(
        ['Method', 'Maps', 'Success body'],
        ['`ToResult(httpContext?)`', '`IOperationResult → IResult`', 'none (bodyless status)'],
        ['`ToResult<T>(httpContext?)`', '`IOperationResult<T> → IResult`', 'raw `T` JSON with the real status'],
        ['`ToEnvelopedResult(httpContext?)`', '`IOperationResult → IResult`', 'the full result object'],
        ['`ToProblemResult(httpContext?)`', 'failed result → Problem Details `IResult`', 'not applicable (failures only)'],
      ),
    ),
    section(
      'example',
      'Example',
      cs(`using ResultHandler.AspNetCore.Extensions;
using ResultHandler.Facade;

var products = app.MapGroup("/api/products").WithTags("Products");

products.MapGet("/{id:int}", (int id, ProductService service, HttpContext http)
    => service.GetById(id).ToResult(http));

products.MapPost("/", (CreateProductRequest request, ProductService service) =>
{
    var validation = service.ValidateCreate(request);
    return validation is not null
        ? validation.ToProblemResult()
        : Result.Created(service.Create(request)).ToResult();
});`),
    ),
    section(
      'envelope-minimal',
      'Envelope endpoints',
      cs(`products.MapGet("/{id:int}/envelope", (int id, ProductService service, HttpContext http)
    => service.GetById(id).ToEnvelopedResult(http));`),
      note('Success returns the full result object as JSON; failure returns Problem Details, exactly like `ToResult`.'),
    ),
    section(
      'openapi',
      'Documenting responses for OpenAPI',
      p('Because the adapters return `IResult`, you can describe the possible outcomes with the standard metadata APIs:'),
      cs(`products.MapGet("/{id:int}", (int id, ProductService service, HttpContext http) => service.GetById(id).ToResult(http))
    .Produces<ProductDto>(StatusCodes.Status200OK)
    .ProducesProblem(StatusCodes.Status404NotFound);`),
      tip('Group endpoints per feature (`MapGroup`); it pairs perfectly with a vertical-slice folder layout. See the [vertical slice recipe](/docs/vertical-slice).'),
    ),
    section(
      'problems-convention',
      'Every error response, one call (13.2)',
      p('Declaring each failure on every endpoint gets repetitive. `ProducesResultProblems()` declares the common ones for a whole group: `500` always, `401` / `403` when the endpoint requires authorization (and is not `AllowAnonymous`), and `422` when it reads a request body. Anything an endpoint already declares is left alone.'),
      cs(`var api = app.MapGroup("/api").ProducesResultProblems();

api.MapGet("/products/{id:int}", (int id, ProductService service) => service.GetById(id).ToResult())
    .Produces<ProductDto>()
    .ProducesProblem(StatusCodes.Status404NotFound); // endpoint-specific statuses stay explicit

api.MapPost("/orders", (CreateOrderCommand command, ISender sender) => ...)
    .RequireAuthorization();                         // documents 401, 403, 422 and 500`),
      note('It only adds standard endpoint metadata (`IProducesResponseTypeMetadata`), so there is no extra package, and both `Microsoft.AspNetCore.OpenApi` and Swashbuckle pick it up.'),
    ),
  ],
};

export const PROBLEM_DETAILS: DocPage = {
  slug: 'problem-details',
  title: 'Problem Details (RFC 9457)',
  icon: 'bi-exclamation-octagon',
  summary: 'How failures are rendered, and why the type URLs point at real RFC sections.',
  sections: [
    section(
      'shape',
      'Shape of a failure',
      json(`{
  "type": "https://tools.ietf.org/html/rfc9110#section-15.5.5",
  "title": "Not Found",
  "status": 404,
  "detail": "Product 999 does not exist.",
  "instance": "/api/products/999"
}`),
      list(
        '`type`: a link to the RFC section that **defines** the status, not a generic placeholder.',
        '`title` / `status` / `detail`: straight from the result.',
        '`instance`: the request path, when you pass `HttpContext`.',
        '`errors`: added when the result carries an error list.',
        '`fieldErrors`: added when the result carries per-field errors.',
      ),
    ),
    section(
      'types',
      'Type URIs per RFC',
      table(
        ['Statuses', 'Defined in', 'Example type'],
        ['Most 4xx / 5xx', 'RFC 9110', '`rfc9110#section-15.5.5` (404)'],
        ['423, 424, 507', 'RFC 4918 (WebDAV)', '`rfc4918#section-11.3` (423)'],
        ['425', 'RFC 8470', '`rfc8470#section-5.2`'],
        ['428, 429, 431, 511', 'RFC 6585', '`rfc6585#section-4` (429)'],
        ['451', 'RFC 7725', '`rfc7725#section-3`'],
        ['506 / 508 / 510', 'RFC 2295 / 5842 / 2774', '`rfc5842#section-7.2` (508)'],
        ['1xx / 2xx / 3xx', 'RFC 9457 §4.2.1', '`about:blank` (they are not problems)'],
      ),
    ),
    section(
      'manual',
      'Building it yourself',
      syncedTabs(
        TAB_GROUPS.apiStyle,
        minimalApi(`app.MapGet("/api/products/{id:int}", (int id, ProductService products, HttpContext http) =>
{
    var result = products.GetById(id);
    if (result.IsSuccessful)
    {
        return Results.Ok(result.Data);
    }

    ProblemDetails problem = result.ToProblemDetails(http);
    problem.Extensions["traceId"] = Activity.Current?.Id ?? http.TraceIdentifier;
    return Results.Problem(problem);
});`),
        mvcController(`ProblemDetails problem = result.ToProblemDetails(HttpContext);
problem.Extensions["traceId"] = Activity.Current?.Id ?? HttpContext.TraceIdentifier;
return new ObjectResult(problem) { StatusCode = problem.Status };`),
      ),
      tip('`ToProblemDetails` returns the standard `Microsoft.AspNetCore.Mvc.ProblemDetails`, so you can enrich it with a trace id, a correlation id or a documentation link before returning it.'),
    ),
    section(
      'validation',
      'Validation example',
      json(`{
  "type": "https://tools.ietf.org/html/rfc9110#section-15.5.21",
  "title": "Validation Failed",
  "status": 422,
  "errors": ["Name is required.", "Price must be greater than zero."],
  "fieldErrors": {
    "Name": ["Name is required."],
    "Price": ["Price must be greater than zero."]
  }
}`),
      note('Clients can use `errors` for a summary banner and `fieldErrors` to highlight individual inputs. See [consuming from a client](/docs/client-consumption).'),
    ),
  ],
};
