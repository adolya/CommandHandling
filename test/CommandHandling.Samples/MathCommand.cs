using System;
using CommandHandling.Samples.Models;

namespace CommandHandling.Samples
{
    /// <summary>
    /// sUPER Mega math class
    /// </summary>
    public class MathCommand
    {
        /// <summary>
        /// Returns the square root of a specified number.
        /// </summary>
        /// <param name="size">The number whose square root is to be found.</param>
        /// <returns>One of the values in the following table.
        ///     d parameter – Return value
        ///     Zero or positive – The positive square root of d.
        ///     Negative –System.Double.NaN
        ///     Equals System.Double.NaN –System.Double.NaN
        ///     Equals System.Double.PositiveInfinity –System.Double.PositiveInfinity
        /// </returns>
        public string SquareRoot(int size)
        {
            return $"{Math.Sqrt(size)}";
        }

        /// <summary>
        /// Weakly described class
        /// </summary>
        /// <param name="request">Some request here</param>
        /// <returns>some response here</returns>
        public ResultResponse Square(SizeRequest request)
        {
            return new ResultResponse { Result = $"{ request.Size * request.Size }"};
        }
    }
}
