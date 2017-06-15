using System;
using System.Collections.Generic;

namespace Paketti.Packaging
{
    /// <summary>
    /// Stores packages.
    /// </summary>
    /// <seealso cref="Paketti.Packaging.IPackageStore" />
    public class PackageStore :
        IPackageStore
    {
        private readonly Dictionary<string, Package> _packages = new Dictionary<string, Package>();

        /// <summary>
        /// Gets the packages.
        /// </summary>
        /// <value>
        /// The packages.
        /// </value>
        public IEnumerable<Package> Packages
            => _packages.Values;

        /// <summary>
        /// Stores the specified content.
        /// </summary>
        /// <param name="content">The content.</param>
        /// <param name="kind">The kind of the package.</param>
        /// <param name="typeDependencies">The type dependencies.</param>
        public void Store(string content, PackageKind kind, IEnumerable<string> typeDependencies)
            => Store(new Package(content, kind, typeDependencies));

        /// <summary>
        /// Stores the specified package.
        /// </summary>
        /// <param name="package">The package.</param>
        /// <exception cref="System.ArgumentException">package</exception>
        private void Store(Package package)
        {
            if (package == null) throw new ArgumentException(nameof(package));

            if (_packages.ContainsKey(package.Key))
                _packages[package.Key] = _packages[package.Key].Append(package);
            else
                _packages[package.Key] = package;
        }
    }
}