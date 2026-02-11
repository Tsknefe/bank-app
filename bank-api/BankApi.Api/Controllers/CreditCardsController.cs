using BankApi.Application.Auth.Dtos;
using BankApi.Domain.Entities;
using BankApi.Domain.Enums;
using BankApi.Infrastructure.Persistence;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace BankApi.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class CreditCardsController : ControllerBase
{
    private readonly BankaDbContext _context;

    public CreditCardsController(BankaDbContext context)
    {
        _context = context;
    }

    [HttpPost]
    public async Task<IActionResult> Create([FromBody] CreateCreditCardRequest req, CancellationToken ct)
    {
        if (req.CustomerId == Guid.Empty)
            return BadRequest("CustomerId zorunludur.");

        if (string.IsNullOrWhiteSpace(req.CardNo) || req.CardNo.Trim().Length != 16)
            return BadRequest("CardNo 16 haneli olmalıdır.");

        if (string.IsNullOrWhiteSpace(req.Cvv) || req.Cvv.Trim().Length != 3)
            return BadRequest("CVV 3 haneli olmalıdır.");

        if (req.Limit <= 0)
            return BadRequest("Limit 0'dan büyük olmalıdır.");

        var customerExists = await _context.Customers
            .AnyAsync(x => x.Id == req.CustomerId && x.IsActive, ct);

        if (!customerExists)
            return NotFound("Customer bulunamadı veya pasif.");

        var cardNo = req.CardNo.Trim();

        var cardNoExists = await _context.CreditCards
            .AnyAsync(x => x.CardNo == cardNo, ct);

        if (cardNoExists)
            return Conflict("Bu CardNo zaten kayıtlı.");

        var (hash, salt) = BankApi.Application.Security.CvvHasher.Hash(req.Cvv.Trim());

        var expireUtc = req.ExpireAt.Kind == DateTimeKind.Unspecified
            ? DateTime.SpecifyKind(req.ExpireAt, DateTimeKind.Utc)
            : req.ExpireAt.ToUniversalTime();

        var card = new CreditCard
        {
            CustomerId = req.CustomerId,
            CardNo = cardNo,
            CvvHash = hash,
            CvvSalt = salt,
            ExpireAt = expireUtc,
            Limit = req.Limit,
            CurrentDebt = 0m,
            IsActive = true
        };

        _context.CreditCards.Add(card);
        await _context.SaveChangesAsync(ct);

        var res = ToResponse(card);
        return CreatedAtAction(nameof(GetById), new { id = card.Id }, res);
    }

    [HttpGet("{id:guid}")]
    public async Task<IActionResult> GetById([FromRoute] Guid id, CancellationToken ct)
    {
        var card = await _context.CreditCards.FirstOrDefaultAsync(x => x.Id == id, ct);
        if (card is null) return NotFound();

        return Ok(ToResponse(card));
    }

    [HttpGet("by-customer/{customerId:guid}")]
    public async Task<IActionResult> GetByCustomer([FromRoute] Guid customerId, CancellationToken ct)
    {
        var cards = await _context.CreditCards
            .Where(x => x.CustomerId == customerId)
            .OrderByDescending(x => x.Id)
            .ToListAsync(ct);

        return Ok(cards.Select(ToResponse));
    }

    [HttpPatch("{id:guid}/deactivate")]
    public async Task<IActionResult> Deactivate([FromRoute] Guid id, CancellationToken ct)
    {
        var card = await _context.CreditCards.FirstOrDefaultAsync(x => x.Id == id, ct);
        if (card is null) return NotFound();

        card.IsActive = false;
        await _context.SaveChangesAsync(ct);

        return Ok(new { card.Id, card.IsActive, message = "CreditCard deactivated." });
    }

    [HttpPatch("{id:guid}/activate")]
    public async Task<IActionResult> Activate([FromRoute] Guid id, CancellationToken ct)
    {
        var card = await _context.CreditCards.FirstOrDefaultAsync(x => x.Id == id, ct);
        if (card is null) return NotFound();

        card.IsActive = true;
        await _context.SaveChangesAsync(ct);

        return Ok(new { card.Id, card.IsActive, message = "CreditCard activated." });
    }

    [HttpPost("{id:guid}/spend")]
    public async Task<IActionResult> Spend([FromRoute] Guid id, [FromBody] CreditCardSpendRequest req, CancellationToken ct)
    {
        if (req.Amount <= 0)
            return BadRequest("Amount 0'dan büyük olmalıdır.");

        await using var tx = await _context.Database.BeginTransactionAsync(ct);

        var card = await _context.CreditCards.FirstOrDefaultAsync(x => x.Id == id, ct);
        if (card is null) return NotFound("CreditCard not found.");
        if (!card.IsActive) return BadRequest("CreditCard is not active.");

        var newDebt = card.CurrentDebt + req.Amount;
        if (newDebt > card.Limit)
            return BadRequest($"Limit exceeded. Limit={card.Limit}, CurrentDebt={card.CurrentDebt}, Amount={req.Amount}");

        card.CurrentDebt = newDebt;
        _context.CardTransactions.Add(new CardTransaction
        {
            Type = BankApi.Domain.Enums.CardTransactionType.Spend,
            Amount = req.Amount,
            Description = req.Description ?? "CreditCard Spend",
            CreditCardId = card.Id,
            AccountId = null,
            CreatedAtUtc = DateTime.UtcNow
        });


        await _context.SaveChangesAsync(ct);
        await tx.CommitAsync(ct);

        return Ok(new
        {
            cardId = card.Id,
            amount = req.Amount,
            card.Limit,
            card.CurrentDebt,
            message = "Spend successful."
        });
    }



    [HttpPost("{id:guid}/pay")]
    public async Task<IActionResult> Pay([FromRoute] Guid id, [FromBody] CreditCardPayRequest req, CancellationToken ct)
    {
        if (req.Amount <= 0)
            return BadRequest("Amount must be more from zero");

        var card = await _context.CreditCards.FirstOrDefaultAsync(x => x.Id == id, ct);
        if (card is null) return NotFound("CreditCard not found.");
        if (!card.IsActive) return BadRequest("CreditCard is not active.");

        var nowUtc = DateTime.UtcNow;

        var expUtc = card.ExpireAt.Kind == DateTimeKind.Unspecified
            ? DateTime.SpecifyKind(card.ExpireAt, DateTimeKind.Utc)
            : card.ExpireAt.ToUniversalTime();
        if (expUtc <= nowUtc) return BadRequest("CreditCard expired.");

        if (req.Amount > card.CurrentDebt)
            return BadRequest($"Payment exceeds debt. Debt={card.CurrentDebt}, Amount={req.Amount}");

        if (card.AutoPayAccountId is null || card.AutoPayAccountId == Guid.Empty)
            return BadRequest("AutoPayAccountId is not configured. First configure autopay.");

        if (card.DueDay < 1 || card.DueDay > 28)
            return BadRequest("DueDay invalid its must between the 1 and 28");

        var account = await _context.Accounts.FirstOrDefaultAsync(x => x.Id == card.AutoPayAccountId.Value, ct);
        if (account is null) return NotFound("Account not found.");
        if (!account.IsActive) return BadRequest("Account is not active.");

        var scheduledAtUtc =NextDueDateUtc(card.DueDay, nowUtc);

        var exists = await _context.CreditCardPaymentInstructions.AnyAsync(x =>
            x.CreditCardId == card.Id &&
            x.Status == PaymentInstructionStatus.Pending &&
            x.ScheduledAtUtc == scheduledAtUtc, ct);

        if (exists)
            return Conflict("The payment request already exist");

        var instruction = new CreditCardPaymentInstruction
        {
            CreditCardId = card.Id,
            AccountId = account.Id,
            Amount = req.Amount,
            ScheduledAtUtc = scheduledAtUtc,
            Status = PaymentInstructionStatus.Pending
        };

        _context.CreditCardPaymentInstructions.Add(instruction);
        await _context.SaveChangesAsync(ct);

        return Ok(new
        {
            instructionId = instruction.Id,
            cardId = card.Id,
            autoPayAccountId=account.Id,
            amount = instruction.Amount,
            scheduledAtUtc = instruction.ScheduledAtUtc,
            status = instruction.Status.ToString(),
            message = "Payment instruction created. It will be collected on the scheduled date if balance is sufficient."
        });
    }

    static DateTime NextDueDateUtc(int dueDay, DateTime nowUtc)
    {
        var thisMonthDue = new DateTime(nowUtc.Year, nowUtc.Month, dueDay, 0, 0, 0, DateTimeKind.Utc);
        if (nowUtc <= thisMonthDue) return thisMonthDue;

        var next = nowUtc.AddMonths(1);
        return new DateTime(next.Year, next.Month, dueDay, 0, 0, 0, DateTimeKind.Utc);
    }

    private static CreditCardResponse ToResponse(CreditCard x) =>
            new(x.Id, x.CardNo, x.ExpireAt, x.Limit, x.CurrentDebt, x.IsActive, x.CustomerId);

    [HttpPatch("{id:guid}/autopay")]
    public async Task<IActionResult> ConfigureAutoPay([FromRoute] Guid id, [FromBody] ConfigureAutoPayRequest req, CancellationToken ct)
    {
        if (req.AutoPayAccountId == Guid.Empty)
            return BadRequest("AutoPayAccountId zorunludur.");

        if (req.DueDay < 1 || req.DueDay > 28)
            return BadRequest("DueDay 1-28 arasında olmalıdır.");

        var card = await _context.CreditCards.FirstOrDefaultAsync(x => x.Id == id, ct);
        if (card is null) return NotFound("CreditCard not found.");

        var account = await _context.Accounts.FirstOrDefaultAsync(x => x.Id == req.AutoPayAccountId, ct);
        if (account is null) return NotFound("Account not found.");
        if (!account.IsActive) return BadRequest("Account is not active.");

        if (account.CustomerId != card.CustomerId)
            return BadRequest("AutoPay account must belong to the same customer.");

        card.AutoPayAccountId = account.Id;
        card.DueDay = req.DueDay;

        await _context.SaveChangesAsync(ct);

        return Ok(new
        {
            cardId = card.Id,
            card.AutoPayAccountId,
            card.DueDay,
            message = "AutoPay configured."
        });
    }

}
