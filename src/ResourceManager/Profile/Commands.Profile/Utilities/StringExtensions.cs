using System;

namespace Microsoft.Azure.Commands.Profile.Utilities
{
    internal static class StringExtensions
    {
        public static string AsFormatString(this string formatString, params object[] arguments) =>
            String.Format(formatString, arguments);
    }
}
