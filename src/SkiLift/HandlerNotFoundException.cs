namespace SkiLift;

/// <summary>
/// Exception thrown when no handler is found for a given request type.
/// </summary>
public class HandlerNotFoundException(Type requestType) : Exception($"No handler found for request of type {requestType.Name}")
{
}
