using System;
using System.Net.Http;

namespace CommandHandling.Mvc.DependencyInjection
{
    public class RouteInfo
    {
        public string Path {get; set;}

        public HttpMethod Method {get; set;}
    }
}
