using FluentAssertions;
using CommandHandling.Mvc.DependencyInjection.Extensions;
using CommandHandling.Mvc.DependencyInjection;
using Xunit;
using CommandHandling.Samples;
using CommandHandling.Samples.Models;
using Microsoft.AspNetCore.Hosting;
using CommandHandling.Api.Tests.Startups;
using System.Linq;

namespace CommandHandling.Api.Tests
{
    public class StartupTests
    {
        private WebHostBuilder ConfigureServer()
        {
            var webHostBuilder = new WebHostBuilder();
            webHostBuilder.UseStartup<DefaultStartup>();
            webHostBuilder.ConfigureServices(services => 
            {   
                services.RegisterHandler<MathCommand, int, string>((math, size) => math.SquareRoot(size)); // simple types
                services.RegisterHandler<MathCommand, SizeRequest, ResultResponse>((math, size) => math.Square(size)); 
                services.RegisterHandler<MathCommand, SizeRequest>((math, size) => math.Calculate(size)); 
                services.RegisterHandler<MathCommand, string>((math) => math.Pi()); 
                services.RegisterHandler<MathCommand>((math) => math.Do()); 
                 
                services.AddHandlers( o => o.GenaratedFilesPath = "d:\\tmp\\controllers");
            });

            return webHostBuilder;
        }

        [Fact]
        public void Startup_RegistersControllersUsingCommands()
        {
            using (var webServer = ConfigureServer().Build())
            {
                var handlerOptions = (CommandHandlersOptions)webServer.Services.GetService(typeof(CommandHandlersOptions));

                handlerOptions.Controllers.Should().HaveCount(5);
                handlerOptions.Controllers.All(_ => !string.IsNullOrEmpty(_.GeneratedCode)).Should().BeTrue();
                handlerOptions.Controllers.All(_ => !string.IsNullOrEmpty(_.ControllerName())).Should().BeTrue();
                handlerOptions.Controllers.All(_ => _.HttpMethod() == "Post").Should().BeTrue();
            }
        }
    }
}
