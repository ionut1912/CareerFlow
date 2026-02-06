namespace CareerFlow.Core.Domain.Exceptions
{
    public class TokenExpiredException(string message) : Exception(message)
    {
    }
}
