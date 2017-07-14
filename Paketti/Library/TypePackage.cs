using System;
using System.Collections.Generic;
using System.Text;
using Paketti.Contexts;

namespace Paketti.Library
{
    internal class TypePackage :
        IMergeablePackage<TypePackage>
    {
        public string Key { get; }

        public string Content { get; }

        public PackageKind Kind
            => PackageKind.TypePackage;

        public InterweaveDescriptions Descriptions
            => new InterweaveDescriptions(new TypeContext[] { });

        public TypePackage MergeWith(TypePackage package)
        {
            throw new NotImplementedException();
        }
    }
}