using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
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
        
        public static void GenerateControllerType<TCommand, TRequest, TResponse>(this IControllerDetails controllerDetails)
        {
            var httpMethod = controllerDetails.HttpMethod();
            var requestTypePrefix = httpMethod == "Post" ? "[FromBody]" : string.Empty; // Ugly solution TODO something better

            var controllerAttributes = string.Join($"{Environment.NewLine}", new string[] {
                "[ApiController]",
                $"    [Route(\"{controllerDetails.ActionDetails.CommandName}\")]",
                $"    [ApiExplorerSettings(GroupName = \"{typeof(TCommand).Name}\")]"});
            
            var comments = (string.IsNullOrEmpty(controllerDetails.ActionDetails.Comments) ? 
                                new string[0] 
                                : controllerDetails.ActionDetails.Comments.Split(Environment.NewLine))
                            .Where(_ => !string.IsNullOrWhiteSpace(_))
                            .Select(_ => $"/// {_}" ).ToList();
            comments.Add($"[Http{httpMethod}(\"{controllerDetails.Route()}\")]");
            var methodAttributes = string.Join($"{Environment.NewLine}", comments);
            controllerDetails.GeneratedCode = $@"using Microsoft.AspNetCore.Mvc;
            
namespace CommandHandling.GeneratedControllers
{{
    {controllerAttributes}
    public class {controllerDetails.ControllerName()} : ControllerBase
    {{
        private readonly {typeof(TCommand).FullName} _{controllerDetails.ActionDetails.CommandName};
        public {controllerDetails.ControllerName()}({typeof(TCommand).FullName} {controllerDetails.ActionDetails.CommandName})
        {{
            _{controllerDetails.ActionDetails.CommandName} = {controllerDetails.ActionDetails.CommandName};
        }}

        {methodAttributes}
        public {typeof(TResponse).FullName} Process({requestTypePrefix}{typeof(TRequest).FullName} {controllerDetails.ActionDetails.ParameterName})
        {{
            return _{controllerDetails.ActionDetails.CommandName}.{controllerDetails.ActionDetails.MethodName}({controllerDetails.ActionDetails.ParameterName});
        }}
    }}
}}";          
        }

        public static Assembly ToAssembly(this IEnumerable<IControllerDetails> handlerInfos)
        {
            var syntaxTrees = handlerInfos.Select(_ => CSharpSyntaxTree.ParseText(_.GeneratedCode)).ToList();
            var dependcies = handlerInfos.SelectMany(_ => _.References).Select(_ => _.Assembly.Location).Distinct().ToList();
            var assemblyPath = Path.GetDirectoryName(typeof(object).Assembly.Location);
            if (!string.IsNullOrEmpty(assemblyPath))
            {
                dependcies.Add(Path.Combine(assemblyPath, "mscorlib.dll"));
                dependcies.Add(Path.Combine(assemblyPath, "System.dll"));
                dependcies.Add(Path.Combine(assemblyPath, "System.Core.dll"));
                dependcies.Add(Path.Combine(assemblyPath, "System.Runtime.dll"));
                dependcies.Add(Path.Combine(assemblyPath, "netstandard.dll"));
            }
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

            var xmlDocPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, $"{GeneratedNamespace}.xml");
            // Delete the file if it exists.
            if (File.Exists(xmlDocPath))
            {
                File.Delete(xmlDocPath);
            }
            // Compile it to a memory stream
            var memoryStream = new MemoryStream();
            
            //Create the file.
            using (FileStream fs = File.Create(xmlDocPath))
            {
                var result = compilation.Emit(
                                memoryStream,
                                null, 
                                fs);
                if (!result.Success)
                {
                    var stringBuilder = new StringBuilder();
                    foreach (var diagnostic in result.Diagnostics)
                    {
                        stringBuilder.AppendLine(diagnostic.ToString());
                    }
                    Console.WriteLine(stringBuilder.ToString());
                }
                memoryStream.Dispose();
            }
                        // If it was not successful, throw an exception to fail the test
            

            var dynamicallyCompiledAssembly = Assembly.Load(memoryStream.ToArray());
            return dynamicallyCompiledAssembly;
        }
    }
}
