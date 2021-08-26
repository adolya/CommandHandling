using System;
using System.IO;
using System.Net.Http;
using System.Reflection;
using CommandHandling.Mvc.DependencyInjection.Extensions;
using CommandHandling.Samples;
using CommandHandling.Samples.Models;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Controllers;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.OpenApi.Models;

namespace CommandHandling.WebApi.Sample
{
    public class Startup
    {
        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        public IConfiguration Configuration { get; }

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {
            services.AddControllers();

            services.RegisterHandler<Some, WeatherForecastInput, WeatherForecast>(
                        (math, size) => math.Do(size),
                        ctx => ctx.Route = "some/do");
            
            services.RegisterHandler<MathCommand, int, string>((math, size) => math.SquareRoot(size)); // simple types
            services.RegisterHandler<MathCommand, SizeRequest, ResultResponse>((math, size) => math.Square(size)); 
            services.RegisterHandler<MathCommand, SizeRequest>((math, size) => math.Calculate(size)); 
            services.RegisterHandler<MathCommand, string>((math) => math.Pi()); 
            services.RegisterHandler<MathCommand>((math) => math.Do()); 

            services.AddHandlers(/* o => o.GenaratedFilesPath = "d:\\tmp\\controllers" */);


            services.AddSwaggerGen(c =>
            {
                c.SwaggerDoc("v1", new OpenApiInfo { Title = "CommandHandling.WebApi.Sample", Version = "v1" });

                var path1 = AppDomain.CurrentDomain.BaseDirectory;
                var path2 = Assembly.GetEntryAssembly()?.GetName().Name;
                var path = Path.Combine(path1, path2);
                var xmlDocPath = $@"{path}.xml";

                if (File.Exists(xmlDocPath))
                {
                    c.IncludeXmlComments(xmlDocPath);
                }
                
                c.AddHandlerDocs();
                
                c.TagActionsBy(api =>
                    {
                        if (api.GroupName != null)
                        {
                            return new[] { api.GroupName };
                        }

                        var controllerActionDescriptor = api.ActionDescriptor as ControllerActionDescriptor;
                        if (controllerActionDescriptor != null)
                        {
                            return new[] { controllerActionDescriptor.ControllerName };
                        }

                        throw new InvalidOperationException("Unable to determine tag for endpoint.");
                    });
                c.DocInclusionPredicate((name, api) => true);
            });
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
                app.UseSwagger();
                app.UseSwaggerUI(c => c.SwaggerEndpoint("/swagger/v1/swagger.json", "CommandHandling.WebApi.Sample v1"));
            }

            app.UseHttpsRedirection();

            app.UseRouting();

            app.UseAuthorization();

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllers();
            });
        }
    }
}
