// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using Microsoft.CodeAnalysis;
using Xunit;

namespace Microsoft.DotNet.CodeFormatting.Tests
{
    public class LocalVariableNamingRuleTests : GlobalSemanticRuleTestBase
    {
        internal override IGlobalSemanticFormattingRule Rule
        {
            get { return new Rules.LocalVariableNamingRule(); }
        }


        public sealed class CSharpFields : LocalVariableNamingRuleTests
        {

            [Fact]
            public void Single_Char_Variables_Are_Written_In_Lowercase()
            {
                var text = @"
class C
{
	public void foo()
	{
		var X = 0;
		var y = \""brb\"";
		var _ = 10.0d;
	}
}
";

                var expected = @"
class C
{
	public void foo()
	{
		var x = 0;
		var y = \""brb\"";
		var _ = 10.0d;
	}
}
";

                Verify(text, expected, runFormatter: false);
            }

            [Fact]
            public void Var_Keyword_Variables_Are_Written_In_Camelcase()
            {
                var text = @"
class C
{
	public void foo()
	{
		var AnyInt = 0;
		var AnyString = \""brb\"";
		var AnyDouble = 10.0d;
	}
}
";

                var expected = @"
class C
{
	public void foo()
	{
		var anyInt = 0;
		var anyString = \""brb\"";
		var anyDouble = 10.0d;
	}
}
";

                Verify(text, expected, runFormatter: false);
            }

            [Fact]
            public void Multiple_Declarators_Are_Written_In_Lowercase()
            {
                var text = @"
class C
{
	public void foo()
	{
		int Field1, Field2, Field3;
	}
}
";

                var expected = @"
class C
{
	public void foo()
	{
		int field1,field2,field3;
	}
}
";

                Verify(text, expected, runFormatter: false);
            }

            [Fact]
            public void Multiple_Static_Declarators_Are_Written_In_Lowercase()
            {
                var text = @"
class C
{
	public void foo()
	{
		static int Field1, Field2, Field3;
	}
}
";

                var expected = @"
class C
{
	public void foo()
	{
		static int field1,field2,field3;
	}
}
";

                Verify(text, expected, runFormatter: false);
            }

            /// <summary>
            /// If the name is pascal cased make it camel cased during the rewrite.  If it is not
            /// pascal cased then do not change the casing.
            /// </summary>
            [Fact]
            public void Simple_Names_Will_Be_CamelCase()
            {
                var text = @"
class C
{
	public void foo()
	{
		int Field;
		static int Other;
		int FieldName;
		static int OtherName;
		int ExtendedFieldName;
		static int OtherExtendedFieldName;

		int Field2 = -1;
		static int Other2 = -1;
		int FieldName2 = -1;
		static int OtherName2 = -1;
		int ExtendedFieldName2 = -1;
		static int OtherExtendedFieldName2 = -1;
	}
}
";

                var expected = @"
class C
{
	public void foo()
	{
		int field;
		static int other;
		int fieldName;
		static int otherName;
		int extendedFieldName;
		static int otherExtendedFieldName;

		int field2 = -1;
		static int other2 = -1;
		int fieldName2 = -1;
		static int otherName2 = -1;
		int extendedFieldName2 = -1;
		static int otherExtendedFieldName2 = -1;
	}
}
";
                Verify(text, expected, runFormatter: false);
            }

            /// <summary>
            /// If the name is pascal cased make it camel cased during the rewrite.  If it is not
            /// pascal cased then do not change the casing.
            /// </summary>
            [Fact]
            public void Non_PascalCase_Names_Will_Stay_PascalCase()
            {
                var text = @"
class C
{
	public void foo()
	{
		int GCField;
		static int GCOther;
		int GCField2 = -1;
		static int GCOther2 = -1;
	}
}
";

                var expected = @"
class C
{
	public void foo()
	{
		int GCField;
		static int GCOther;
		int GCField2 = -1;
		static int GCOther2 = -1;
	}
}
";
                Verify(text, expected, runFormatter: false);
            }

            [Fact]
            public void Type_Names_Are_Not_Touched_Even_If_Variables_Are_Named_Equally()
            {
                var text = @"
class C
{
	public LockType foo(bool val)
	{
		LockType LockType = LockType.Open;
		if (val)
			LockType = LockType.Locked;

		return LockType;
	}

	public enum LockType
	{
		Open = 0;
		Locked = 1;
	}
}
";

                var expected = @"
class C
{
	public LockType foo(bool val)
	{
		LockType lockType = LockType.Open;
		if (val)
			lockType = LockType.Locked;

		return lockType;
	}

	public enum LockType
	{
		Open = 0;
		Locked = 1;
	}
}
";
                Verify(text, expected, runFormatter: false);
            }
        }


        public sealed class VisualBasicFields : LocalVariableNamingRuleTests
        {
            [Fact]
            public void Dim_Variables_Without_As_Are_Lowercase()
            {
                var text = @"
Class C 
    Public Sub New()
		Dim AnyInt = 0
		Dim AnyString = ""brb""
		Dim AnyDouble = 0.0
	End Sub
End Class";

                var expected = @"
Class C 
    Public Sub New()
		Dim anyInt = 0
		Dim anyString = ""brb""
		Dim anyDouble = 0.0
	End Sub
End Class";

                Verify(text, expected, runFormatter: false, languageName: LanguageNames.VisualBasic);
            }

            [Fact]
            public void Simple()
            {
                var text = @"
Class C 
    Public Sub New()
		Dim Field1 As Integer
		Dim Field2 As Int32
	End Sub
End Class";

                var expected = @"
Class C 
    Public Sub New()
		Dim field1 As Integer
		Dim field2 As Int32
	End Sub
End Class";

                Verify(text, expected, runFormatter: false, languageName: LanguageNames.VisualBasic);
            }

            [Fact]
            public void Single_Char_Variables_Are_Written_In_Lowercase()
            {
                var text = @"
Class C 
    Public Sub New()
		Dim X As Integer
		Dim y As Int32
		Dim _ As Char
	End Sub
End Class";

                var expected = @"
Class C 
    Public Sub New()
		Dim x As Integer
		Dim y As Int32
		Dim _ As Char
	End Sub
End Class";

                Verify(text, expected, runFormatter: false, languageName: LanguageNames.VisualBasic);
            }

            [Fact]
            public void MultipleDeclarations()
            {
                var text = @"
Class C 
    Public Sub New()
		Dim Field1, Field2 As Integer
		Dim Field3, Field4 As Int32
	End Sub
End Class";

                var expected = @"
Class C 
    Public Sub New()
		Dim field1,field2 As Integer
		Dim field3,field4 As Int32
	End Sub
End Class";

                Verify(text, expected, runFormatter: false, languageName: LanguageNames.VisualBasic);
            }

            [Fact]
            public void FieldAndUse()
            {
                var text = @"
Class C 
    Sub M()
		Dim Field As Integer
        Console.WriteLine(Field)
    End Sub
End Class";

                var expected = @"
Class C 
    Sub M()
		Dim field As Integer
        Console.WriteLine(field)
    End Sub
End Class";

                Verify(text, expected, runFormatter: false, languageName: LanguageNames.VisualBasic);
            }

            [Fact]
            public void Type_Names_Are_Not_Touched_Even_If_Variables_Are_Named_Equally()
            {
                var text = @"
Public Class C
	Public Function foo(bool val) As LockType
		Dim LockType As LockType = LockType.Open;
		If (val) Then
			LockType = LockType.Locked;
		End If

		Return LockType;
	End Function

	Public Enum LockType
		Open = 0
		Locked = 1
	End Enum
End Class
";

                var expected = @"
Public Class C
	Public Function foo(bool val) As LockType
		Dim lockType As LockType = LockType.Open;
		If (val) Then
			lockType = LockType.Locked;
		End If

		Return lockType;
	End Function

	Public Enum LockType
		Open = 0
		Locked = 1
	End Enum
End Class
";
                Verify(text, expected, runFormatter: false, languageName: LanguageNames.VisualBasic);
            }
        }
    }
}
