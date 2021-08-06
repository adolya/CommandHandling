namespace CommandHandling.Mvc.DependencyInjection.Extensions
{
    public class ControllerTemplate
    {
        public const string GreneralTemplate = @"using CommandHandling.CommandHandlers;
using Microsoft.AspNetCore.Mvc;

namespace CommandHandling.GeneratedControllers
{
    public class {controllerName}Controller : ControllerBase
    {
        private readonly CommandHandler<{genericSignature}> CommandHandler;
        public {controllerName}Controller(CommandHandler<{genericSignature}> commandHandler)
        {
            CommandHandler = commandHandler;
        }

        [{httpMethodAttribute}]
        public {responseType} Process({requestTypePrefix}{requestType} request)
        {
            return CommandHandler.Handle(request);
        }
    }
}

        ";
    }
}