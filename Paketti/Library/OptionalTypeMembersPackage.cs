using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Paketti.Extensions;

namespace Paketti.Library
{
    /// <summary>
    /// A package of the optional members for a Type.
    /// </summary>
    public class OptionalTypeMembersPackage :
        IMergeablePackage<OptionalTypeMembersPackage>
    {
        /// <summary>
        /// Gets the name of the class / struct / interface.
        /// </summary>
        public string TypeName { get; }

        /// <summary>
        /// Gets the functions and properties for the type.
        /// </summary>
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
        public OptionalTypeMembersPackage(string typeName, string content, IEnumerable<string> typeDependencies)
        {
            TypeName = typeName ?? throw new ArgumentNullException(nameof(typeName));
            Content = content?.Trim() ?? throw new ArgumentNullException(nameof(content));
            TypeDependencies = typeDependencies?.ToList() ?? throw new ArgumentNullException(nameof(typeDependencies));

            Key = $"{Kind} {typeName} {typeDependencies.OrderBy(x => x).ToCommaSeparated()}";
        }

        /// <summary>
        /// Returns <see cref="PackageKind.OptionalTypeMembersPackage"/>
        /// </summary>
        public PackageKind Kind
            => PackageKind.OptionalTypeMembersPackage;

        /// <summary>
        /// Appends the specified package to this one.
        /// The Kind and Keys of the package must match this package.
        /// </summary>
        /// <param name="package">The package.</param>
        /// <returns></returns>
        /// <exception cref="System.ArgumentNullException">package</exception>
        /// <exception cref="System.ArgumentException">
        /// Key does not match.
        /// or
        /// TypeName does not match.
        /// </exception>
        public OptionalTypeMembersPackage MergeWith(OptionalTypeMembersPackage package)
        {
            if (package == null) throw new ArgumentNullException(nameof(package));
            if (package.Key != this.Key) throw new ArgumentException("Key does not match.");
            if (package.TypeName != this.TypeName) throw new ArgumentException("TypeName does not match.");

            var newContent = this.Content.Trim() + Environment.NewLine + Environment.NewLine + package.Content;

            return new OptionalTypeMembersPackage(TypeName, newContent, TypeDependencies.ToList());
        }
    }
}