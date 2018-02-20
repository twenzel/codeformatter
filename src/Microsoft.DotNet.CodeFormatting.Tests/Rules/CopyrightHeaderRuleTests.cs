// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using Microsoft.CodeAnalysis;
using System.Collections.Immutable;
using Xunit;

namespace Microsoft.DotNet.CodeFormatting.Tests
{
    public sealed class CopyrightHeaderRuleTests : SyntaxRuleTestBase
    {
        private readonly FormattingOptions _options = new FormattingOptions();

        internal override ISyntaxFormattingRule Rule
        {
            get { return new Rules.CopyrightHeaderRule(_options); }
        }

        [Fact]
        public void CSharpSimple()
        {
            _options.CopyrightHeader = ImmutableArray.Create("test");
            var source = @"
class C
{
}";

            var expected = @"// test

class C
{
}";
            Verify(source, expected);
        }

        [Fact]
        public void CSharpSimpleMultiline()
        {
            _options.CopyrightHeader = ImmutableArray.Create("test1", "test2");
            var source = @"
class C
{
}";

            var expected = @"// test1
// test2

class C
{
}";
            Verify(source, expected);
        }

        [Fact]
        public void CSharpPreserveExisting()
        {
            _options.CopyrightHeader = ImmutableArray.Create("test");
            var source = @"// test

class C
{
}";

            var expected = @"// test

class C
{
}";
            Verify(source, expected);
        }

        [Fact]
        public void CSharpPreserveExistingMultiline()
        {
            _options.CopyrightHeader = ImmutableArray.Create("test1", "test2");
            var source = @"// test1
// test2

class C
{
}";

            var expected = @"// test1
// test2

class C
{
}";
            Verify(source, expected);
        }

        [Fact]
        public void CSharpPreserveExistingWithCommentMultiline()
        {
            _options.CopyrightHeader = ImmutableArray.Create("test1", "test2");
            var source = @"// test1
// test2





// test3


class C
{
}";

            var expected = @"// test1
// test2





// test3


class C
{
}";
            Verify(source, expected);
        }

        [Fact]
        public void CSharpDontDoubleComment()
        {
            _options.CopyrightHeader = ImmutableArray.Create("// test");
            var source = @"
class C
{
}";

            var expected = @"// test

class C
{
}";
            Verify(source, expected);
        }

        [Fact]
        public void CSharpRemoveOlder()
        {
            _options.CopyrightHeader = ImmutableArray.Create("test");
            var source = @"// copyright

class C
{
}";

            var expected = @"// test

class C
{
}";
            Verify(source, expected);
        }

        [Fact]
        public void CSharpHeaderBeginsWithTargetHeader()
        {
            _options.CopyrightHeader = ImmutableArray.Create("// test", "// test2");
            var source = @"// test
// test2
// file summary

class C
{
}";

            var expected = @"// test
// test2
// file summary

class C
{
}";
            Verify(source, expected);
        }

        [Fact]
        public void VisualBasicSimple()
        {
            _options.CopyrightHeader = ImmutableArray.Create("test");
            var source = @"
Public Class C
End Class";

            var expected = @"' test

Public Class C
End Class";

            Verify(source, expected, languageName: LanguageNames.VisualBasic);
        }

        [Fact]
        public void VisualBasicNormalizeComment()
        {
            _options.CopyrightHeader = ImmutableArray.Create("// test");
            var source = @"
Public Class C
End Class";

            var expected = @"' test

Public Class C
End Class";

            Verify(source, expected, languageName: LanguageNames.VisualBasic);
        }

        [Fact]
        public void VisualBasicPreserveExisting()
        {
            _options.CopyrightHeader = ImmutableArray.Create("// test");
            var source = @"' test

Public Class C
End Class";

            var expected = @"' test

Public Class C
End Class";

            Verify(source, expected, languageName: LanguageNames.VisualBasic);
        }
    }
}
