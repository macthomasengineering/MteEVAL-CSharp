/**********************************************************************************
 '*
 '* Mtelog.cs - Trace and debug logger
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
using System.IO;
using System.Text;

// ---------------
// ToDo
// 
// - Make started readonly
// - Add finally to exceptions. 
// - Add specific exceptions to catch.

namespace MteUtility
{
    
    //------------------------------------------------------------------------ Mtelog()
    //
    static class Mtelog
    {
        private static string logFileName = "mtelog.txt";
        private static long logMaxSize = 1024 * 1024;
        private static long logSize = 0;
        private static bool logEnabled = false;
        private static string logDirectory = "";
        private static bool debugTypeEnabled = true;
        public static bool started = false;
        private static StreamWriter logStream = null;

        //------------------------------------------------------------------- DebugOn()
        //
        public static bool DebugOn
        {
            get { return debugTypeEnabled;  }
            set { debugTypeEnabled = value; }
        }
        
        //--------------------------------------------------------------------- Start()
        //
        public static bool Start()
        {

            // optimistic start
            Mtelog.started = true;

            // Set and create log directory
            SetLogDirectory();

            // Open the log for writing
            logEnabled = OpenLog();

            return (logEnabled);
        }

        //---------------------------------------------------------------------- Stop()
        //
        public static void Stop()
        {

            if (logEnabled)
            {
                logEnabled = false;
                CloseLog();
            }

        }

        //---------------------------------------------------------------------- Info()
        //
        public static void Info(string logMessage)
        {

            if (logEnabled)
            {
                WriteLogEntry( "i", logMessage );
            }

        }

        //------------------------------------------------------------------- Console()
        //
        public static void Console(string logMessage)
        {

            if (logEnabled)
            {
                WriteLogEntry( "i", logMessage );
                System.Console.WriteLine(logMessage);
            }

        }
        
        //--------------------------------------------------------------------- Debug()
        //
        public static void Debug(string logMessage)
        {

            if (debugTypeEnabled && logEnabled)
            {
                WriteLogEntry( "d", logMessage);
            }

        }

        //----------------------------------------------------------- SetLogDirectory()
        //
        private static void SetLogDirectory()
        {

            // Get full path of running program
            string path = System.Reflection.Assembly.GetEntryAssembly().Location;

            // Extract just the applciation name
            string appName = Path.GetFileNameWithoutExtension(path);

            // Build folder path where log will be written
            string appDataFolder = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), appName);

            // Create log directory
            try
            {
                Directory.CreateDirectory(appDataFolder);
            }
            catch (Exception ex)
            {
                System.Console.WriteLine("Error: Creating log directory, path= " + appDataFolder + ", ex=" + ex.Message);
                throw;
            }

            // Store log directory
            logDirectory = appDataFolder;

        }
        
        //------------------------------------------------------------------- OpenLog()
        //
        private static bool OpenLog()
        {
            bool success = false;
            StreamWriter sw;
            
            // Build full name of log file
            string logFile = Path.Combine(logDirectory, logFileName);

            // Get info about file
            FileInfo logFileInfo = new FileInfo(logFile);
            
            try
            {

                // Create log or truncate if log too big
                if ( !logFileInfo.Exists || logFileInfo.Length > logMaxSize )   // Caution: FileInfo.Length
                {
                    sw = File.CreateText(logFile);
                    logSize = 0;
                }
                // Append to log
                else
                {
                    sw = File.AppendText(logFile);
                    logSize = logFileInfo.Length;
                }

                logStream   = sw;
                success     = true;
            }
            catch (Exception ex)
            {

                System.Console.WriteLine("Error: Creating logfile. logFile=" + logFile + ", ex=" + ex.Message);
                throw;
            }

            return (success);
        }

        //------------------------------------------------------------------ CloseLog()
        //
        private static void CloseLog()
        {
            try
            {
                logStream.Close();
            }
            catch (Exception ex)
            {
                System.Console.WriteLine("Error: Closing log file. ex=" + ex.Message);
                throw;
            }
        }

        //------------------------------------------------------------------ ResetLog()
        //
        private static bool ResetLog()
        {

            // Close and reopen log
            CloseLog();
            logEnabled = OpenLog();

            if ( logEnabled )
            {
                Mtelog.Info("<-- Reset Log --");
            }

            return (logEnabled);

        }

        //------------------------------------------------------------- WriteLogEntry()
        //
        private static void WriteLogEntry( string logType, string logMessage )
        {
            const int CRLF_Length = 2;

            // Reset on max log size
            if ( logSize > logMaxSize)
            {
                if (!ResetLog()) return;
            }
                        
            string dateString = DateTime.Now.ToString("MMdd-HHmmss");

            // Build log entry
            StringBuilder logEntry = new StringBuilder();
            logEntry.Append(dateString)
                    .Append("[")
                    .Append(logType)
                    .Append("]: ")
                    .Append(logMessage);

            // Write to log
            try
            {
                logStream.WriteLine(logEntry.ToString());
                logStream.Flush();
                logSize = logSize + logEntry.Length + CRLF_Length;
                
            catch (Exception ex)
            {
                System.Console.WriteLine("Error: Writing log entry. ex=" + ex.Message);
                throw;
            }
        }

    }
}

