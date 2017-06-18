using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.CodeAnalysis;
using Paketti.Contexts;

namespace Paketti.Extensions
{
    internal static class ITypeMemberContextExtensions
    {
        /// <summary>
        /// Gets a single key defined by a collection of keys.
        /// The keys are sorted to ensure two enumerations with the same keys will match.
        /// </summary>
        /// <param name="keyedItems"></param>
        /// <returns></returns>
        internal static Result EnsureExistInDocuments(this IEnumerable<ITypeMemberContext> members, IEnumerable<Document> documents)
        {
            var membersNotInDocuments =
                members.Where(m => !documents.Any(d => d.ContainsNode(m.Declaration)));

            if (membersNotInDocuments.Any())
            {
                var list = membersNotInDocuments.Select(x => x.ToString()).ToSeparatedString(Environment.NewLine);
                return Result.Fail<Project>("Every member should exist within a document. These did not: " + list);
            }
            else
                return Result.Ok();
        }
    }
}