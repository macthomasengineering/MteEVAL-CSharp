/**********************************************************************************
 '*
 '* Error.cs - Error codes
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

namespace MteEval
{
    //------------------------------------------------------------------------- Error()
    //
    public class Error
    {

        public static readonly Error None = new Error(0, "None");
        public static readonly Error Syntax = new Error(1, "Syntax Error");
        public static readonly Error MissingBracket = new Error(2, "{} bracket not found");
        public static readonly Error MissingPipe = new Error(3, "|| pipe not found");
        public static readonly Error MissingParen = new Error(4, "Missing parenthesis");
        public static readonly Error MissingComma = new Error(5, "Missing comma");
        public static readonly Error MissingArg = new Error(6, "Missing argument");
        public static readonly Error NotAVar = new Error(7, "Unknown parameter");
        public static readonly Error MissingParam = new Error(8, "Missing parameter");
        public static readonly Error MissingExpr = new Error(9, "Missing expression");
        public static readonly Error ReservedWord = new Error(10, "Reserved word");
        public static readonly Error TooManyArgs = new Error(11, "Too many arguments");
        public static readonly Error UnbalancedParens = new Error(12, "Unbalanced parens");
        public static readonly Error Putback = new Error(13, "Internal parser error");
        public static readonly Error UnsupportedOper = new Error(14, "Unsupported operator");
        public static readonly Error NoCode = new Error(20, "No code to execute");
        public static readonly Error IllegalCode = new Error(21, "Illegal instruction");
        public static readonly Error InsufficientArgs = new Error(22, "Insufficient arguments");
        public static readonly Error StackOverflow = new Error(23, "Stack overflow");
        public static readonly Error DivideByZero = new Error(24, "Divide by zero");
        public static readonly Error ArgNotNumber = new Error(25, "Not a number");
        public static readonly Error IllegalAssignment = new Error(26, "Illegal Assignment");
        public static readonly Error ConditionalAssignment = new Error(27, "Assignment in conditional not permitted. To enable, set Codeblock.AllowConditionalAssignment=true");
        public static readonly Error StackMemoryLeak = new Error(28, "Stack Memory Leak");
        public static readonly Error Other = new Error(33, "Other error");

        public static IEnumerable<Error> Values
        {
            get
            {
                yield return None;
                yield return Syntax;
                yield return MissingBracket;
                yield return MissingPipe;
                yield return MissingParen;
                yield return MissingComma;
                yield return MissingArg;
                yield return NotAVar;
                yield return MissingParam;
                yield return MissingExpr;
                yield return ReservedWord;
                yield return TooManyArgs;
                yield return UnbalancedParens;
                yield return Putback;
                yield return UnsupportedOper;
                yield return NoCode;
                yield return IllegalCode;
                yield return InsufficientArgs;
                yield return StackOverflow;
                yield return DivideByZero;
                yield return ArgNotNumber;
                yield return IllegalAssignment;
                yield return ConditionalAssignment;
                yield return StackMemoryLeak;
                yield return Other;
            }
        }

        private readonly int errCode;
        private readonly string errDesc;

        public Error(int errCode, String errDesc)
        {
            this.errCode = errCode;
            this.errDesc = errDesc;
        }

        public int ErrCode { get { return errCode; } }
        public string ErrDesc {  get { return errDesc; } }

        public override string ToString()
        {
            return errDesc;
        }

    }

}
