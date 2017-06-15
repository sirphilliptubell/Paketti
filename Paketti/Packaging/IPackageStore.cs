using System.Collections.Generic;

namespace Paketti.Packaging
{
    /// <summary>
    /// Stores Paketti package information.
    /// </summary>
    public interface IPackageStore
    {
        /// <summary>
        /// Gets the packages.
        /// </summary>
        /// <value>
        /// The packages.
        /// </value>
        IEnumerable<Package> Packages { get; }

        /// <summary>
        /// Stores the specified content.
        /// </summary>
        /// <param name="content">The content.</param>
        /// <param name="kind">The kind of the package.</param>
        /// <param name="typeDependencies">The type dependencies.</param>
        void Store(string content, PackageKind kind, IEnumerable<string> typeDependencies);
    }
}