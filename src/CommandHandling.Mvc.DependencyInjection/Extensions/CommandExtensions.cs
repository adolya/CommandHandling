using System;
using System.Linq.Expressions;
using Microsoft.Extensions.DependencyInjection;

namespace CommandHandling.Mvc.DependencyInjection.Extensions
{
    public static class CommandExtensions
    {
        public static IServiceCollection RegisterHandler<TCommand, TRequest, TResponse>(
                                    this IServiceCollection services, 
                                    Expression<Func<TCommand, TRequest, TResponse>> handler, 
                                    Action<ControllerOptions> ctx = null)
            where TCommand : class
        {
            var handlerOptions = services.GetCommandHandlersOptions();
            
            var compiled = handler.Compile();
            services.AddScoped<TCommand>();

            var controllerDetails = new ControllerDetails<TCommand, TRequest, TResponse>();
            
            if (ctx != null)
                ctx(controllerDetails.Options);

            handlerOptions.XmlDocs.LoadXmlDocumentation(typeof(TCommand).Assembly);
            controllerDetails.ActionDetails.Fill(handler, handlerOptions.XmlDocs);
            controllerDetails.GenerateControllerType<TCommand, TRequest, TResponse>();
            handlerOptions.Controllers.Add(controllerDetails);
            Console.WriteLine(controllerDetails.GeneratedCode);

            return services;
        }
    }
}
