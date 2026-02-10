namespace BankApi.Application.Auth.Dtos;

public class CreditCardSpendRequest
{
    public decimal Amount { get; set; }
    public string? Description { get; set; }
}
