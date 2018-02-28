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

    public delegate void TestEvent(int newValue);
}";
                var expected = @"
class A
{
    public void TestA(int value) { }
    public void TestB(string valueTest, string other, string doNotRenameMe) { }

    public delegate void TestEvent(int newValue);
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

    public delegate void TestEvent(int NewValue);
}";
                var expected = @"
class A
{
    public void TestA(int value) { }
    public void TestB(string value, string other, string renameMe) { }
    public void TestC(string sOtherTest) { }

    public delegate void TestEvent(int newValue);
}";
                Verify(text, expected);
            }

            [Fact]
            public void Rename_Critical()
            {
                var text = @"
class A
{
    public void TestA(int v_Value, int b_Value) { }
}";
                var expected = @"
class A
{
    public void TestA(int vValue, int bValue) { }
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

        Public Event OneStepMore(valNew as Integer)
        Public Delegate Sub TestEvent(newVal As Integer)
    End Class";

                var expected = @"
    Class A
        public Sub TestA(value as Int)
        End Sub

        public Sub TestB(valueTest as string, other as string, doNotRenameMe as string)
        End Sub

        Public Event OneStepMore(valNew as Integer)
        Public Delegate Sub TestEvent(newVal As Integer)
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

        Public Event OneStepMore(ValNew as Integer)
        Public Delegate Sub TestEvent(NewVal As Integer)
    End Class";

                var expected = @"
    Class A
        public Sub TestA(value as Int)
        End Sub
        public Sub TestB(value as string, other as string, renameMe as string)
        End Sub
        public Sub TestA(sOtherTest as string)
        End Sub

        Public Event OneStepMore(valNew as Integer)
        Public Delegate Sub TestEvent(newVal As Integer)
    End Class";

                Verify(text, expected, runFormatter: false, languageName: LanguageNames.VisualBasic);
            }
        }
    }
}
