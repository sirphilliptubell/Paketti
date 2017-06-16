using System;
using System.Collections.Generic;
using System.Text;

namespace Paketti.Library
{
    /// <summary>
    /// Represents a package.
    /// </summary>
    public interface IPackage
    {
        /// <summary>
        /// A unique identifier for this package.
        /// This should not only be distinct for the package type, but also for it's dependencies.
        /// </summary>
        string Key { get; }

        /// <summary>
        /// Gets the kind of package.
        /// </summary>
        /// <value>
        /// The kind.
        /// </value>
        PackageKind Kind { get; }

        /// <summary>
        /// Gets the content of the package.
        /// </summary>
        /// <value>
        /// The content.
        /// </value>
        string Content { get; }

        /// <summary>
        /// Gets the types this package depends on.
        /// </summary>
        /// <value>
        /// The type dependencies.
        /// </value>
        IList<string> TypeDependencies { get; }
    }
}