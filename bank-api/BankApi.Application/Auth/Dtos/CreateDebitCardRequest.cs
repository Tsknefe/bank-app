namespace BankApi.Application.Auth.Dtos;

public class CreateDebitCardRequest
{
    public Guid AccountId { get; set; }
    public string CardNo { get; set; } = null!;
    public string Cvv { get; set; } = null!;
    public DateTime ExpireAt { get; set; } 
}
