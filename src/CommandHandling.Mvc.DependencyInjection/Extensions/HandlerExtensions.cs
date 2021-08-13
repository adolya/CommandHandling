using System;
using System.IO;
using Microsoft.Extensions.DependencyInjection;

namespace CommandHandling.Mvc.DependencyInjection.Extensions
{
    public static class HandlerExtensions
    {
        public static IServiceCollection AddHandlers(this IServiceCollection services, Action<CommandHandlersOptions> ctx = null)
        {
            var handlerOptions = services.GetCommandHandlersOptions();
            
            if (ctx != null)
                ctx(handlerOptions);

            if (Directory.Exists(handlerOptions.GenaratedFilesPath))
            {
                foreach (var controller in handlerOptions.Controllers)
                {   
                    using (StreamWriter writer = new StreamWriter(
                                    Path.Combine(
                                        handlerOptions.GenaratedFilesPath, 
                                        $"{GeneratorExtensions.GeneratedNamespace}_{controller.ControllerName()}.cs")))  
                    {  
                        writer.Write(controller.GeneratedCode);  
                    }  
                }
            }
            var assembly = handlerOptions.Controllers.ToAssembly();
            
            services.AddMvc().ConfigureApplicationPartManager(
                            m => m.FeatureProviders.Add(new BaseControllerFeatureProvider(assembly)));

            return services;
        }
    }
}
