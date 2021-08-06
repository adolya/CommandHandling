using System;
using CommandHandling.Api.Tests.Startups;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.TestHost;
using Xunit;

namespace CommandHandling.Api.Tests
{
    public class BaseTest<TStartup> : IDisposable
        where TStartup : class
    {
        protected TestServer WebServer;

        public BaseTest()
        {
            var webBuilder = new WebHostBuilder();
            webBuilder.UseStartup<DefaultStartup>();
            webBuilder.UseStartup<TStartup>();
    
            WebServer = new TestServer(webBuilder);
        }

        public void Dispose()
        {
            WebServer.Dispose();
        }
    }
}
