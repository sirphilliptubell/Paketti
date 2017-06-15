using System;
using System.Linq;
using Paketti.Contexts;
using Paketti.Extensions;
using Paketti.Logging;
using Paketti.Utilities;

namespace Paketti.Packaging
{
    /// <summary>
    /// Creates packages.
    /// </summary>
    /// <seealso cref="Paketti.Packaging.IPackager" />
    public class Packager :
        IPackager
    {
        private readonly ILog _log;
        private readonly ProjectContext _projectContext;
        private readonly IPackageStore _store;
        private readonly IDependencyWalker _walker;

        /// <summary>
        /// Initializes a new instance of the <see cref="Packager"/> class.
        /// </summary>
        /// <param name="projectContext">The project context.</param>
        /// <param name="store">The package store.</param>
        /// <param name="walkerFactory">The dependency walker factory.</param>
        /// <param name="log">The log.</param>
        /// <exception cref="System.ArgumentException">
        /// projectContext
        /// or
        /// store
        /// or
        /// walkerFactory
        /// or
        /// log
        /// </exception>
        public Packager(ProjectContext projectContext, IPackageStore store, Func<ProjectContext, IDependencyWalker> walkerFactory, ILog log)
        {
            _store = store ?? throw new ArgumentException(nameof(store));
            _walker = walkerFactory?.Invoke(_projectContext) ?? throw new ArgumentException(nameof(walkerFactory));
            _log = log ?? throw new ArgumentException(nameof(log));
        }

        /// <summary>
        /// Packs the specified project context.
        /// </summary>
        /// <param name="projectContext">The project context.</param>
        /// <returns></returns>
        public Result<IPackageStore> Pack(ProjectContext projectContext)
        {
            PackExtensionMethods();

            return Result.Ok(_store);
        }

        /// <summary>
        /// Packs the extension methods of the project.
        /// </summary>
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