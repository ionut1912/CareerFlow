namespace CareerFlow.Core.Domain.Exceptions
{
    public class TokenRevokedException(string message) : Exception(message)
    {
    }
}
