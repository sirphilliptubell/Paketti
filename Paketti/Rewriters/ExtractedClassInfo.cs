using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.CodeAnalysis;
using Paketti.Contexts;

namespace Paketti.Rewriters
{
    /// <summary>
    /// Contains the information for a class that was extracted from a document.
    /// </summary>
    public class ExtractedClassInfo
    {
        /// <summary>
        /// Gets the DocumentContext the class was extracted from.
        /// </summary>
        public DocumentContext Document { get; }

        /// <summary>
        /// Gets the class that was extracted.
        /// </summary>
        public ClassContext Class { get; }

        /// <summary>
        /// Initializes a new instance of the <see cref="ExtractedClassInfo"/> class.
        /// </summary>
        /// <param name="documentContext">The document the class was extracted from.</param>
        /// <param name="classContext">The class that was extracted.</param>
        public ExtractedClassInfo(DocumentContext documentContext, ClassContext classContext)
        {
            Document = documentContext ?? throw new ArgumentException(nameof(documentContext));
            Class = classContext ?? throw new ArgumentException(nameof(classContext));
        }
    }
}