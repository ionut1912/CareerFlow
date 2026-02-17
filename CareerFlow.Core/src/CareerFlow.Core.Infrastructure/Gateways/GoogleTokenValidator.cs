using CareerFlow.Core.Domain.Abstractions.Gateways;
using CareerFlow.Core.Domain.Abstractions.Gateways.Dtos;
using Google.Apis.Auth;

namespace CareerFlow.Core.Infrastructure.Gateways;

public class GoogleTokenValidator : IGoogleTokenValidator
{
    public async Task<GoogleUserDto> ValidateIdTokenAsync(string idToken, string clientId)
    {
        var settings = new GoogleJsonWebSignature.ValidationSettings
        {
            Audience = [clientId]
        };

        var payload = await GoogleJsonWebSignature.ValidateAsync(idToken, settings);

        return new GoogleUserDto(payload.Email, payload.GivenName);
    }
}