/**********************************************************************************
 '*
 '* Pcode.cs - Op codes
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

namespace MteEval
{
    //--------------------------------------------------------------------------- Pcode
    //
    enum Pcode
    {
        None=0,

        // Stack
        Push,            
        PushVar,         
        PushConst,       

        // Load and store
        LoadConst,
        LoadVar,  
        StoreVar, 

        // Math
        Neg,      
        Add,      
        Subtract,
        Multiply,
        Divide,
        Modulo,

        // Logical
        LogicalOr,
        LogicalAnd,
        LogicalNot,

        // Relational
        Equal,
        NotEqual,
        LessThan,
        LessEqual,
        GreaterThan,
        GreaterEqual,

        // Bitwise
        BitAnd,
        BitOr,
        BitXor,
        BitNot,
        BitShiftLeft,
        BitShiftRight,

        // Jumps
        JumpAlways,
        JumpFalse,
        JumpTrue,

        // Internal functions
        FuncAbs,
        FuncIIF,
        FuncMax,
        FuncMin,
        FuncSqrt,
        FuncPower,
        FuncRound,
        FuncFloor,
        FuncCeil,
        FuncCos,
        FuncCosD,
        FuncSin,
        FuncSinD,
        FuncTan,
        FuncTanD,
        FuncAcos,
        FuncAcosD,
        FuncASin,
        FuncASinD,
        FuncAtan,
        FuncAtanD,
        FuncNumberFormat,
        FuncAvg,

        // The end
        EndCode

    };
}
