namespace CareerFlow.Core.Domain.Exceptions;

public class PasswordNotEmptyException(string message) : Exception(message)
{
}
