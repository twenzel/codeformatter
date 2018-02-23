using Microsoft.CodeAnalysis;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Xunit;

namespace Microsoft.DotNet.CodeFormatting.Tests
{
    public class InterfaceNamingRuleTests : GlobalSemanticRuleTestBase
    {
        internal override IGlobalSemanticFormattingRule Rule
        {
            get { return new Rules.InterfaceNamingRule(); }
        }

        public sealed class CSharpFields : InterfaceNamingRuleTests
        {
            [Fact]
            public void DoNotRename()
            {
                var text = @"
interface ITest
{
}

class OtherTest
{
}";
                var expected = @"
interface ITest
{
}

class OtherTest
{
}";
                Verify(text, expected);
            }

            [Fact]
            public void Rename()
            {
                var text = @"
interface Test
{
}

interface test2
{
}";
                var expected = @"
interface ITest
{
}

interface ITest2
{
}";
                Verify(text, expected);
            }
        }

        public sealed class VisualBasicFields : InterfaceNamingRuleTests
        {
            [Fact]
            public void DoNotRename()
            {
                var text = @"
Interface ITest 
    
End Interface

Class OtherTest 
    
End Class";

                var expected = @"
Interface ITest 
    
End Interface

Class OtherTest 
    
End Class";

                Verify(text, expected, runFormatter: false, languageName: LanguageNames.VisualBasic);
            }

            [Fact]
            public void Rename()
            {
                var text = @"
Interface Test
End Interface

Interface test2
End Interface";

                var expected = @"
Interface ITest
End Interface

Interface ITest2
End Interface";

                Verify(text, expected, runFormatter: false, languageName: LanguageNames.VisualBasic);
            }


        }
    }
}
