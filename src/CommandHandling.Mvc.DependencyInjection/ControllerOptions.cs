using System.Net.Http;

namespace CommandHandling.Mvc.DependencyInjection
{
    public class ControllerOptions
    {
        public string Route {get; set;}

        public HttpMethod Method {get; set;}

        public bool AllowAnonymous {get; set;} // TODO
    }
}
