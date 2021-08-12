using System;
using System.IO;
using Microsoft.Extensions.DependencyInjection;
using Swashbuckle.AspNetCore.SwaggerGen;

namespace CommandHandling.Mvc.DependencyInjection.Extensions
{
    public static class SwaggerExtensions
    {
        public static void AddHandlerDocs(this SwaggerGenOptions options)
        {
            var xmlDocPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, $"{GeneratorExtensions.GeneratedNamespace}.xml");

            if (options != null && File.Exists(xmlDocPath))
            {
                options.IncludeXmlComments(xmlDocPath);
            }
        }
    }
}
