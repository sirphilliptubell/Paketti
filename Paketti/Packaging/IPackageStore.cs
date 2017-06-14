using System.Collections.Generic;

namespace Paketti.Packaging
{
    public interface IPackageStore
    {
        IEnumerable<Package> Packages { get; }

        void Store(string content, PackageKind kind, IEnumerable<string> typeDependencies);
    }
}