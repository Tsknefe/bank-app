namespace BankApi.Application.Auth.Dtos;

public class CreditCardPayRequest
{
    public Guid AccountId { get; set; }
    public decimal Amount { get; set; }

    public DateTime? ScheduledAtUtc { get; set; }
}
