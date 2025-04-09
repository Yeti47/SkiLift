using Microsoft.Extensions.DependencyInjection;

namespace SkiLift.Configuration;

/// <summary>
/// Extension methods for configuring SkiLift services.
/// </summary>
public static class SkiLiftConfigExtensions
{
    /// <summary>
    /// Adds SkiLift services to the service collection.
    /// </summary>
    /// <param name="services">The service collection.</param>
    /// <param name="configure">A delegate to configure SkiLift.</param>
    /// <returns>The updated service collection.</returns>
    public static IServiceCollection AddSkiLift(this IServiceCollection services, Action<SkiLiftConfiguration> configure)
    {
        var skiLiftConfig = new SkiLiftConfiguration(services);
        configure(skiLiftConfig);

        var dispatcherServiceDesriptor = new ServiceDescriptor(typeof(IRequestDispatcher), skiLiftConfig.DispatcherType, skiLiftConfig.DispatcherLifetime);
        services.Add(dispatcherServiceDesriptor);

        return services;
    }
}
