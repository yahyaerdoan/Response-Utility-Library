using Microsoft.AspNetCore.Mvc;

namespace ResultHandler.Tests.Results;

[ApiController]
[Route("mvc")]
public sealed class ThrowingController : ControllerBase
{
    [HttpGet("throw")]
    public IActionResult Throw() => throw new FormatException("Total is not a number.");
}
