using System;
using System.Collections.Generic;
using System.Linq;
using Paketti.Extensions;

namespace Paketti.Library
{
    /// <summary>
    /// A package of Extension Methods.
    /// </summary>
    public class ExtensionMethodsPackage :
        IMergeablePackage<ExtensionMethodsPackage>
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="ExtensionMethodsPackage"/> class.
        /// </summary>
        /// <param name="content">The content.</param>
        /// <param name="kind">The kind.</param>
        /// <param name="typeDependencies">The type dependencies.</param>
        public ExtensionMethodsPackage(string content, IEnumerable<string> typeDependencies)
        {
            Content = content.Trim();
            TypeDependencies = typeDependencies.ToList();
            Key = $"{Kind} {typeDependencies.ToCommaSeparated()}";
        }

        /// <summary>
        /// Gets the content of the package.
        /// </summary>
        /// <value>
        /// The content.
        /// </value>
        public string Content { get; }

        /// <summary>
        /// Gets the key that uniquely identifies this package.
        /// </summary>
        /// <value>
        /// The key.
        /// </value>
        public string Key { get; }

        /// <summary>
        /// Gets the kind of package.
        /// </summary>
        /// <value>
        /// The package kind.
        /// </value>
        public PackageKind Kind
            => PackageKind.ExtensionMethods;

        /// <summary>
        /// Gets the types this package depends on.
        /// </summary>
        /// <value>
        /// The type dependencies.
        /// </value>
        public IList<string> TypeDependencies { get; }

        /// <summary>
        /// Appends the specified package to this one.
        /// The Kind and Keys of the package must match this package.
        /// </summary>
        /// <param name="package">The package.</param>
        /// <returns></returns>
        /// <exception cref="System.InvalidOperationException">
        /// Package kind does not match.
        /// or
        /// Package key does not match.
        /// </exception>
        public ExtensionMethodsPackage MergeWith(ExtensionMethodsPackage package)
        {
            if (package.Kind != this.Kind) throw new InvalidOperationException("Package kind does not match.");
            if (package.Key != this.Key) throw new InvalidOperationException("Package key does not match.");

            return new ExtensionMethodsPackage(
                Append(this.Content, package.Content),
                TypeDependencies);
        }

        private static string Append(string oldCode, string newCode)
            //oldCode should already be trimmed
            => (oldCode + Environment.NewLine + Environment.NewLine + newCode.Trim());
    }
}