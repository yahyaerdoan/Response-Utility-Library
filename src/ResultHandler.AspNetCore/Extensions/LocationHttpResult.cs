using Microsoft.AspNetCore.Http;

namespace ResultHandler.AspNetCore.Extensions;

/// <summary>Minimal API success result that writes the <c>Location</c> header, then the inner result. Status and value pass through for tests.</summary>
internal sealed class LocationHttpResult(IResult inner, string location) : IResult, IStatusCodeHttpResult, IValueHttpResult
{
    public IResult Inner { get; } = inner;

    public string Location { get; } = location;

    public int? StatusCode => (Inner as IStatusCodeHttpResult)?.StatusCode;

    public object? Value => (Inner as IValueHttpResult)?.Value;

    public Task ExecuteAsync(HttpContext httpContext)
    {
        ArgumentNullException.ThrowIfNull(httpContext);
        httpContext.Response.Headers.Location = Location;
        return Inner.ExecuteAsync(httpContext);
    }
}
