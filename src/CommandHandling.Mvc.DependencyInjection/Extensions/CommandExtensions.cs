using System;
using System.Linq.Expressions;
using Microsoft.Extensions.DependencyInjection;

namespace CommandHandling.Mvc.DependencyInjection.Extensions
{
    public static class CommandExtensions
    {
        public static IServiceCollection RegisterHandler<TCommand>(
                                    this IServiceCollection services, 
                                    Expression<Action<TCommand>> handler)
            where TCommand : class
        {
            return services.RegisterHandler(handler, (o) => {});
        }

        public static IServiceCollection RegisterHandler<TCommand, TResponse>(
                                    this IServiceCollection services, 
                                    Expression<Func<TCommand, TResponse>> handler)
            where TCommand : class
        {
            return services.RegisterHandler(handler, (o) => {});
        }

        public static IServiceCollection RegisterHandler<TCommand, TRequest>(
                                    this IServiceCollection services, 
                                    Expression<Action<TCommand, TRequest>> handler)
            where TCommand : class
        {
            return services.RegisterHandler(handler, (o) => {});
        }

        public static IServiceCollection RegisterHandler<TCommand, TRequest, TResponse>(
                                    this IServiceCollection services, 
                                    Expression<Func<TCommand, TRequest, TResponse>> handler)
            where TCommand : class
        {
            return services.RegisterHandler(handler, (o) => {});
        }

        public static IServiceCollection RegisterHandler<TCommand>(
                                    this IServiceCollection services, 
                                    Expression<Action<TCommand>> handler, 
                                    Action<ControllerOptions> ctx)
            where TCommand : class
        {
            var handlerOptions = services.GetCommandHandlersOptions();
            handlerOptions.XmlDocs.LoadXmlDocumentation(typeof(TCommand).Assembly);

            services.AddScoped<TCommand>();

            var controllerDetails = new ControllerDetails<TCommand>(
                                                ActionDetails.Fill<TCommand>(handler, (mi) => handlerOptions.XmlDocs.GetDocumentation(mi)),
                                                new Type[]{typeof(TCommand)});
            
            if (ctx != null)
                ctx(controllerDetails.Options);

            controllerDetails.GenerateControllerType<TCommand>();
            handlerOptions.Controllers.Add(controllerDetails);
            Console.WriteLine(controllerDetails.GeneratedCode);

            return services;
        }

        public static IServiceCollection RegisterHandler<TCommand, TResponse>(
                                    this IServiceCollection services, 
                                    Expression<Func<TCommand, TResponse>> handler, 
                                    Action<ControllerOptions> ctx)
            where TCommand : class
        {
            var handlerOptions = services.GetCommandHandlersOptions();
            handlerOptions.XmlDocs.LoadXmlDocumentation(typeof(TCommand).Assembly);

            services.AddScoped<TCommand>();

            var controllerDetails = new ControllerDetails<TCommand>(
                                                ActionDetails.Fill<TCommand, TResponse>(handler, (mi) => handlerOptions.XmlDocs.GetDocumentation(mi)),
                                                new Type[]{typeof(TCommand), typeof(TResponse)});
            
            if (ctx != null)
                ctx(controllerDetails.Options);

            controllerDetails.GenerateControllerType<TCommand>();
            handlerOptions.Controllers.Add(controllerDetails);
            Console.WriteLine(controllerDetails.GeneratedCode);

            return services;
        }

        public static IServiceCollection RegisterHandler<TCommand, TRequest>(
                                    this IServiceCollection services, 
                                    Expression<Action<TCommand, TRequest>> handler, 
                                    Action<ControllerOptions> ctx)
            where TCommand : class
        {
            var handlerOptions = services.GetCommandHandlersOptions();
            handlerOptions.XmlDocs.LoadXmlDocumentation(typeof(TCommand).Assembly);

            services.AddScoped<TCommand>();

            var controllerDetails = new ControllerDetails<TCommand>(
                                                ActionDetails.Fill<TCommand, TRequest>(handler, (mi) => handlerOptions.XmlDocs.GetDocumentation(mi)),
                                                new Type[]{typeof(TCommand), typeof(TRequest)});
            
            if (ctx != null)
                ctx(controllerDetails.Options);

            controllerDetails.GenerateControllerType<TCommand>();
            handlerOptions.Controllers.Add(controllerDetails);
            Console.WriteLine(controllerDetails.GeneratedCode);

            return services;
        }

        public static IServiceCollection RegisterHandler<TCommand, TRequest, TResponse>(
                                    this IServiceCollection services, 
                                    Expression<Func<TCommand, TRequest, TResponse>> handler, 
                                    Action<ControllerOptions> ctx)
            where TCommand : class
        {
            var handlerOptions = services.GetCommandHandlersOptions();
            handlerOptions.XmlDocs.LoadXmlDocumentation(typeof(TCommand).Assembly);

            services.AddScoped<TCommand>();

            var controllerDetails = new ControllerDetails<TCommand>(
                                                ActionDetails.Fill<TCommand, TRequest, TResponse>(handler, (mi) => handlerOptions.XmlDocs.GetDocumentation(mi)),
                                                new Type[]{typeof(TCommand), typeof(TRequest), typeof(TResponse)});
            
            if (ctx != null)
                ctx(controllerDetails.Options);

            controllerDetails.GenerateControllerType<TCommand>();
            handlerOptions.Controllers.Add(controllerDetails);
            Console.WriteLine(controllerDetails.GeneratedCode);

            return services;
        }
    }
}
