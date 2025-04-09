using Microsoft.Extensions.DependencyInjection;

namespace SkiLift;

/// <summary>
/// Default implementation of <see cref="IRequestDispatcher"/>.
/// </summary>
public class RequestDispatcher(IServiceProvider serviceProvider) : IRequestDispatcher
{
    /// <summary>
    /// Sends a request and returns the response.
    /// </summary>
    /// <typeparam name="TResponse">The response type.</typeparam>
    /// <param name="request">The request to send.</param>
    /// <param name="cancellationToken">A token to cancel the operation.</param>
    /// <returns>The response from the handler.</returns>
    /// <exception cref="HandlerNotFoundException">Thrown if no handler is found for the request.</exception>
    public async Task<TResponse> Send<TResponse>(IRequest<TResponse> request, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(request);

        if (serviceProvider.GetService<IRequestHandler<IRequest<TResponse>, TResponse>>() is not IRequestHandler<IRequest<TResponse>, TResponse> handler)
            throw new HandlerNotFoundException(request.GetType());

        var pipelineBehaviors = serviceProvider.GetServices<IPipelineBehavior<IRequest<TResponse>, TResponse>>();

        Func<Task<TResponse>> handlerDelegate = () => handler.Handle(request, cancellationToken);

        foreach (var behavior in pipelineBehaviors.Reverse())
        {
            var next = handlerDelegate;
            handlerDelegate = () => behavior.Handle(request, next, cancellationToken);
        }

        return await handlerDelegate();
    }

    /// <summary>
    /// Sends a request that does not return a response.
    /// </summary>
    /// <param name="request">The request to send.</param>
    /// <param name="cancellationToken">A token to cancel the operation.</param>
    /// <exception cref="HandlerNotFoundException">Thrown if no handler is found for the request.</exception>
    public async Task Send(IRequest request, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(request);

        if (serviceProvider.GetService<IRequestHandler<IRequest>>() is not IRequestHandler<IRequest> handler)
            throw new HandlerNotFoundException(request.GetType());

        var pipelineBehaviors = serviceProvider.GetServices<IPipelineBehavior<IRequest>>();

        Func<Task> handlerDelegate = () => handler.Handle(request, cancellationToken);

        foreach (var behavior in pipelineBehaviors.Reverse())
        {
            var next = handlerDelegate;
            handlerDelegate = () => behavior.Handle(request, next, cancellationToken);
        }

        await handlerDelegate();
    }
}
