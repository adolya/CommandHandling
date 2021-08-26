using FluentAssertions;
using CommandHandling.Mvc.DependencyInjection.Extensions;
using CommandHandling.Mvc.DependencyInjection;
using Xunit;
using CommandHandling.Samples;
using CommandHandling.Samples.Models;
using Microsoft.AspNetCore.Hosting;
using CommandHandling.Api.Tests.Startups;
using System.Linq;
using System;
using System.Linq.Expressions;

namespace CommandHandling.Api.Tests
{
    public class ActionDetailsTests
    {
        const string Comments = "Comments";

        [Fact]
        public void ActionDetails_FillWithoutParametersAndResponse()
        {
            Validate(ActionDetails.Fill<MathCommand>((math) => math.Do(), (mi) => { return Comments; }),
                string.Empty,
                string.Empty,
                "void",
                "math.Do()",
                "Do",
                "math");
        }

        [Fact]
        public void ActionDetails_FillWithoutParameters()
        {
            Validate(ActionDetails.Fill<MathCommand, string>((math) => math.Pi(), (mi) => { return Comments; }),
                string.Empty,
                string.Empty,
                typeof(string).FullName,
                "math.Pi()",
                "Pi",
                "math");
        }

        [Fact]
        public void ActionDetails_FillWithoutResponse()
        {
            Validate(ActionDetails.Fill<MathCommand, SizeRequest>((math, size) => math.Calculate(size), (mi) => { return Comments; }),
                typeof(SizeRequest).FullName,
                "size",
                "void",
                "math.Calculate(size)",
                "Calculate",
                "math");
        }

        [Fact]
        public void ActionDetails_FillRequestResponse()
        {
            Validate(ActionDetails.Fill<MathCommand, SizeRequest, ResultResponse>((math, size) => math.Square(size), (mi) => { return Comments; }),
                typeof(SizeRequest).FullName,
                "size",
                typeof(ResultResponse).FullName,
                "math.Square(size)",
                "Square",
                "math");
        }

        // TODO [Fact]
        public void ActionDetails_FillsAnonimousRequestResponse()
        {
            Func<MathCommand, SizeRequest, ResultResponse> funca = (math, size) => {
                    var doubleSize = new SizeRequest {Size = size.Size * 2};
                    return math.Square(doubleSize);
                };
            Expression<Func<MathCommand, SizeRequest, ResultResponse>> expression = (math, size) => funca(math, size);
            Validate(ActionDetails.Fill(expression, (mi) => { return Comments; } ),
                typeof(SizeRequest).FullName,
                "size",
                typeof(ResultResponse).FullName,
                @"{
                    var doubleSize = new SizeRequest {Size = size.Size * 2};
                    return math.Square(doubleSize);
                }",
                "funca",
                "math");
        }

        private void Validate(ActionDetails actionDetails,
            string paramType,
            string paramName,
            string responseType,
            string methodBody,
            string methodName,
            string commandName)
        {
            actionDetails.ResponseType.Should().BeEquivalentTo(responseType);
            actionDetails.RequestType.Should().BeEquivalentTo(paramType);
            actionDetails.ParameterName.Should().BeEquivalentTo(paramName);
            actionDetails.MethodBody.Should().BeEquivalentTo(methodBody);
            actionDetails.CommandName.Should().BeEquivalentTo(commandName);
            actionDetails.Comments.Should().BeEquivalentTo(Comments);
        }
    }
}
