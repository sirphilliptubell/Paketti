namespace Paketti.Library
{
    /// <summary>
    /// Represents a package that can be merged with another package.
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public interface IMergeablePackage<T> :
        IPackage
        where T : IPackage
    {
        /// <summary>
        /// Creates a new package using the information from this package and the specified package.
        /// </summary>
        /// <param name="package">The package.</param>
        T MergeWith(T package);
    }
}