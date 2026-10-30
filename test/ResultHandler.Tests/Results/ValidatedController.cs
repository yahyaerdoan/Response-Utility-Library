using Microsoft.AspNetCore.Mvc;

namespace ResultHandler.Tests.Results;

[ApiController]
[Route("mvc/receipts")]
public sealed class ValidatedController : ControllerBase
{
    [HttpPost]
    public IActionResult Create(ReceiptRequest request) => Ok(request.Number);

    public sealed record ReceiptRequest(string Number, decimal Total);
}
