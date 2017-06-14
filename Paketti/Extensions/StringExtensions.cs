using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Paketti.Extensions
{
    internal static class StringExtensions
    {
        internal static string ToCommaSeparated(this IEnumerable<string> s)
            => ToSeparatedString(s, ",");

        internal static string ToSeparatedString(this IEnumerable<string> s, string separator)
            => s.Aggregate(
                seed: new StringBuilder(),
                func: (sb, nextString) => sb.AppendWithSeparatorAfterFirstEntry(nextString, separator),
                resultSelector: sb => sb.ToString());

        private static StringBuilder AppendWithSeparatorAfterFirstEntry(this StringBuilder sb, string s, string separator = ",")
                            => sb.Length == 0
            ? sb.Append(s)
            : sb.Append(separator).Append(s);
    }
}