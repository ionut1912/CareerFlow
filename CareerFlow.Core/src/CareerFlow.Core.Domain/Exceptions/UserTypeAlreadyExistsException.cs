namespace CareerFlow.Core.Domain.Exceptions;

public class UserTypeAlreadyExistsException(string message) : Exception(message)
{
}