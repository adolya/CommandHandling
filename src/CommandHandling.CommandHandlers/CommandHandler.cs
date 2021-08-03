using System;
using System.Threading;
using System.Threading.Tasks;

namespace CommandHandling.CommandHandlers
{
    public interface ICommandHandler {};
    public class CommandHandler<TCommand, TRequest, TResponse> :ICommandHandler
     where TCommand : class
    {
        private readonly Func<TCommand, TRequest, TResponse> _handler;
        private readonly TCommand _command;

        public CommandHandler(TCommand command, Func<TCommand, TRequest, TResponse> handler)
        {
            _handler = handler;
            _command = command;
        }
        public TResponse Handle(TRequest request)
        {
            return _handler(_command, request);
        } 
    }
}
