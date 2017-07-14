namespace Paketti.Library
{
    /// <summary>
    /// The Kind of a Package.
    /// </summary>
    public enum PackageKind
    {
        /// <summary>
        /// A package that contains only extension methods that use interwoven types.
        /// </summary>
        InterwovenExtensionMethods,

        /// <summary>
        /// A package that contains instance/static interwoven members for a Type. Does not include extension methods.
        /// </summary>
        InterwovenTypeMembersPackage,

        /// <summary>
        /// A package that contains a type that can contain members. eg: instance/static class/struct/interface.
        /// </summary>
        MemberContainerPackage
    }
}