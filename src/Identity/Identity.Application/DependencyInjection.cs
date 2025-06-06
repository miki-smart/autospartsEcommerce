using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Options;
using MediatR;
using FluentValidation;
using AutoMapper;
using System.Reflection;
using Identity.Application.Common.Mappings;
using Identity.Application.Common.Configuration;
using Identity.Application.Services;
using Identity.Application.Services.Implementation;

namespace Identity.Application;

public static class DependencyInjection
{
    public static IServiceCollection AddApplication(this IServiceCollection services, IConfiguration configuration)
    {
        // Register MediatR
        services.AddMediatR(cfg => cfg.RegisterServicesFromAssembly(Assembly.GetExecutingAssembly()));
        
        // Register AutoMapper
        services.AddAutoMapper(typeof(MappingProfile));
        
        // Register FluentValidation
        services.AddValidatorsFromAssembly(Assembly.GetExecutingAssembly());
        
        // Register validation behavior
        services.AddTransient(typeof(IPipelineBehavior<,>), typeof(ValidationBehavior<,>));
          // Register JWT Settings
        services.Configure<JwtSettings>(configuration.GetSection(JwtSettings.SectionName));
          // Register Application Services
        services.AddScoped<ITokenService, TokenService>();
        services.AddScoped<IDeviceTrackingService, DeviceTrackingService>();
        services.AddScoped<ITwoFactorService, TwoFactorService>();
        
        return services;
    }
}

// Validation Pipeline Behavior
public class ValidationBehavior<TRequest, TResponse> : IPipelineBehavior<TRequest, TResponse>
    where TRequest : IRequest<TResponse>
{
    private readonly IEnumerable<IValidator<TRequest>> _validators;

    public ValidationBehavior(IEnumerable<IValidator<TRequest>> validators)
    {
        _validators = validators;
    }

    public async Task<TResponse> Handle(TRequest request, RequestHandlerDelegate<TResponse> next, CancellationToken cancellationToken)
    {
        if (_validators.Any())
        {
            var context = new ValidationContext<TRequest>(request);
            
            var validationResults = await Task.WhenAll(
                _validators.Select(v => v.ValidateAsync(context, cancellationToken)));
            
            var failures = validationResults
                .SelectMany(r => r.Errors)
                .Where(f => f != null)
                .ToList();

            if (failures.Any())
            {
                throw new ValidationException(failures);
            }
        }
        
        return await next();
    }
}
