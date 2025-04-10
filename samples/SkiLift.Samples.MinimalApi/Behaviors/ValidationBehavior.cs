using System.ComponentModel.DataAnnotations;
using SkiLift;

namespace SkiLift.Samples.MinimalApi.Behaviors;

public class ValidationBehavior<TRequest, TResponse> : IPipelineBehavior<TRequest, TResponse>
{
    public Task<TResponse> Handle(TRequest request, Func<Task<TResponse>> next, CancellationToken cancellationToken)
    {
        ValidateRequest(request);
        
        return next();
    }

    private void ValidateRequest(TRequest request)
    {
        // Simple validation using reflection to check properties
        foreach (var prop in typeof(TRequest).GetProperties())
        {
            if (prop.PropertyType == typeof(string))
            {
                var value = prop.GetValue(request) as string;
                
                if (string.IsNullOrWhiteSpace(value))
                {
                    throw new ValidationException($"Property {prop.Name} cannot be empty.");
                }
            }
            else if (prop.PropertyType == typeof(decimal))
            {
                var value = (decimal)prop.GetValue(request)!;
                
                if (value <= 0)
                {
                    throw new ValidationException($"Property {prop.Name} must be greater than zero.");
                }
            }
        }
    }
}