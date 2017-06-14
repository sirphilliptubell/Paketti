using System;
using System.Collections.Generic;

namespace Paketti.Packaging
{
    public class PackageStore :
        IPackageStore
    {
        private readonly Dictionary<string, Package> _packages = new Dictionary<string, Package>();

        public IEnumerable<Package> Packages
            => _packages.Values;

        public void Store(string content, PackageKind kind, IEnumerable<string> typeDependencies)
            => Store(new Package(content, kind, typeDependencies));

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