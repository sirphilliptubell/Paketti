using System;
using System.Linq;
using Paketti.Contexts;
using Paketti.Extensions;
using Paketti.Logging;
using Paketti.Utilities;

namespace Paketti.Packaging
{
    public class Packager :
        IPackager
    {
        private readonly ILog _log;
        private readonly ProjectContext _projectContext;
        private readonly IPackageStore _store;
        private readonly IDependencyWalker _walker;

        public Packager(ProjectContext projectContext, IPackageStore store, Func<ProjectContext, IDependencyWalker> walkerFactory, ILog log)
        {
            _projectContext = projectContext ?? throw new ArgumentException(nameof(projectContext));
            _store = store ?? throw new ArgumentException(nameof(store));
            _walker = walkerFactory?.Invoke(_projectContext) ?? throw new ArgumentException(nameof(walkerFactory));
            _log = log ?? throw new ArgumentException(nameof(log));
        }

        public Result<IPackageStore> Pack()
        {
            PackExtensionMethods();

            return Result.Ok(_store);
        }

        private void PackExtensionMethods()
        {
            using (_log.LogStep($"{nameof(Packager)}.{nameof(PackExtensionMethods)}"))
            {
                var groups =
                    _projectContext
                    .GetExtensionMethods()
                    .GroupBy(x => _walker.GetTypeDependencies(x).OnlyLocal())
                    .ToList();

                foreach (var group in groups)
                {
                    var content =
                        group
                        .Select(x => x.ToFormattedCode())
                        .ToSeparatedString(Config.PACKAGE_LINE_SEPARATOR);

                    var dependencies =
                        group
                        .Key
                        .Select(x => x.ToString());

                    _store.Store(content, PackageKind.ExtensionMethods, dependencies);
                }
            }
        }
    }
}