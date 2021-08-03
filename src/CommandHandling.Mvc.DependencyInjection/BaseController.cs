using System;
using System.Net.Http;
using CommandHandling.CommandHandlers;
using Microsoft.AspNetCore.Mvc;

namespace CommandHandling.Mvc.DependencyInjection
{
    public class BaseController<TCommand, TRequest, TResponse> : ControllerBase
        where TCommand : class
    {
        protected readonly CommandHandler<TCommand, TRequest, TResponse> CommandHandler;
        public BaseController(CommandHandler<TCommand, TRequest, TResponse> commandHandler)
        {
            CommandHandler = commandHandler;
        }

        protected TResponse Process(TRequest request)
        {
            return CommandHandler.Handle(request);
        }
    }
}
