using CareerFlow.Core.Domain.Exceptions;

using Shared.Domain.Common;

namespace CareerFlow.Core.Domain.ValueObjects;

public class JobStatus : ValueObject
{
    public static readonly JobStatus Pending = new("Pending");
    public static readonly JobStatus Processing = new("Processing");
    public static readonly JobStatus Done = new("Done");
    public static readonly JobStatus Failed = new("Failed");

    private JobStatus(string value)
    {
        Value = value;
    }

    public string Value { get; }

    protected override IEnumerable<object> GetEqualityComponents()
    {
        yield return Value;
    }


    public static JobStatus FromString(string jobStatus)
    {
        return jobStatus.Trim().ToLowerInvariant() switch
        {
            "pending" => Pending,
            "processing" => Processing,
            "done" => Done,
            "failed" => Failed,
            _ => throw new InvalidJobStatusException($"Statusul {jobStatus} e invalid")
        };
    }
}
