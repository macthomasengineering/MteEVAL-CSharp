/**********************************************************************************
 '*
 '* Program.cs - MteEVAL Readme examples and test cases
 '*
 **********************************************************************************/
/**********************************************************************************
 '*
 '* Copyright (c) 2016-2017, MacThomas Engineering
 '* All rights reserved.
 '*
 '* You may use this file under the terms of the BSD license as follows:
 '*
 '* Redistribution and use in source and binary forms, with or without
 '* modification, are permitted provided that the following conditions are met:
 '*
 '* 1. Redistributions of source code must retain the above copyright notice, this
 '*    list of conditions, and the following disclaimer.
 '*
 '* 2. Redistributions in binary form must reproduce the above copyright notice,
 '*    this list of conditions and the following disclaimer in the documentation
 '*    and/or other materials provided with the distribution.
 '*
 '* 3. MacThomas Engineering may not be used to endorse or promote products derived
 '*    from this software without specific prior written permission.
 '*
 '* THIS SOFTWARE IS PROVIDED BY THE COPYRIGHT HOLDERS AND CONTRIBUTORS "AS IS" AND
 '* ANY EXPRESS OR IMPLIED WARRANTIES, INCLUDING, BUT NOT LIMITED TO, THE IMPLIED
 '* WARRANTIES OF MERCHANTABILITY AND FITNESS FOR A PARTICULAR PURPOSE ARE
 '* DISCLAIMED. IN NO EVENT SHALL THE COPYRIGHT OWNER OR CONTRIBUTORS BE LIABLE FOR
 '* ANY DIRECT, INDIRECT, INCIDENTAL, SPECIAL, EXEMPLARY, OR CONSEQUENTIAL DAMAGES
 '* (INCLUDING, BUT NOT LIMITED TO, PROCUREMENT OF SUBSTITUTE GOODS OR SERVICES;
 '* LOSS OF USE, DATA, Or PROFITS; OR BUSINESS INTERRUPTION) HOWEVER CAUSED And
 '* ON ANY THEORY OF LIABILITY, WHETHER IN CONTRACT, STRICT LIABILITY, OR TORT
 '* (INCLUDING NEGLIGENCE OR OTHERWISE) ARISING IN ANY WAY OUT OF THE USE OF THIS
 '* SOFTWARE, EVEN IF ADVISED OF THE POSSIBILITY OF SUCH DAMAGE.
 '*
 **********************************************************************************/

using System;
using System.Collections.Generic;
using MteEval;
using MteUtility;

namespace MteEvalTestLib
{
    //----------------------------------------------------------------------- Program()
    //
    class Program
    {
        static void Main(string[] args)
        {

            ReadMe.DoExamples();
            Tests.DoTests();
            Console.ReadKey();
        }
    }

    //------------------------------------------------------------------------ ReadMe()
    // Github Readme Examples
    //
    public class ReadMe
    {
        //---------------------------------------------------------------- DoExamples()
        //
        public static void DoExamples()
        {
            // Example 1: Codeblock without parameters
            Codeblock cb = new Codeblock();
            cb.Compile("{||5 + 3}");
            double result = cb.Eval();           // result=8
            Console.WriteLine("The result is: " + result);

            // Example 2: Codeblock with parameters
            cb.Compile("{|length,width|length*width}");
            double area = cb.Eval(3, 17);        // area=51
            Console.WriteLine("The area is: " + area);

            // Example 3: Codeblock compile, eval and repeat
            cb.Compile("{|sales,r1,r2| r1*sales + iif( sales > 100000, (sales-100000)*r2, 0 ) }");
            double commission1 = cb.Eval(152000, .08, .05);    // commission1=14760
            double commission2 = cb.Eval(186100, .08, .07);    // commission2=20915
            double commission3 = cb.Eval(320000, .08, .05);    // commission3=36600
            Console.WriteLine("commission1: " + commission1);
            Console.WriteLine("commission2: " + commission2);
            Console.WriteLine("commission3: " + commission3);
        }
    }
    
    //------------------------------------------------------------------------- Tests()
    //
    public class Tests
    {
        static public int testExecuted;
        static public int testPassed;

        //------------------------------------------------------------------- DoTests()
        //
        public static void DoTests()
        {
            // Start logger
            Mtelog.Start();

            Mtelog.Console("-------------------------------");
            Mtelog.Console("MteEval Library C# v" + VersionInfo.Text);
            Mtelog.Console("-------------------------------");

            // Reset test counts
            testExecuted = 0;
            testPassed = 0;

            DoTestCase(0, "Optimize Example", "{|sales,r1,r2| r1*sales + iif( sales > 100000, (sales-100000)*r2, 0 ) }", new double[] { 152000, .08, .05 }, new ExpectedResult(Error.None, 14760));


            //DoTestCase(1, "Add and Subtract", "{||5+6-3}", new double[] { }, new ExpectedResult(Error.None, 8));
            //DoTestCase(2, "Divide By Zero", "{||5+6/0}", new double[] { }, new ExpectedResult(Error.DivideByZero, 0));
            //DoTestCase(3, "Cirumference of circle, radius=7", "{|r|2*r*CPi}", new double[] { 7 }, new ExpectedResult(Error.None, 43.98));
            //DoTestCase(4, "Area of circle, radius=9", "{|r|CPi*(r*r)}", new double[] { 9 }, new ExpectedResult(Error.None, 254.47));
            //DoTestCase(5, "Missing parenthesis", "{||35*3+6)}", new double[] { 5 }, new ExpectedResult(Error.UnbalancedParens, 0));
            //DoTestCase(6, "Min with iif()", "{|a,b| iif( a > b, b, a) }", new double[] { 7, 12 }, new ExpectedResult(Error.None, 7));
            //DoTestCase(7, "Find length of diagonal", "{|a,b| sqrt(a*a+b*b)}", new double[] { 7, 9 }, new ExpectedResult(Error.None, 11.40));
            //DoTestCase(8, "Report e", "{|| ce }", new double[] { }, new ExpectedResult(Error.None, Math.E));
            //DoTestCase(9, "Kitchen Sink", "{|a,b,c,d,e,f|-abs(iif(a+c*f <= e/d*a, iif( min(a,d) >= max(c,e), b*b, c*c ), iif( 5 == c || 5 == e || 77 > 22 && !(10 < 3), Sqrt(c), Sqrt(d))))}", new double[] { 34, 43, 17, 25, 45, 13 }, new ExpectedResult(Error.None, -4.12));
            //DoTestCase(10, "Sequence test", "{|| iif( 7 > 6, (1,2,3,4,5), (10,11,12)) }", new double[] { }, new ExpectedResult(Error.None, 5));
            //DoTestCase(11, "Bitwise operators", "{|r,g,b|((r & 0x0ff) << 16) | ((g & 0x0ff) << 8) | (b & 0x0ff)}", new double[] { 178, 255, 102 }, new ExpectedResult(Error.None, 11730790));
            //DoTestCase(12, "Bit Not", "{|| ~0x6B2}", new double[] { }, new ExpectedResult(Error.None, unchecked((int)0xFFFFF94D))); // -171)5
            //DoTestCase(13, "Commission", "{|sales,r1,r2,r3| r1*sales + iif( sales > 200000, (sales-200000)*r3 + r2*100000,  iif( sales > 100000, (sales-100000)*r2, 0 )  ) }", new double[] { 320000, .08, .05, .03 }, new ExpectedResult(Error.None, 34200));
            //DoTestCase(14, "Commission with assignments", "{|sales,r1,r2,r3,s100,s200| (iif( sales > 200000, (s200=sales-200000, s100=100000), s100=iif( sales > 100000, sales-100000, 0 )),sales*r1 + s100*r2 + s200*r3) }", new double[] { 320000, .08, .05, .03, 0, 0 }, new ExpectedResult(Error.None, 34200));
            //DoTestCase(15, "Compare", "{|A1| 100 > A1 && A1 > 80 }", new double[] { 90 }, new ExpectedResult(Error.None, 1));
            //DoTestCase(16, "CosD, SinD, TanD", "{|deg| TanD(deg) == SinD(deg)/CosD(deg) }", new double[] { 35 }, new ExpectedResult(Error.None, 1));
            //DoTestCase(17, "ATanD", "{|t|ATanD(t)}", new double[] { 1 }, new ExpectedResult(Error.None, 45));
            //DoTestCase(18, "Assignment", "{|a,b,c|(a=b=c,a)}", new double[] { 5, 10, 33 }, new ExpectedResult(Error.None, 33));
            //DoTestCase(19, "Average", "{|ref,inp|5+NumberFormat(avg(ref,inp),0,2)}", new double[] { 56.54, 38.33 }, new ExpectedResult(Error.None, 52.44));
            //DoTestCase(20, "Insufficient arguments for internal function", "{|a,b|avg(a)}", new double[] { 6, 3 }, new ExpectedResult(Error.InsufficientArgs, 0));

            Mtelog.Console("");
            Mtelog.Console("");
            Mtelog.Console("-----------------");
            Mtelog.Console("TEST RESULTS");
            Mtelog.Console("-----------------");
            Mtelog.Console("Test executed: " + testExecuted);
            Mtelog.Console("  Test passed: " + testPassed);
            Mtelog.Console("-----------------");

            Console.WriteLine("");
            Console.WriteLine("Test output written to: " + Mtelog.LogDirectory );

            // Stop logger
            Mtelog.Stop();

        }

        //---------------------------------------------------------------- DoTestCase()
        //
        public static void DoTestCase(int testnumber, string description, string codeblockText, 
                                        double[] args, ExpectedResult expectedResult)
        {
            Codeblock cb = new Codeblock();
            int errCode = Error.None.ErrCode;
            double result = 0d;
            bool decompile = true;
            cb.OptimizerEnabled = true;

            // Count test
            testExecuted++;

            // Test description
            Mtelog.Console("********************");
            Mtelog.Console(testnumber + " - " + description);
            Mtelog.Console("Codeblock=" + codeblockText);

            // Compile and report error (if any)
            errCode = cb.Compile(codeblockText);
            if (cb.LastError != Error.None)
            {
                if (cb.LastError == expectedResult.lastError)
                    testPassed++;

                Mtelog.Console("Compile error=" + cb.LastError.ErrCode);
                Mtelog.Console("Error Description=" + cb.LastError.ErrDesc);
                Mtelog.Console("Error Detail=" + cb.ErrDetail);
                return;
            }
            Mtelog.Console("Codeblock compiled.");

            // Decompile
            if (decompile)
            {
                // Decompile code
                List<string> codelist = cb.Decompile();

                // Output code listing    
                foreach (var item in codelist)
                {
                    Mtelog.Console(item);
                }
            }

            // Evaluate the Codeblock
            result = cb.Eval(args);

            Mtelog.Console("Codeblock executed!");

            // Show results
            Mtelog.Console("Result=" + result.ToString());

            // Report error
            if (cb.LastError != expectedResult.lastError)
            {
                Mtelog.Console("Eval() error=" + cb.LastError.ErrCode);
                Mtelog.Console("Error Description=" + cb.LastError.ErrDesc);
                Mtelog.Console("Error Detail:" + cb.ErrDetail);
                return;
            }

            // Format results
            string resultText = string.Format("{0:0.00}", result);
            string expectText = string.Format("{0:0.00}", expectedResult.result);

            // Verify results
            if (!resultText.Equals(expectText))
            {
                Mtelog.Console("**************************");
                Mtelog.Console("Error: Unexpected result. " + resultText + "!=" + expectText);
                Mtelog.Console("**************************");
                return;
            }

            // Count test passed
            testPassed++;

            return;
        }
    }

    //----------------------------------------------------------------- ExpectedRsult()
    //
    public class ExpectedResult
    {
        public Error lastError;
        public double result;

        public ExpectedResult(Error lastError, double result)
        {
            this.lastError = lastError;
            this.result = result;
        }
    }
}
