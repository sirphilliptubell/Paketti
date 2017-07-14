using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using Paketti.Contexts;

namespace Paketti.Library
{
    [DebuggerDisplay("{DebuggerDisplay,nq}")]
    public class MemberContainerPackage :
        IMergeablePackage<MemberContainerPackage>
    {
        /// <summary>
        /// Gets the key that uniquely identifies this package.
        /// </summary>
        /// <value>
        /// The key.
        /// </value>
        public string Key { get; }

        /// <summary>
        /// Gets the members for the type.
        /// </summary>
        public string Content { get; }

        /// <summary>
        /// Returns <see cref="PackageKind.MemberContainerPackage"/>
        /// </summary>
        public PackageKind Kind
            => PackageKind.MemberContainerPackage;

        /// <summary>
        /// Gets the kind of object this package contains.
        /// </summary>
        public MemberContainerKind ContainerKind { get; }

        /// <summary>
        /// Gets the usings this package requires.
        /// </summary>
        public IEnumerable<string> Usings;

        /// <summary>
        /// Returns an empty list.
        /// </summary>
        public InterweaveDescriptions Descriptions
            => new InterweaveDescriptions(new TypeContext[] { });

        /// <summary>
        /// Get the name of the member container.
        /// </summary>
        public string Name { get; }

        /// <summary>
        /// Gets the number of arguments the member container has.
        /// </summary>
        public byte GenericArgumentCount { get; }

        /// <summary>
        /// Gets the declaration information for the member container.
        /// </summary>
        public string Declaration { get; }

        /// <summary>
        ///Initializes a new instance of the <see cref="MemberContainerPackage"/> class.
        /// </summary>
        /// <param name="usings">The usings required by the package.</param>
        /// <param name="name">The name of the member container</param>
        /// <param name="genericArgumentCount">The number of generic arguments</param>
        /// <param name="declaration">The declaration for the object, eg: the definition of the class/struct/interface.</param>
        /// <param name="content">The content within the class.</param>
        public MemberContainerPackage(MemberContainerKind containerKind, IEnumerable<string> usings, string name, byte genericArgumentCount, string declaration, string content)
        {
            if (usings == null) throw new ArgumentNullException(nameof(usings));
            if (string.IsNullOrWhiteSpace(name)) throw new ArgumentNullException(nameof(name));
            if (string.IsNullOrWhiteSpace(declaration)) throw new ArgumentNullException(nameof(declaration));

            ContainerKind = containerKind;
            Usings = usings;
            Name = name;
            GenericArgumentCount = genericArgumentCount;
            Declaration = declaration;
            Content = content.Trim();

            Key = $"{Kind} {ContainerKind} {Name}´{GenericArgumentCount}´";
        }

        /// <summary>
        /// Merges this package with another package.
        /// </summary>
        /// <param name="package"></param>
        /// <returns></returns>
        public MemberContainerPackage MergeWith(MemberContainerPackage package)
        {
            if (this.Name != package.Name) throw new ArgumentException("Package names must match");
            if (this.GenericArgumentCount != package.GenericArgumentCount) throw new ArgumentException("Generic Argument Count must match");

            var newUsings = new HashSet<string>(this.Usings.Union(package.Usings));
            //take the longer declaration (most likely the one with the most comments)
            var newDeclaration = this.Declaration.Length < package.Declaration.Length ? package.Declaration : this.Declaration;
            var newContent = this.Content.Trim() + Environment.NewLine + Environment.NewLine + package.Content;

            return new MemberContainerPackage(ContainerKind, newUsings, this.Name, this.GenericArgumentCount, newDeclaration, newContent);
        }

        private string DebuggerDisplay
            => Key;
    }
}