using System.Text.Json.Serialization;

namespace BankApi.Domain.Entities;

public class DebitCard
{
    public Guid Id { get; set; } = Guid.NewGuid();

    public string CardNo { get; set; } = null!;
    public DateTime ExpireAt { get; set; }
    [JsonIgnore]
    public string Cvv { get; set; } = null!;

    public bool IsActive { get; set; } = true;

    public Guid AccountId { get; set; }
    public Account Account { get; set; } = null!;
}
