using System;
using System.Collections.Generic;

namespace CommandHandling.Mvc.DependencyInjection
{
    public interface IControllerDetails 
    {
        ControllerOptions Options {get; set;}
        IEnumerable<Type> References {get;}
        string GeneratedCode {get; set;}
        ActionDetails ActionDetails {get;}

        string HttpMethod();
        string ControllerName();
        string Route();
        string MethodBody();
        string RequestMetaDataAttribute();
    }

    public class ControllerDetails<TCommand> : IControllerDetails
    {
        public ControllerOptions Options {get; set;}

        public IEnumerable<Type> References {get; private set; }

        public string GeneratedCode {get; set;} = string.Empty;

        public ActionDetails ActionDetails { get; private set; }

        public ControllerDetails(ActionDetails actionDetails, IEnumerable<Type> references)
        {
            Options = new ControllerOptions();
            ActionDetails = actionDetails;
            References = references;
        }

        public string Route()
        {
            return Options.Route ?? $"{ActionDetails.CommandName}/{ActionDetails.MethodName}";
        }
        public string HttpMethod()
        {
            return Char.ToUpperInvariant(Options.Method.Method[0]) + Options.Method.Method.Substring(1).ToLowerInvariant();
        }

        public string ControllerName()
        {
            return  $"{typeof(TCommand).Name}_{ActionDetails.MethodName}_{HttpMethod()}Controller";
        }

        public string MethodBody()
        {
            return ActionDetails.MethodBody.Trim().StartsWith('{')
            ? ActionDetails.MethodBody
            : $"=> {ActionDetails.MethodBody};";
        }

        public string RequestMetaDataAttribute()
        {
            if (string.IsNullOrEmpty(ActionDetails.RequestType))
                return string.Empty;

            if (Options.Method == System.Net.Http.HttpMethod.Post)
                return "[FromBody]";

            return string.Empty;
        }
    }
}
