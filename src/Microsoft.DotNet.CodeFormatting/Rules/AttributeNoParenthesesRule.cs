// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System;
using System.Linq;

using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace Microsoft.DotNet.CodeFormatting.Rules
{
    [SyntaxRule(Name = Name, Description = Description, Order = GlobalSemanticRuleOrder.AttributeNoParenthesesRule)]
    internal sealed class AttributeNoParenthesesRule : CSharpOnlyFormattingRule, ISyntaxFormattingRule
    {
        internal const string Name = "AttributeNoParentheses";
        internal const string Description = "Ensure attribites without arguments to not use parantheses";

        public SyntaxNode Process(SyntaxNode syntaxRoot, string languageName)
        {
            var attributes = syntaxRoot.DescendantNodes()
                                       .OfType<AttributeSyntax>()
                                       .Where(a => a.ArgumentList != null &&
                                                   a.ArgumentList.Arguments.Count == 0 &&
                                                   (!a.ArgumentList.OpenParenToken.IsMissing || !a.ArgumentList.CloseParenToken.IsMissing));

            return syntaxRoot.ReplaceNodes(attributes, (a, n) => a.WithArgumentList(null));
        }
    }
}
