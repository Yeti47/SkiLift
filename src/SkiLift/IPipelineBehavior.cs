namespace SkiLift;

/// <summary>
/// Interface for defining pipeline behaviors that wrap request handling.
/// </summary>
/// <typeparam name="TRequest">The request type.</typeparam>
/// <typeparam name="TResponse">The response type.</typeparam>
public interface IPipelineBehavior<TRequest, TResponse> where TRequest : IRequest<TResponse>
{
    /// <summary>
    /// Handles the request and invokes the next behavior or handler in the pipeline.
    /// </summary>
    /// <param name="request">The request to handle.</param>
    /// <param name="next">The next delegate in the pipeline.</param>
    /// <param name="cancellationToken">A token to cancel the operation.</param>
    /// <returns>The response from the pipeline or handler.</returns>
    Task<TResponse> Handle(TRequest request, Func<Task<TResponse>> next, CancellationToken cancellationToken);
}

/// <summary>
/// Interface for defining pipeline behaviors that wrap request handling for requests without a response.
/// </summary>
/// <typeparam name="TRequest">The request type.</typeparam>
public interface IPipelineBehavior<TRequest> where TRequest : IRequest
{
    /// <summary>
    /// Handles the request and invokes the next behavior or handler in the pipeline.
    /// </summary>
    /// <param name="request">The request to handle.</param>
    /// <param name="next">The next delegate in the pipeline.</param>
    /// <param name="cancellationToken">A token to cancel the operation.</param>
    /// <returns>A task representing the completion of the pipeline.</returns>
    Task Handle(TRequest request, Func<Task> next, CancellationToken cancellationToken);
}
