namespace CareerFlow.Core.Domain.Exceptions;

public sealed class OpenAIException(int statusCode, string message) : Exception(message)
{
    public int StatusCode { get; } = statusCode;
}