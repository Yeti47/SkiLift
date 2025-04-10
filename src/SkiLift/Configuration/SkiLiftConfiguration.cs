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

        var pipelineBehaviorTypes = assembly.GetTypes()
            .Where(t => t.IsClass 
                && !t.IsAbstract 
                && !t.IsInterface 
                && !t.IsNestedPrivate
                && t.IsPublic 
                && IsPipelineBehavior(t)
            );
            
        foreach (var pipelineBehaviorType in pipelineBehaviorTypes)
        {
            var lifetime = lifetimeSelector(pipelineBehaviorType);

            var descriptor = new ServiceDescriptor(typeof(IPipelineBehavior<,>), pipelineBehaviorType, lifetime);

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
    /// <param name="pipelineBehaviorType">Type of the pipeline behavior.</param>
    /// <param name="lifetime">The lifetime of the pipeline behavior.</param>
    /// <returns>The current <see cref="SkiLiftConfiguration"/> instance.</returns>
    /// <exception cref="ArgumentException">Thrown if the type does not implement <see cref="IPipelineBehavior{TRequest, TResponse}"/>.</exception>
    /// <exception cref="ArgumentException">Thrown if the type is not a generic type.</exception>
    /// <remarks>
    /// This method allows you to register a specific pipeline behavior type with a specified lifetime.
    /// The type must implement <see cref="IPipelineBehavior{TRequest, TResponse}"/>.
    /// If the type is not generic or does not implement the required interfaces, an exception will be thrown.
    /// </remarks>
    /// <example>
    /// <code>
    /// // Register a specific pipeline behavior type
    /// config.AddPipelineBehavior(typeof(MyCustomPipelineBehavior), ServiceLifetime.Singleton);
    /// </code>
    /// </example>
    public SkiLiftConfiguration AddPipelineBehavior(Type pipelineBehaviorType, ServiceLifetime lifetime = ServiceLifetime.Transient)
    {
        // Register the given pipeline behavior type, but only if it implements the correct interface (IPipelineBehavior<,> or IPipelineBehavior<>)

        if (pipelineBehaviorType.IsAbstract || pipelineBehaviorType.IsInterface)
        {
            throw new ArgumentException($"Type {pipelineBehaviorType.Name} must be a concrete class.");
        }

        if (pipelineBehaviorType.IsGenericTypeDefinition)
        {
            if (IsPipelineBehavior(pipelineBehaviorType))
            {
                var descriptor = new ServiceDescriptor(typeof(IPipelineBehavior<,>), pipelineBehaviorType, lifetime);
                services.Add(descriptor);
            }
            else
            {
                throw new ArgumentException($"Type {pipelineBehaviorType.Name} must implement IPipelineBehavior<,>.");
            }
        }
        else
        {
            throw new ArgumentException($"Type {pipelineBehaviorType.Name} must be a generic type definition.");
        }

        return this;
    }

    private static bool IsPipelineBehavior(Type type)
    {
        return type.IsGenericType
            && type.GetInterfaces().Any(i => i.GetGenericTypeDefinition() == typeof(IPipelineBehavior<,>));
    }
}
