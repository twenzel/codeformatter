using System;
using System.Collections.Generic;
using System.Linq;

using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace Microsoft.DotNet.CodeFormatting.Rules
{
    internal partial class PrivateConstantFieldNamingRule
    {
        private sealed class CSharpRule : CommonRule
        {
            protected override SyntaxNode AddPrivateFieldAnnotations(SyntaxNode syntaxNode, out int count)
            {
                return CSharpPrivateFieldAnnotationsRewriter.AddAnnotations(syntaxNode, out count);
            }

            protected override SyntaxNode RemoveRenameAnnotations(SyntaxNode syntaxNode)
            {
                var rewriter = new CSharpRemoveRenameAnnotationsRewriter();
                return rewriter.Visit(syntaxNode);
            }
        }

        /// <summary>
        /// This will add an annotation to any private field that needs to be renamed.
        /// </summary>
        internal sealed class CSharpPrivateFieldAnnotationsRewriter : CSharpSyntaxRewriter
        {
            private int _count;

            internal static SyntaxNode AddAnnotations(SyntaxNode node, out int count)
            {
                var rewriter = new CSharpPrivateFieldAnnotationsRewriter();
                var newNode = rewriter.Visit(node);
                count = rewriter._count;
                return newNode;
            }

            public override SyntaxNode VisitFieldDeclaration(FieldDeclarationSyntax node)
            {
                if (NeedsRewrite(node))
                {
                    var list = new List<VariableDeclaratorSyntax>(node.Declaration.Variables.Count);
                    foreach (var v in node.Declaration.Variables)
                    {
                        var fieldName = v.Identifier.Text;

                        if (IsGoodPrivateConstFieldName(fieldName))
                        {
                            list.Add(v);
                        }
                        else
                        {
                            list.Add(v.WithAdditionalAnnotations(s_markerAnnotation));
                            _count++;
                        }
                    }

                    var declaration = node.Declaration.WithVariables(SyntaxFactory.SeparatedList(list));
                    node = node.WithDeclaration(declaration);

                    return node;
                }

                return node;
            }

            private static bool NeedsRewrite(FieldDeclarationSyntax fieldSyntax)
            {
                if (!IsPrivateConstField(fieldSyntax))
                {
                    return false;
                }

                foreach (var v in fieldSyntax.Declaration.Variables)
                {
                    if (!IsGoodPrivateConstFieldName(v.Identifier.ValueText))
                    {
                        return true;
                    }
                }

                return false;
            }

            private static bool IsPrivateConstField(FieldDeclarationSyntax fieldSyntax)
            {
                var isPrivate = true;
                var isConst = false;
                foreach (var modifier in fieldSyntax.Modifiers)
                {
                    switch (modifier.Kind())
                    {
                        case SyntaxKind.PublicKeyword:
                        case SyntaxKind.InternalKeyword:
                        case SyntaxKind.ProtectedKeyword:
                            isPrivate = false;
                            break;
                        case SyntaxKind.ConstKeyword:
                            isConst = true;
                            break;
                    }
                }

                return isPrivate && isConst;
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
