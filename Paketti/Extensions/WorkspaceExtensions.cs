using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.CodeAnalysis;

namespace Paketti
{
    public static class WorkspaceExtensions
    {
        /// <summary>
        /// Creates a clone of the specified Workspace into an AdHocWorkspace.
        /// </summary>
        /// <param name="workspace">The workspace.</param>
        /// <returns></returns>
        public static AdhocWorkspace CreateClone(this Workspace workspace)
        {
            var result = new AdhocWorkspace();

            var solutionInfo = SolutionInfo.Create(workspace.CurrentSolution.Id, workspace.CurrentSolution.Version);
            result.AddSolution(solutionInfo);

            foreach (var project in workspace.CurrentSolution.Projects)
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