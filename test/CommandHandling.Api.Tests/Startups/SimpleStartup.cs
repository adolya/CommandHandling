using System.Net.Http;
using CommandHandling.Mvc.DependencyInjection;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.DependencyInjection;

namespace CommandHandling.Api.Tests.Startups
{
    public class SimpleStartup
    {
        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {
            services.RegisterHandler<MathCommand, int, string>(
                        (math, size) => math.Square(size), 
                        o => {
                            o.Method = HttpMethod.Get; 
                            o.Route = "routed";
                            }); 
            services.AddHandlers(o => o.GenaratedFilesPath = "d:\\tmp\\controllers");
        }

        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {}
    }
}
