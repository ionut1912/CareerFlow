namespace CareerFlow.Core.Domain.Exceptions;

public class PasswordNotMatchException(string message) : Exception(message);