using System;

namespace SFA.DAS.Commitments.Application.Extensions
{
    public static class StringExtensions
    {
        public static T ToEnum<T>(this string value) where T : struct
        {
            return (T) Enum.Parse(typeof(T), value);
        }
    }
}