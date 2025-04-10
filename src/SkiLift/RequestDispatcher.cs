using Microsoft.Extensions.DependencyInjection;
using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace SkiLift;

/// <summary>
/// Default implementation of <see cref="IRequestDispatcher"/>.
/// </summary>
public class RequestDispatcher(IServiceProvider serviceProvider) : IRequestDispatcher
{
    /// <inheritdoc/>
    public async Task<TResponse> Send<TResponse>(IRequest<TResponse> request, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(request);

        var requestType = request.GetType();
        
        var dispatchMethod = this.GetType().GetMethod(nameof(Dispatch), System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
        
        var genericDispatchMethod = dispatchMethod!.MakeGenericMethod(requestType, typeof(TResponse));
        
        var response = await (Task<TResponse>)genericDispatchMethod.Invoke(this, new object[] { request, cancellationToken })!;
        
        return response;
    }

    private async Task<TResponse> Dispatch<TRequest, TResponse>(TRequest request, CancellationToken cancellationToken)
        where TRequest : IRequest<TResponse>
    {
        ArgumentNullException.ThrowIfNull(request);

        if (serviceProvider.GetService<IRequestHandler<TRequest, TResponse>>() is not IRequestHandler<TRequest, TResponse> handler)
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

    /// <inheritdoc/>
    public async Task Send<TRequest>(TRequest request, CancellationToken cancellationToken = default) where TRequest : IRequest
    {
        ArgumentNullException.ThrowIfNull(request);

        var requestType = request.GetType();

        if (serviceProvider.GetService<IRequestHandler<TRequest>>() is not IRequestHandler<TRequest> handler)
            throw new HandlerNotFoundException(requestType);

        var pipelineBehaviors = serviceProvider.GetServices<IPipelineBehavior<TRequest, Void>>();

        Func<Task<Void>> handlerDelegate = async () => {

            await handler.Handle(request, cancellationToken);
            return Void.Instance;
            
        };

        foreach (var behavior in pipelineBehaviors.Reverse())
        {
            var next = handlerDelegate;
            handlerDelegate = () => behavior.Handle(request, next, cancellationToken);
        }

        await handlerDelegate();
    }
}
