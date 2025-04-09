using System.Reflection;
using Microsoft.Extensions.DependencyInjection;

namespace SkiLift.Configuration;

/// <summary>
/// Configuration class for setting up SkiLift services.
/// </summary>
public class SkiLiftConfiguration(IServiceCollection services)
{
    private Type _dispatcherType = typeof(RequestDispatcher);

    /// <summary>
    /// Gets the service collection.
    /// </summary>
    public IServiceCollection Services => services;

    /// <summary>
    /// Gets or sets the type of the request dispatcher.
    /// </summary>
    /// <exception cref="ArgumentException">Thrown if the type does not implement <see cref="IRequestDispatcher"/>.</exception>
    public Type DispatcherType
    {
        get => _dispatcherType;
        set
        {
            if (!typeof(IRequestDispatcher).IsAssignableFrom(value))
            {
                throw new ArgumentException($"Type {value.Name} must implement {nameof(IRequestDispatcher)}");
            }

            _dispatcherType = value;
        }
    }

    /// <summary>
    /// Gets or sets the lifetime of the dispatcher service.
    /// </summary>
    public ServiceLifetime DispatcherLifetime { get; set; } = ServiceLifetime.Singleton;

    /// <summary>
    /// Registers all request handlers from the specified assembly.
    /// </summary>
    /// <param name="assembly">The assembly to scan for handlers.</param>
    /// <param name="lifetimeSelector">Optional selector for determining service lifetime.</param>
    /// <returns>The current <see cref="SkiLiftConfiguration"/> instance.</returns>
    public SkiLiftConfiguration AddHandlersFromAssembly(Assembly assembly, Func<Type, ServiceLifetime>? lifetimeSelector = null)
    {
        lifetimeSelector ??= static _ => ServiceLifetime.Transient;

        var handlerInterfaceType = typeof(IRequestHandler<,>);
        var voidHandlerInterfaceType = typeof(IRequestHandler<>);

        var handlerTypes = assembly.GetTypes()
            .Where(t => t.IsClass && !t.IsAbstract)
            .SelectMany(t => t.GetInterfaces(), (type, iface) => (Type: type, Interface: iface))
            .Where(t => t.Interface.IsGenericType &&
                (t.Interface.GetGenericTypeDefinition() == handlerInterfaceType ||
                 t.Interface.GetGenericTypeDefinition() == voidHandlerInterfaceType));

        foreach (var handlerType in handlerTypes)
        {
            var lifetime = lifetimeSelector(handlerType.Type);

            var descriptor = new ServiceDescriptor(handlerType.Interface, handlerType.Type, lifetime);

            services.Add(descriptor);
        }

        return this;
    }

    /// <summary>
    /// Registers all request handlers from the specified assembly with a fixed lifetime.
    /// </summary>
    /// <param name="assembly">The assembly to scan for handlers.</param>
    /// <param name="lifetime">The lifetime to use for all handlers.</param>
    /// <returns>The current <see cref="SkiLiftConfiguration"/> instance.</returns>
    public SkiLiftConfiguration AddHandlersFromAssembly(Assembly assembly, ServiceLifetime lifetime)
    {
        return AddHandlersFromAssembly(assembly, _ => lifetime);
    }

    /// <summary>
    /// Registers all request handlers from the assembly containing the specified type.
    /// </summary>
    /// <typeparam name="T">A type in the target assembly.</typeparam>
    /// <param name="lifetimeSelector">Optional selector for determining service lifetime.</param>
    /// <returns>The current <see cref="SkiLiftConfiguration"/> instance.</returns>
    public SkiLiftConfiguration AddHandlersFromAssemblyContaining<T>(Func<Type, ServiceLifetime>? lifetimeSelector = null)
    {
        return AddHandlersFromAssembly(typeof(T).Assembly, lifetimeSelector);
    }

    /// <summary>
    /// Registers all request handlers from the assembly containing the specified type with a fixed lifetime.
    /// </summary>
    /// <typeparam name="T">A type in the target assembly.</typeparam>
    /// <param name="lifetime">The lifetime to use for all handlers.</param>
    /// <returns>The current <see cref="SkiLiftConfiguration"/> instance.</returns>
    public SkiLiftConfiguration AddHandlersFromAssemblyContaining<T>(ServiceLifetime lifetime)
    {
        return AddHandlersFromAssembly(typeof(T).Assembly, _ => lifetime);
    }

    /// <summary>
    /// Registers a specific request handler type.
    /// </summary>
    /// <typeparam name="TRequest">The request type.</typeparam>
    /// <typeparam name="TResponse">The response type.</typeparam>
    /// <typeparam name="THandler">The handler type.</typeparam>
    /// <param name="lifetime">The lifetime of the handler.</param>
    /// <returns>The current <see cref="SkiLiftConfiguration"/> instance.</returns>
    public SkiLiftConfiguration AddHandler<TRequest, TResponse, THandler>(ServiceLifetime lifetime = ServiceLifetime.Transient)
        where THandler : IRequestHandler<TRequest, TResponse>
        where TRequest : IRequest<TResponse>
    {
        var descriptor = new ServiceDescriptor(typeof(IRequestHandler<TRequest, TResponse>), typeof(THandler), lifetime);
        services.Add(descriptor);

        return this;
    }

    /// <summary>
    /// Registers a specific request handler type for requests without a response.
    /// </summary>
    /// <typeparam name="TRequest">The request type.</typeparam>
    /// <typeparam name="THandler">The handler type.</typeparam>
    /// <param name="lifetime">The lifetime of the handler.</param>
    /// <returns>The current <see cref="SkiLiftConfiguration"/> instance.</returns>
    public SkiLiftConfiguration AddHandler<TRequest, THandler>(ServiceLifetime lifetime = ServiceLifetime.Transient)
        where THandler : IRequestHandler<TRequest>
        where TRequest : IRequest
    {
        var descriptor = new ServiceDescriptor(typeof(IRequestHandler<TRequest>), typeof(THandler), lifetime);
        services.Add(descriptor);

        return this;
    }

    /// <summary>
    /// Registers all pipeline behaviors from the specified assembly.
    /// </summary>
    /// <param name="assembly">The assembly to scan for pipeline behaviors.</param>
    /// <param name="lifetimeSelector">Optional selector for determining service lifetime.</param>
    /// <returns>The current <see cref="SkiLiftConfiguration"/> instance.</returns>
    public SkiLiftConfiguration AddPipelineBehaviorsFromAssembly(Assembly assembly, Func<Type, ServiceLifetime>? lifetimeSelector = null)
    {
        lifetimeSelector ??= static _ => ServiceLifetime.Transient;

        var pipelineBehaviorInterfaceType = typeof(IPipelineBehavior<,>);
        var voidPipelineBehaviorInterfaceType = typeof(IPipelineBehavior<>);

        var pipelineBehaviorTypes = assembly.GetTypes()
            .Where(t => t.IsClass && !t.IsAbstract)
            .SelectMany(t => t.GetInterfaces(), (type, iface) => (Type: type, Interface: iface))
            .Where(t => t.Interface.IsGenericType &&
                (t.Interface.GetGenericTypeDefinition() == pipelineBehaviorInterfaceType ||
                 t.Interface.GetGenericTypeDefinition() == voidPipelineBehaviorInterfaceType));

        foreach (var pipelineBehaviorType in pipelineBehaviorTypes)
        {
            var lifetime = lifetimeSelector(pipelineBehaviorType.Type);

            var descriptor = new ServiceDescriptor(pipelineBehaviorType.Interface, pipelineBehaviorType.Type, lifetime);

            services.Add(descriptor);
        }

        return this;
    }

    /// <summary>
    /// Registers all pipeline behaviors from the specified assembly with a fixed lifetime.
    /// </summary>
    /// <param name="assembly">The assembly to scan for pipeline behaviors.</param>
    /// <param name="lifetime">The lifetime to use for all pipeline behaviors.</param>
    /// <returns>The current <see cref="SkiLiftConfiguration"/> instance.</returns>
    public SkiLiftConfiguration AddPipelineBehaviorsFromAssembly(Assembly assembly, ServiceLifetime lifetime)
    {
        return AddPipelineBehaviorsFromAssembly(assembly, _ => lifetime);
    }

    /// <summary>
    /// Registers all pipeline behaviors from the assembly containing the specified type.
    /// </summary>
    /// <typeparam name="T">A type in the target assembly.</typeparam>
    /// <param name="lifetimeSelector">Optional selector for determining service lifetime.</param>
    /// <returns>The current <see cref="SkiLiftConfiguration"/> instance.</returns>
    public SkiLiftConfiguration AddPipelineBehaviorsFromAssemblyContaining<T>(Func<Type, ServiceLifetime>? lifetimeSelector = null)
    {
        return AddPipelineBehaviorsFromAssembly(typeof(T).Assembly, lifetimeSelector);
    }

    /// <summary>
    /// Registers all pipeline behaviors from the assembly containing the specified type with a fixed lifetime.
    /// </summary>
    /// <typeparam name="T">A type in the target assembly.</typeparam>
    /// <param name="lifetime">The lifetime to use for all pipeline behaviors.</param>
    /// <returns>The current <see cref="SkiLiftConfiguration"/> instance.</returns>
    public SkiLiftConfiguration AddPipelineBehaviorsFromAssemblyContaining<T>(ServiceLifetime lifetime)
    {
        return AddPipelineBehaviorsFromAssembly(typeof(T).Assembly, _ => lifetime);
    }

    /// <summary>
    /// Registers a specific pipeline behavior type.
    /// </summary>
    /// <typeparam name="TRequest">The request type.</typeparam>
    /// <typeparam name="TResponse">The response type.</typeparam>
    /// <typeparam name="TBehavior">The behavior type.</typeparam>
    /// <param name="lifetime">The lifetime of the behavior.</param>
    /// <returns>The current <see cref="SkiLiftConfiguration"/> instance.</returns>
    public SkiLiftConfiguration AddPipelineBehavior<TRequest, TResponse, TBehavior>(ServiceLifetime lifetime = ServiceLifetime.Transient)
        where TBehavior : IPipelineBehavior<TRequest, TResponse>
        where TRequest : IRequest<TResponse>
    {
        var descriptor = new ServiceDescriptor(typeof(IPipelineBehavior<TRequest, TResponse>), typeof(TBehavior), lifetime);
        services.Add(descriptor);

        return this;
    }

    /// <summary>
    /// Registers a specific pipeline behavior type for requests without a response.
    /// </summary>
    /// <typeparam name="TRequest">The request type.</typeparam>
    /// <typeparam name="TBehavior">The behavior type.</typeparam>
    /// <param name="lifetime">The lifetime of the behavior.</param>
    /// <returns>The current <see cref="SkiLiftConfiguration"/> instance.</returns>
    public SkiLiftConfiguration AddPipelineBehavior<TRequest, TBehavior>(ServiceLifetime lifetime = ServiceLifetime.Transient)
        where TBehavior : IPipelineBehavior<TRequest>
        where TRequest : IRequest
    {
        var descriptor = new ServiceDescriptor(typeof(IPipelineBehavior<TRequest>), typeof(TBehavior), lifetime);
        services.Add(descriptor);

        return this;
    }
}
