namespace CareerFlow.Core.Application.Requests
{
    public record CreateAccountRequest(string Email, string Password, string Username, bool AcceptedPrivacyPolicy, bool AcceptedTermsAndConditions);
}
