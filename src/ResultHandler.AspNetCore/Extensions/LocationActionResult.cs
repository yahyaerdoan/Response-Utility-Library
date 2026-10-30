using Microsoft.AspNetCore.Mvc;

namespace ResultHandler.AspNetCore.Extensions;

/// <summary>MVC success result that writes the <c>Location</c> header, then the inner result.</summary>
internal sealed class LocationActionResult(IActionResult inner, string location) : IActionResult
{
    public IActionResult Inner { get; } = inner;

    public string Location { get; } = location;

    public Task ExecuteResultAsync(ActionContext context)
    {
        ArgumentNullException.ThrowIfNull(context);
        context.HttpContext.Response.Headers.Location = Location;
        return Inner.ExecuteResultAsync(context);
    }
}
