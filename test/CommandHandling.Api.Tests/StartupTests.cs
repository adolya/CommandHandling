using System;
using CommandHandling.Api.Tests.Startups;
using CommandHandling.Mvc.DependencyInjection;
using Xunit;

namespace CommandHandling.Api.Tests
{
    public class StartupTests : BaseTest<SimpleStartup>
    {
        public StartupTests() : base()
        {
        }
        
        [Fact]
        public void Test1()
        {
            var handlerOptions = (CommandHandlersOptions)WebServer.Services.GetService(typeof(CommandHandlersOptions));
            //var assembly = handlerOptions.Handlers.ToAssembly();
        }

    }
}
