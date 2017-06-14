using System;

namespace Paketti.Packaging
{
    public interface IPackager
    {
        Result<IPackageStore> Pack();
    }
}