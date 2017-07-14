using System;
using Paketti.Contexts;

namespace Paketti.Rewriters
{
    /// <summary>
    /// Contains the information for a struct that was extracted from a document.
    /// </summary>
    public class ExtractedStructInfo
    {
        /// <summary>
        /// Gets the DocumentContext the struct was extracted from.
        /// </summary>
        public DocumentContext Document { get; }

        /// <summary>
        /// Gets the struct that was extracted.
        /// </summary>
        public StructContext Struct { get; }

        /// <summary>
        /// Initializes a new instance of the <see cref="ExtractedClassInfo"/> class.
        /// </summary>
        /// <param name="documentContext">The document the struct was extracted from.</param>
        /// <param name="structContext">The struct that was extracted.</param>
        public ExtractedStructInfo(DocumentContext documentContext, StructContext structContext)
        {
            Document = documentContext ?? throw new ArgumentException(nameof(documentContext));
            Struct = structContext ?? throw new ArgumentException(nameof(structContext));
        }
    }
}