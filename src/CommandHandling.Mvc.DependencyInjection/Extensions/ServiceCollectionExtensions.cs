using System.Linq;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;

namespace CommandHandling.Mvc.DependencyInjection.Extensions
{
    internal static class ServiceCollectionExtensions
    {
        internal static CommandHandlersOptions GetCommandHandlersOptions(this IServiceCollection services)
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
    }
}