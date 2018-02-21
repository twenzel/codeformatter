// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;

using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Rename;
using Microsoft.CodeAnalysis.VisualBasic;
using Microsoft.CodeAnalysis.VisualBasic.Syntax;

namespace Microsoft.DotNet.CodeFormatting.Rules
{
    internal partial class LocalVariableNamingRule
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

            public override SyntaxNode VisitVariableDeclarator(VariableDeclaratorSyntax node)
            {
                // only local variables!
                var p = node.Parent as LocalDeclarationStatementSyntax;
                if (p == null)
                    return node;

                if (!NeedsRewrite(node))
                    return node;

                var list = new List<ModifiedIdentifierSyntax>(node.Names.Count);

                foreach (var v in node.Names)
                {
                    var local = v;
                    if (!IsGoodVariableName(v.Identifier.ValueText))
                    {
                        local = local.WithAdditionalAnnotations(s_markerAnnotationArray);
                        _count++;
                    }

                    list.Add(local);
                }

                return node.WithNames(SyntaxFactory.SeparatedList(list));
            }

            private bool NeedsRewrite(VariableDeclaratorSyntax node)
            {
                foreach (var v in node.Names)
                {
                    if (!IsGoodVariableName(v.Identifier.ValueText))
                        return true;
                }

                return false;
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
