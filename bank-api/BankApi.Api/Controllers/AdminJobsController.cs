using BankApi.Application.Services;
using Microsoft.AspNetCore.Mvc;

namespace BankApi.Api.Controllers;

[ApiController]
[Route("api/admin/jobs")]
public class AdminJobsController : ControllerBase
{
    private readonly CreditCardAutoCollectionService _svc;

    public AdminJobsController(CreditCardAutoCollectionService svc)
    {
        _svc = svc;
    }

    [HttpPost("creditcard-autocollect/run")]
    public async Task<IActionResult> RunAutoCollect(CancellationToken ct)
    {
        var nowUtc = DateTime.UtcNow;
        var count = await _svc.CollectDuePaymentsAsync(nowUtc, ct);

        return Ok(new
        {
            executedCount = count,
            nowUtc
        });
    }
}
