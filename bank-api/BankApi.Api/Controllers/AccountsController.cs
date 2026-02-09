using BankApi.Application.Auth.Dtos;
using BankApi.Domain.Entities;
using BankApi.Domain.Enums;
using BankApi.Infrastructure.Persistence;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Cryptography.X509Certificates;

namespace BankApi.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class AccountsController : ControllerBase
{
    private readonly BankaDbContext _context;

    public AccountsController(BankaDbContext context)
    {
        _context = context;
    }


    [HttpPost]
    public async Task<IActionResult> Create([FromBody] CreateAccountRequest req, CancellationToken ct)
    {
        var customerExists = await _context.Customers
            .AnyAsync(x => x.Id == req.CustomerId && x.IsActive, ct);

        if (!customerExists)
            return NotFound("Customer have not exist or customer is non active ");

        var ibanExists = await _context.Accounts
            .AnyAsync(x => x.Iban == req.Iban, ct);

        if (ibanExists)
            return Conflict("Bu IBAN zaten kayıtlı.");

        var account = new Account
        {
            Name = req.Name.Trim(),
            Iban = req.Iban.Trim(),
            AccountType = req.AccountType,
            CustomerId = req.CustomerId,
            Balance = 0,
            IsActive = true
        };

        _context.Accounts.Add(account);
        await _context.SaveChangesAsync(ct);

        return CreatedAtAction(nameof(GetById), new { id = account.Id }, account);
    }

    [HttpGet("{id:guid}")]
    public async Task<IActionResult> GetById([FromRoute] Guid id, CancellationToken ct)
    {
        var account = await _context.Accounts.FirstOrDefaultAsync(x => x.Id == id, ct);
        if (account is null) return NotFound();

        return Ok(account);
    }

    [HttpGet("by-customer/{customerId:guid}")]
    public async Task<IActionResult> GetByCustomer([FromRoute] Guid customerId, CancellationToken ct)
    {
        var accounts = await _context.Accounts
            .Where(x => x.CustomerId == customerId)
            .OrderByDescending(x => x.Id)
            .ToListAsync(ct);

        return Ok(accounts);
    }
    [HttpPost("{id:guid}/deposit")]
    public async Task<IActionResult> Deposit([FromRoute] Guid id, [FromBody] MoneyRequest req, CancellationToken ct)
    {
        if (req.Amount <= 0)
            return BadRequest("Amount must be greater than 0.");

        var account = await _context.Accounts.FirstOrDefaultAsync(x => x.Id == id, ct);
        if (account is null) return NotFound("Account not found.");

        if (!account.IsActive)
            return BadRequest("Account is not active.");

        account.Balance += req.Amount;

        await _context.SaveChangesAsync(ct);

        return Ok(new
        {
            account.Id,
            account.Iban,
            account.Balance,
            message = "Deposit successful."
        });
    }
    [HttpPost("{id:guid}/withdraw")]
    public async Task<IActionResult> Withdraw([FromRoute] Guid id, [FromBody] MoneyRequest req, CancellationToken ct)
    {
        if (req.Amount <= 0)
            return BadRequest("Amount must be greater than 0.");

        var account = await _context.Accounts.FirstOrDefaultAsync(x => x.Id == id, ct);
        if (account is null) return NotFound("Account not found.");

        if (!account.IsActive)
            return BadRequest("Account is not active.");

        if (account.Balance < req.Amount)
            return BadRequest("Insufficient balance.");

        account.Balance -= req.Amount;

        await _context.SaveChangesAsync(ct);

        return Ok(new
        {
            account.Id,
            account.Iban,
            account.Balance,
            message = "Withdraw successful."
        });
    }
    [HttpPost("transfer")]
    public async Task<IActionResult> Transfer([FromBody] TransferRequest req, CancellationToken ct)
    {
        if (req.Amount <= 0)
            return BadRequest("Amount must be greater than 0.");

        if (req.FromAccountId == req.ToAccountId)
            return BadRequest("From and To accounts cannot be the same.");

        await using var tx = await _context.Database.BeginTransactionAsync(ct);

        var from = await _context.Accounts.FirstOrDefaultAsync(x => x.Id == req.FromAccountId, ct);
        if (from is null) return NotFound("From account not found.");
        if (!from.IsActive) return BadRequest("From account is not active.");

        var to = await _context.Accounts.FirstOrDefaultAsync(x => x.Id == req.ToAccountId, ct);
        if (to is null) return NotFound("To account not found.");
        if (!to.IsActive) return BadRequest("To account is not active.");

        if (from.Balance < req.Amount)
            return BadRequest("Insufficient balance.");

        from.Balance -= req.Amount;
        to.Balance += req.Amount;

        await _context.SaveChangesAsync(ct);
        await tx.CommitAsync(ct);

        return Ok(new
        {
            fromAccountId = from.Id,
            toAccountId = to.Id,
            amount = req.Amount,
            fromBalance = from.Balance,
            toBalance = to.Balance,
            message = "Transfer successful."
        });
    }
    [HttpPatch("{id:guid}/deactivate")]
    public async Task<IActionResult> Deactivate(Guid id, CancellationToken ct)
    {
        var account = await _context.Accounts.FirstOrDefaultAsync(x => x.Id == id, ct);
        if (account is null) return NotFound();

        account.IsActive = false;
        await _context.SaveChangesAsync(ct);

        return Ok(new { account.Id, account.IsActive });

    }
    [HttpPatch("{id:guid}/activate")]
    public async Task<IActionResult> Activate(Guid id, CancellationToken ct)
    {
        var account = await _context.Accounts.FirstOrDefaultAsync(x => x.Id == id, ct);
        if (account is null) return NotFound();

        account.IsActive = true;
        await _context.SaveChangesAsync(ct); return Ok(new { account.Id, account.IsActive });
    }
}
