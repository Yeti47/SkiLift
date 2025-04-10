namespace SkiLift;

/// <summary>
/// Interface for dispatching requests to their handlers.
/// </summary>
public interface IRequestDispatcher
{
    /// <summary>
    /// Sends a request and returns the response.
    /// </summary>
    /// <typeparam name="TResponse">The response type.</typeparam>
    /// <param name="request">The request to send.</param>
    /// <param name="cancellationToken">A token to cancel the operation.</param>
    /// <returns>The response from the handler.</returns>
    /// <exception cref="HandlerNotFoundException">Thrown if no handler is found for the request.</exception>
    /// <exception cref="ArgumentNullException">Thrown if the request is null.</exception>
    Task<TResponse> Send<TResponse>(IRequest<TResponse> request, CancellationToken cancellationToken = default);

    /// <summary>
    /// Sends a request that does not return a response.
    /// </summary>
    /// <typeparam name="TRequest">The request type.</typeparam>
    /// <param name="request">The request to send.</param>
    /// <param name="cancellationToken">A token to cancel the operation.</param>
    /// <exception cref="HandlerNotFoundException">Thrown if no handler is found for the request.</exception>
    Task Send<TRequest>(TRequest request, CancellationToken cancellationToken = default) where TRequest : IRequest;
}
