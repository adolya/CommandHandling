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
        
        public static string GenerateControllerType(
            string genericSignature, 
            string controllerName, 
            string httpMethodAttribute, 
            string requestType,
            string responseType,
            string requestTypePrefix)
        {
            return ControllerTemplate.GreneralTemplate
            .Replace("{genericSignature}", genericSignature)
            .Replace("{controllerName}", controllerName)
            .Replace("{httpMethodAttribute}", httpMethodAttribute)
            .Replace("{requestType}", requestType)
            .Replace("{responseType}", responseType)
            .Replace("{requestTypePrefix}", requestTypePrefix);  
        }
        public static string GenerateControllerType<TCommand, TRequest, TResponse>(this Expression<Func<TCommand, TRequest, TResponse>> handler, string route, string httpMethod)
        {
            Type commandType = typeof(TCommand); 
            Type request = typeof(TRequest); 
            Type response = typeof(TResponse);

            return GenerateControllerType(
                $"{commandType.FullName}, {request.FullName}, {response.FullName}",
                $"{route.Replace('/', '_')}_{httpMethod}Controller",
                $"Http{httpMethod}(\"{route}\")",
                request.FullName,
                response.FullName,
                httpMethod == "Post" ? "[FromBody]" : string.Empty // Ugly solution TODO something better
            ); 
        }

        public static string GenerateControllerTypeS<TCommand, TRequest, TResponse>(this Expression<Func<TCommand, TRequest, TResponse>> handler, string route, string httpMethod)
        {
            Type commandType = typeof(TCommand); 
            Type request = typeof(TRequest); 
            Type response = typeof(TResponse);

            string generatedTypeName = $"{route.Replace('/', '_')}_{httpMethod}Controller";
            var genericArgumentsSignarture = $"{commandType.FullName}, {request.FullName}, {response.FullName}";
            var syntaxFactory = SyntaxFactory.CompilationUnit();

            // Add System using statement: (using CommandHandling.CommandHandlers)
            syntaxFactory = syntaxFactory.AddUsings(SyntaxFactory.UsingDirective(SyntaxFactory.ParseName("CommandHandling.CommandHandlers")));
            // Add System using statement: (using Microsoft.AspNetCore.Mvc)
            syntaxFactory = syntaxFactory.AddUsings(SyntaxFactory.UsingDirective(SyntaxFactory.ParseName("Microsoft.AspNetCore.Mvc")));

            // Create a namespace: (namespace Microsoft.AspNetCore.Mvc)
            var @namespace = SyntaxFactory.NamespaceDeclaration(SyntaxFactory.ParseName(GeneratedNamespace)).NormalizeWhitespace();

            //  Create a class: (class CommandController<,,>)
            var classDeclaration = SyntaxFactory.ClassDeclaration(generatedTypeName);

            // Add the public modifier: (public class CommandController<,,>)
            classDeclaration = classDeclaration.AddModifiers(SyntaxFactory.Token(SyntaxKind.PublicKeyword));

            // Inherit ControllerBase: (public class CommandController<TCommand, TRequest, TResponse> : ControllerBase)
            classDeclaration = classDeclaration.AddBaseListTypes(
                SyntaxFactory.SimpleBaseType(SyntaxFactory.ParseTypeName("ControllerBase")));

            // Create a string variable: (CommandHandler<TCommand, TRequest, TResponse> CommandHandler;)
            var variableDeclaration = SyntaxFactory.VariableDeclaration(SyntaxFactory.ParseTypeName($"CommandHandler<{genericArgumentsSignarture}>"))
                .AddVariables(SyntaxFactory.VariableDeclarator("CommandHandler"));

            // Create a field declaration: (private readonly CommandHandler<TCommand, TRequest, TResponse> CommandHandler;)
            var propertyDeclaration = SyntaxFactory.FieldDeclaration(variableDeclaration)
                .AddModifiers(
                    SyntaxFactory.Token(SyntaxKind.PrivateKeyword),
                    SyntaxFactory.Token(SyntaxKind.ReadOnlyKeyword));

            // Create a stament with the body of a method.
            var ctorSyntax = SyntaxFactory.ParseStatement("CommandHandler = commandHandler;");

            // Create a ctor
            var ctorDeclaration = SyntaxFactory.ConstructorDeclaration(classDeclaration.Identifier)
                .AddModifiers(SyntaxFactory.Token(SyntaxKind.PublicKeyword))
                .AddParameterListParameters(
                    SyntaxFactory.Parameter(
                        SyntaxFactory.Identifier("commandHandler"))
                        .WithType(SyntaxFactory.ParseTypeName($"CommandHandler<{genericArgumentsSignarture}>"))
                )
                .WithBody(SyntaxFactory.Block(ctorSyntax));

            var methodSyntax = SyntaxFactory.ParseStatement("return CommandHandler.Handle(request);");
            var httpMethodAttribute = SyntaxFactory.Attribute(
                            SyntaxFactory.ParseName($"Http{httpMethod}"),
                            SyntaxFactory.ParseAttributeArgumentList($"(\"{route}\")"));

            
            string requestType = request.FullName;
            if (httpMethod == "Post") // TODO find a better solution here
            {
                requestType = $"[FromBody]{requestType}";
            }

            // Create a HttpMethod
            var methodDeclaration = 
             SyntaxFactory.MethodDeclaration(SyntaxFactory.ParseTypeName($"{response.FullName}"), "Process")
                .AddModifiers(SyntaxFactory.Token(SyntaxKind.PublicKeyword))
                .AddParameterListParameters(
                    SyntaxFactory.Parameter(
                        SyntaxFactory.Identifier("request"))
                        .WithType(SyntaxFactory.ParseTypeName(requestType).NormalizeWhitespace())
                )
                .AddAttributeLists(
                    SyntaxFactory.AttributeList(
                        SyntaxFactory.SingletonSeparatedList(
                            httpMethodAttribute
                        )
                    )
                )
                .WithBody(SyntaxFactory.Block(methodSyntax));

            // Add the field, the property and method to the class.
            classDeclaration = classDeclaration.AddMembers(propertyDeclaration, ctorDeclaration, methodDeclaration);

            // Add the class to the namespace.
            @namespace = @namespace.AddMembers(classDeclaration);

            // Add the namespace to the compilation unit.
            syntaxFactory = syntaxFactory.AddMembers(@namespace);

            // Normalize and get code as string.
            var code = syntaxFactory
                .NormalizeWhitespace()
                .ToFullString();

            // Output new code to the console.
            Console.WriteLine(code);

            return code;
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
