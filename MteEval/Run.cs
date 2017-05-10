/**********************************************************************************
 '*
 '* Run.cs - Virtual machine that executes the code
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
using System.Text;

namespace MteEval
{
    //--------------------------------------------------------------------------- Run()
    //
    class Run
    {
        private const int StackSize = 50;
        private const int MemorySize = 20;
        private const int CodeHeaderParameterCount = 0;
        private const int CodeStartsHere = 1;
        private static Codeblock codeBlock;

        //-------------------------------------------------------------------- Execute()
        //
        public static double ExecuteCodeblock(Codeblock cb, double[] args)
        {
            double result = 0;

            // Store reference to codeblock
            codeBlock = cb;

            // If no code then return here
            if ( cb.code.ByteCode == null )
            {
                SetError(Error.NoCode, "Check compile error.");
                return (0d);
            }

            // Execute 
            result = ExecuteCode(cb.code, args);

            return (result);
        }

        //---------------------------------------------------------------- ExecuteCode()
        //
        private static double ExecuteCode( Code cbCode, double[] args )
        {
            Pcode pcode;
            bool run;
            double retval = 0d;
            double radians = 0d;
            int index;
            int paramcount;
            double ax = 0d;                                 // Accumulator
            int ip = 0;                                     // Instruction pointer
            int sp = 0;                                     // Stack pointer
            double[] stack = new double[StackSize + 1];     // StackSize + 1 because our stack is one based.
            double[] varmemory = new double[MemorySize];    // Variable memory

            // Set to bytecode and constants data table
            int[] code = cbCode.ByteCode;
            double[] constData = cbCode.ConstData;

            // get parameter count
            paramcount = code[CodeHeaderParameterCount];
            
            // Invalid number of parameters? 
            if (paramcount > args.Length)
            {
                SetError(Error.InsufficientArgs, "Expecting " + paramcount + " arguments.");
                return (0);
            }

            // Too many parameters?
            if (paramcount > MemorySize)
            {
                SetError(Error.TooManyArgs, "Max arguments=" + MemorySize + ", argcount=" + paramcount);
                return (0);
            }

            // Store parameters in memory
            if (paramcount > 0)
            {
                for (int i = 0; i < paramcount; i++)
                {
                    varmemory[i] = args[i];
                }
            }

            // Set instruction pointer
            ip = CodeStartsHere;

            // Execute code
            run = true;
            while ( run )
            {
                // Get instruction
                pcode = (Pcode)code[ip];

                switch (pcode)
                {
                    case Pcode.Push:

                        // Overflow?
                        if (++sp > StackSize)
                        {
                            StackOverflowError(ip, ax, sp);
                            return (0d);
                        }
                        stack[sp] = ax;
                        break;

                    case Pcode.PushVar:

                        // get index into memory block
                        index = code[++ip];

                        // store on stack
                        stack[++sp] = varmemory[index];
                        break;

                    case Pcode.PushConst:
                        
                        // get index into memory block
                        index = code[++ip];

                        // store on stack
                        stack[++sp] = constData[index];
                        break;

                    case Pcode.LoadConst:

                        // get index into const data table
                        index = code[++ip];

                        // Get value from constant data table
                        ax = constData[index];
                        break;

                    case Pcode.LoadVar:

                        // get index into memory for this var
                        index = code[++ip];

                        // Get value from memory block
                        ax = varmemory[index];
                        break;

                    case Pcode.StoreVar:

                        // get index into memory for this var
                        index = code[++ip];

                        // store value in memory block
                        varmemory[index] = ax;
                        break;

                    case Pcode.Neg:

                        ax = -ax;
                        break;

                    case Pcode.Add:

                        ax = stack[sp] + ax;
                        sp--;                        // pop
                        break;

                    case Pcode.Subtract:

                        ax = stack[sp] - ax;
                        sp--;                        // pop
                        break;

                    case Pcode.Multiply:

                        ax = stack[sp] * ax;
                        sp--;                        // pop
                        break;

                    case Pcode.Divide:

                        // Check for devide by zero
                        if (ax == 0d)
                        {
                            DivideByZeroError(ip, ax, sp);
                            return (0d);
                        }
                        ax = stack[sp] / ax;
                        sp--;                        // pop
                        break;

                    case Pcode.Modulo:

                        ax = (int)stack[sp] % (int)ax;
                        sp--;                        // pop
                        break;

                    case Pcode.LogicalOr:

                        // A > 0 or B > 0
                        ax = (stack[sp] > 0 || ax > 0) ? 1d : 0d;
                        sp--;                        // pop
                        break;

                    case Pcode.LogicalAnd:

                        ax = (stack[sp] > 0 && ax > 0) ? 1d : 0d;
                        sp--;                        // pop
                        break;

                    case Pcode.LogicalNot:

                        ax = (ax == 0) ? 1 : 0;
                        break;

                    case Pcode.Equal:

                        ax = (stack[sp] == ax) ? 1 : 0;
                        sp--;
                        break;

                    case Pcode.NotEqual:

                        ax = (stack[sp] != ax) ? 1 : 0;
                        sp--;
                        break;

                    case Pcode.LessThan:

                        ax = (stack[sp] < ax) ? 1 : 0;
                        sp--;
                        break;

                    case Pcode.LessEqual:

                        ax = (stack[sp] <= ax) ? 1 : 0;
                        sp--;
                        break;

                    case Pcode.GreaterThan:

                        ax = (stack[sp] > ax) ? 1 : 0;
                        sp--;
                        break;

                    case Pcode.GreaterEqual:

                        ax = (stack[sp] >= ax) ? 1 : 0;
                        sp--;
                        break;

                    case Pcode.BitAnd:

                        ax = (int)stack[sp] & (int)ax;
                        sp--;
                        break;

                    case Pcode.BitOr:

                        ax = (int)stack[sp] | (int)ax;
                        sp--;
                        break;

                    case Pcode.BitXor:

                        ax = (int)stack[sp] ^ (int)ax;
                        sp--;
                        break;

                    case Pcode.BitNot:

                        ax = ~(int)ax;
                        break;

                    case Pcode.BitShiftLeft:

                        ax = (int)stack[sp] << (int)ax;
                        sp--;
                        break;

                    case Pcode.BitShiftRight:

                        ax = (int)stack[sp] >> (int)ax;
                        sp--;
                        break;

                    case Pcode.JumpAlways:

                        ip += code[ip + 1];
                        break;

                    case Pcode.JumpFalse:

                        if (ax == 0d) ip += code[ip + 1]; else ip++;
                        break;

                    case Pcode.JumpTrue:

                        if (ax > 0d) ip += code[ip + 1]; else ip++;
                        break;

                    case Pcode.FuncAbs:

                        ax = Math.Abs(stack[sp]);
                        sp--;
                        break;

                    case Pcode.FuncMax:

                        ax = (stack[sp - 1] > stack[sp]) ? stack[sp - 1] : stack[sp];
                        sp -= 2;
                        break;

                    case Pcode.FuncMin:

                        ax = (stack[sp - 1] < stack[sp]) ? stack[sp - 1] : stack[sp];
                        sp -= 2;
                        break;

                    case Pcode.FuncSqrt:

                        ax = Math.Sqrt(stack[sp]);
                        sp--;
                        break;

                    case Pcode.FuncPower:

                        ax = Math.Pow(stack[sp - 1], stack[sp]);
                        sp -= 2;
                        break;

                    case Pcode.FuncRound:

                        ax = Math.Round(stack[sp]);
                        sp--;
                        break;

                    case Pcode.FuncFloor:

                        ax = Math.Floor(stack[sp]);
                        sp--;
                        break;

                    case Pcode.FuncCeil:

                        ax = Math.Ceiling(stack[sp]);
                        sp--;
                        break;

                    case Pcode.FuncCos:

                        ax = Math.Cos(stack[sp]);
                        sp--;
                        break;

                    case Pcode.FuncCosD:

                        radians = stack[sp] * Math.PI / 180.0d;
                        ax = Math.Cos(radians);
                        sp--;
                        break;

                    case Pcode.FuncSin:

                        ax = Math.Sin(stack[sp]);
                        sp--;
                        break;

                    case Pcode.FuncSinD:

                        radians = stack[sp] * Math.PI / 180.0d;
                        ax = Math.Sin(radians);
                        sp--;
                        break;

                    case Pcode.FuncTan:

                        ax = Math.Tan(stack[sp]);
                        sp--;
                        break;

                    case Pcode.FuncTanD:

                        radians = stack[sp] * Math.PI / 180.0d;
                        ax = Math.Tan(radians);
                        sp--;
                        break;

                    case Pcode.FuncAcos:

                        ax = Math.Acos(stack[sp]);
                        sp--;
                        break;

                    case Pcode.FuncAcosD:

                        radians = Math.Acos(stack[sp]);
                        ax = radians / Math.PI * 180.0d;   // convert to degrees
                        sp--;
                        break;

                    case Pcode.FuncASin:

                        ax = Math.Asin(stack[sp]);
                        sp--;
                        break;

                    case Pcode.FuncASinD:

                        radians = Math.Asin(stack[sp]);
                        ax = radians / Math.PI * 180.0d;   // convert to degrees
                        sp--;
                        break;

                    case Pcode.FuncAtan:

                        ax = Math.Atan(stack[sp]);
                        sp--;
                        break;

                    case Pcode.FuncAtanD:

                        radians = Math.Atan(stack[sp]);
                        ax = radians / Math.PI * 180.0d;   // convert to degrees
                        sp--;
                        break;

                    case Pcode.FuncNumberFormat:

                        // Format number
                        ax = NumberFormatDouble(stack[sp-2], (int)stack[sp-1], (int)stack[sp]);
                        sp -= 3;

                        break;

                    case Pcode.FuncAvg:

                        ax = (stack[sp - 1] + stack[sp]) / 2.0d;
                        sp -= 2;
                        break;

                    case Pcode.EndCode:

                        run = false;
                        retval = ax;
                        break;

                    default:

                        SetError(Error.IllegalCode, "Pcode=" + pcode);
                        return (0);
                }

                // Advance to next instruction
                ip++;
            }

            // Report error, if items remaining on the stack
            if ( sp != 0 )
            {
                SetError(Error.StackMemoryLeak, String.Format("VCPU state: IP={0}, AX={1}, SP={2}", ip, ax, sp ));
            }

            return (retval);
        }


        //--------------------------------------------------------- NumberFormatDouble()
        //
        private static double NumberFormatDouble(double number, int minIntegers, int maxFractions)
        {
            // Begin format spec
            StringBuilder formatSpec = new StringBuilder("{0:");

            // Format integer part
            if (minIntegers > 0)
            {
                formatSpec.Append( "".PadLeft(minIntegers, '0'));
            }
            else
            {
                formatSpec.Append("#");
            }

            // Format decimal part
            if (maxFractions > 0)
            {
                formatSpec.Append(".".PadRight(maxFractions + 1, '0'));
            }

            // Complete format spec
            formatSpec.Append("}");

            //  Format number
            var formatted = string.Format(formatSpec.ToString(), number);

            // Convert back to double
            bool success = double.TryParse(formatted, out double result);

            return (result);
        }


        //--------------------------------------------------------- StackOverflowError()
        //
        private static Error StackOverflowError( int ip, double ax, int sp )
        {
            string detail = "IP=" + ip +
                          ", AX=" + ax +
                          ", SP=" + sp;

            return (SetError(Error.StackOverflow, detail));
        }

        //---------------------------------------------------------- DivideByZeroError()
        //
        private static Error DivideByZeroError(int ip, double ax, int sp)
        {
            string detail = "IP=" + ip +
                          ", AX=" + ax +
                          ", SP=" + sp;

            return (SetError(Error.DivideByZero, detail));
        }


        //------------------------------------------------------------------- SetError()
        //
        private static Error SetError( Error err, string errDetail )
        {

            // Store last error and detail
            codeBlock.LastError = err;
            codeBlock.ErrDetail = errDetail;

            return (err);
        }

    }
}
