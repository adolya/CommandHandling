using System.Linq;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;

namespace CommandHandling.Mvc.DependencyInjection.Extensions
{
    internal static class ServiceCollectionExtensions
    {
        internal static CommandHandlersOptions GetCommandHandlersOptions(this IServiceCollection services)
        {
            var commandHandlersOptions = GetSingletonServiceFromCollection<CommandHandlersOptions>(services);



            return commandHandlersOptions;
        }
                
        private static T GetSingletonServiceFromCollection<T>(IServiceCollection services)
            where T: class, new()
        {
            T? svc = services
                .LastOrDefault(d => d.ServiceType == typeof(T))
                ?.ImplementationInstance as T;
            
            if (svc == null)
            {
                svc = new T();
                services.TryAddSingleton(svc);
            }

            return svc;
        }
    }
}