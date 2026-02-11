namespace CareerFlow.Core.Application.Validators.LegalDocs;

public static class LegalDocValidationExtensions
{
    public static readonly string[] AllowedTypes = ["PrivacyPolicy", "TermsOfService"];
    public static bool IsValidLegalDocType(this string value)
    {
        if (string.IsNullOrWhiteSpace(value))
            return false;

        if (AllowedTypes == null)
            return false;

        return AllowedTypes.Contains(value, StringComparer.OrdinalIgnoreCase);
    }

}
