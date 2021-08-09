using System;
using System.Collections.Generic;
using System.Net.Http;

namespace CommandHandling.Mvc.DependencyInjection
{
    public class CommandHandlersOptions
    {
        public string GenaratedFilesPath { get; set; }

        public bool GroupByCommand {get; set;} = false;  
        
        public IList<IControllerDetails> Controllers { get; } = new List<IControllerDetails> ();
    }
}
