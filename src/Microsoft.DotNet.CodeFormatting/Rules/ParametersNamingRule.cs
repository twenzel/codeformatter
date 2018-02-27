using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Rename;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Microsoft.DotNet.CodeFormatting.Rules
{
    [GlobalSemanticRule(Name = ParametersNamingRule.Name, Description = ParametersNamingRule.Description, Order = GlobalSemanticRuleOrder.ParametersNamingRule)]
    internal sealed partial class ParametersNamingRule : IGlobalSemanticFormattingRule
    {
        internal const string Name = "ParametersNaming";
        internal const string Description = "Write parameters in camel case";

        #region CommonRule

        private abstract class CommonRule
        {
            protected abstract SyntaxNode AddParameterAnnotations(SyntaxNode syntaxNode, out int count);

            /// <summary>
            /// This method exists to work around DevDiv 1086632 in Roslyn.  The Rename action is 
            /// leaving a set of annotations in the tree.  These annotations slow down further processing
            /// and eventually make the rename operation unusable.  As a temporary work around we manually
            /// remove these from the tree.
            /// </summary>
            protected abstract SyntaxNode RemoveRenameAnnotations(SyntaxNode syntaxNode);

            public async Task<Solution> ProcessAsync(Document document, SyntaxNode syntaxRoot, CancellationToken cancellationToken)
            {
                int count;
                var newSyntaxRoot = AddParameterAnnotations(syntaxRoot, out count);

                if (count == 0)
                {
                    return document.Project.Solution;
                }

                var documentId = document.Id;
                var solution = document.Project.Solution;
                solution = solution.WithDocumentSyntaxRoot(documentId, newSyntaxRoot);
                solution = await RenameParameter(solution, documentId, count, cancellationToken);
                return solution;
            }

            private async Task<Solution> RenameParameter(Solution solution, DocumentId documentId, int count, CancellationToken cancellationToken)
            {
                Solution oldSolution = null;
                for (int i = 0; i < count; i++)
                {
                    oldSolution = solution;

                    var semanticModel = await solution.GetDocument(documentId).GetSemanticModelAsync(cancellationToken);
                    var root = await semanticModel.SyntaxTree.GetRootAsync(cancellationToken);
                    var declaration = root.GetAnnotatedNodes(s_markerAnnotation).ElementAt(i);

                    var parameterSymbol = semanticModel.GetDeclaredSymbol(declaration, cancellationToken);

                    if (parameterSymbol == null)
                        continue;

                    var newName = GetNewParameterName(parameterSymbol);

                    // Can happen with pathologically bad field names like _
                    if (newName == parameterSymbol.Name)
                        continue;

                    solution = await Renamer.RenameSymbolAsync(solution, parameterSymbol, newName, solution.Workspace.Options, cancellationToken).ConfigureAwait(false);
                    solution = await CleanSolutionAsync(solution, oldSolution, cancellationToken);
                }

                return solution;
            }

            private static string GetNewParameterName(ISymbol fieldSymbol)
            {
                var name = fieldSymbol.Name;

                // "x_XXX" -> "XXX"
                if (name.Length > 2 && char.IsLetter(name[0]) && name[1] == '_')
                    name = name.Substring(2);

                if (name.Length == 0)
                    return fieldSymbol.Name;

                // "Y" -> "y"
                if (name.Length == 1 && char.IsUpper(name[0]))
                    name = name.ToLower();

                // "AnyVariable" -> "anyVariable"
                if (isPascalCase(name))
                    name = char.ToLower(name[0]) + name.Substring(1);

                if (isUpperCase(name))
                    name = name.ToLower();

                return name;
            }

            private async Task<Solution> CleanSolutionAsync(Solution newSolution, Solution oldSolution, CancellationToken cancellationToken)
            {
                var solution = newSolution;

                foreach (var projectChange in newSolution.GetChanges(oldSolution).GetProjectChanges())
                {
                    foreach (var documentId in projectChange.GetChangedDocuments())
                    {
                        solution = await CleanSolutionDocument(solution, documentId, cancellationToken);
                    }
                }

                return solution;
            }

            private async Task<Solution> CleanSolutionDocument(Solution solution, DocumentId documentId, CancellationToken cancellationToken)
            {
                var document = solution.GetDocument(documentId);
                var syntaxNode = await document.GetSyntaxRootAsync(cancellationToken);
                if (syntaxNode == null)
                {
                    return solution;
                }

                var newNode = RemoveRenameAnnotations(syntaxNode);
                return solution.WithDocumentSyntaxRoot(documentId, newNode);
            }
        }

        #endregion

        private const string s_renameAnnotationName = "Rename";

        private readonly static SyntaxAnnotation s_markerAnnotation = new SyntaxAnnotation("ParameeterToRename");

        // Used to avoid the array allocation on calls to WithAdditionalAnnotations
        private readonly static SyntaxAnnotation[] s_markerAnnotationArray;

        static ParametersNamingRule()
        {
            s_markerAnnotationArray = new[] { s_markerAnnotation };
        }

        private readonly CSharpRule _csharpRule = new CSharpRule();
        private readonly VisualBasicRule _visualBasicRule = new VisualBasicRule();

        public bool SupportsLanguage(string languageName)
        {
            return
                languageName == LanguageNames.CSharp ||
                languageName == LanguageNames.VisualBasic;
        }

        public Task<Solution> ProcessAsync(Document document, SyntaxNode syntaxRoot, CancellationToken cancellationToken)
        {
            switch (document.Project.Language)
            {
                case LanguageNames.CSharp:
                    return _csharpRule.ProcessAsync(document, syntaxRoot, cancellationToken);
                case LanguageNames.VisualBasic:
                    return _visualBasicRule.ProcessAsync(document, syntaxRoot, cancellationToken);
                default:
                    throw new NotSupportedException();
            }
        }

        private static bool IsGoodParameterName(string name)
        {
            if (!isCamelCase(name))
                return false;

            // x_XX
            if (name.Length > 2 && char.IsLetter(name[0]) && name[1] == '_')
                return false;

            return true;
        }

        private static bool isPascalCase(string token)
        {
            if (string.IsNullOrEmpty(token))
                return false;

            return token.Length > 2 && char.IsUpper(token[0]) && char.IsLower(token[1]);
        }

        private static bool isCamelCase(string token)
        {
            if (string.IsNullOrEmpty(token))
                return false;

            return token.Length > 2 && !isPascalCase(token) && !isUpperCase(token);
        }

        private static bool isUpperCase(string token)
        {
            return token.ToUpper() == token;
        }
    }
}
