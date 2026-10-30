using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc;

namespace ResultHandler.Tests.Results;

[ApiController]
[Route("mvc/orders")]
public sealed class ModelStateController : ControllerBase
{
    [HttpPost]
    public IActionResult Create(OrderRequest request) => Ok(request.ServiceId);

    public sealed record OrderRequest(Guid ServiceId, [Range(1, 10)] int Quantity);
}
