using System.Text.Json.Serialization;
using CareerFlow.Core.Application.Dtos;
using CareerFlow.Core.Application.Requests.Account;
using CareerFlow.Core.Application.Requests.Course;
using CareerFlow.Core.Application.Requests.LegalDoc;
using CareerFlow.Core.Application.Requests.UserProfile;
using ForgotPasswordRequest = Microsoft.AspNetCore.Identity.Data.ForgotPasswordRequest;
using LoginRequest = Microsoft.AspNetCore.Identity.Data.LoginRequest;
using ResetPasswordRequest = Microsoft.AspNetCore.Identity.Data.ResetPasswordRequest;


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
[JsonSerializable(typeof(CourseDto))]
[JsonSerializable(typeof(List<CourseDto>))]
[JsonSerializable(typeof(ChapterDto))]
[JsonSerializable(typeof(List<ChapterDto>))]
[JsonSerializable(typeof(SubChapterDto))]
[JsonSerializable(typeof(CourseRequest))]
[JsonSerializable(typeof(FinishChapterRequest))]
[JsonSerializable(typeof(UploadCourseDocumentRequest))]
[JsonSerializable(typeof(List<SubChapterDto>))]
[JsonSourceGenerationOptions(
    PropertyNamingPolicy = JsonKnownNamingPolicy.CamelCase,
    DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull)]
public partial class CareerFlowJsonContext : JsonSerializerContext
{
}