﻿using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.CodeAnalysis;
using Paketti.Contexts;
using Paketti.Logging;
using Paketti.Utilities;

namespace Paketti.Rewriters
{
    public class SolutionRewriter
    {
        private readonly ICompiler _compiler;

        private readonly ILog _log;

        private readonly IReadOnlyCollection<IRewriter> _rewriters = new IRewriter[] {
            new EnsureTopLevelClassesAndStructsArePartialRewriter(),
            new RemoveRegionRewriter()
        };

        public SolutionRewriter(ICompiler compiler, ILog log)
        {
            _compiler = compiler ?? throw new ArgumentException(nameof(compiler));
            _log = log ?? throw new ArgumentException(nameof(log));
        }

        public Result<Workspace> Rewrite(Workspace workspace)
        {
            var modifiedSolution = workspace.CurrentSolution;

            var solutionContext = SolutionContext.Create(workspace);
            if (solutionContext.IsFailure)
                return Result.Fail<Workspace>("Could not get SolutionContext before rewriting: " + solutionContext.Error);

            var preCheck = _compiler.Compile(solutionContext.Value.ProjectContext, _log);
            if (preCheck.IsFailure)
                return Result.Fail<Workspace>("Project didn't compile before rewriting anything: " + preCheck.Error);

            var rewriteResults =
                Result.CombineAll(
                    _rewriters
                    .Select(rewriter =>
                    {
                        var rewriteResult = Rewrite(modifiedSolution, rewriter);

                        //if the rewrite was successful, use the new modified solution in the next rewrite
                        if (rewriteResult.IsSuccess)
                            modifiedSolution = rewriteResult.Value;

                        return rewriteResult;
                    })
                );

            if (rewriteResults.IsFailure)
                return Result.Fail<Workspace>(rewriteResults.Error);
            else
            {
                var appyResult = workspace.TryApplyChanges(modifiedSolution);
                if (!appyResult)
                    return Result.Fail<Workspace>("Workspace.TryApplyChanges() failed.");
                else
                    return workspace;
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

            var newDocResult = rewriter.Rewrite(originalDocument, _log);
            if (newDocResult.IsFailure)
                return Result.Fail<DocumentContext>(newDocResult.Error);
            var newDoc = newDocResult.Value;

            var newProject = newDoc.Project;
            var newProjectContext = ProjectContext.Create(newProject);
            if (newProjectContext.IsFailure)
                return Result.Fail<DocumentContext>(newProjectContext.Error);
            else
            {
                if (rewriter.ShouldRecompileToValidate)
                {
                    var compileResult = _compiler.Compile(newProjectContext.Value, _log);
                    if (compileResult.IsFailure)
                        return Result.Fail<DocumentContext>(compileResult.Error);
                }

                var newDocContext = newProjectContext.Value.Documents.Where(x => x.Document.Id == originalDocument.Document.Id).Single();
                return Result.Ok(newDocContext);
            }
        }
    }
}