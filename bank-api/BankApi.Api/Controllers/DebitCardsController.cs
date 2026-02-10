using BankApi.Application.Auth.Dtos;
using BankApi.Domain.Entities;
using BankApi.Infrastructure.Persistence;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace BankApi.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class DebitCardsController : ControllerBase
{
    private readonly BankaDbContext _context;
    public DebitCardsController(BankaDbContext context) => _context = context;
    public record DebitCardResponse(Guid Id, string CardNo, DateTime ExpireAt, bool IsActive, Guid AccountId);


    [HttpPost]
    public async Task<IActionResult> Create([FromBody] CreateDebitCardRequest req, CancellationToken ct)
    {
        if (req.AccountId == Guid.Empty)
            return BadRequest("AccountId zorunludur.");

        if (string.IsNullOrWhiteSpace(req.CardNo) || req.CardNo.Trim().Length != 16)
            return BadRequest("CardNo 16 haneli olmalıdır.");

        if (string.IsNullOrWhiteSpace(req.Cvv) || req.Cvv.Trim().Length != 3)
            return BadRequest("CVV 3 haneli olmalıdır.");

        var account = await _context.Accounts.FirstOrDefaultAsync(x => x.Id == req.AccountId, ct);
        if (account is null) return NotFound("Account bulunamadı.");
        if (!account.IsActive) return BadRequest("Account aktif değil.");

        var cardNo = req.CardNo.Trim();
        var exists = await _context.DebitCards.AnyAsync(x => x.CardNo == cardNo, ct);
        if (exists) return Conflict("Bu CardNo zaten kayıtlı.");

        var expireUtc = req.ExpireAt.Kind == DateTimeKind.Unspecified
            ? DateTime.SpecifyKind(req.ExpireAt, DateTimeKind.Utc)
            : req.ExpireAt.ToUniversalTime();

        var card = new DebitCard
        {
            AccountId = req.AccountId,
            CardNo = cardNo,
            Cvv = req.Cvv.Trim(),
            ExpireAt = expireUtc,
            IsActive = true
        };

        _context.DebitCards.Add(card);
        await _context.SaveChangesAsync(ct);

        return CreatedAtAction(nameof(GetById), new { id = card.Id },
            new DebitCardResponse(card.Id, card.CardNo, card.ExpireAt, card.IsActive, card.AccountId));
    }

    [HttpGet("{id:guid}")]
    public async Task<IActionResult> GetById([FromRoute] Guid id, CancellationToken ct)
    {
        var card = await _context.DebitCards.FirstOrDefaultAsync(x => x.Id == id, ct);
        if (card is null) return NotFound();
        return Ok(card);
    }

    [HttpPatch("{id:guid}/deactivate")]
    public async Task<IActionResult> Deactivate([FromRoute] Guid id, CancellationToken ct)
    {
        var card = await _context.DebitCards.FirstOrDefaultAsync(x => x.Id == id, ct);
        if (card is null) return NotFound();

        card.IsActive = false;
        await _context.SaveChangesAsync(ct);

        return Ok(new { card.Id, card.IsActive, message = "DebitCard deactivated" });
    }

    [HttpPatch("{id:guid}/activate")]
    public async Task<IActionResult> Activate([FromRoute] Guid id, CancellationToken ct)
    {
        var card = await _context.DebitCards.FirstOrDefaultAsync(x => x.Id == id, ct);
        if (card is null) return NotFound();

        card.IsActive = true;
        await _context.SaveChangesAsync(ct);

        return Ok(new { card.Id, card.IsActive, message = "DebitCard activated" });
    }
}
