using SkiLift;

namespace SkiLift.Samples.MinimalApi.Behaviors;

public class LoggingBehavior<TRequest, TResponse>(ILogger<LoggingBehavior<TRequest, TResponse>> logger) : IPipelineBehavior<TRequest, TResponse>
{
    public async Task<TResponse> Handle(TRequest request, Func<Task<TResponse>> next, CancellationToken cancellationToken)
    {
        logger.LogInformation("Handling {RequestType}", typeof(TRequest).Name);
        
        var response = await next();
        
        logger.LogInformation("Handled {RequestType}", typeof(TRequest).Name);
        
        return response;
    }
}