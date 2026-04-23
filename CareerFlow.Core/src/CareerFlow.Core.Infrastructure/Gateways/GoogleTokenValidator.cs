using CareerFlow.Core.Domain.Abstractions.Gateways;
using CareerFlow.Core.Domain.Abstractions.Gateways.Dtos;

using Google.Apis.Auth;

using JetBrains.Annotations;

namespace CareerFlow.Core.Infrastructure.Gateways;

[UsedImplicitly]
public class GoogleTokenValidator : IGoogleTokenValidator
{
    public async Task<GoogleUserDto> ValidateIdTokenAsync(string idToken, string clientId)
    {
        var settings = new GoogleJsonWebSignature.ValidationSettings { Audience = [clientId] };

        GoogleJsonWebSignature.Payload? payload = await GoogleJsonWebSignature.ValidateAsync(idToken, settings);

        return new GoogleUserDto(payload.Email, payload.GivenName);
    }
}
