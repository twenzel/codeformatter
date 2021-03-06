// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

namespace Microsoft.DotNet.CodeFormatting.Rules
{
    // Please keep these values sorted by number, not rule name.    
    internal static class SyntaxRuleOrder
    {
        public const int HasNoCustomCopyrightHeaderFormattingRule = 1;
        public const int UsingLocationFormattingRule = 2;
        public const int NewLineAboveFormattingRule = 3;
        public const int BraceNewLineRule = 4;
        public const int NonAsciiChractersAreEscapedInLiterals = 5;
        public const int CopyrightHeaderRule = 6;
    }

    // Please keep these values sorted by number, not rule name.    
    internal static class LocalSemanticRuleOrder
    {
        public const int HasNoIllegalHeadersFormattingRule = 1;
        public const int ExplicitVisibilityRule = 2;
        public const int IsFormattedFormattingRule = 3;
        public const int RemoveExplicitThisRule = 4;
        public const int AssertArgumentOrderRule = 5;
        public const int HasNoUnusedUsingsRule = 6;
    }

    // Please keep these values sorted by number, not rule name.    
    internal static class GlobalSemanticRuleOrder
    {
        public const int PrivateFieldNamingRule = 1;
        public const int MarkReadonlyFieldsRule = 2;
        public const int AttributeNoParenthesesRule = 3;
        public const int AttributeSeparateListsRule = 4;
        public const int LocalVariableNamingRule = 5;
        public const int ConstantFieldNamingRule = 6;
        public const int InterfaceNamingRule = 7;
        public const int ParametersNamingRule = 8;
    }
}
