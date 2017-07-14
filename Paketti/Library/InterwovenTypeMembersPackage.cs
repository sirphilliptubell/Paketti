using System;
using System.Diagnostics;

namespace Paketti.Library
{
    /// <summary>
    /// A package of the instance/static type members which depend on interwoven types.
    /// </summary>
    [DebuggerDisplay("{DebuggerDisplay,nq}")]
    public class InterwovenTypeMembersPackage :
        IMergeablePackage<InterwovenTypeMembersPackage>
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
        /// Gets the descriptions of the interweaves this package depends on.
        /// </summary>
        public InterweaveDescriptions Descriptions { get; }

        /// <summary>
        /// Initializes a new instance of the <see cref="InterwovenExtensionMethodsPackage"/> class.
        /// </summary>
        /// <param name="content">The content.</param>
        /// <param name="kind">The kind.</param>
        /// <param name="typeDependencies">The type dependencies.</param>
        public InterwovenTypeMembersPackage(string typeName, string content, InterweaveDescriptions descriptions)
        {
            TypeName = typeName ?? throw new ArgumentNullException(nameof(typeName));
            Content = content?.Trim() ?? throw new ArgumentNullException(nameof(content));
            Descriptions = descriptions ?? throw new ArgumentNullException(nameof(descriptions));

            Key = $"{Kind} {typeName} {Descriptions.Key}";
        }

        /// <summary>
        /// Returns <see cref="PackageKind.InterwovenTypeMembersPackage"/>
        /// </summary>
        public PackageKind Kind
            => PackageKind.InterwovenTypeMembersPackage;

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
        public InterwovenTypeMembersPackage MergeWith(InterwovenTypeMembersPackage package)
        {
            if (package == null) throw new ArgumentNullException(nameof(package));
            if (package.Key != this.Key) throw new ArgumentException("Key does not match.");
            if (package.TypeName != this.TypeName) throw new ArgumentException("TypeName does not match.");

            var newContent = this.Content.Trim() + Environment.NewLine + Environment.NewLine + package.Content;

            return new InterwovenTypeMembersPackage(TypeName, newContent, Descriptions);
        }

        private string DebuggerDisplay
            => Key;
    }
}