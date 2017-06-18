namespace Paketti.Library
{
    /// <summary>
    /// The Kind of a Package.
    /// </summary>
    public enum PackageKind
    {
        /// <summary>
        /// A package that contains only extension methods.
        /// </summary>
        ExtensionMethods,

        /// <summary>
        /// A package that contains optional members for a Type.
        /// </summary>
        OptionalTypeMembersPackage
    }
}