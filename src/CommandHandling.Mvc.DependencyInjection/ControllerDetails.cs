using System;
using System.Collections.Generic;

namespace CommandHandling.Mvc.DependencyInjection
{
    public interface IControllerDetails 
    {
        ControllerOptions Options {get; set;}

        IEnumerable<Type> References {get; set;}

        string Name {get; set;}
    
        string Code {get; set;}
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

        public string Name {get; set;}
        
        public string Code {get; set;}
    }
}
