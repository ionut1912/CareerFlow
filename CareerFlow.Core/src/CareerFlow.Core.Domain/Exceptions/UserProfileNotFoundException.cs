namespace CareerFlow.Core.Domain.Exceptions;

public class UserProfileNotFoundException(string message) : Exception(message)
{
}