using System.Text.Json.Serialization;
using CareerFlow.Core.Application.Dtos;
using CareerFlow.Core.Application.Requests.Account;
using CareerFlow.Core.Application.Requests.LegalDoc;
using CareerFlow.Core.Application.Requests.UserProfile;

namespace CareerFlow.Core.Application.Serialization;

[JsonSerializable(typeof(UserProfileDto))]
[JsonSerializable(typeof(List<UserProfileDto>))]
[JsonSerializable(typeof(AccountDto))]
[JsonSerializable(typeof(List<AccountDto>))]
[JsonSerializable(typeof(RefreshTokenDto))]
[JsonSerializable(typeof(CreateUserProfileRequest))]
[JsonSerializable(typeof(AcceptLegalDocRequest))]
[JsonSerializable(typeof(CreateAccountRequest))]
[JsonSerializable(typeof(ForgotPasswordRequest))]
[JsonSerializable(typeof(LoginRequest))]
[JsonSerializable(typeof(RefreshTokenRequest))]
[JsonSerializable(typeof(ResetPasswordRequest))]
[JsonSerializable(typeof(UpdateUserProfileRequest))]
[JsonSourceGenerationOptions(
    PropertyNamingPolicy = JsonKnownNamingPolicy.CamelCase,
    DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull)]
public partial class CareerFlowJsonContext : JsonSerializerContext
{
}