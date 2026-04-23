namespace CareerFlow.Core.Domain.Exceptions;

public class DomainAlreadyExistsException(string message) : Exception(message);
