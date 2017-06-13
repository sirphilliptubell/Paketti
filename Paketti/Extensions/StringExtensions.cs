using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Paketti
{
    internal static class StringExtensions
    {
        private static StringBuilder AppendWithCommaAfterFirstEntry(this StringBuilder sb, string s)
            => sb.Length == 0
            ? sb.Append(s)
            : sb.Append(",").Append(s);

        internal static string ToCommaSeparated(this IEnumerable<string> s)
            => s.Aggregate(
                seed: new StringBuilder(),
                func: (sb, nextString) => sb.AppendWithCommaAfterFirstEntry(nextString),
                resultSelector: sb => sb.ToString());
    }
}