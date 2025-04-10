namespace SkiLift;

/// <summary>
/// Marker interface for requests that return a response of type <typeparamref name="TResponse"/>.
/// </summary>
/// <typeparam name="TResponse">The response type.</typeparam>
public interface IRequest<out TResponse>
{
}

/// <summary>
/// Marker interface for requests that do not return a response.
/// </summary>
public interface IRequest
{
}


/// <summary>
/// Marker interface for commands that return a response of type <typeparamref name="TResponse"/>.
/// </summary>
/// <typeparam name="TResponse">The response type.</typeparam>
public interface ICommand<out TResponse> : IRequest<TResponse>
{
}

/// <summary>
/// Marker interface for commands that do not return a response.
/// </summary>
public interface ICommand : IRequest
{
}

/// <summary>
/// Marker interface for queries that return a response of type <typeparamref name="TResponse"/>.
/// </summary>
/// <typeparam name="TResponse">The response type.</typeparam>
public interface IQuery<out TResponse> : IRequest<TResponse>
{
}
