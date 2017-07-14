using System;
using System.Diagnostics;

namespace Paketti.Library
{
    /// <summary>
    /// A package of Interwoven Extension Methods.
    /// </summary>
    [DebuggerDisplay("{DebuggerDisplay,nq}")]
    public class InterwovenExtensionMethodsPackage :
        IMergeablePackage<InterwovenExtensionMethodsPackage>
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
        /// Gets the descriptions of the interweaves this package depends on.
        /// </summary>
        public InterweaveDescriptions Descriptions { get; }

        /// <summary>
        /// Initializes a new instance of the <see cref="InterwovenExtensionMethodsPackage"/> class.
        /// </summary>
        /// <param name="content">The content.</param>
        /// <param name="kind">The kind.</param>
        /// <param name="typeDependencies">The type dependencies.</param>
        public InterwovenExtensionMethodsPackage(string content, InterweaveDescriptions descriptions)
        {
            Content = content?.Trim() ?? throw new ArgumentNullException(nameof(content));
            Descriptions = descriptions ?? throw new ArgumentNullException(nameof(descriptions));

            Key = $"{Kind} {descriptions.Key}";
        }

        /// <summary>
        /// Returns <see cref="PackageKind.InterwovenExtensionMethods"/>
        /// </summary>
        public PackageKind Kind
            => PackageKind.InterwovenExtensionMethods;

        /// <summary>
        /// Appends the specified package to this one.
        /// The Kind and Keys of the package must match this package.
        /// </summary>
        /// <param name="package">The package.</param>
        /// <returns></returns>
        /// <exception cref="System.ArgumentNullException">package</exception>
        /// <exception cref="System.ArgumentException">Key does not match.</exception>
        public InterwovenExtensionMethodsPackage MergeWith(InterwovenExtensionMethodsPackage package)
        {
            if (package == null) throw new ArgumentNullException(nameof(package));
            if (this.Key != package.Key) throw new ArgumentException("Key does not match.");

            var newContent = this.Content.Trim() + Environment.NewLine + Environment.NewLine + package.Content;

            return new InterwovenExtensionMethodsPackage(newContent, Descriptions);
        }

        private string DebuggerDisplay
            => Key;
    }
}