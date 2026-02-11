namespace BankApi.Application.Auth.Dtos;

public record ConfigureAutoPayRequest(Guid AutoPayAccountId, int DueDay);
