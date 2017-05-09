/**********************************************************************************
 '*
 '* Internalfunc.cs - Built-in function definitions
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

using System.Collections.Generic;

namespace MteEval
{

    //------------------------------------------------------------- InternalFuncTable()
    //
    class InternalFuncTable
    {
        private static InternalFunc[] funcTable = new InternalFunc[]
        {
            new InternalFunc("####",    Pcode.None,         0 ),
            new InternalFunc("abs",     Pcode.FuncAbs,      1 ),
            new InternalFunc("iif",     Pcode.FuncIIF,      3 ),
            new InternalFunc("if",      Pcode.FuncIIF,      3 ),
            new InternalFunc("min",     Pcode.FuncMin,      2 ),
            new InternalFunc("max",     Pcode.FuncMax,      2 ),
            new InternalFunc("sqrt",    Pcode.FuncSqrt,     1 ),
            new InternalFunc("power",   Pcode.FuncPower,    2 ),
            new InternalFunc("round",   Pcode.FuncRound,    1 ),
            new InternalFunc("floor",   Pcode.FuncFloor,    1 ),
            new InternalFunc("ceil",    Pcode.FuncCeil,     1 ),
            new InternalFunc("cos",     Pcode.FuncCos,      1 ),
            new InternalFunc("cosd",    Pcode.FuncCosD,     1 ),
            new InternalFunc("sin",     Pcode.FuncSin,      1 ),
            new InternalFunc("sind",    Pcode.FuncSinD,     1 ),
            new InternalFunc("tan",     Pcode.FuncSqrt,     1 ),
            new InternalFunc("tand",    Pcode.FuncTanD,     1 ),
            new InternalFunc("acos",    Pcode.FuncAcos,     1 ),
            new InternalFunc("acosd",   Pcode.FuncAcosD,    1 ),
            new InternalFunc("asin",    Pcode.FuncASin,     1 ),
            new InternalFunc("asind",   Pcode.FuncASinD,    1 ),
            new InternalFunc("atan",    Pcode.FuncAtan,     1 ),
            new InternalFunc("atand",   Pcode.FuncAtanD,    1 ),
            new InternalFunc("avg",     Pcode.FuncAvg,      2 ),
            new InternalFunc("numberformat", Pcode.FuncNumberFormat, 3 )
        };

        public static IEnumerable<InternalFunc> Values
        {
            get
            {
                for (int i = 0; i < funcTable.Length; i++)
                {
                    yield return funcTable[i];
                }
            }
        }

        public static InternalFunc FuncNone 
        {
            get { return funcTable[0]; }
        }

    }

    //------------------------------------------------------------------ InternalFunc()
    //
    struct InternalFunc
    {
        public string name;
        public Pcode pcode;
        public int argcount;

        public InternalFunc(string name, Pcode pcode, int argcount)
        {
            this.name = name;
            this.pcode = pcode;
            this.argcount = argcount;
        }
    }
    
}
