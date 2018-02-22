using System;
using System.Collections.Generic;
using System.Linq;

using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.VisualBasic;
using Microsoft.CodeAnalysis.VisualBasic.Syntax;

namespace Microsoft.DotNet.CodeFormatting.Rules
{
    internal partial class PrivateConstantFieldNamingRule
    {
        private sealed class VisualBasicRule : CommonRule
        {
            protected override SyntaxNode AddPrivateFieldAnnotations(SyntaxNode syntaxNode, out int count)
            {
                return VisualBasicPrivateFieldAnnotationRewriter.AddAnnotations(syntaxNode, out count);
            }

            protected override SyntaxNode RemoveRenameAnnotations(SyntaxNode syntaxNode)
            {
                var rewriter = new VisualBasicRemoveRenameAnnotationsRewriter();
                return rewriter.Visit(syntaxNode);
            }
        }

        private sealed class VisualBasicPrivateFieldAnnotationRewriter : VisualBasicSyntaxRewriter
        {
            private int _count;
            private bool _inModule;

            internal static SyntaxNode AddAnnotations(SyntaxNode node, out int count)
            {
                var rewriter = new VisualBasicPrivateFieldAnnotationRewriter();
                var newNode = rewriter.Visit(node);
                count = rewriter._count;
                return newNode;
            }

            public override SyntaxNode VisitModuleBlock(ModuleBlockSyntax node)
            {
                var savedInModule = _inModule;
                try
                {
                    _inModule = true;
                    return base.VisitModuleBlock(node);
                }
                finally
                {
                    _inModule = savedInModule;
                }
            }

            public override SyntaxNode VisitFieldDeclaration(FieldDeclarationSyntax node)
            {
                if (!NeedsRewrite(node))
                {
                    return node;
                }

                var declarators = new List<VariableDeclaratorSyntax>(node.Declarators.Count);
                foreach (var d in node.Declarators)
                {
                    var list = new List<ModifiedIdentifierSyntax>(d.Names.Count);
                    foreach (var v in d.Names)
                    {
                        var local = v;
                        if (!IsGoodPrivateConstFieldName(v.Identifier.ValueText))
                        {
                            local = local.WithAdditionalAnnotations(s_markerAnnotationArray);
                            _count++;
                        }

                        list.Add(local);
                    }

                    declarators.Add(d.WithNames(SyntaxFactory.SeparatedList(list)));
                }

                return node.WithDeclarators(SyntaxFactory.SeparatedList(declarators));
            }

            private bool NeedsRewrite(FieldDeclarationSyntax fieldSyntax)
            {
                if (!IsPrivateConstField(fieldSyntax))
                {
                    return false;
                }

                foreach (var d in fieldSyntax.Declarators)
                {
                    foreach (var v in d.Names)
                    {
                        if (!IsGoodPrivateConstFieldName(v.Identifier.ValueText))
                        {
                            return true;
                        }
                    }
                }

                return false;
            }

            private bool IsPrivateConstField(FieldDeclarationSyntax node)
            {
                var isPrivate = true;
                var isConst = false;

                foreach (var modifier in node.Modifiers)
                {
                    switch (modifier.Kind())
                    {
                        case SyntaxKind.PublicKeyword:
                        case SyntaxKind.FriendKeyword:
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
        private sealed class VisualBasicRemoveRenameAnnotationsRewriter : VisualBasicSyntaxRewriter
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
