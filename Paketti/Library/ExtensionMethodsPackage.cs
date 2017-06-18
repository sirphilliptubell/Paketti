﻿using System;
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
        /// Gets the types this package depends on.
        /// </summary>
        /// <value>
        /// The type dependencies.
        /// </value>
        public IReadOnlyList<string> TypeDependencies { get; }

        /// <summary>
        /// Initializes a new instance of the <see cref="ExtensionMethodsPackage"/> class.
        /// </summary>
        /// <param name="content">The content.</param>
        /// <param name="kind">The kind.</param>
        /// <param name="typeDependencies">The type dependencies.</param>
        public ExtensionMethodsPackage(string content, IEnumerable<string> typeDependencies)
        {
            Content = content?.Trim() ?? throw new ArgumentNullException(nameof(content));
            TypeDependencies = typeDependencies?.ToList() ?? throw new ArgumentNullException(nameof(typeDependencies));

            Key = $"{Kind} {typeDependencies.OrderBy(x => x).ToCommaSeparated()}";
        }

        /// <summary>
        /// Returns <see cref="PackageKind.ExtensionMethods"/>
        /// </summary>
        public PackageKind Kind
            => PackageKind.ExtensionMethods;

        /// <summary>
        /// Appends the specified package to this one.
        /// The Kind and Keys of the package must match this package.
        /// </summary>
        /// <param name="package">The package.</param>
        /// <returns></returns>
        /// <exception cref="System.ArgumentNullException">package</exception>
        /// <exception cref="System.ArgumentException">Key does not match.</exception>
        public ExtensionMethodsPackage MergeWith(ExtensionMethodsPackage package)
        {
            if (package == null) throw new ArgumentNullException(nameof(package));
            if (this.Key != package.Key) throw new ArgumentException("Key does not match.");

            var newContent = this.Content.Trim() + Environment.NewLine + Environment.NewLine + package.Content;

            return new ExtensionMethodsPackage(newContent, TypeDependencies);
        }
    }
}