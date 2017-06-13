using System;
using System.Linq;
using Microsoft.CodeAnalysis;

namespace Paketti.Contexts
{
    public class SolutionContext
    {
        public Result<ProjectContext> GetProjectModel(Solution solution)
            => GetProject(solution) //get the single project in the solution
            .OnSuccess(ProjectContext.Create); //convert to a context

        public Result<Project> GetProject(Solution solution)
        {
            if (solution == null) throw new ArgumentNullException(nameof(solution));

            var projects =
                solution
                .Projects
                .Take(2)
                .ToList();

            if (projects.Count == 2)
                return Result.Fail<Project>("Only one project is currently supported");
            if (projects.Count == 0)
                return Result.Fail<Project>("Workspace has no projects");

            return Result.Ok(projects.First());
        }
    }
}