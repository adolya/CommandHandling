using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Net.Http;
using CommandHandling.CommandHandlers;
using CommandHandling.Mvc.DependencyInjection.Extensions;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;

namespace CommandHandling.Mvc.DependencyInjection
{
    public static class CommandRegistrationExtensions
    {

        public static IServiceCollection RegisterHandler<TCommand, TRequest, TResponse>(
                                    this IServiceCollection services, 
                                    Expression<Func<TCommand, TRequest, TResponse>> handler, 
                                    HttpMethod httpMethod, 
                                    string route = null)
            where TCommand : class
        {
            string methodName = Char.ToUpperInvariant(httpMethod.Method[0]) + httpMethod.Method.Substring(1).ToLowerInvariant();
            return services.RegisterHandler(handler, methodName, route);
        }
        public static IServiceCollection RegisterHandler<TCommand, TRequest, TResponse>(
                                    this IServiceCollection services, 
                                    Expression<Func<TCommand, TRequest, TResponse>> handler, 
                                    string httpMethod = "Post", 
                                    string route = null)
            where TCommand : class
        {
            var registrations = GetCommandHandlersOptions(services);
            
            var compiled = handler.Compile();
            services.AddScoped<TCommand>();
            services.AddTransient<CommandHandler<TCommand, TRequest, TResponse>>(provider => {
                var command = provider.GetService<TCommand>();
                return new CommandHandler<TCommand, TRequest, TResponse>(command, compiled);
            });

            var handlerInfo = new CommandHandlerInfo { 
                    Path = route,
                    Method = httpMethod,
                    References = new Type[] {typeof(TCommand), typeof(TRequest), typeof(TResponse)},
                    ControllerCode = handler.GenerateControllerType(route, httpMethod) 
                }; 
            registrations.Handlers.Add(handlerInfo);
            Console.WriteLine(handlerInfo.ControllerCode);

            return services;
        }

        public static IServiceCollection AddHandlers(this IServiceCollection services)
        {
            var handlerOptions = GetCommandHandlersOptions(services);

            var assembly = handlerOptions.Handlers.ToAssembly();
            
            services.AddMvc().ConfigureApplicationPartManager(
                            m => m.FeatureProviders.Add(new BaseControllerFeatureProvider(assembly)));

            return services;
        }

        
        private static CommandHandlersOptions GetCommandHandlersOptions(IServiceCollection services)
        {
            var commandHandlersOptions = GetServiceFromCollection<CommandHandlersOptions>(services);

            if (commandHandlersOptions == null)
            {
                commandHandlersOptions = new CommandHandlersOptions();
                services.TryAddSingleton(commandHandlersOptions);
            }

            return commandHandlersOptions;
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
