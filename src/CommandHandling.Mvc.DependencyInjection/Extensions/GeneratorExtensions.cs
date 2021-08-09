using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Text;
using Microsoft.AspNetCore.Mvc;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;

namespace CommandHandling.Mvc.DependencyInjection.Extensions
{
    public static class GeneratorExtensions
    {
        internal const string GeneratedNamespace = "CommandHandling.GeneratedControllers";
        
        public static void GenerateControllerType<TCommand, TRequest, TResponse>(this IControllerDetails controllerDetails, Expression<Func<TCommand, TRequest, TResponse>> handler)
        {
            var httpMethod = Char.ToUpperInvariant(controllerDetails.Options.Method.Method[0]) + controllerDetails.Options.Method.Method.Substring(1).ToLowerInvariant();
            var handlerDetails = ExtractMethodName(handler);      

            controllerDetails.Name = $"{handlerDetails.objectName}_{handlerDetails.methodName}_{httpMethod}Controller";

            var requestTypePrefix = httpMethod == "Post" ? "[FromBody]" : string.Empty; // Ugly solution TODO something better

            var controllerAttributes = string.Join($"{Environment.NewLine}", new string[] {
                "[ApiController]",
                $"    [Route(\"{handlerDetails.objectName}\")]",
                $"    [ApiExplorerSettings(GroupName = \"{typeof(TCommand).Name}\")]"});

            controllerDetails.Code = $@"using Microsoft.AspNetCore.Mvc;
            
namespace CommandHandling.GeneratedControllers
{{
    {controllerAttributes}
    public class {controllerDetails.Name} : ControllerBase
    {{
        private readonly {typeof(TCommand).FullName} _{handlerDetails.objectName};
        public {controllerDetails.Name}({typeof(TCommand).FullName} {handlerDetails.objectName})
        {{
            _{handlerDetails.objectName} = {handlerDetails.objectName};
        }}

        [Http{httpMethod}(""{controllerDetails.Options.Route ?? handlerDetails.methodName}"")]
        public {typeof(TResponse).FullName} Process({requestTypePrefix}{typeof(TRequest).FullName} {handlerDetails.parameterName})
        {{
            return _{handlerDetails.objectName}.{handlerDetails.methodName}({handlerDetails.parameterName});
        }}
    }}
}}";          
        }

        public static Assembly ToAssembly(this IEnumerable<IControllerDetails> handlerInfos)
        {
            var syntaxTrees = handlerInfos.Select(_ => CSharpSyntaxTree.ParseText(_.Code)).ToList();
            var dependcies = handlerInfos.SelectMany(_ => _.References).Select(_ => _.Assembly.Location).Distinct().ToList();
            // var assemblyPath = Path.GetDirectoryName(typeof(object).Assembly.Location);
            // dependcies.Add(Path.Combine(assemblyPath, "mscorlib.dll"));
            // dependcies.Add(Path.Combine(assemblyPath, "System.dll"));
            // dependcies.Add(Path.Combine(assemblyPath, "System.Core.dll"));
            // dependcies.Add(Path.Combine(assemblyPath, "System.Runtime.dll"));
            // dependcies.Add(Path.Combine(assemblyPath, "netstandard.dll"));
            dependcies.Add(typeof(object).Assembly.Location);
            dependcies.Add(typeof(ControllerBase).Assembly.Location);
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

        private static (string objectName, string methodName, string parameterName) ExtractMethodName<TCommand, TRequest, TResponse>(Expression<Func<TCommand, TRequest, TResponse>> handler)
        {
            var body = handler.Body as MethodCallExpression;
            var instance = body.Object as ParameterExpression;
            var instanceName = instance.Name;
            var methodName = body.Method.Name;
            var parameterName = body.Method.GetParameters();
            return (objectName: instance.Name, methodName: methodName, parameterName: parameterName.First().Name);
        }
    }
}
