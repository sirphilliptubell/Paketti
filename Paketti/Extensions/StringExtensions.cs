using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Paketti.Extensions
{
    internal static class StringExtensions
    {
        /// <summary>
        /// Combines multiple strings into a single comma separated string (no whitespace is added).
        /// </summary>
        /// <param name="s">The s.</param>
        /// <returns></returns>
        internal static string ToCommaSeparated(this IEnumerable<string> s)
            => ToSeparatedString(s, ",");

        /// <summary>
        /// Combines multiple strings into a single string, using the specified separator.
        /// </summary>
        /// <param name="s">The s.</param>
        /// <param name="separator">The separator.</param>
        /// <returns></returns>
        internal static string ToSeparatedString(this IEnumerable<string> s, string separator)
            => s.Aggregate(
                seed: new StringBuilder(),
                func: (sb, nextString) => sb.AppendWithSeparatorAfterFirstEntry(nextString, separator),
                resultSelector: sb => sb.ToString());

        /// <summary>
        /// Appends the specified string.
        /// First appends the separator if the StringBuilder already contains anything.
        /// </summary>
        /// <param name="sb">The sb.</param>
        /// <param name="s">The s.</param>
        /// <param name="separator">The separator.</param>
        /// <returns></returns>
        private static StringBuilder AppendWithSeparatorAfterFirstEntry(this StringBuilder sb, string s, string separator = ",")
            => sb.Length == 0
            ? sb.Append(s)
            : sb.Append(separator).Append(s);
    }
}