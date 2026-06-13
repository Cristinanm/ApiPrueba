namespace ApiPrueba.Services;

public sealed class BusinessRuleException(string message) : Exception(message);

public sealed class EntityNotFoundException(string message) : Exception(message);
