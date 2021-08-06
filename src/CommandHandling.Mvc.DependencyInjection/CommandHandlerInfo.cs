using System;
using System.Collections.Generic;
using System.Net.Http;

namespace CommandHandling.Mvc.DependencyInjection
{
    public class CommandHandlerInfo
    {
        public string Path {get; set;}

        public string Method {get; set;}

        public IEnumerable<Type> References {get; set;}

        public string ControllerCode {get; set;}
    }

    public class CommandHandlersOptions
    {
        public IList<CommandHandlerInfo> Handlers { get; } = new List<CommandHandlerInfo> ();
    }
}
