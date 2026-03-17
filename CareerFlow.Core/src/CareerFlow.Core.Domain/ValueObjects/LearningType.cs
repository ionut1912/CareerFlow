using System.ComponentModel.DataAnnotations.Schema;
using CareerFlow.Core.Domain.Exceptions;
using Shared.Domain.Common;

namespace CareerFlow.Core.Domain.ValueObjects;

[NotMapped]
public class LearningType : ValueObject
{
    public static readonly LearningType Visual = new("Visual");
    public static readonly LearningType Auditory = new("Auditory");
    public static readonly LearningType ReadWrite = new("ReadWrite");
    public static readonly LearningType Combined = new("Combined");

    private LearningType(string value)
    {
        Value = value;
    }

    public string Value { get; }

    protected override IEnumerable<object> GetEqualityComponents()
    {
        yield return Value;
    }

    public static LearningType FromString(string learningType)
    {
        return learningType.ToLower() switch
        {
            "visual" => Visual,
            "auditory" => Auditory,
            "readwrite" => ReadWrite,
            "combined" => Combined,
            _ => throw new InvalidLearningTypeException($"Tipul de invatare {learningType} e invalid")
        };
    }
}