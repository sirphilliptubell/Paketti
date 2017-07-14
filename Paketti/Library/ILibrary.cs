using System.Collections.Generic;

namespace Paketti.Library
{
    /// <summary>
    /// A library of packages.
    /// </summary>
    public interface ILibrary
    {
        /// <summary>
        /// Gets the packages.
        /// </summary>
        /// <value>
        /// The packages.
        /// </value>
        IEnumerable<IPackage> Packages { get; }

        /// <summary>
        /// Stores the specified package of extension methods.
        /// </summary>
        /// <param name="package">The package.</param>
        /// <exception cref="System.ArgumentException">package</exception>
        void AddOrMerge(InterwovenExtensionMethodsPackage package);

        /// <summary>
        /// Stores the specified package of optional members for a type.
        /// </summary>
        /// <param name="package">The package.</param>
        void AddOrMerge(InterwovenTypeMembersPackage package);
    }
}