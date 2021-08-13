using System;
using System.Collections.Generic;

namespace CommandHandling.Mvc.DependencyInjection
{
    public interface IControllerDetails 
    {
        ControllerOptions Options {get; set;}

        IEnumerable<Type> References {get; set;}

        string GeneratedCode {get; set;}
        
        ActionDetails ActionDetails {get; set;}

        string HttpMethod();

        string ControllerName();
    }

    public class ControllerDetails<TCommand, TRequest, TResponse> : IControllerDetails
    {
        public ControllerDetails()
        {
            References = new Type[] {typeof(TCommand), typeof(TRequest), typeof(TResponse)};
            Options = new ControllerOptions {
                Method = System.Net.Http.HttpMethod.Post,
                AllowAnonymous = true
            };
        }
        public ControllerOptions Options {get; set;}

        public IEnumerable<Type> References {get; set;}

        public string GeneratedCode {get; set;}

        public ActionDetails ActionDetails { get; set; } = new ActionDetails();

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
