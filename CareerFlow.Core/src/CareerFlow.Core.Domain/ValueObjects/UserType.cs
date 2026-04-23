using System.ComponentModel.DataAnnotations.Schema;

using CareerFlow.Core.Domain.Exceptions;

using Shared.Domain.Common;

namespace CareerFlow.Core.Domain.ValueObjects;

[NotMapped]
public class UserType : ValueObject
{
    public static readonly UserType Student = new("Student");
    public static readonly UserType JobSearcher = new("JobSearcher");
    public static readonly UserType HobbyLearner = new("HobbyLearner");


    private UserType(string value)
    {
        Value = value;
    }

    public string Value { get; }

    protected override IEnumerable<object> GetEqualityComponents()
    {
        yield return Value;
    }

    public static UserType FromString(string userType)
    {
        return userType.Trim().ToLowerInvariant() switch
        {
            "student" => Student,
            "jobsearcher" => JobSearcher,
            "hobbylearner" => HobbyLearner,
            _ => throw new InvalidUserTypeException($"Tipul {userType} este invalid")
        };
    }
}
