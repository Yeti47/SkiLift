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
    Task<TResponse> Send<TResponse>(IRequest<TResponse> request, CancellationToken cancellationToken = default);
}
