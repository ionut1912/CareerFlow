namespace CareerFlow.Core.Domain.Exceptions;

public class DocumentEtagExistsException(string message) : Exception(message);