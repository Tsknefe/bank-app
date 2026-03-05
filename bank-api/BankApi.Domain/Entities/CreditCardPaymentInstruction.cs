using BankApi.Domain.Enums;

namespace BankApi.Domain.Entities;

public class CreditCardPaymentInstruction
{
    public Guid Id { get; set; } = Guid.NewGuid();

    public Guid CreditCardId { get; set; }
    public CreditCard CreditCard { get; set; } = null!;

    public Guid AccountId { get; set; }
    public Account Account { get; set; } = null!;

    public decimal Amount { get; set; }

    public DateTime ScheduledAtUtc { get; set; }

    public PaymentInstructionStatus Status { get; set; } = PaymentInstructionStatus.Pending;

    public DateTime CreatedAtUtc { get; set; } = DateTime.UtcNow;
    public DateTime? ExecutedAtUtc { get; set; }

    public string? FailureReason { get; set; }
}
