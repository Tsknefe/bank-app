using BankApi.Domain.Enums;

namespace BankApi.Domain.Entities;

public class CardTransaction
{
    public Guid Id { get; set; } = Guid.NewGuid();

    public CardTransactionType Type { get; set; }

    public decimal Amount { get; set; }
    public string? Description { get; set; }

    public DateTime CreatedAtUtc { get; set; } = DateTime.UtcNow;

    public Guid? CreditCardId { get; set; }
    public CreditCard? CreditCard { get; set; }

    public Guid? DebitCardId { get; set; }
    public DebitCard? DebitCard { get; set; }

    public Guid? AccountId { get; set; }
    public Account? Account { get; set; }
}
