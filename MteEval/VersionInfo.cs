/**********************************************************************************
 '*
 '* VersionInfo.cs - Version history
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

#region  ToDo
/*******************************************************************************
' 
'  ToDo
'  =====
'
' Codegen.cs
' ----------
' - Use dictionary for variable/parameter names
' - Replace iif() with conditional operator.
' - Replace double with decimal
' - Add test that token was consumed. Catches errors in parser.
'
'  Done
'  ----
'   
'******************************************************************************/
#endregion

#region Revision History
/******************************************************************************
'* Revision History:
'*
'* No.        Who  Date        Description
'* =====      ===  ==========  ==================================================
'* 1.09.2     MTE  2017/05/10  - Fixed Codeblock.Decompile() method signature
'* 1.09.1     MTE  2017/05/10  - Updated Readme.md
'* 1.09       MTE  2017/05/10  - Added test app to project.  Misc edits.
'* 1.08       MTE  2017/05/09  - Fixed errors in bitwise vs logical operator 
'*                               regular expresions.
'* 1.07       MTE  2017/05/08  - Now detects illegal assignment aka "no lvalue"
'* 1.06       MTE  2017/05/08  - Completed library port to C#
'* 0.01       MTE  2017/04/26  - Begin here!
'
********************************************************************************/
#endregion

namespace MteEval
{
    public static class VersionInfo
    {
        private const string versionText="1.09.2";
        static public string Text
        {
            get { return versionText; }
        }
    }
}


