using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using Paketti.Contexts;
using Paketti.Extensions;

namespace Paketti.Library
{
    /// <summary>
    /// A collection of <see cref="InterweaveDescription"/>s.
    /// </summary>
    [DebuggerDisplay("{DebuggerDisplay,nq}")]
    public class InterweaveDescriptions :
        IKeyable
    {
        /// <summary>
        /// Gets the <see cref="InterweaveDescription"/>s.
        /// </summary>
        public IOrderedEnumerable<InterweaveDescription> Descriptions { get; }

        /// <summary>
        /// Initializes a new instance of the <see cref="InterweaveDescriptions"/> class.
        /// </summary>
        /// <param name="descriptions"></param>
        public InterweaveDescriptions(IEnumerable<InterweaveDescription> descriptions)
        {
            if (descriptions == null) throw new ArgumentException(nameof(descriptions));

            Descriptions =
                descriptions
                .Distinct()
                .OrderBy(x => x.Key);
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="InterweaveDescriptions"/> class.
        /// </summary>
        /// <param name="typeContexts"></param>
        public InterweaveDescriptions(IEnumerable<TypeContext> typeContexts)
        {
            if (typeContexts == null) throw new ArgumentException(nameof(typeContexts));

            Descriptions =
                typeContexts
                .Select(x => new InterweaveDescription(x))
                .Distinct()
                .OrderBy(x => x.Key);
        }

        /// <summary>
        /// Gets the key that uniquely identifies the collection of <see cref="InterweaveDescription"/>s.
        /// </summary>
        public string Key
            => Descriptions.GetCollectiveOrderedKey();

        private string DebuggerDisplay
            => Key;
    }
}