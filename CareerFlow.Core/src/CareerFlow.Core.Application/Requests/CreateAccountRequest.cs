namespace CareerFlow.Core.Application.Requests
{
    public record CreateAccountRequest(string Email, string Password, string Username,string Name, bool AcceptedPrivacyPolicy, bool AcceptedTermsAndConditions);
}
