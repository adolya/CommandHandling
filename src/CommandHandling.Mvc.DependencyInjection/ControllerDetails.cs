using System;
using System.Collections.Generic;

namespace CommandHandling.Mvc.DependencyInjection
{
    public interface IControllerDetails 
    {
        ControllerOptions Options {get; set;}

        IEnumerable<Type> References {get; set;}

        string GeneratedCode {get; set;}
        
        ActionDetails ActionDetails {get;}

        string HttpMethod();

        string ControllerName();

        string Route();
    }

    public class ControllerDetails<TCommand, TRequest, TResponse> : IControllerDetails
    {
        public ControllerOptions Options {get; set;}

        public IEnumerable<Type> References {get; set;} = new Type[] {typeof(TCommand), typeof(TRequest), typeof(TResponse)};

        public string GeneratedCode {get; set;} = string.Empty;

        public ActionDetails ActionDetails { get; private set; }

        public ControllerDetails(ActionDetails actionDetails)
        {
            Options = new ControllerOptions();
            ActionDetails = actionDetails;
        }

        public string Route()
        {
            return Options.Route ?? ActionDetails.MethodName;
        }
        public string HttpMethod()
        {
            return Char.ToUpperInvariant(Options.Method.Method[0]) + Options.Method.Method.Substring(1).ToLowerInvariant();
        }

        public string ControllerName()
        {
            return  $"{ActionDetails.CommandName}_{ActionDetails.MethodName}_{HttpMethod()}Controller";
        }
    }
}
