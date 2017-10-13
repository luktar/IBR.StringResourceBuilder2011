using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using ResxFinder.Model;
using System.Collections.Generic;

namespace ResxFinderTests
{
    [TestClass]
    public class UtilsTests
    {
        [TestMethod]
        public void TestMethod1()
        {
            string input = @"string variable1 = ""variable 1\"";
            string variable2 = ""variable 2"";
            string variable3 = ""Neque porro quisquam est qui dolorem ipsum "" +
                ""quia dolor sit amet, consectetur, adipisci velit...""; ";

            List<CodeTextElement> codeElements = TextUtils.GetCodeElements(10, input);


        }
    }
}
