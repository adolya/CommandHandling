using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;

namespace CommandHandling.Mvc.DependencyInjection
{
    public class ActionDetails
    {
        public string CommandName { get; private set; }
        public string MethodName {get; set;}
        public string MethodBody {get; set;}
        public string ParameterName {get; set;}
        public string Comments {get; set;}
        public string RequestType {get; set;}
        public string ResponseType {get; set;}

        public ActionDetails(
            string commandName, 
            string methodName, 
            string methodBody, 
            string parameterName, 
            string comments, 
            string requestType, 
            string responseType)
        {
            CommandName = commandName;
            MethodName = methodName;
            MethodBody = methodBody;
            ParameterName = parameterName;
            Comments = comments;
            RequestType = requestType;
            ResponseType = responseType;
        }

        public static ActionDetails Fill<TCommand, TRequest, TResponse>(
            Expression<Func<TCommand, TRequest, TResponse>> expression, Func<MethodInfo, string> extractComments)
        {
            return ExtractFromLambda(expression, expression.Parameters[1], extractComments);
        }

        public static ActionDetails Fill<TCommand, TRequest>(
                Expression<Action<TCommand, TRequest>> expression, Func<MethodInfo, string> extractComments)
        {
            return ExtractFromLambda(expression, expression.Parameters[1], extractComments);
        }

        public static ActionDetails Fill<TCommand, TResponse>(
                Expression<Func<TCommand, TResponse>> expression, Func<MethodInfo, string> extractComments)
        {
            return ExtractFromLambda(expression, null, extractComments);
        }

        public static ActionDetails Fill<TCommand>(
                Expression<Action<TCommand>> expression, Func<MethodInfo, string> extractComments)
        {
            return ExtractFromLambda(expression, null, extractComments);
        }

        private static ActionDetails ExtractFromLambda(LambdaExpression expression, ParameterExpression? requestParam, Func<MethodInfo, string> extractComments)
        {
            var methodName = string.Empty;
            var methodBody = expression.Body.ToString();
            var comments = string.Empty;
            var commandName = string.Empty;
            if (expression.Body is MethodCallExpression methodCallExpression)
            {
                methodName = methodCallExpression.Method.Name;
                comments = extractComments(methodCallExpression.Method);
                if (methodCallExpression.Object is ParameterExpression parameterExpression)
                    commandName = parameterExpression.Name;
            }

            if (expression.Body is InvocationExpression invocationExpression)
            {
                if (invocationExpression.Expression is MemberExpression innerExpression)
                {
                    methodName = innerExpression.Member.Name;
                    var constExpression = innerExpression.Expression as ConstantExpression;
                }
                
                throw new NotSupportedException("Comming soon..."); 
            }

            if (string.IsNullOrEmpty(commandName) && expression.Parameters.Any())
                commandName = expression.Parameters[0].Name;

            return new ActionDetails(
                                    commandName ?? string.Empty,
                                    methodName,
                                    methodBody,
                                    requestParam?.Name ?? string.Empty,
                                    comments,
                                    requestParam?.Type.FullName ?? string.Empty,
                                    string.IsNullOrEmpty(expression?.ReturnType?.FullName) ? string.Empty : 
                                        (expression.ReturnType.FullName == "System.Void" ? "void" : expression.ReturnType.FullName));
        }
    }
}
