namespace BankApi.Application.Auth.Dtos;

public record CreditCardResponse(
    Guid Id,
    string CardNo,
    DateTime ExpireAt,
    decimal Limit,
    decimal CurrentDebt,
    bool IsActive,
    Guid CustomerId
);
