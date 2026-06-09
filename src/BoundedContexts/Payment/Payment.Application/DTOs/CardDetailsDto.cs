namespace Payment.Application.DTOs;

public record CardDetailsDto(string Last4Digits, string CardType, int ExpiryMonth, int ExpiryYear, string CardHolderName);
