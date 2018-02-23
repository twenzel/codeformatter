using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Microsoft.DotNet.CodeFormatting.Rules
{
    internal partial class InterfaceNamingRule
    {
        private sealed class CSharpRule : CommonRule
        {
            protected override SyntaxNode AddInterfaceAnnotations(SyntaxNode syntaxNode, out int count)
            {
                return CSharInterfaceAnnotationsRewriter.AddAnnotations(syntaxNode, out count);
            }

            protected override SyntaxNode RemoveRenameAnnotations(SyntaxNode syntaxNode)
            {
                var rewriter = new CSharpRemoveRenameAnnotationsRewriter();
                return rewriter.Visit(syntaxNode);
            }
        }

        /// <summary>
        /// This will add an annotation to any interface that needs to be renamed.
        /// </summary>
        internal sealed class CSharInterfaceAnnotationsRewriter : CSharpSyntaxRewriter
        {
            private int _count;

            internal static SyntaxNode AddAnnotations(SyntaxNode node, out int count)
            {
                var rewriter = new CSharInterfaceAnnotationsRewriter();
                var newNode = rewriter.Visit(node);
                count = rewriter._count;
                return newNode;
            }

            public override SyntaxNode VisitInterfaceDeclaration(InterfaceDeclarationSyntax node)
            {
                if (!IsGoodInterfaceName(node.Identifier.Text))
                {
                    node = node.WithAdditionalAnnotations(s_markerAnnotation);
                    _count++;
                }

                return node;
            }
        }

        /// <summary>
        /// This rewriter exists to work around DevDiv 1086632 in Roslyn.  The Rename action is 
        /// leaving a set of annotations in the tree.  These annotations slow down further processing
        /// and eventually make the rename operation unusable.  As a temporary work around we manually
        /// remove these from the tree.
        /// </summary>
        private sealed class CSharpRemoveRenameAnnotationsRewriter : CSharpSyntaxRewriter
        {
            public override SyntaxNode Visit(SyntaxNode node)
            {
                node = base.Visit(node);
                if (node != null && node.ContainsAnnotations && node.GetAnnotations(s_renameAnnotationName).Any())
                {
                    node = node.WithoutAnnotations(s_renameAnnotationName);
                }

                return node;
            }

            public override SyntaxToken VisitToken(SyntaxToken token)
            {
                token = base.VisitToken(token);
                if (token.ContainsAnnotations && token.GetAnnotations(s_renameAnnotationName).Any())
                {
                    token = token.WithoutAnnotations(s_renameAnnotationName);
                }

                return token;
            }
        }
    }
}
