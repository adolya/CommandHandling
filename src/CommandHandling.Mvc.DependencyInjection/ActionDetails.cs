using System;
using System.Linq;
using System.Linq.Expressions;

namespace CommandHandling.Mvc.DependencyInjection
{
    public class ActionDetails
    {
        public string CommandName { get; private set; }
        
        public string MethodName {get; set;}
        
        public string ParameterName {get; set;}

        public string Comments {get; set;}

        public ActionDetails(string commandName, string methodName, string parameterName, string comments)
        {
            CommandName = commandName;
            MethodName = methodName;
            ParameterName = parameterName;
            Comments = comments;
        }

        internal static ActionDetails Fill<TCommand, TRequest, TResponse>(
                Expression<Func<TCommand, TRequest, TResponse>> handler,
                XmlDocumentation xmlDocs) 
            where TCommand : class
        {
            var body = handler.Body as MethodCallExpression;
            ParameterExpression? instance = body.Object as ParameterExpression;
            return new ActionDetails(
                                    instance.Name, 
                                    body.Method.Name,
                                    body.Method.GetParameters().First().Name,
                                    xmlDocs.GetDocumentation(body.Method));
        }
    }
}
