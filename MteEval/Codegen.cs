/**********************************************************************************
 '*
 '* Codegen.cs - Parser and code generator
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
using System.Globalization;
using System.Collections.Generic;
using System.Text;
using MteUtility;
using System.Text.RegularExpressions;

namespace MteEval
{
    //----------------------------------------------------------------------- Codegen()
    //
    class Codegen
    {

        private static Codeblock codeBlock;
        private static List<int> byteCode;
        private static List<double> constData;
        private static List<bool> isPcode;          // Needed for peephole optimizatin
        private static int codeIndex = 0;           // Index in byte code where next instruction will be stored

        // Parameter and evel expressions
        private static string exprParam;
        private static string exprEval;

        // Putback 
        private static int putbackCount;
        private static int putbackIndex;

        // Jump table
        private static MteJump[] jumpTable = new MteJump[20];
        private static int jumpCount = 0;

        // Labels
        private static int[] labelTargets = new int[20];
        private static int labelIndex = 0;

        // Parameter names
        private static string[] parameters = new string[20];
        private static int parameterCount = 0;

        // Parens
        private static int parenCount = 0;

        // Compiling iif
        private static int iifCount = 0;

        // Token list and navigation
        public static List<string> tokenList;
        private static MteToken token = new MteToken(TokenType.None, "");
        private static int tokenIndex = 0;
        private static int tokenEndIndex = 0;

        // Expression Parsing
        private const string CodeblockMatch = "(\\{)?(\\|)?([^|\\}]*)(\\|)?([^}]*)(\\})?";
        // private const string CodeblockMatch = @"(\{)?(\|)?([^|\}]*)(\|)?([^}]*)(\})?";   // As literal
        private const string TokenizerMatch = "\\(|\\)|>=|<=|<>|\\|\\||&&|!=|==|<<|>>|0x[.\\da-z]+|[&\\^|~]|[+><=*/\\-!%,]|[.\\d]+|\\b\\w+\\b";

        // Expression group
        private const int GroupExpectedCount = 6;

        // Abort used to unwind parser when error found
        private const bool Abort = false;
        private const bool Success = true;

        // Internal func lookup table
        public static Dictionary<string, InternalFunc> funcTableMap;
        public static bool isFuncTableLoaded = false;

        //---------------------------------------------------------- CompileCodeblock()
        //
        public static Error CompileCodeblock(Codeblock cb)
        {
            Error err = Error.None;

            // Store local reference to codeblock
            codeBlock = cb;

            // On first pass, code and constants stored in list
            byteCode = new List<int>();
            constData = new List<double>();

            // Create list to track location of pcode vs inline values
            isPcode = new List<bool>();

            // Reset code and index tables
            ResetCode();

            // Parse expression into components
            err = ExtractExpressions();

            // If no error, continue
            if (err == Error.None)
            {

                // Process codeblock parameters
                err = CompileParameters();

                // If no error, continue
                if (err == Error.None)
                {

                    // Store parameter count in the code
                    EmitCodeHeader(parameterCount);

                    // Compile expression
                    err = CompileExpression();
                }
            }

            // If error, delete the code
            if (err != Error.None)
            {
                byteCode.Clear();
                return (err);
            }

            //-------------------------------------------------------------
            // Convert code and constant storage from list to fixed arrays

            // convert constant data to array of double
            int size = constData.Count;
            codeBlock.code.ConstData = new double[size];
            for (int i = 0; i < size; i++)
            {
                codeBlock.code.ConstData[i] = constData[i];
            }

            // convert bytecode to array of ints
            size = byteCode.Count;
            codeBlock.code.ByteCode = new int[size];
            for (int i = 0; i < size; i++)
            {
                codeBlock.code.ByteCode[i] = byteCode[i];
            }

            return (err);
        }

        //-------------------------------------------------------- ExtractExpressions()
        //
        private static Error ExtractExpressions()
        {
            const int GroupOpenBracket = 1;
            const int GroupOpenPipe = 2;
            const int GroupParamExpr = 3;
            const int GroupClosePipe = 4;
            const int GroupEvalExpr = 5;
            const int GroupCloseBracket = 6;
            Error err = Error.None;

            // Trim and format
            string exprTrimmed = (codeBlock.codeblockText.Trim()).ToLower();

            // Parse expression into component parts
            Regex p = new Regex(CodeblockMatch);
            MatchCollection Matcher = p.Matches(exprTrimmed);
            Match matchParts = Matcher[0];

            // Get match count
            int groupcount = matchParts.Groups.Count;

            // If no matches, return here
            if (groupcount == 0)
            {
                return (SetError(Error.Syntax, ""));
            }

            // Mtelog.Console("Group count=" + data.Count + ", Value=" + match.Value);

            // Prepare for the worst
            StringBuilder errDetail = new StringBuilder();

            // Inspect the groups
            for (int i = 1; i < groupcount; i++)
            {
                // Get part
                Group group = matchParts.Groups[i];

                // Verify parts of expression
                switch (i)
                {
                    case GroupOpenBracket:
                        if (!group.Value.Equals("{"))
                        {
                            err = Error.MissingBracket;
                        }
                        break;
                    case GroupOpenPipe:
                        if (!group.Value.Equals("|"))
                        {
                            err = Error.MissingPipe;
                        }
                        break;
                    case GroupParamExpr:
                        err = Error.None;
                        break;
                    case GroupClosePipe:
                        if (!group.Value.Equals("|"))
                        {
                            err = Error.MissingPipe;
                        }
                        break;
                    case GroupEvalExpr:
                        if (group.Value.Length == 0)
                        {
                            err = Error.MissingExpr;
                        }
                        break;
                    case GroupCloseBracket:
                        if (!group.Value.Equals("}"))
                        {
                            err = Error.MissingExpr;
                        }
                        break;
                }

                // if error, complete detail and return here
                if (err != Error.None)
                {
                    // Append error code
                    errDetail.Append("<e")
                             .Append(err.ErrCode.ToString())
                             .Append(">");

                    // Set error and return
                    return (SetError(err, errDetail.ToString()));
                }

                // Build detail string
                errDetail.Append(group.Value);

                // Mtelog.Console(i.ToString() + ") index=" + group.Index + ", Group.Value=" + group.Value);
            }

            // Store parameter and main expression
            exprParam = matchParts.Groups[GroupParamExpr].Value;
            exprEval = matchParts.Groups[GroupEvalExpr].Value;

            return (err);
        }

        //--------------------------------------------------------- CompileParameters()
        //
        private static Error CompileParameters()
        {
            Error err = Error.None;
            bool finished = false;
            int commacount = 0;

            // Reset parameter count
            parameterCount = 0;

            // Tokenize parameter experssion
            tokenList = TokenizeExpr(exprParam);

            // Build table of parameter names
            do
            {
                // Get parameter
                GetToken();

                switch (token.type)
                {
                    case TokenType.Delimiter:

                        // Not a comma?
                        if (!token.text.Equals(","))
                        {
                            return SetError(Error.MissingComma);
                        }

                        // Missed argument?
                        if (commacount > 0)
                        {
                            return SetError(Error.MissingParam);
                        }

                        // Bump comma count
                        commacount++;
                        break;

                    case TokenType.Identifier:

                        // Reserved word?
                        if (token.text.Equals("ce") || token.text.Equals("cpi"))
                        {
                            return (SetError(Error.ReservedWord, token.text));
                        }

                        // Store variable name
                        parameters[parameterCount] = token.text;

                        // Increment parameter count
                        parameterCount++;

                        // ToDo - Generate error if more parameters than memory

                        // Reset comma count
                        commacount = 0;
                        break;

                    case TokenType.Finished:
                        if (commacount > 0)
                        {
                            return SetError(Error.MissingParam);
                        }

                        finished = true;
                        break;

                    default:
                        return SetError(Error.MissingParam);
                }


            } while (!finished);

            return (err);
        }

        //--------------------------------------------------------- CompileExpression()
        //
        private static Error CompileExpression()
        {
            bool finished = false;
            bool success = true;

            // Tokenenize main expression
            tokenList = TokenizeExpr(exprEval);

            while (!finished)
            {

                // Run parser and generate code
                success = EvalExpression();
                if (success == Abort)
                    break;

                // Check for completeion or unexpected error
                switch (token.type)
                {
                    case TokenType.None:
                        SetError(Error.Other, "Token type none.");
                        finished = true;
                        break;
                    case TokenType.Unknown:
                        SetError(Error.Other, "Unknown token.");
                        finished = true;
                        break;
                    case TokenType.Finished:
                        DoEndCode();
                        FixupJumps();
                        finished = true;
                        break;
                }
            }

            return (codeBlock.LastError);
        }

        //------------------------------------------------------------ EvalExpression()
        //
        private static bool EvalExpression()
        {
            bool success = false;

            // Get this party started!
            GetToken();

            // Evaluate assignment
            success = EvalAssignment();
            if (success == Abort) return (Abort);

            // Return token to the input stream.  This is needed due to the "look ahead"
            // nature of the parser
            success = PutBack();
            if (success == Abort) return (Abort);

            return (Success);
        }

        //------------------------------------------------------------ EvalAssignment()
        //
        private static bool EvalAssignment()
        {
            bool success = false;
            int varindex;

            // Possible variable?
            if (token.type == TokenType.Identifier)
            {

                // Look for it
                varindex = FindParameter(token.text);
                if (varindex >= 0)
                {

                    // Save token
                    MteToken saveToken = new MteToken(token.type, String.Copy(token.text));

                    // Assignment operator?
                    GetToken();
                    if (token.text.Equals("="))
                    {
                        // If compiling iif() and attempting assignment in conditional
                        if ( iifCount > 0 && codeBlock.AllowConditionalAssignment==false )
                        {
                            SetError(Error.ConditionalAssignment);
                            return (Abort);
                        }

                        // Could be a series of assignments
                        GetToken();
                        success = EvalAssignment();
                        if (success == Abort) return (Abort);

                        // Store in memory
                        DoStoreVariable(varindex);

                        // Done
                        return (Success);

                    }

                    // Not an assignment?
                    else
                    {

                        // Put back in stream
                        PutBack();

                        // Restore token
                        token.text = saveToken.text;
                        token.type = saveToken.type;

                    }
                }
            }

            // Next precedence
            success = EvalLogicalOr();
            if (success == Abort) return (Abort);

            // Check current token for illegal assignment
            if (token.text.Equals("="))
            {
                SyntaxError(Error.IllegalAssignment);
                return (Abort);
            }

            return (Success);
        }

     /**------------------------------------------------------------ EvalLogicalOr()
     */
        private static bool EvalLogicalOr()
        {
            string oper;
            int dropout;
            bool success = false;

            // Next precedence (left operand)
            success = EvalLogicalAnd();
            if (success == Abort) return (Abort);

            // Save operator on local stack
            oper = token.text;

            // Process Or
            while ( oper.Equals("||") ) {

                // Gen label for dropout
                dropout = NewLabel();

                // If true skip right operand
                BranchTrue(dropout);

                // Push, get, and do next level
                Push();
                GetToken();

                // Get right operand
                success = EvalLogicalAnd();
                if (success == Abort) return (Abort);

                // Gen code
                DoLogicalOr();

                // Post dropout label
                PostLabel(dropout);

                // Update oper
                oper = token.text;

            }

            return (Success);
        }

        /**----------------------------------------------------------- EvalLogicalAnd()
         */
        private static bool EvalLogicalAnd()
        {
            string oper;
            int dropout;
            bool success = false;

            // Next higher precedence
            success = EvalBitwiseAndOrXor();
            if (success == Abort) return (Abort);

            // Save operator on local stack
            oper = token.text;

            // Process And
            while ( oper.Equals("&&") ) {

                // Gen label for dropout
                dropout = NewLabel();

                // If false skip right operand
                BranchFalse(dropout);

                // Push, get, and do next level
                Push();
                GetToken();

                // Get right operand
                success = EvalBitwiseAndOrXor();
                if (success == Abort) return (Abort);

                // Gen code
                DoLogicalAnd();

                // Post dropout label
                PostLabel(dropout);

                // Update operator
                oper = token.text;

            }
            return (Success);
        }

        /**------------------------------------------------------ evalBitwiseAndOrXor()
         */
        private static bool EvalBitwiseAndOrXor()
        {
            string oper;
            bool success = true;
            Match m;

            success = EvalRelational();
            if (success == Abort) return (Abort);

            // Store operator 
            oper = token.text;

            m = TokenPattern.bitOper2.Match(oper);
            while (m.Success)
            {
                // Push on stack and continue
                Push();
                GetToken();

                // Get right operand
                success = EvalRelational();
                if (success == Abort) return (Abort);

                // Generate code
                switch ( oper ) {
                case "&":
                    DoBitwiseAnd();
                    break;
                case "|":
                    DoBitwiseOr();
                    break;
                case "^":
                    DoBitwiseXor();
                    break;
                }

                // Update operator as token may have changed
                oper = token.text;

                // Refresh
                m = TokenPattern.bitOper2.Match( oper );
            }

            return (Success);
        }

        /**----------------------------------------------------------- EvalRelational()
         */
        private static bool EvalRelational()
        {
            string oper;
            bool success = true;
            Match m;

            // Nex precedence
            success = EvalBitShift();
            if (success == Abort) return (Abort);

            // Store operator
            oper = token.text;

            m = TokenPattern.relOper2.Match(oper);
            if (m.Success)
            {
                // Push on stack and continue
                Push();
                GetToken();

                // Get right operand
                success = EvalBitShift();
                if (success == Abort) return (Abort);

                // Generate code
                switch (oper) {
                case "<":
                    DoLess();
                    break;
                case "<=":
                    DoLessEqual();
                    break;
                case ">":
                    DoGreater();
                    break;
                case ">=":
                    DoGreaterEqual();
                    break;
                case "==":
                    DoEqual();
                    break;
                case "!=":
                    DoNotEqual();
                    break;
                }
            }
            return (Success);
        }

        /**------------------------------------------------------------- evalBitShift()
         */
        private static bool EvalBitShift()
        {
            string oper;
            bool success = true;
            Match m;

            success = EvalAddSub();
            if (success == Abort) return (Abort);

            // Store oper on local stack
            oper = token.text;

            m = TokenPattern.bitOper.Match( oper );
            if (m.Success)
            {

                // Push on stack and continue
                Push();
                GetToken();

                // Get right operand
                success = EvalAddSub();
                if (success == Abort) return (Abort);

                // Generate code
                switch ( oper ) {
                case "<<":
                    DoBitShiftLeft();
                    break;
                case ">>":
                    DoBitShiftRight();
                    break;
                }
            }

            return (Success);
        }

        /**--------------------------------------------------------------- evalAddSub()
         */
        private static bool EvalAddSub()
        {
            string oper;
            bool success = true;
            Match m;

            success = EvalFactor();
            if (success == Abort) return (Abort);

            // Store operator
            oper = token.text;

            m = TokenPattern.isAddSub.Match( oper );
            while (m.Success)
            {

                // Push on stack and continue
                Push();
                GetToken();

                success = EvalFactor();
                if (success == Abort) return (Abort);

                // Generate code
                switch ( oper ) {
                case "-":
                    DoSubtract();
                    break;
                case "+":
                    DoAdd();
                    break;
                }

                // Update oper as token may have changed
                oper = token.text;

                // Refresh matcher
                m = TokenPattern.isAddSub.Match( oper );
            }

            return (Success);
        }

        /**--------------------------------------------------------------- evalFactor()
         */
        private static bool EvalFactor()
        {
            string oper;
            bool success = true;
            Match m;

            success = EvalUnary();
            if (success == Abort) return (Abort);

            // Store operator
            oper = token.text;

            // While multiply, divide, or modulo
            m = TokenPattern.isFactor.Match( oper );
            while (m.Success)
            {

                // Push on stack and continue
                Push();
                GetToken();

                success = EvalUnary();
                if (success == Abort) return (Abort);

                // Generate code
                switch ( oper ) {
                case "*":
                    DoMultiply();
                    break;
                case "/":
                    DoDivide();
                    break;
                case "%":
                    DoModulo();
                    break;
                }

                // Update oper as token may have changed
                oper = token.text;

                // Refresh matcher
                m = TokenPattern.isFactor.Match( oper );
            }

            return (Success);
        }

        /**---------------------------------------------------------------- EvalUnary()
         */
        private static bool EvalUnary()
        {
            string oper = "";
            bool success = true;
            Match m;

            // Is this a unary oper?
            m = TokenPattern.isUnary.Match(token.text);
            if (m.Success)
            {
                // Save operator and continue
                oper = token.text;
                GetToken();
                success = EvalUnary();
                if (success == Abort) return (Abort);

            }
            else
            {
                // Next higher precedence
                success = EvalParen();
                if (success == Abort) return (Abort);
            }

            // Which one?
            switch ( oper ) {
            case "-":
                DoNegate();
                break;
            case "!":
                DoLogicalNot();
                break;
            case "~":
                DoBitNot();
                break;
            }

            return (Success);
        }

        /**---------------------------------------------------------------- evalParen()
         */
        private static bool EvalParen()
        {
            bool success = false;
            bool finished = false;

            // Is this an open parenthesis?
            if (token.text.Equals("("))
            {

                // Count open parenthesis
                parenCount++;

                // get token
                GetToken();

                // Eval sub expression
                while (!finished)
                {

                    success = EvalAssignment();
                    if (success == Abort) return (Abort);

                    // If comma, then continue
                    if (token.text.Equals(","))
                    {
                        GetToken();
                    }
                    else
                    {
                        finished = true;
                    }
                }

                // Expecting a closed parenthesis here
                if (!token.text.Equals(")"))
                {
                    SyntaxError(Error.MissingParen);
                    return (Abort);
                }

                // Reduce count
                parenCount--;

                // Get next token
                GetToken();
            }
            else
            {

                success = EvalAtom();
                if (success == Abort) return (Abort);
            }
            return (Success);
        }

        /**----------------------------------------------------------------- evalAtom()
         */
        private static bool EvalAtom()
        {
            int parameterindex = 0;
            bool success;
            double value;
            InternalFunc funcInfo;

            switch (token.type)
            {
                case TokenType.Identifier:

                    // Find internal function
                    funcInfo = FindInternalFunc(token.text);

                    // If function found
                    if (funcInfo.pcode > 0)
                    {

                        // IIF is special
                        if (funcInfo.pcode == Pcode.FuncIIF)
                        {

                            success = DoIIF();
                            if (success == Abort) return (Abort);

                        }
                        // Call internal function
                        else
                        {
                            // Output instruction to call internal func
                            success = DoCallInternalFunc(funcInfo);
                            if (success == Abort) return (Abort);
                        }
                    }
                    // Either built-in constant or parameter
                    else
                    {

                        switch (token.text)
                        {
                            case "ce":
                                DoLoadNumber(Math.E);
                                break;
                            case "cpi":
                                DoLoadNumber(Math.PI);
                                break;
                            default:
                                parameterindex = FindParameter(token.text);
                                if (parameterindex >= 0)
                                {
                                    DoLoadVariable(parameterindex);
                                }
                                else
                                {
                                    SyntaxError(Error.NotAVar);
                                    return (Abort);
                                }
                                break;
                        }
                    }

                    // Get next token
                    GetToken();
                    break;

                case TokenType.Number:

                    // Convert string to double
                    if (!double.TryParse(token.text, out value))
                        value = 0d;
                    
                    // Output instruction to load number
                    DoLoadNumber(value);

                    // Get next token
                    GetToken();
                    break;

                case TokenType.Delimiter:

                    if (token.text.Equals(")") && parenCount == 0)
                    {
                        SyntaxError(Error.UnbalancedParens);
                        return (Abort);
                    }
                    return (Success);

                case TokenType.Finished:

                    return (Success);

                case TokenType.HexNumber:

                    // Convert hex to decimal
                    int intvalue = 0;
                    bool parsed = int.TryParse(token.text.Substring(2), NumberStyles.HexNumber, null, out intvalue);
                    
                    // Ouput instruction to load number
                    DoLoadNumber((double) intvalue);

                    // Get next token
                    GetToken();
                    break;

                default:

                    SyntaxError(Error.Other);
                    return (Abort);
            }
            return (Success);
        }

        //------------------------------------------------------------------- GetArgs()
        //
        private static bool GetArgs(int expectedArgCount)
        {
            bool success, finished;
            int argcount = 0;

            // Get next token
            GetToken();

            // If not opening paren
            if (!token.text.Equals("("))
            {
                SyntaxError(Error.MissingParen);
                return (Abort);
            }

            // Get next token
            GetToken();

            // If closing paren, no args. This is ok.
            if (token.text.Equals(")"))
            {
                return (Success);
            }

            // Return token to stream
            PutBack();

            finished = false;
            while (!finished)
            {

                // Parse arguments
                success = EvalExpression();
                if (success == Abort) return (Abort);

                // Count args. Too many?
                argcount++;
                if (argcount > expectedArgCount)
                {
                    SyntaxError(Error.TooManyArgs);
                    return (Abort);
                }

                // Push value on stack and get next token
                Push();
                GetToken();

                // If no comma, we've consumed all the arguments
                if (!token.text.Equals(","))
                {
                    finished = true;
                }
            }

            // Short arguments?
            if (argcount < expectedArgCount)
            {
                SyntaxError(Error.InsufficientArgs);
                return (Abort);
            }

            // Should be closing paren here
            if (!token.text.Equals(")"))
            {
                SyntaxError(Error.MissingParen);
                return (Abort);
            }

            return (Success);
        }

        //------------------------------------------------------------------- PutBack()
        //
        private static bool PutBack()
        {

            // Safety check to prevent parser from hanging on bug
            if (putbackIndex == tokenIndex)
            {
                putbackCount++;
                if (putbackCount > 5)
                {
                    SyntaxError(Error.Putback);
                    return (Abort);
                }
            }
            else
            {
                putbackIndex = tokenIndex;
                putbackCount = 0;
            }

            // Decrement token index
            tokenIndex--;

            return (Success);
        }

        //-------------------------------------------------------------- TokenizeExpr()
        //
        private static List<string> TokenizeExpr(string exprTarget)
        {

            var exprTokens = new List<string>();

            Regex p = new Regex(TokenizerMatch);
            MatchCollection matchExpr = p.Matches(exprTarget);

            // Load list with tokens
            foreach (Match match in matchExpr)
            {
                exprTokens.Add(match.Value);
            }

            // Init list navigation
            tokenIndex = -1;
            tokenEndIndex = exprTokens.Count - 1;

            return (exprTokens);
        }

        //------------------------------------------------------------------ GetToken()
        //
        private static TokenType GetToken()
        {
            string tokenText;
            Match matchToken;

            token.type = TokenType.None;
            token.text = "";  // NULL_TOKEN;

            // Advance index
            tokenIndex++;

            // If index is past the end, no more tokens
            if (tokenIndex > tokenEndIndex)
            {
                token.type = TokenType.Finished;
                return (token.type);
            }

            // Get token
            tokenText = tokenList[tokenIndex];

            // Relational oper?
            matchToken = TokenPattern.relOper.Match(tokenText);
            if (matchToken.Success)
            {

                token.text = tokenText;
                token.type = TokenType.Delimiter;
                return (token.type);
            }

            // Bit shift?
            matchToken = TokenPattern.bitOper.Match(tokenText);
            if (matchToken.Success)
            {

                token.text = tokenText;
                token.type = TokenType.Delimiter;
                return (token.type);
            }

            // General delimiter?
            matchToken = TokenPattern.genOper.Match(tokenText);
            if (matchToken.Success)
            {

                token.text = tokenText;
                token.type = TokenType.Delimiter;
                return (token.type);
            }

            // Is hex number?
            matchToken = TokenPattern.hexNumber.Match(tokenText);
            if (matchToken.Success)
            {

                token.text = tokenText;
                token.type = TokenType.HexNumber;
                return (token.type);
            }

            // Is number?
            matchToken = TokenPattern.floatNumber.Match(tokenText);
            if (matchToken.Success)
            {

                token.text = tokenText;
                token.type = TokenType.Number;
                return (token.type);
            }

            // Is text?
            matchToken = TokenPattern.isText.Match(tokenText);
            if (matchToken.Success)
            {

                token.text = tokenText;
                token.type = TokenType.Identifier;
                return (token.type);

            }
            else
            {
                SyntaxError(Error.Other);
                token.text = tokenText;
                token.type = TokenType.Unknown;
            }

            return (token.type);
        }

        //=======================
        // Code generator
        //=======================


        /**--------------------------------------------------------------------- push()
             */
        private static void Push()
        {
            // Mtelog.debug( "push");
            DoPush();
        }

        /**------------------------------------------------------------------- doPush()
         */
        private static void DoPush()
        {

            // Mtelog.debug( "doPush");

            // If optimizer enabled, attempt "peephole" optimization of push
            if (codeBlock.OptimizerEnabled)
            {

                // "Peep" at previous instruction
                int peepindex = codeIndex - 2;
                int peep = (peepindex > 0 && isPcode[peepindex]) ? GetShortCode(peepindex) : (int)Pcode.None;

                // Optimize loadvar and loadconst
                switch ((Pcode)peep)
                {
                    case Pcode.LoadVar:
                        EmitShortCode(peepindex, Pcode.PushVar);   // Push var directly on stack
                        break;
                    case Pcode.LoadConst:
                        EmitShortCode(peepindex, Pcode.PushConst); // Push const directly on stack
                        break;
                    default:
                        EmitShortCode(Pcode.Push);
                        break;
                }
            }
            else
            {
                EmitShortCode(Pcode.Push);
            }
        }

        /**------------------------------------------------------------ getShortCode()
         */
        private static int GetShortCode(int index)
        {
            return (byteCode[index]);
        }

        //------------------------------------------------------------- EmitCodeHeader()
        //
        private static void EmitCodeHeader(int paramcount)
        {
            byteCode.Add(paramcount);
            isPcode.Add(false);
            codeIndex++;
        }

        /**------------------------------------------------------------ EmitShortCode()
         */
        private static void EmitShortCode(Pcode pcode)
        {

            // Add instruction
            byteCode.Add((int)pcode);
            isPcode.Add(true);
            codeIndex++;

        }
        /**------------------------------------------------------------ EmitShortCode()
         */
        private static void EmitShortCode(int index, Pcode pcode)
        {
            // Change instruction
            byteCode[index] = (int)pcode;
        }

        /**------------------------------------------------------------- EmitLongCode()
         */
        private static void EmitLongCode(Pcode pcode, int value)
        {
            // Add pcode
            byteCode.Add((int)pcode);
            isPcode.Add(true);
            codeIndex++;

            // Add value inline
            byteCode.Add(value);
            isPcode.Add(false);
            codeIndex++;
        }

        /**-------------------------------------------------------------- AddConstant()
         */
        private static int AddConstant(double value)
        {

            // Add value to constants list
            constData.Add(value);

            return (constData.Count - 1);
        }

        /**------------------------------------------------------- DoCallInternalFunc()
         */
        private static bool DoCallInternalFunc(InternalFunc funcInfo)
        {
            bool success = false;

            // Mtelog.debug("doCallInternalFunc");

            // Get arguments and push on stack
            success = GetArgs(funcInfo.argcount);
            if (success == Abort) return (Abort);

            // call func
            EmitShortCode(funcInfo.pcode);

            return (Success);
        }

        /**-------------------------------------------------------------------- doIIF()
         *
         * After GetToken returns the TokenIndex is here
         * ----------------------------------------------
         *  1. IIF( ..., ..., ...)
         *        ^
         *  2. IIF( ..., ..., ...)
         *             ^
         *  3. IIF( ..., ..., ...)
         *                  ^
         *  4. IIF( ..., ..., ...)
         *                       ^
         */
        private static bool DoIIF()
        {
            bool success = false;
            int iffalse, endofif;

            // Mtelog.debug( "DoIIF");

            // 1. Get next token
            GetToken();

            // If not parethesis
            if (!token.text.Equals("("))
            {
                SyntaxError(Error.MissingParen);
                return (Abort);
            }

            // Compiling iif() conditional expression
            iifCount++;

            // Eval conditional expression
            success = EvalExpression();
            if (success == Abort) return (Abort);

            // Done compiling
            iifCount--;

            // Get label
            iffalse = NewLabel();

            // 2. Get next token
            GetToken();

            // Expect comma here
            if (!token.text.Equals(","))
            {
                SyntaxError(Error.MissingComma);
                return (Abort);
            }

            // Post false branch
            BranchFalse(iffalse);

            // Get 'then' condition
            success = EvalExpression();
            if (success == Abort) return (Abort);

            // 3. Get next token
            GetToken();

            // Expect comma here
            if (!token.text.Equals(","))
            {
                SyntaxError(Error.MissingComma);
                return (Abort);
            }

            // Post label for "else"
            endofif = NewLabel();
            Branch(endofif);
            PostLabel(iffalse);

            // Compile "else" condition
            success = EvalExpression();
            if (success == Abort) return (Abort);

            // 4. Get next token
            GetToken();

            // If not closing parethesis
            if (!token.text.Equals(")"))
            {
                SyntaxError(Error.MissingParen);
                return (Abort);
            }

            // End of IIF
            PostLabel(endofif);

            return (Success);
        }

        /**------------------------------------------------------------- doBitwiseAnd()
         */
        private static void DoBitwiseAnd()
        {
            // Mtelog.debug("doBitwiseAnd()");
            EmitShortCode(Pcode.BitAnd);
        }

        /**-------------------------------------------------------------- doBitwiseOr()
         */
        private static void DoBitwiseOr()
        {
            // Mtelog.debug("doBitwiseOr()");
            EmitShortCode(Pcode.BitOr);
        }

        /**----------------------------------------------------------- doBitwiseXor()
         */
        private static void DoBitwiseXor()
        {
            // Mtelog.debug("doBitwiseXor()");
            EmitShortCode(Pcode.BitXor);
        }

        /**--------------------------------------------------------- doBitShiftLeft()
         */
        private static void DoBitShiftLeft()
        {
            // Mtelog.debug("doBitShiftLeft()");
            EmitShortCode(Pcode.BitShiftLeft);
        }

        /**--------------------------------------------------------- doBitShiftRight()
         */
        private static void DoBitShiftRight()
        {
            // Mtelog.debug("doBitShiftRight()");
            EmitShortCode(Pcode.BitShiftRight);
        }

        /**----------------------------------------------------------------- doBitNot()
         */
        private static void DoBitNot()
        {
            // Mtelog.debug("doBitNot()");
            EmitShortCode(Pcode.BitNot);
        }

        /**------------------------------------------------------------- doLoadNumber()
         */
        private static void DoLoadNumber(double value)
        {

            // Mtelog.debug("doLoadNumber()");

            // Add constant to table
            int constindex = AddConstant(value);

            EmitLongCode(Pcode.LoadConst, constindex);
        }

        /**----------------------------------------------------------- doLoadVariable()
         */
        private static void DoLoadVariable(int index)
        {
            // Mtelog.debug("doLoadVariable()");
            EmitLongCode(Pcode.LoadVar, index);
        }

        /**---------------------------------------------------------- doStoreVariable()
         */
        private static void DoStoreVariable(int index)
        {

            // Mtelog.debug("doStoreVariable()");
            EmitLongCode(Pcode.StoreVar, index);
        }

        /**---------------------------------------------------------------- doMultiply()
         */
        private static void DoMultiply()
        {

            // Mtelog.debug("doMultiply()");
            EmitShortCode(Pcode.Multiply);
        }

        /**----------------------------------------------------------------- doDivide()
         */
        private static void DoDivide()
        {

            // Mtelog.debug("doDivide()");
            EmitShortCode(Pcode.Divide);
        }

        /**----------------------------------------------------------------- doModulo()
         */
        private static void DoModulo()
        {
            // Mtelog.debug("DoModulo()");
            EmitShortCode(Pcode.Modulo);
        }
        /**----------------------------------------------------------------- doNegate()
         */
        private static void DoNegate()
        {
            // Mtelog.debug("DoNegate()");
            EmitShortCode(Pcode.Neg);
        }

        /**-------------------------------------------------------------- doLogicalNot()
         */
        private static void DoLogicalNot()
        {
            // Mtelog.debug("DoLogicalNot()");
            EmitShortCode(Pcode.LogicalNot);
        }

        /**----------------------------------------------------------------- doSubtract()
         */
        private static void DoSubtract()
        {
            // Mtelog.debug("DoSubtract");
            EmitShortCode(Pcode.Subtract);
        }

        /**--------------------------------------------------------------------- doAdd()
         */
        private static void DoAdd()
        {
            // Mtelog.debug("DoAdd()");
            EmitShortCode(Pcode.Add);
        }

        /**-------------------------------------------------------------------- doLess()
         */
        private static void DoLess()
        {
            // Mtelog.debug("DoLess()");
            EmitShortCode(Pcode.LessThan);
        }

        /**--------------------------------------------------------------- doLessEqual()
         */
        private static void DoLessEqual()
        {
            // Mtelog.debug("DoLessEqual()");
            EmitShortCode(Pcode.LessEqual);
        }

        /**----------------------------------------------------------------- doBxitNot()
         */
        private static void DoGreater()
        {
            // Mtelog.debug("DoGreater()");
            EmitShortCode(Pcode.GreaterThan);
        }

        /**----------------------------------------------------------------- doBxitNot()
         */
        private static void DoGreaterEqual()
        {
            // Mtelog.debug("DoGreaterEqual()");
            EmitShortCode(Pcode.GreaterEqual);
        }

        /**----------------------------------------------------------------- doBxitNot()
         */
        private static void DoEqual()
        {
            // Mtelog.debug("DoEqual()");
            EmitShortCode(Pcode.Equal);
        }

        /**----------------------------------------------------------------- doBxitNot()
         */
        private static void DoNotEqual()
        {
            // Mtelog.debug("doNotEqual()");
            EmitShortCode(Pcode.NotEqual);
        }

        /**----------------------------------------------------------------- doEndCode()
         */
        private static void DoEndCode()
        {
            // Mtelog.debug("doEndCode()");
            EmitShortCode(Pcode.EndCode);
        }

        /**----------------------------------------------------------------- newLabel()
         */
        private static int NewLabel()
        {
            int nextLabel = labelIndex;
            labelIndex++;
            return (nextLabel);
        }

        /**------------------------------------------------------------------ addJump()
         */
        private static void AddJump(int jumptarget)
        {
            // Create new jump
            MteJump jump = new MteJump();
            jump.codeIndex = codeIndex;
            jump.labelIndex = jumptarget;

            // Save the location of the jump instruction in the code
            jumpTable[jumpCount] = jump;

            // Bump count
            jumpCount++;
        }

        /**--------------------------------------------------------------- fixupJumps()
         */
        private static void FixupJumps()
        {
            int codeindex, jumpindex, jumpoffset;

            // Any jumps to fixup?
            if (jumpCount > 0)
            {

                // Fix jumps
                for (int i = 0; i < jumpCount; i++)
                {
                    // This is the location of the jump pcode
                    codeindex = jumpTable[i].codeIndex;

                    // This is the index where we want to jump to
                    jumpindex = labelTargets[jumpTable[i].labelIndex];

                    // Calculate offset
                    jumpoffset = (jumpindex - codeindex) - 1;

                    // Replace inline value with corrected offset
                    // EmitShortCode( codeindex + 1, jumpoffset );
                    byteCode[codeindex + 1] = jumpoffset;
                }
            }

            // Reset jumps and label counts
            jumpCount = 0;
            labelIndex = 0;
        }

        /**---------------------------------------------------------------- postLabel()
         */
        private static void PostLabel(int labelIndex)
        {

            // This is the location (codeindex) where this label should jump to
            labelTargets[labelIndex] = codeIndex;

        }

        /**------------------------------------------------------------------- branch()
         */
        private static void Branch(int labelIndex)
        {

            // Mtelog.debug( "branch");

            AddJump(labelIndex);
            EmitLongCode(Pcode.JumpAlways, 0);
        }

        /**-------------------------------------------------------------- branchFalse()
         */
        private static void BranchFalse(int labelIndex)
        {

            // Mtelog.debug( "branchFalse");

            AddJump(labelIndex);
            EmitLongCode(Pcode.JumpFalse, 0);
        }
        
        /**--------------------------------------------------------------- branchTrue()
         */
        private static void BranchTrue(int labelIndex)
        {

            // Mtelog.debug( "branchTrue");

            AddJump(labelIndex);
            EmitLongCode(Pcode.JumpTrue, 0);
        }

        /**-------------------------------------------------------------- doLogicalOr()
         */
        private static void DoLogicalOr()
        {
            // Mtelog.debug( "doLogicalOr()" );
            EmitShortCode(Pcode.LogicalOr);
        }

        /**------------------------------------------------------------- doLogicalAnd()
         */
        private static void DoLogicalAnd()
        {
            // Mtelog.debug( "doLogicalAnd()" );
            EmitShortCode(Pcode.LogicalAnd);
        }

        //-------------------------------------------------------------- FindParameter()
        //
        private static int FindParameter(string nameTarget)
        {

            // Any parameters?
            if (parameterCount == 0) return (-1);

            // Find parameter in list
            for (int i = 0; i < parameterCount; i++)
            {
                if (parameters[i].Equals(nameTarget))
                    return (i);
            }

            return (-1);
        }

        //----------------------------------------------------------------- ResetCode()
        //
        private static void ResetCode()
        {
            // ToDo - Does this include all to reset?

            // Init code index
            codeIndex = 0;  // First item in bytecode is the codeblock parameter count
                            // Pcodes start at byteCode[ 1 ]

            // Reset counters and indexes
            jumpCount = 0;
            labelIndex = 0;
            parameterCount = 0;
            parenCount = 0;
            putbackCount = 0;
            putbackIndex = 0;

            // reset expressions
            exprParam = "";
            exprEval = "";

            // iif() counter
            iifCount = 0;


        }

        //------------------------------------------------------------- LoadFuncTable()
        //
        private static void LoadFuncTable()
        {

            // Create dictionary and load it
            funcTableMap = new Dictionary<string, InternalFunc>();
            foreach (var func in InternalFuncTable.Values)
            {
                funcTableMap.Add(func.name, func);
            }

            // Mark table loaded
            isFuncTableLoaded = true;
        }

        //---------------------------------------------------------- FindInternalFunc()
        //
        private static InternalFunc FindInternalFunc(string name)
        {

            // Load table as required
            if (!isFuncTableLoaded)
                LoadFuncTable();

            // Search for func
            if (!funcTableMap.TryGetValue(name, out InternalFunc funcInfo))
            {
                funcInfo = InternalFuncTable.FuncNone;
            }

            return (funcInfo);
        }

        //=======================
        // Error
        //=======================

        //------------------------------------------------------------------- SetError()
        //
        // Set error with detail
        // 
        private static Error SetError(Error err, string errDetail)
        {
            // Store last error and detail
            codeBlock.LastError = err;
            codeBlock.ErrDetail = errDetail;
            return (err);
        }

        //------------------------------------------------------------------- SetError()
        //
        private static Error SetError(Error err)
        {
            string errDetail = BuildErrorDetail(err);
            return (SetError(err, errDetail));
        }

        //----------------------------------------------------------- BuildErrorDetail()
        //
        private static string BuildErrorDetail(Error err)
        {

            // No tokens?
            if (tokenIndex < 0 || tokenList.Count == 0) return ("");

            // Build detail from tokens
            StringBuilder sb = new StringBuilder();
            int k = (tokenIndex <= tokenEndIndex) ? tokenIndex : tokenEndIndex;
            for (int i = 0; i <= k; i++)
            {
                sb.Append(tokenList[i]);
            }

            // Add error code
            sb.Append("<e").Append(err.ErrCode.ToString()).Append(">");

            return (sb.ToString());
        }

        //---------------------------------------------------------------- SyntaxError()
        //
        private static void SyntaxError(Error err)
        {
            // Set error in codeblock with detail
            SetError(err);
        }

    }

    //---------------------------------------------------------------------- MteToken()
    //
    struct MteToken
    {
        public TokenType type;
        public string text;
        
        public MteToken(TokenType type, string text)
        {
            this.type = type;
            this.text = text;
        }
    }

    //----------------------------------------------------------------------- MteJump()
    //
    struct MteJump
    {
        public int codeIndex;
        public int labelIndex;
    }

    //------------------------------------------------------------------- TokenPatter()
    //
    class TokenPattern
    {
        public static Regex relOper = new Regex("<=|>=|==|<|>|!=|\\|\\||&&|&");
        public static Regex bitOper = new Regex("<<|>>");
        public static Regex genOper = new Regex("[+\\-*^/%(),!|~=]");
        public static Regex hexNumber = new Regex("0x[\\.\\da-z]+");
        public static Regex isText = new Regex("\\w+");
        public static Regex floatNumber = new Regex("[-+]?\\b[0-9]*\\.?[0-9]+\\b");

        // -----
        //public static Regex bitOper2 = new Regex("&|\\^|\\|");

        // New
        public static Regex bitOper2 = new Regex(@"(?<!&)&(?!&)|(?<!\|)\|(?!\|)");
        
        // public static Regex relOper2 = new Regex("<=|>=|==|(?<!:)<(?!<)|(?<!>)>(?!>)|!="); // ToDo. should relOper be updated to relOper2?

        // New
        public static Regex relOper2 = new Regex(@"<=|>=|==|(?<!<)<(?!<)|(?<!>)>(?!>)|!="); // ToDo. should relOper be updated to relOper2?

        public static Regex isAddSub = new Regex("[+\\-]");
        public static Regex isFactor = new Regex("[\\*/%]");
        public static Regex isUnary = new Regex("[+\\-!~]");
    }

    //----------------------------------------------------------------------- TokenType
    //
    enum TokenType
    {
        None = 0,
        Delimiter,
        Identifier,
        Number,
        Keyword,
        Temp,
        String,
        Block,
        Unknown,
        Finished,
        HexNumber
    };

    //----------------------------------------------------------------------- ExprGroup
    //
    enum ExprGroup
    {
        OpenBracket = 1,
        OpenPipe,
        ParamExpr,
        ClosePipe,
        EvalExpr,
        CloseBracket
    };
}