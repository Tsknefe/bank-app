namespace BankApi.Application.Auth.Dtos;

public class CreateCreditCardRequest
{
    public Guid CustomerId { get; set; }
    public string CardNo { get; set; } = null!;
    public string Cvv { get; set; } = null!;
    public DateTime ExpireAt { get; set; }
    public decimal Limit { get; set; }
    public int DueDay = 1;
    public Guid? AutoPayAccountId = null;
}
