using System;
using System.Collections.Generic;
using System.Text;
using Paketti.Contexts;

namespace Paketti
{
    public class ProjectAnalyzer
    {
        private readonly ProjectContext _project;

        public ProjectAnalyzer(ProjectContext project)
        {
            _project = project ?? throw new ArgumentException(nameof(project));
        }
    }
}