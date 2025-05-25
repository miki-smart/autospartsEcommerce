using System.Reflection;
using Microsoft.Extensions.DependencyInjection;
using FluentValidation;
using MediatR;
using Catalog.Application.Common.Behaviors;

namespace Catalog.Application
{
    public static class DependencyInjection
    {
        public static IServiceCollection AddApplication(this IServiceCollection services)
        {
            services.AddMediatR(cfg => 
            {
                cfg.RegisterServicesFromAssembly(Assembly.GetExecutingAssembly());
                cfg.AddBehavior(typeof(IPipelineBehavior<,>), typeof(ValidationBehavior<,>));
            });

            // Register validators
            var assembly = Assembly.GetExecutingAssembly();
            var validatorType = typeof(IValidator<>);
            var validatorTypes = assembly.GetTypes()
                .Where(t => t.GetInterfaces().Any(i => i.IsGenericType && 
                                                       i.GetGenericTypeDefinition() == validatorType))
                .ToList();

            foreach (var validator in validatorTypes)
            {
                var validatorInterface = validator.GetInterfaces()
                    .First(i => i.IsGenericType && 
                                i.GetGenericTypeDefinition() == validatorType);
                
                services.AddScoped(validatorInterface, validator);
            }

            return services;
        }
    }
}
