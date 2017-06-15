using System;
using Paketti.Contexts;

namespace Paketti.Packaging
{
    /// <summary>
    /// Creates Paketti packages.
    /// </summary>
    public interface IPackager
    {
        /// <summary>
        /// Packs a package and returns the store it was stored in.
        /// </summary>
        /// <returns></returns>
        Result<IPackageStore> Pack();
    }
}