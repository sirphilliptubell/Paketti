using Microsoft.CodeAnalysis;

namespace Paketti.Extensions
{
    public static class SolutionExtensions
    {
        /// <summary>
        /// Creates a clone of the specified Solution into an AdHocWorkspace.
        /// </summary>
        /// <param name="workspace">The workspace.</param>
        /// <returns></returns>
        public static AdhocWorkspace CloneIntoWorkspace(this Solution solution)
        {
            var result = new AdhocWorkspace();

            var solutionInfo = SolutionInfo.Create(solution.Id, solution.Version);
            result.AddSolution(solutionInfo);

            foreach (var project in solution.Projects)
            {
                var projectInfo = ProjectInfo.Create(project.Id, project.Version, project.Name, project.AssemblyName, project.Language,
                    projectReferences: project.AllProjectReferences,
                    metadataReferences: project.MetadataReferences);

                result.AddProject(projectInfo);

                foreach (var doc in project.Documents)
                {
                    result.AddDocument(project.Id, doc.Name, doc.GetTextAsync().Result);
                }
            }

            return result;
        }
    }
}