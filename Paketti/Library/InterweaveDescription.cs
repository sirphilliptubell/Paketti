using System;
using System.Diagnostics;
using System.Text;
using Paketti.Contexts;
using Paketti.Extensions;
using Paketti.Primitives;

namespace Paketti.Library
{
    /// <summary>
    /// A description for an interweaved type.
    /// </summary>
    [DebuggerDisplay("{DebuggerDisplay,nq}")]
    public class InterweaveDescription :
        IKeyable
    {
        /// <summary>
        /// Gets the <see cref="TypeContext"/> for the interweaved type.
        /// </summary>
        public TypeContext TypeContext { get; }

        /// <summary>
        /// Initializes a new instance of the <see cref="InterweaveDescription"/> class.
        /// </summary>
        /// <param name="interweaveTypeContext"></param>
        public InterweaveDescription(TypeContext interweaveTypeContext)
        {
            if (interweaveTypeContext == null) throw new ArgumentNullException(nameof(interweaveTypeContext));
            if (!interweaveTypeContext.IsInterweave) throw new ArgumentException(nameof(interweaveTypeContext) + " is not an interweave");

            TypeContext = interweaveTypeContext;
        }

        /// <summary>
        /// Gets the key that describes the interweaved type.
        /// </summary>
        public string Key
            => TypeContext.FullNameWithoutGenericNames;

        /// <summary>
        /// Returns the Key.
        /// </summary>
        /// <returns></returns>
        public override string ToString()
            => Key;

        public override bool Equals(object obj)
            => obj is InterweaveDescription dep && dep.Key == this.Key;

        public override int GetHashCode()
            => Key.GetHashCode();

        private string DebuggerDisplay
            => Key;
    }
}