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
    [GlobalSemanticRule(Name = InterfaceNamingRule.Name, Description = InterfaceNamingRule.Description, Order = GlobalSemanticRuleOrder.InterfaceNamingRule)]
    internal sealed partial class InterfaceNamingRule : IGlobalSemanticFormattingRule
    {
        internal const string Name = "InterfaceNaming";
        internal const string Description = "Ensure all interfaces starts with an 'I' and continuing pascal case";

        #region CommonRule

        private abstract class CommonRule
        {
            protected abstract SyntaxNode AddInterfaceAnnotations(SyntaxNode syntaxNode, out int count);

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
                var newSyntaxRoot = AddInterfaceAnnotations(syntaxRoot, out count);

                if (count == 0)
                {
                    return document.Project.Solution;
                }

                var documentId = document.Id;
                var solution = document.Project.Solution;
                solution = solution.WithDocumentSyntaxRoot(documentId, newSyntaxRoot);
                solution = await RenameInterface(solution, documentId, count, cancellationToken);
                return solution;
            }

            private async Task<Solution> RenameInterface(Solution solution, DocumentId documentId, int count, CancellationToken cancellationToken)
            {
                Solution oldSolution = null;
                for (int i = 0; i < count; i++)
                {
                    oldSolution = solution;

                    var semanticModel = await solution.GetDocument(documentId).GetSemanticModelAsync(cancellationToken);
                    var root = await semanticModel.SyntaxTree.GetRootAsync(cancellationToken);
                    var declaration = root.GetAnnotatedNodes(s_markerAnnotation).ElementAt(i);

                    var interfaceSymbol = semanticModel.GetDeclaredSymbol(declaration, cancellationToken);
                    var newName = GetNewInterfaceName(interfaceSymbol);

                    // Can happen with pathologically bad field names like _
                    if (newName == interfaceSymbol.Name)
                    {
                        continue;
                    }

                    solution = await Renamer.RenameSymbolAsync(solution, interfaceSymbol, newName, solution.Workspace.Options, cancellationToken).ConfigureAwait(false);
                    solution = await CleanSolutionAsync(solution, oldSolution, cancellationToken);
                }

                return solution;
            }

            private static string GetNewInterfaceName(ISymbol fieldSymbol)
            {
                var name = fieldSymbol.Name.Trim('I');

                if (name.Length == 0)
                {
                    return fieldSymbol.Name;
                }

                // do pascal case
                if (name.Length > 2 && char.IsLower(name[0]))
                {
                    name = char.ToUpper(name[0]) + name.Substring(1);
                }

                return "I" + name;
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

        private readonly static SyntaxAnnotation s_markerAnnotation = new SyntaxAnnotation("InterfaceToRename");

        // Used to avoid the array allocation on calls to WithAdditionalAnnotations
        private readonly static SyntaxAnnotation[] s_markerAnnotationArray;

        static InterfaceNamingRule()
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

        private static bool IsGoodInterfaceName(string name)
        {
            return name.Length > 1 && (name[0] == 'I') && char.IsUpper(name[1]);
        }
    }
}
