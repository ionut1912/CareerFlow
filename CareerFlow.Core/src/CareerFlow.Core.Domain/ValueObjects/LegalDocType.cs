using CareerFlow.Core.Domain.Exceptions;
using Shared.Domain.Common;

namespace CareerFlow.Core.Domain.ValueObjects;

public class LegalDocType : ValueObject
{
    public static readonly LegalDocType TermsOfService = new("TermsOfService");
    public static readonly LegalDocType PrivacyPolicy = new("PrivacyPolicy");

    private LegalDocType(string value)
    {
        Value = value;
    }

    public string Value { get; }

    public static LegalDocType FromString(string value)
    {
        return value.ToLower() switch
        {
            "termsofservice" => TermsOfService,
            "privacypolicy" => PrivacyPolicy,
            _ => throw new InvalidLegalDocTypeException($"Invalid legal document type: {value}")
        };
    }
    protected override IEnumerable<object?> GetEqualityComponents()
    {
        yield return Value;
    }
}
