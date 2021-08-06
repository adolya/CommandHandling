using System;
using CommandHandling.Api.Tests.Models;

namespace CommandHandling.Api.Tests
{
    public class MathCommand
    {
        public string Square(int size)
        {
            return $"Result is : {size * size}";
        }

        public SummaryResponse Square(TestRequest request)
        {
            return new SummaryResponse { Result = $"{ request.Size * request.Size }"};
        }
    }
}
