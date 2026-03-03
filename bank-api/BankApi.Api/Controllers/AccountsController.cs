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
        _context.Transactions.Add(new Transaction
        {
            AccountId = account.Id,
            Type = TransactionType.Deposit,
            Amount = req.Amount,
            Description = "Deposit"
        }
            );

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
        _context.Transactions.Add(new Transaction
        {
            AccountId = account.Id,
            Type = TransactionType.Withdraw,
            Amount = req.Amount,
            Description = "Withdraw"
        });


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

        try
        {
            var ids = new[] { req.FromAccountId, req.ToAccountId }.OrderBy(x => x).ToArray();

            var accounts = await _context.Accounts
                .Where(a => ids.Contains(a.Id))
                .ToListAsync(ct);

            if (accounts.Count != 2)
                return NotFound("One or both accounts not found.");

            var from = accounts.Single(a => a.Id == req.FromAccountId);
            var to = accounts.Single(a => a.Id == req.ToAccountId);

            if (!from.IsActive) return BadRequest("From account is not active.");
            if (!to.IsActive) return BadRequest("To account is not active.");

            if (from.Balance < req.Amount)
                return BadRequest("Insufficient balance.");

            from.Balance -= req.Amount;
            to.Balance += req.Amount;

            _context.Transactions.Add(new Transaction
            {
                AccountId = from.Id,
                RelatedAccountId = to.Id,
                Type = TransactionType.TransferOut,
                Amount = req.Amount,
                Description = req.Description ?? "Transfer Out"
            });

            _context.Transactions.Add(new Transaction
            {
                AccountId = to.Id,
                RelatedAccountId = from.Id,
                Type = TransactionType.TransferIn,
                Amount = req.Amount,
                Description = req.Description ?? "Transfer In"
            });

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
        catch (OperationCanceledException)
        {
            await tx.RollbackAsync(CancellationToken.None);
            throw;
        }
        catch (Exception)
        {
            await tx.RollbackAsync(ct);
            throw;
        }
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
    [HttpGet("{id:guid}/transactions")]
    public async Task<IActionResult> GetTransactions(
    [FromRoute] Guid id,
    [FromQuery] int skip = 0,
    [FromQuery] int take = 50,
    CancellationToken ct = default)
    {
        if (take <= 0 || take > 200) take = 50;

        var exists = await _context.Accounts.AnyAsync(x => x.Id == id, ct);
        if (!exists) return NotFound("Account not found.");

        var txs = await _context.Transactions
            .Where(x => x.AccountId == id)
            .OrderByDescending(x => x.CreatedAt)
            .Skip(skip)
            .Take(take)
            .ToListAsync(ct);

        return Ok(txs);
    }

}
