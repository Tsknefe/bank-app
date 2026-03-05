using BankApi.Application.Services;
using Microsoft.AspNetCore.Mvc;

namespace BankApi.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class AutoCollectionController : ControllerBase
{
    private readonly CreditCardAutoCollectionService _service;

    public AutoCollectionController(CreditCardAutoCollectionService service)
    {
        _service = service;
    }

    [HttpPost("run")]
    public async Task<IActionResult> Run(CancellationToken ct)
    {
        var nowUtc = DateTime.UtcNow;
        var executed = await _service.CollectDuePaymentsAsync(nowUtc, ct);

        return Ok(new
        {
            nowUtc,
            executed,
            message = "Auto-collection executed for due pending instructions."
        });
    }
}
