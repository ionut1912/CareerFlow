namespace CareerFlow.Core.Domain.Exceptions;

public class UserAlreadyExistsException(string message) : Exception(message);