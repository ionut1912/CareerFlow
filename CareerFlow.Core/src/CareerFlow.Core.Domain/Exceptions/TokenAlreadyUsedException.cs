namespace CareerFlow.Core.Domain.Exceptions;

public class TokenAlreadyUsedException(string message) : Exception(message);
