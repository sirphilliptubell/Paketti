using System;
using System.Collections.Generic;

namespace Paketti.Library
{
    /// <summary>
    /// A library of packages.
    /// </summary>
    /// <seealso cref="Paketti.Packaging.ILibrary" />
    public class Library :
        ILibrary
    {
        private readonly Dictionary<string, IPackage> _packages = new Dictionary<string, IPackage>();

        /// <summary>
        /// Gets the packages.
        /// </summary>
        /// <value>
        /// The packages.
        /// </value>
        public IEnumerable<IPackage> Packages
            => _packages.Values;

        /// <summary>
        /// Stores the specified package of extension methods.
        /// </summary>
        /// <param name="package">The package.</param>
        /// <exception cref="System.ArgumentException">package</exception>
        public void AddOrMerge(InterwovenExtensionMethodsPackage package)
        {
            if (package == null) throw new ArgumentException(nameof(package));

            if (_packages.ContainsKey(package.Key))
                _packages[package.Key] = ((InterwovenExtensionMethodsPackage)_packages[package.Key]).MergeWith(package);
            else
                _packages[package.Key] = package;
        }

        /// <summary>
        /// Stores the specified package of optional members for a type.
        /// </summary>
        /// <param name="package">The package.</param>
        /// <exception cref="System.ArgumentException">package</exception>
        public void AddOrMerge(InterwovenTypeMembersPackage package)
        {
            if (package == null) throw new ArgumentException(nameof(package));

            if (_packages.ContainsKey(package.Key))
                _packages[package.Key] = ((InterwovenTypeMembersPackage)_packages[package.Key]).MergeWith(package);
            else
                _packages[package.Key] = package;
        }

        /// <summary>
        /// Stores the specified member container package.
        /// </summary>
        /// <param name="package">The package.</param>
        public void AddOrMerge(MemberContainerPackage package)
        {
            if (package == null) throw new ArgumentException(nameof(package));

            if (_packages.ContainsKey(package.Key))
                _packages[package.Key] = ((MemberContainerPackage)_packages[package.Key]).MergeWith(package);
            else
                _packages[package.Key] = package;
        }
    }
}