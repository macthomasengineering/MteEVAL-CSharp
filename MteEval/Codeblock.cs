/**********************************************************************************
'*
'* Codeblock.cs - Compilable blocks of code
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
    //--------------------------------------------------------------------- Codeblock()
    //
    public class Codeblock
    {
        public Code code;
        public string codeblockText;
        public Error LastError { get; set; }
        public string ErrDetail { get; set; }
        public bool OptimizerEnabled { get; set; }
        public bool AllowConditionalAssignment { get; set; }
        public string Version { get { return VersionInfo.Text; } }

        //----------------------------------------------------------------- Codeblock()
        //
        public Codeblock()
        {
            LastError = Error.None;
            ErrDetail = "";
            OptimizerEnabled = true;
            AllowConditionalAssignment = false;
            
            // Debug
            // if (!Mtelog.started) Mtelog.Start();
        }

        //------------------------------------------------------------------- Compile()
        //
        //  Compile expression into a Codeblock
        //
        public int Compile(string codeblockText)
        {
            Error err = Error.None;

            // Store codeblock text
            this.codeblockText = codeblockText;

            // Create new code object
            code = new Code(codeblockText);

            // Compile the block 
            err = Codegen.CompileCodeblock(this);

            return (err.ErrCode);
        }

        //----------------------------------------------------------------- Decompile()
        //
        // Decompile a Codeblock 
        //
        public List<string> Decompile(string codeblockText)
        {
            List<string> codelist;

            // Decompile compiled code to list
            codelist = MteEval.Decompile.DumpCodeblock(this);
            
            return (codelist);
        }

        //---------------------------------------------------------------------- Eval()
        //
        // Evaluate a Codeblock
        // 
        public double Eval( params double[] args)
        {
            double result = 0d;

            // Execute code
            result = Run.ExecuteCodeblock(this, args);

            return (result);
        }

    }
}