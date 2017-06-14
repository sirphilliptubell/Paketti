using System;
using System.Collections.Generic;
using System.Linq;
using Paketti.Extensions;

namespace Paketti.Packaging
{
    public class Package
    {
        public Package(string content, PackageKind kind, IEnumerable<string> typeDependencies)
        {
            Content = content.Trim();
            Kind = kind;
            TypeDependencies = typeDependencies.ToList();
            Key = $"{kind} {typeDependencies.ToCommaSeparated()}";
        }

        public string Content { get; }

        public string Key { get; }
        public PackageKind Kind { get; }

        public List<string> TypeDependencies { get; }

        internal Package Append(Package package)
        {
            if (package.Kind != this.Kind) throw new InvalidOperationException("Package kind does not match.");
            if (package.Key != this.Key) throw new InvalidOperationException("Package key does not match.");

            return new Package(
                Append(this.Content, package.Content),
                Kind,
                TypeDependencies);
        }

        private static string Append(string oldCode, string newCode)
            //oldCode should already be trimmed
            => (oldCode + Environment.NewLine + Environment.NewLine + newCode.Trim());
    }
}