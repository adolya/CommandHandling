using System;
using System.Linq;
using System.Linq.Expressions;

namespace CommandHandling.Mvc.DependencyInjection
{
    public class ActionDetails
    {
        public string CommandName {get; set;}
        
        public string MethodName {get; set;}
        
        public string ParameterName {get; set;}

        public string Comments {get; set;}

        internal void Fill<TCommand, TRequest, TResponse>(
                Expression<Func<TCommand, TRequest, TResponse>> handler,
                XmlDocumentation xmlDocs) 
            where TCommand : class
        {
            var body = handler.Body as MethodCallExpression;
            var instance = body.Object as ParameterExpression;
            CommandName = instance.Name;
            MethodName = body.Method.Name;
            ParameterName = body.Method.GetParameters().First().Name;
            Comments = xmlDocs.GetDocumentation(body.Method);
        }
    }
}
