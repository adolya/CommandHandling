using System;
using System.Linq.Expressions;
using Microsoft.Extensions.DependencyInjection;

namespace CommandHandling.Mvc.DependencyInjection.Extensions
{
    public static class CommandExtensions
    {
        public static IServiceCollection RegisterHandler<TCommand, TRequest, TResponse>(
                                    this IServiceCollection services, 
                                    Expression<Func<TCommand, TRequest, TResponse>> handler)
            where TCommand : class
        {
            return services.RegisterHandler(handler, (o) => {});
        }

        public static IServiceCollection RegisterHandler<TCommand, TRequest, TResponse>(
                                    this IServiceCollection services, 
                                    Expression<Func<TCommand, TRequest, TResponse>> handler, 
                                    Action<ControllerOptions> ctx)
            where TCommand : class
        {
            var handlerOptions = services.GetCommandHandlersOptions();
            handlerOptions.XmlDocs.LoadXmlDocumentation(typeof(TCommand).Assembly);

            var compiled = handler.Compile();
            services.AddScoped<TCommand>();

            var controllerDetails = new ControllerDetails<TCommand, TRequest, TResponse>(
                                                ActionDetails.Fill(handler, handlerOptions.XmlDocs));
            
            if (ctx != null)
                ctx(controllerDetails.Options);

            controllerDetails.GenerateControllerType<TCommand, TRequest, TResponse>();
            handlerOptions.Controllers.Add(controllerDetails);
            //Console.WriteLine(controllerDetails.GeneratedCode);

            return services;
        }
    }
}
