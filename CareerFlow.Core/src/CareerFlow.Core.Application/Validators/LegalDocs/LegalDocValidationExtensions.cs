namespace CareerFlow.Core.Application.Validators.LegalDocs;

public static class LegalDocValidationExtensions
{
    public static readonly string[] _allowedTypes = ["PrivacyPolicy", "TermsOfService"];
    public static bool IsValid(this string value)
    {
        if (string.IsNullOrWhiteSpace(value))
            return false;

        if (_allowedTypes == null)
            return false;

        return _allowedTypes.Contains(value, StringComparer.OrdinalIgnoreCase);
    }

}
