using BankApi.Domain.Entities;
using BankApi.Domain.Enums;
using BankApi.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace BankApi.Application.Services;

public class CreditCardAutoCollectionService
{
    private readonly BankaDbContext _context;

    public CreditCardAutoCollectionService(BankaDbContext context)
    {
        _context = context;
    }

    public async Task<int> CollectDuePaymentsAsync(DateTime nowUtc, CancellationToken ct)
    {
        await using var dbtx = await _context.Database.BeginTransactionAsync(ct);

        var due = await _context.CreditCardPaymentInstructions
            .Where(x => x.Status == PaymentInstructionStatus.Pending && x.ScheduledAtUtc <= nowUtc)
            .OrderBy(x => x.ScheduledAtUtc)
            .ToListAsync(ct);

        var executedCount = 0;

        foreach (var ins in due)
        {
            var card = await _context.CreditCards.FirstOrDefaultAsync(x => x.Id == ins.CreditCardId, ct);
            var account = await _context.Accounts.FirstOrDefaultAsync(x => x.Id == ins.AccountId, ct);

            if (card is null || account is null)
            {
                ins.Status = PaymentInstructionStatus.Failed;
                ins.FailureReason = "Card or Account not found.";
                continue;
            }

            if (!card.IsActive || !account.IsActive)
            {
                ins.Status = PaymentInstructionStatus.Failed;
                ins.FailureReason = "Card or Account inactive.";
                continue;
            }

            var expUtc = card.ExpireAt.Kind == DateTimeKind.Unspecified
                ? DateTime.SpecifyKind(card.ExpireAt, DateTimeKind.Utc)
                : card.ExpireAt.ToUniversalTime();

            if (expUtc <= nowUtc)
            {
                ins.Status = PaymentInstructionStatus.Failed;
                ins.FailureReason = "Card expired.";
                continue;
            }

            if (ins.Amount <= 0)
            {
                ins.Status = PaymentInstructionStatus.Failed;
                ins.FailureReason = "Invalid amount.";
                continue;
            }

            if (ins.Amount > card.CurrentDebt)
            {
                ins.Status = PaymentInstructionStatus.Failed;
                ins.FailureReason = "Amount exceeds current debt.";
                continue;
            }

            if (account.Balance < ins.Amount)
            {
                ins.Status = PaymentInstructionStatus.Failed;
                ins.FailureReason = "Insufficient balance.";
                continue;
            }

            account.Balance -= ins.Amount;
            card.CurrentDebt -= ins.Amount;

            _context.Transactions.Add(new Transaction
            {
                AccountId = account.Id,
                Type = TransactionType.Withdraw, 
                Amount = ins.Amount,
                Description = $"CreditCardPayment: {card.CardNo}"
            });

            _context.CardTransactions.Add(new CardTransaction
            {
                Type = CardTransactionType.Payment,
                Amount = ins.Amount,
                Description = $"Auto-collection from account {account.Iban}",
                CreditCardId = card.Id,
                AccountId = account.Id,
                CreatedAtUtc = nowUtc
            });

            ins.Status = PaymentInstructionStatus.Executed;
            ins.ExecutedAtUtc = nowUtc;
            ins.FailureReason = null;

            executedCount++;
        }

        await _context.SaveChangesAsync(ct);
        await dbtx.CommitAsync(ct);

        return executedCount;
    }
}
