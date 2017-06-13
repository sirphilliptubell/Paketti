using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.CodeAnalysis;
using Paketti.Contexts;

namespace Paketti.Rewriters
{
    public class SolutionRewriter
    {
        private readonly IReadOnlyCollection<IRewriter> _rewriters = new IRewriter[] {
            //new DeleteExtensionMethodsRewriter(),
            new DeleteFieldsRewriter()
        };

        private readonly ICompiler _compiler;

        public SolutionRewriter(ICompiler compiler)
        {
            _compiler = compiler ?? throw new ArgumentException(nameof(compiler));
        }

        public Result<Workspace> Rewrite(Workspace workspace)
        {
            var modifiedSolution = workspace.CurrentSolution;

            var badResults = new List<Result>();

            foreach (var rewriter in _rewriters)
            {
                var rewriteSolutionResult = Rewrite(modifiedSolution, rewriter);

                if (rewriteSolutionResult.IsFailure)
                    badResults.Add(rewriteSolutionResult.ToResult());
                else
                    modifiedSolution = rewriteSolutionResult.Value;
            }

            return Result.CombineAll(badResults).ToTypedResult(modifiedSolution.Workspace);
        }

        private Result<Solution> Rewrite(Solution originalSolution, IRewriter rewriter)
            => Modify(
                original: originalSolution,
                getIntermediateValues: solution => solution.ProjectIds,
                getEntry: (solution, projectId) => solution.GetProject(projectId),
                modifyEntry: project =>
                {
                    var projectContext = ProjectContext.Create(project);
                    if (projectContext.IsFailure)
                        return Result.Fail<Project>(projectContext.Error);
                    else
                    {
                        var rewriteResult = Rewrite(projectContext.Value, rewriter);

                        if (rewriteResult.IsFailure)
                            return Result.Fail<Project>(rewriteResult.Error);
                        else
                            return Result.Ok(rewriteResult.Value.Project);
                    }
                },
                getReturnValue: project => project.Solution);

        private Result<ProjectContext> Rewrite(ProjectContext originalProjectContext, IRewriter rewriter)
            => Modify(
                original: originalProjectContext,
                getIntermediateValues: projectContext => projectContext.Project.DocumentIds,
                getEntry: (projectContext, documentId) => projectContext.Documents.Where(x => x.Document.Id == documentId).Single(),
                modifyEntry: documentContext =>
                {
                    var rewriteResult = Rewrite(documentContext, rewriter);
                    return rewriteResult;
                },
                getReturnValue: documentContext => documentContext.ProjectContext);

        private Result<DocumentContext> Rewrite(DocumentContext originalDocument, IRewriter rewriter)
        {
            //only rewrite .vb and .cs
            if (originalDocument.Document.SourceCodeKind != SourceCodeKind.Regular)
                return Result.Ok(originalDocument);

            var newDoc = rewriter.Rewrite(originalDocument).Result;

            var newProject = newDoc.Project;
            var newProjectContext = ProjectContext.Create(newProject);
            if (newProjectContext.IsFailure)
                return Result.Fail<DocumentContext>(newProjectContext.Error);
            else
            {
                if (rewriter.ShouldRecompileToValidate)
                {
                    var compileResult = _compiler.Compile(newProjectContext.Value);
                    if (compileResult.IsFailure)
                        return Result.Fail<DocumentContext>(compileResult.Error);
                }

                var newDocContext = newProjectContext.Value.Documents.Where(x => x.Document.Id == originalDocument.Document.Id).Single();
                return Result.Ok(newDocContext);
            }
        }

        private Result<TValue> Modify<TValue, TIntermediate, TEntry>(
            TValue original,
            Func<TValue, IEnumerable<TIntermediate>> getIntermediateValues,
            Func<TValue, TIntermediate, TEntry> getEntry,
            Func<TEntry, Result<TEntry>> modifyEntry,
            Func<TEntry, TValue> getReturnValue)
        {
            TValue current = original;
            var intermediateValues = getIntermediateValues(original);
            foreach (var intermediateValue in intermediateValues)
            {
                var entry = getEntry(current, intermediateValue);

                var alterEntryResult = modifyEntry(entry);

                if (alterEntryResult.IsFailure)
                    return Result.Fail<TValue>(alterEntryResult.Error);
                else
                    current = getReturnValue(alterEntryResult.Value);
            }

            return Result.Ok(current);
        }
    }
}