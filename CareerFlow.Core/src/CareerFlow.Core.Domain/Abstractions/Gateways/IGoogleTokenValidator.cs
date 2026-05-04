using CareerFlow.Core.Domain.Abstractions.Gateways.Dtos;

namespace CareerFlow.Core.Domain.Abstractions.Gateways;

public interface IGoogleTokenValidator
{
    Task<GoogleUserDto> ValidateIdTokenAsync(string idToken, string clientId);
}
