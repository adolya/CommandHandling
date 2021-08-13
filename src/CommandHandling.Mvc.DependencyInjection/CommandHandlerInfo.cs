using System.Collections.Generic;
using System.Reflection;

namespace CommandHandling.Mvc.DependencyInjection
{
    public class CommandHandlersOptions
    {
        public XmlDocumentation XmlDocs = new XmlDocumentation();

        public string GenaratedFilesPath { get; set; }

        public bool GroupByCommand {get; set;} = false;  
        
        public IList<IControllerDetails> Controllers { get; } = new List<IControllerDetails> ();
    }
}
