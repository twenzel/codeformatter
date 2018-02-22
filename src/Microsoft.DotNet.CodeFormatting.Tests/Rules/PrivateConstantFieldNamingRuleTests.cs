using Microsoft.CodeAnalysis;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Xunit;

namespace Microsoft.DotNet.CodeFormatting.Tests
{
    public class PrivateConstantFieldNamingRuleTests : GlobalSemanticRuleTestBase
    {
        internal override IGlobalSemanticFormattingRule Rule
        {
            get { return new Rules.PrivateConstantFieldNamingRule(); }
        }

        public sealed class CSharpFields : PrivateConstantFieldNamingRuleTests
        {
            [Fact]
            public void TestUpperCaseInPrivateConsts()
            {
                var text = @"
using System;
class T
{
    private const int x = 5;
    private const int s_y = 66;
    // some trivia
    const int m_z = 5;
    // some trivia
    private const int k = 1, m_s = 2, rsk_yz = 3, x_y_z = 8;
    //
    private const string tt = ""dd"";

    public const int otherconst = 5;
}";
                var expected = @"
using System;
class T
{
    private const int X = 5;
    private const int S_Y = 66;
    // some trivia
    const int M_Z = 5;
    // some trivia
    private const int K = 1, M_S = 2, RSK_YZ = 3, X_Y_Z = 8;
    //
    private const string TT = ""dd"";

    public const int otherconst = 5;
}";
                Verify(text, expected);
            }

            [Fact]
            public void MultipleDeclarators()
            {
                var text = @"
class C1
{
    private int field1, field2, field3;
}

class C2
{
    private static int field1, field2, field3;
}

class C3
{
    internal int field1, field2, field3;
}

class C4
{
    private const int field1 = 5;
}
";

                var expected = @"
class C1
{
    private int field1, field2, field3;
}

class C2
{
    private static int field1, field2, field3;
}

class C3
{
    internal int field1, field2, field3;
}

class C4
{
    private const int FIELD1 = 5;
}
";

                Verify(text, expected, runFormatter: true);
            }

            /// <summary>
            /// Ensure that Roslyn properly renames fields
            /// </summary>
            [Fact]
            public void Rename()
            {
                var text = @"
class C
{
    const int field = 5;

    int M(C p)
    {
        int x = p.field;
        return x;
    }

    int test1()
    {
        return = field * 6;
    }

    int test2()
    {
        return = this.field * 6;
    }
}";

                var expected = @"
class C
{
    const int FIELD = 5;

    int M(C p)
    {
        int x = p.FIELD;
        return x;
    }

    int test1()
    {
        return = FIELD * 6;
    }

    int test2()
    {
        return = this.FIELD * 6;
    }
}";

                Verify(text, expected);
            }
        }

        public sealed class VisualBasicFields : PrivateConstantFieldNamingRuleTests
        {
            [Fact]
            public void Simple()
            {
                var text = @"
Class C 
    Private const Field As Integer = 5
End Class";

                var expected = @"
Class C 
    Private const FIELD As Integer = 5
End Class";

                Verify(text, expected, runFormatter: false, languageName: LanguageNames.VisualBasic);
            }

            [Fact]
            public void ModuleFields()
            {
                var text = @"
Module C
    Private const Field As Integer = 5
End Module";

                var expected = @"
Module C
    Private const FIELD As Integer = 5
End Module";

                Verify(text, expected, runFormatter: false, languageName: LanguageNames.VisualBasic);
            }

            [Fact]
            public void MultipleDeclarations()
            {
                var text = @"
Class C 
    Private const Field1 As Integer = 1, Field2 As Integer = 2
End Class";

                var expected = @"
Class C 
    Private const FIELD1 As Integer = 1,FIELD2 As Integer = 2
End Class";

                Verify(text, expected, runFormatter: false, languageName: LanguageNames.VisualBasic);
            }

            [Fact]
            public void FieldAndUse()
            {
                var text = @"
Class C 
    Private const Field As Integer = 5

    Sub M()
        Console.WriteLine(Field)
    End Sub
End Class";

                var expected = @"
Class C 
    Private const FIELD As Integer = 5

    Sub M()
        Console.WriteLine(FIELD)
    End Sub
End Class";

                Verify(text, expected, runFormatter: false, languageName: LanguageNames.VisualBasic);
            }

            [Fact]
            public void Rename()
            {
                var text = @"
Class C1
    Private const Field As Integer = 5

    Function M(p As C1) As Integer
        Dim x = p.Field
        Return x
    End Function
End Class";

                var expected = @"
Class C1
    Private const FIELD As Integer = 5

    Function M(p As C1) As Integer
        Dim x = p.FIELD
        Return x
    End Function
End Class";

                Verify(text, expected, languageName: LanguageNames.VisualBasic);
            }

        }
    }
}
