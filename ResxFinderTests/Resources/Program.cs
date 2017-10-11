using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LocalizationTest
{
    class Program
    {
        string atTest = @"Where do bad folks go when they die?
                                They don't go to heaven where the angels fly
                                They go down to the lake of fire and fry
                                Won't see 'em again till the fourth of July

                                I knew a lady who came from Duluth
                                She got bit by a dog with a rabid tooth
                                She went to her grave just a little too soon
                                Flew away howling on the yellow moon

                                Where do bad folks go when they die?
                                They don't go to heaven where the angels fly
                                They go down to the lake of fire and fry
                                Won't see 'em again till the fourth of July

                                the people cry and the people moan
                                Look for a dry place to call their home
                                Try to find some place to rest their bones
                                While the angels and the devils try to make their own

                                Where do bad folks go when they die?
                                They don't go to heaven where the angels fly
                                They go down to the lake of fire and fryz
                                Won't see 'em again till the fourth of July";

        private string stringValue1 = "Private string value 1";
        private const string stringConstValue1 = "String const value 1";
        public string publicStringValue1 = "Public strinv value 1";
        protected string protectedStringValue1 = "Protected string value 1";
        private static string privateStaticStringValue1 = "Private static string value 1";

        public string PublicStringProperty1 { get; set; } = "Public string property 1";

        public string PublicStringProperty2
        {
            get { return "Public string property 2"; }
        }

        public string PublicStringProperty3
        {
            get {
                return "Public " +
                  "string " +
                  "property " +
                  "3";
            }
        }

        public static string PublicStaticStringProperty1
        {
            get { return "public static string property 1"; }
        }

        public Program()
        {
            string variable1 = "variable 1";
            string variable2 = "variable 2";
            string variable3 = "Neque porro quisquam est qui dolorem ipsum " +
                "quia dolor sit amet, consectetur, adipisci velit...";
        }

        static void Main(string[] args)
        {
            string variable4 = "variable 4";
            string variable5 = "variable 5";
            string variable6 = "Neque porro quisquam est qui dolorem ipsum " +
                "quia dolor sit amet, consectetur, adipisci velit...";
        }

        public void VariablesTest()
        {
            string variable7 = "variable 7";
            string variable8 = "variable 8";
            string variable9 = "Neque porro quisquam est qui dolorem ipsum " +
                "quia dolor sit amet, consectetur, adipisci velit...";

            string dolarTest = $"Hello my name is {variable7}, and what's your name {variable8}.";

            string stringFormat = string.Format("Hello {0}, how are you {1}", "world!", "today?");

        }

        public void ParametersTest()
        {
            Console.WriteLine("Console string 1");
            Console.WriteLine("Console string 2" + "Console string 2");
            Console.WriteLine("Console string 3" + "Console string 3" +
                "Console string 3");
        }
    }
}
