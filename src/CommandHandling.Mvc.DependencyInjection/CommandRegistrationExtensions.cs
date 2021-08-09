using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Linq.Expressions;
using CommandHandling.Mvc.DependencyInjection.Extensions;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;

namespace CommandHandling.Mvc.DependencyInjection
{
    public static class CommandRegistrationExtensions
    {
        public static IServiceCollection RegisterHandler<TCommand, TRequest, TResponse>(
                                    this IServiceCollection services, 
                                    Expression<Func<TCommand, TRequest, TResponse>> handler, Action<ControllerOptions> overrideOptions = null)
            where TCommand : class
        {
            var registrations = GetCommandHandlersOptions(services);
            
            var compiled = handler.Compile();
            services.AddScoped<TCommand>();

            var handlerInfo = new ControllerDetails<TCommand, TRequest, TResponse>();
            
            if (overrideOptions != null)
                overrideOptions(handlerInfo.Options);

            handlerInfo.GenerateControllerType(handler);
            registrations.Controllers.Add(handlerInfo);
            Console.WriteLine(handlerInfo.Code);

            return services;
        }

        public static IServiceCollection AddHandlers(this IServiceCollection services, Action<CommandHandlersOptions> overrideOptions = null)
        {
            var handlerOptions = GetCommandHandlersOptions(services);
            
            if (overrideOptions != null)
                overrideOptions(handlerOptions);

            if (Directory.Exists(handlerOptions.GenaratedFilesPath))
            {
                foreach (var controller in handlerOptions.Controllers)
                {   
                    using (StreamWriter writer = new StreamWriter(
                                    Path.Combine(
                                        handlerOptions.GenaratedFilesPath, 
                                        $"{GeneratorExtensions.GeneratedNamespace}_{controller.Name}.cs")))  
                    {  
                        writer.Write(controller.Code);  
                    }  
                }
            }
            var assembly = handlerOptions.Controllers.ToAssembly();
            
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
