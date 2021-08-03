using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Net.Http;
using CommandHandling.CommandHandlers;
using CommandHandling.Mvc.DependencyInjection.Convensions;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;

namespace CommandHandling.Mvc.DependencyInjection
{
    public static class CommandRegistrationExtensions
    {
        public static IServiceCollection RegisterHandler<TCommand, TRequest, TResponse>(
                                    this IServiceCollection services, 
                                    Expression<Func<TCommand, TRequest, TResponse>> handler, 
                                    HttpMethod method, 
                                    string route = null)
            where TCommand : class
        {
            var registrations = GetCommandHandlers(services);
            
            var compiled = handler.Compile();
            services.AddScoped<TCommand>();
            services.AddTransient<CommandHandler<TCommand, TRequest, TResponse>>(provider => {
                
                var command = provider.GetService<TCommand>();
                return new CommandHandler<TCommand, TRequest, TResponse>(command, compiled);
            });
            
            registrations[new RouteInfo { Path = route, Method = method }] =
                typeof(CommandHandler<TCommand, TRequest, TResponse>);

            return services;
        }

        public static IServiceCollection AddHandlers(this IServiceCollection services)
        {
            var handlers = GetCommandHandlers(services);
            services.AddMvc(o => 
                        o.Conventions.Add(new BaseControllerRouteConvention()))
                        .ConfigureApplicationPartManager(
                            m => m.FeatureProviders.Add(new BaseControllerFeatureProvider(handlers)));

            return services;
        }

        
        private static IDictionary<RouteInfo, Type> GetCommandHandlers(IServiceCollection services)
        {
            var commandHandlers = GetServiceFromCollection<IDictionary<RouteInfo, Type>>(services);

            if (commandHandlers == null)
            {
                commandHandlers = new Dictionary<RouteInfo, Type>();
                services.TryAddSingleton(commandHandlers);
            }

            return commandHandlers;
        }

        private static T GetServiceFromCollection<T>(IServiceCollection services)
        {
            return (T)services
                .LastOrDefault(d => d.ServiceType == typeof(T))
                ?.ImplementationInstance;
        }

        private static IEnumerable<Type> GetServicesFromCollection(this IServiceCollection services, Type type)
        {
            return services
                .Where(d => 
                    d.ServiceType.IsGenericType && d.ServiceType.GetGenericTypeDefinition() == type)
                .Select(d => d.ServiceType).ToList();
        }
    }
}
