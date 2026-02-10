using System.Text.Json.Serialization;

namespace BankApi.Domain.Entities;

public class CreditCard
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public string CardNo { get; set; } = null!;
    public DateTime ExpireAt { get; set; }

    [JsonIgnore]
    public byte[] CvvHash { get; set; } = Array.Empty<byte>();

    [JsonIgnore]
    public byte[] CvvSalt { get; set; } = Array.Empty<byte>();

    public decimal Limit { get; set; }
    public decimal CurrentDebt { get; set; }
    public bool IsActive { get; set; } = true;

    public Guid CustomerId { get; set; }
    public Customer Customer { get; set; } = null!;
}
