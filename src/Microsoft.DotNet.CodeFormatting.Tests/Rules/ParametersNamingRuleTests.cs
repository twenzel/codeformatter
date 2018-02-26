using Microsoft.CodeAnalysis;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Xunit;

namespace Microsoft.DotNet.CodeFormatting.Tests
{
    public class ParametersNamingRuleTests : GlobalSemanticRuleTestBase
    {
        internal override IGlobalSemanticFormattingRule Rule
        {
            get { return new Rules.ParametersNamingRule(); }
        }

        public sealed class CSharpFields : ParametersNamingRuleTests
        {
            [Fact]
            public void DoNotRename()
            {
                var text = @"
class A
{
    public void TestA(int value) { }
    public void TestB(string valueTest, string other, string doNotRenameMe) { }
}";
                var expected = @"
class A
{
    public void TestA(int value) { }
    public void TestB(string valueTest, string other, string doNotRenameMe) { }
}";
                Verify(text, expected);
            }

            [Fact]
            public void Rename()
            {
                var text = @"
class A
{
    public void TestA(int Value) { }
    public void TestB(string VALUE, string other, string RenameMe) { }
    public void TestC(string s_otherTest) { }
}";
                var expected = @"
class A
{
    public void TestA(int value) { }
    public void TestB(string value, string other, string renameMe) { }
    public void TestC(string otherTest) { }
}";
                Verify(text, expected);
            }
        }

        public sealed class VisualBasicFields : ParametersNamingRuleTests
        {
            [Fact]
            public void DoNotRename()
            {
                var text = @"
Class A
    public Sub TestA(value as Int)
    End Sub

    public Sub TestB(valueTest as string, other as string, doNotRenameMe as string)
    End Sub
End Class";

                var expected = @"
Class A
    public Sub TestA(value as Int)
    End Sub

    public Sub TestB(valueTest as string, other as string, doNotRenameMe as string)
    End Sub
End Class";

                Verify(text, expected, runFormatter: false, languageName: LanguageNames.VisualBasic);
            }

            [Fact]
            public void Rename()
            {
                var text = @"
Class A
    public Sub TestA(Value as Int)
    End Sub
    public Sub TestB(VALUE as string, other as string, RenameMe as string)
    End Sub
    public Sub TestA(s_otherTest as string)
    End Sub
End Class";

                var expected = @"
Class A
    public Sub TestA(value as Int)
    End Sub
    public Sub TestB(value as string, other as string, renameMe as string)
    End Sub
    public Sub TestA(otherTest as string)
    End Sub
End Class";

                Verify(text, expected, runFormatter: false, languageName: LanguageNames.VisualBasic);
            }
        }
    }
}
