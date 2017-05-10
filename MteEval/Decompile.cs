/**********************************************************************************
 '*
 '* Decompile.cs - Decompiles the code
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
using MteUtility;

namespace MteEval
{
    //--------------------------------------------------------------------- Decompile()
    //
    public static class Decompile
    {
        private const int CodeHeaderParameterCount = 0;
        private const int CodeStartsHere = 1;

        //---------------------------------------------------------------------- Dump()
        //
        public static List<string> DumpCodeblock(Codeblock cb)
        {

            var decode = new List<string>();

            // If no code then return here
            if (cb.code.ByteCode == null)
            {
                decode.Add("Error: No code");
                return (decode);
            }

            // Dump instructions to the list
            DumpCode(cb.code, decode);

            return (decode);
        }

        //------------------------------------------------------------------- DumpCode()
        //
        private static void DumpCode(Code cbCode, List<string> decode )
        {
            Pcode pcode;
            bool run;
            double val = 0d;
            int index;
            int target;
            int paramcount;
            int ip;

            // Set reference to bytecode and constants table
            int[] code = cbCode.ByteCode;
            double[] constData = cbCode.ConstData;

            // get parameter count
            paramcount = code[CodeHeaderParameterCount];
            
            // Begin listing
            decode.Add("-- Header --");
            decode.Add("Parameters=" + paramcount);
            decode.Add("-- Code --");

            // Set instruction pointer
            ip = CodeStartsHere;

            // List code
            run = true;
            while (run)
            {
                // Get instruction
                pcode = (Pcode)code[ip];

                switch (pcode)
                {
                    case Pcode.Push:

                        decode.Add(Pad(ip, "push", "ax"));
                        break;

                    case Pcode.PushVar:

                        ip++;                           // Next instruction has offset into memory table
                        index = code[ip];               // Get index into memory block
                        decode.Add(Pad(ip - 1, "pushv", "varmem[" + index + "]"));
                        break;

                    case Pcode.PushConst:

                        ip++;                           // Next instruction has offset into const table
                        index = code[ip];               // Get index into const table
                        val = constData[index];         // Get value from const table
                        decode.Add(Pad(ip - 1, "pushc", val.ToString()));
                        break;

                    case Pcode.LoadConst:

                        ip++;                           // Next instruction has offset into const table
                        index = code[ip];               // Get index into const table
                        val = constData[index];         // Get value from const table
                        decode.Add(Pad(ip - 1, "loadc", "ax, " + val));
                        break;

                    case Pcode.LoadVar:

                        ip++;                           // Next instruction has offset into memory table
                        index = code[ip];               // Get index into memory block
                        decode.Add(Pad(ip - 1, "loadv", "ax, varmem[" + index + "]"));
                        break;

                    case Pcode.StoreVar:

                        ip++;                           // Next instruction has offset into memory table
                        index = code[ip];               // Get index into memory block
                        decode.Add(Pad(ip - 1, "storev", "varmem[" + index + "]"));
                        break;

                    case Pcode.Neg:

                        decode.Add(Pad(ip, "neg", "ax"));
                        break;

                    case Pcode.Add:

                        decode.Add(Pad(ip, "add", "stack[sp] + ax"));
                        decode.Add(Pad(ip, "pop", ""));
                        break;

                    case Pcode.Subtract:

                        decode.Add(Pad(ip, "sub", "stack[sp] - ax"));
                        decode.Add(Pad(ip, "pop", ""));
                        break;

                    case Pcode.Divide:

                        decode.Add(Pad(ip, "div", "stack[sp] / ax"));
                        decode.Add(Pad(ip, "pop", ""));
                        break;

                    case Pcode.Multiply:

                        decode.Add(Pad(ip, "mul", "stack[sp] * ax"));
                        decode.Add(Pad(ip, "pop", ""));
                        break;

                    case Pcode.Modulo:

                        decode.Add(Pad(ip, "mod", "stack[sp] % ax"));
                        decode.Add(Pad(ip, "pop", ""));
                        break;

                    case Pcode.LogicalOr:

                        decode.Add(Pad(ip, "or", "stack[sp] || ax"));
                        decode.Add(Pad(ip, "pop", ""));
                        break;

                    case Pcode.LogicalAnd:

                        decode.Add(Pad(ip, "and", "stack[sp] && ax"));
                        decode.Add(Pad(ip, "pop", ""));
                        break;

                    case Pcode.LogicalNot:

                        decode.Add(Pad(ip, "not", "ax"));
                        break;

                    case Pcode.Equal:

                        decode.Add(Pad(ip, "eq", "stack[sp] == ax"));
                        decode.Add(Pad(ip, "pop", ""));
                        break;

                    case Pcode.NotEqual:

                        decode.Add(Pad(ip, "neq", "stack[sp] != ax"));
                        decode.Add(Pad(ip, "pop", ""));
                        break;

                    case Pcode.LessThan:

                        decode.Add(Pad(ip, "lt", "stack[sp] < ax"));
                        decode.Add(Pad(ip, "pop", ""));
                        break;

                    case Pcode.LessEqual:

                        decode.Add(Pad(ip, "le", "stack[sp] <= ax"));
                        decode.Add(Pad(ip, "pop", ""));
                        break;

                    case Pcode.GreaterThan:

                        decode.Add(Pad(ip, "gt", "stack[sp] > ax"));
                        decode.Add(Pad(ip, "pop", ""));
                        break;

                    case Pcode.GreaterEqual:

                        decode.Add(Pad(ip, "ge", "stack[sp] >= ax"));
                        decode.Add(Pad(ip, "pop", ""));
                        break;

                    case Pcode.BitAnd:

                        decode.Add(Pad(ip, "bitand", "stack[sp] & ax"));
                        decode.Add(Pad(ip, "pop", ""));
                        break;

                    case Pcode.BitOr:

                        decode.Add(Pad(ip, "bitor", "stack[sp] | ax"));
                        decode.Add(Pad(ip, "pop", ""));
                        break;

                    case Pcode.BitXor:

                        decode.Add(Pad(ip, "bitxor", "stack[sp] ^ ax"));
                        decode.Add(Pad(ip, "pop", ""));
                        break;

                    case Pcode.BitNot:

                        decode.Add(Pad(ip, "bitnot", "~ax"));
                        break;

                    case Pcode.BitShiftLeft:

                        decode.Add(Pad(ip, "bitlft", "stack[sp] << ax"));
                        decode.Add(Pad(ip, "pop", ""));
                        break;

                    case Pcode.BitShiftRight:

                        decode.Add(Pad(ip, "bitrgt", "stack[sp] >> ax"));
                        decode.Add(Pad(ip, "pop", ""));
                        break;

                    case Pcode.JumpAlways:

                        target = ip + code[ip + 1] + 1;   //  + 1 needed for correct location

                        decode.Add(Pad(ip, "jump", target.ToString()));
                        ip++;
                        break;

                    case Pcode.JumpFalse:

                        target = ip + code[ip + 1] + 1;   //  + 1 needed for correct location
                        decode.Add(Pad(ip, "jumpf", target.ToString())); 
                        ip++;
                        break;

                    case Pcode.JumpTrue:

                        target = ip + code[ip + 1] + 1;   //  + 1 needed for correct location
                        decode.Add(Pad(ip, "jumpt", target.ToString())); 
                        ip++;
                        break;

                    case Pcode.FuncAbs:

                        decode.Add(Pad(ip, "call", "abs"));
                        decode.Add(Pad(ip, "pop", ""));
                        break;

                    case Pcode.FuncMax:

                        decode.Add(Pad(ip, "call", "max"));
                        decode.Add(Pad(ip, "pop", "2"));
                        break;

                    case Pcode.FuncMin:

                        decode.Add(Pad(ip, "call", "min"));
                        decode.Add(Pad(ip, "pop", "2"));
                        break;

                    case Pcode.FuncSqrt:

                        decode.Add(Pad(ip, "call", "sqrt"));
                        decode.Add(Pad(ip, "pop", ""));
                        break;

                    case Pcode.FuncPower:

                        decode.Add(Pad(ip, "call", "power"));
                        decode.Add(Pad(ip, "pop", "2"));
                        break;

                    case Pcode.FuncRound:

                        decode.Add(Pad(ip, "call", "round"));
                        decode.Add(Pad(ip, "pop", ""));
                        break;

                    case Pcode.FuncFloor:

                        decode.Add(Pad(ip, "call", "floor"));
                        decode.Add(Pad(ip, "pop", ""));
                        break;

                    case Pcode.FuncCeil:

                        decode.Add(Pad(ip, "call", "ceil"));
                        decode.Add(Pad(ip, "pop", ""));
                        break;

                    case Pcode.FuncCos:

                        decode.Add(Pad(ip, "call", "cos"));
                        decode.Add(Pad(ip, "pop", ""));
                        break;

                    case Pcode.FuncCosD:

                        decode.Add(Pad(ip, "call", "cosd"));
                        decode.Add(Pad(ip, "pop", ""));
                        break;

                    case Pcode.FuncSin:

                        decode.Add(Pad(ip, "call", "sin"));
                        decode.Add(Pad(ip, "pop", ""));
                        break;

                    case Pcode.FuncSinD:

                        decode.Add(Pad(ip, "call", "sind"));
                        decode.Add(Pad(ip, "pop", ""));
                        break;

                    case Pcode.FuncTan:

                        decode.Add(Pad(ip, "call", "tan"));
                        decode.Add(Pad(ip, "pop", ""));
                        break;

                    case Pcode.FuncTanD:

                        decode.Add(Pad(ip, "call", "tand"));
                        decode.Add(Pad(ip, "pop", ""));
                        break;

                    case Pcode.FuncAcos:

                        decode.Add(Pad(ip, "call", "acos"));
                        decode.Add(Pad(ip, "pop", ""));
                        break;

                    case Pcode.FuncAcosD:

                        decode.Add(Pad(ip, "call", "acosd"));
                        decode.Add(Pad(ip, "pop", ""));
                        break;

                    case Pcode.FuncASin:

                        decode.Add(Pad(ip, "call", "asin"));
                        decode.Add(Pad(ip, "pop", ""));
                        break;

                    case Pcode.FuncASinD:

                        decode.Add(Pad(ip, "call", "asind"));
                        decode.Add(Pad(ip, "pop", ""));
                        break;

                    case Pcode.FuncAtan:

                        decode.Add(Pad(ip, "call", "atan"));
                        decode.Add(Pad(ip, "pop", ""));
                        break;

                    case Pcode.FuncAtanD:

                        decode.Add(Pad(ip, "call", "atand"));
                        decode.Add(Pad(ip, "pop", ""));
                        break;

                    case Pcode.FuncNumberFormat:

                        decode.Add(Pad(ip, "call", "numberformat"));
                        decode.Add(Pad(ip, "pop", "3"));
                        break;

                    case Pcode.FuncAvg:

                        decode.Add(Pad(ip, "call", "avg"));
                        decode.Add(Pad(ip, "pop", "2"));
                        break;

                    case Pcode.EndCode:

                        decode.Add(Pad(ip, "end", ""));
                        run = false;
                        break;

                    default:

                        decode.Add(Pad(ip, "err", "pcode=" + pcode));
                        run = false;
                        break;

                }

                // Advance to next instruction
                ip++;
            }
            
            return;
        }


        //------------------------------------------------------------------------ Pad()
        //
        private static string Pad(int ip, string instruct, string operands)
        {
            //001234576890
            //":          ";
            
            string formatted = String.Format("{0,-7:###':'}{1,-8}{2}", ip, instruct, operands);

            return (formatted);
        }
    }
}
