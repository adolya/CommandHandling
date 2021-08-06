using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Text;
using CommandHandling.CommandHandlers;
using Microsoft.AspNetCore.Mvc;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;

namespace CommandHandling.Mvc.DependencyInjection.Extensions
{
    public static class RoslynExtensions
    {
        internal const string GeneratedNamespace = "CommandHandling.GeneratedControllers";
        
        public static string GenerateControllerType<TCommand, TRequest, TResponse>(this Expression<Func<TCommand, TRequest, TResponse>> handler, string route, string httpMethod)
        {
            var requestTypePrefix = httpMethod == "Post" ? "[FromBody]" : string.Empty; // Ugly solution TODO something better
            var controllerName = $"{route.Replace('/', '_')}_{httpMethod}";
            var handlerName = $"ExtractMe";      

            return $@"using Microsoft.AspNetCore.Mvc;
namespace CommandHandling.GeneratedControllers
{{
    [ApiController]
    public class {controllerName}Controller : ControllerBase
    {{
        private readonly {typeof(TCommand).FullName} Command;
        public {controllerName}Controller({typeof(TCommand).FullName} command)
        {{
            Command = command;
        }}

        [Http{httpMethod}(""{route}"")]
        public {typeof(TResponse).FullName} Process({requestTypePrefix}{typeof(TRequest).FullName} request)
        {{
            return Command.{handlerName}(request);
        }}
    }}
}}";          
        }

        public static Assembly ToAssembly(this IEnumerable<CommandHandlerInfo> handlerInfos)
        {
            var syntaxTrees = handlerInfos.Select(_ => CSharpSyntaxTree.ParseText(_.ControllerCode)).ToList();
            var dependcies = handlerInfos.SelectMany(_ => _.References).Select(_ => _.Assembly.Location).Distinct().ToList();
            var assemblyPath = Path.GetDirectoryName(typeof(object).Assembly.Location);
            dependcies.Add(Path.Combine(assemblyPath, "mscorlib.dll"));
            dependcies.Add(Path.Combine(assemblyPath, "System.dll"));
            dependcies.Add(Path.Combine(assemblyPath, "System.Core.dll"));
            dependcies.Add(Path.Combine(assemblyPath, "System.Runtime.dll"));
            dependcies.Add(Path.Combine(assemblyPath, "netstandard.dll"));
            dependcies.Add(typeof(object).Assembly.Location);
            dependcies.Add(typeof(ControllerBase).Assembly.Location);
            dependcies.Add(typeof(CommandHandler<,,>).Assembly.Location);
            dependcies = dependcies.Distinct().ToList();
            //dependcies.Add(typeof(HttpPostAttribute).Assembly);
            var compilation = CSharpCompilation.Create(
                $"{GeneratedNamespace}.dll",
                syntaxTrees,
                dependcies.Select(_ => MetadataReference.CreateFromFile(_)).ToArray(),
                new CSharpCompilationOptions(
                    OutputKind.DynamicallyLinkedLibrary,
                    optimizationLevel: OptimizationLevel.Release)
            );

            // Compile it to a memory stream
            var memoryStream = new MemoryStream();
            var result = compilation.Emit(memoryStream);

                        // If it was not successful, throw an exception to fail the test
            if (!result.Success)
            {
                var stringBuilder = new StringBuilder();
                foreach (var diagnostic in result.Diagnostics)
                {
                    stringBuilder.AppendLine(diagnostic.ToString());
                }
                Console.WriteLine(stringBuilder.ToString());
            }

            var dynamicallyCompiledAssembly = Assembly.Load(memoryStream.ToArray());
            return dynamicallyCompiledAssembly;
        }
    }
}
