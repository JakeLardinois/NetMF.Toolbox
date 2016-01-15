using System;
using System.IO;
using Toolbox.NETMF;
using Toolbox.NETMF.NET;

/*
 * Copyright 2012-2014 Stefan Thoolen (http://www.netmftoolbox.com/)
 *
 * Licensed under the Apache License, Version 2.0 (the "License");
 * you may not use this file except in compliance with the License.
 * You may obtain a copy of the License at
 *
 *     http://www.apache.org/licenses/LICENSE-2.0
 *
 * Unless required by applicable law or agreed to in writing, software
 * distributed under the License is distributed on an "AS IS" BASIS,
 * WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
 * See the License for the specific language governing permissions and
 * limitations under the License.
 */

namespace Programs
{
    /// <summary>
    /// Simple test editor
    /// </summary>
    /// <remarks>Most commands are immitated as described at http://en.wikipedia.org/wiki/Edlin </remarks>
    public static class Edlin
    {

        /// <summary>
        /// Binds to the Shell Core
        /// </summary>
        /// <param name="Shell">The ShellCore object</param>
        public static void Bind(ShellCore Shell)
        {
            Shell.OnCommandReceived += new ShellCore.CommandReceived(Shell_OnCommandReceived);
        }

        /// <summary>
        /// Unbinds from the Shell Core
        /// </summary>
        /// <param name="Shell">The ShellCore object</param>
        public static void Unbind(ShellCore Shell)
        {
            Shell.OnCommandReceived -= new ShellCore.CommandReceived(Shell_OnCommandReceived);
        }

        /// <summary>
        /// Triggered when a command has been given
        /// </summary>
        /// <param name="Shell">Reference to the shell</param>
        /// <param name="Arguments">Command line arguments</param>
        /// <param name="SuspressError">Set to 'true' if you could do anything with the command</param>
        /// <param name="Time">Current timestamp</param>
        private static void Shell_OnCommandReceived(ShellCore Shell, string[] Arguments, ref bool SuspressError, DateTime Time)
        {
            if (Arguments[0].ToUpper() == "EDLIN")
            {
                if (Arguments.Length == 1) throw new ArgumentException("Missing filename. See HELP EDLIN for more");
                EditFile(Shell.TelnetServer, Shell.LastCommandline.Substring(6).Trim());
                SuspressError = true;
            }
            if (Arguments[0].ToUpper() == "HELP")
            {
                if (Arguments.Length == 1)
                {
                    Shell.TelnetServer.Print("EDLIN [FILE]                       Edits a text file");
                    SuspressError = true;
                }
                else if (Arguments[1] == "EDLIN")
                {
                    Shell.TelnetServer.Print("EDLIN [FILE]                       Edits a text file");
                    Shell.TelnetServer.Print("- [FILE]  The file to edit");
                    SuspressError = true;
                }
            }
        }
        
        /// <summary>
        /// Splits the command line
        /// </summary>
        /// <param name="CommandLine">Inputed command line</param>
        /// <returns>An array with all arguments</returns>
        private static string[] SplitCommandline(string CommandLine)
        {
            if (CommandLine == "") return new string[0];
            // Used to check if a character is a number
            string Numbers = "0123456789";
            // We start with 10 keys, will probably be lesser later
            string[] Retval = { "", "", "", "", "", "", "", "", "", "" };
            // Start at index 0
            int Index = 0;
            // Will go to false when a non-number has been reached
            bool NumbersOnly = true;

            // Loops through all command lines
            for (int Pos = 0; Pos < CommandLine.Length; ++Pos)
            {
                if (NumbersOnly)
                {
                    if (CommandLine[Pos] == ',') { ++Index; continue; }
                    if (Numbers.IndexOf(CommandLine[Pos]) < 0)
                    {
                        if (Retval[Index] != "") ++Index;
                        NumbersOnly = false;
                    }
                }
                // To prevent errors. So many commas are invalid anyways, but we don't want to error out here
                if (Index == Retval.Length) --Index;
                // Add the character to the current value
                Retval[Index] += CommandLine.Substring(Pos, 1);
            }

            // Okay, so we know how much commands are enterred, so we can build the final array
            string[] Final = new string[Index + 1];
            for (int Pos = 0; Pos < Final.Length; ++Pos)
                Final[Pos] = Retval[Pos];

            return Final;
        }

        /// <summary>
        /// Updates a line in a text file
        /// </summary>
        /// <param name="Tempfile">The file to modify</param>
        /// <param name="LineNo">The line to modify</param>
        /// <param name="Text">New contents of the line</param>
        private static void UpdateLine(string Tempfile, int LineNo, string Text)
        {
            // Moves the temp file to the buffer file
            string Bufferfile = ReplaceExt(Tempfile, ".buf");
            if (File.Exists(Bufferfile)) File.Delete(Bufferfile);
            File.Move(Tempfile, Bufferfile);

            // Reads the buffer file and creates a new temp file
            FileStream Read = File.OpenRead(Bufferfile);
            FileStream Write = File.OpenWrite(Tempfile);

            // Reads and writes all data
            int CurLine = 1;
            // Do we need to edit the first line? Then we need to act now
            if (LineNo == 1)
                Write.Write(Tools.Chars2Bytes((Text + "\r\n").ToCharArray()), 0, Text.Length + 2);

            for (long Pos = 0; Pos < Read.Length; ++Pos)
            {
                int Byte = Read.ReadByte();
                if (CurLine != LineNo) Write.WriteByte((byte)Byte);
                if (Byte == 10)
                {
                    ++CurLine;
                    if (CurLine == LineNo) Write.Write(Tools.Chars2Bytes((Text + "\r\n").ToCharArray()), 0, Text.Length + 2);
                }
            }

            // Closes the streams
            Write.Close();
            Read.Close();

            // Removes the buffer file
            File.Delete(Bufferfile);
        }

        /// <summary>
        /// Replaces the extension of a filename
        /// </summary>
        /// <param name="Filename">Filename</param>
        /// <param name="NewExt">New extension</param>
        /// <returns>New filename</returns>
        private static string ReplaceExt(string Filename, string NewExt)
        {
            FileInfo info = new FileInfo(Filename);
            string NoExt = info.FullName.Substring(0, info.FullName.Length - info.Extension.Length);
            return NoExt + NewExt;
        }

        /// <summary>
        /// Starts editting a file
        /// </summary>
        /// <param name="Server">Reference to the telnet server</param>
        /// <param name="Filename">File to edit</param>
        private static void EditFile(TelnetServer Server, string Filename)
        {
            if (!File.Exists(Filename)) throw new Exception("File does not exist: " + Filename);

            // Makes a working file
            string Tempfile = ReplaceExt(Filename, ".$$$");
            string Bakfile = ReplaceExt(Filename, ".bak");
            File.Copy(Filename, Tempfile, true);

            // Detecting line endings
            FileStream Stream = File.OpenRead(Tempfile);
            int Lines = 0;
            int Byte = 0;
            for (long Pos = 0; Pos < Stream.Length; ++Pos)
            {
                Byte = Stream.ReadByte();
                if (Byte == 10) ++Lines;
            }
            Stream.Close();
            // Appairently the file doesn't end with a line ending
            if (Byte != 10) ++Lines;

            // Okay, we know the amount of lines
            Server.Print(Lines.ToString() + " lines. Type ? for help.");

            // Lets start the main loop
            while (true)
            {
                // Shows a prompt and asks for a command
                Server.Print("*", true);
                string[] Arguments = SplitCommandline(Server.Input());
                if (Arguments.Length == 0) continue;

                switch (Arguments[Arguments.Length - 1].Substring(0, 1).ToUpper())
                {
                    case "Q": // Quits, without saving
                        File.Delete(Tempfile);
                        return;
                    case "E": // Quits, with saving
                        if (File.Exists(Bakfile)) File.Delete(Bakfile);
                        File.Move(Filename, Bakfile);
                        File.Move(Tempfile, Filename);
                        return;
                    case "L": // Lists some lines
                        if (Arguments.Length > 2) List(Server, Tempfile, int.Parse(Arguments[0]), int.Parse(Arguments[1]));
                        else if (Arguments.Length > 1) List(Server, Tempfile, int.Parse(Arguments[0]), Lines);
                        else List(Server, Tempfile, 1, Lines);
                        break;
                    case "?": // Shows help
                        EdlinHelp(Server);
                        break;
                    default:
                        // Is this a line number, or unknown entry?
                        int LineNo = -1;
                        try { LineNo = int.Parse(Arguments[0]); } catch { }
                        if (LineNo == -1) Server.Print("Entry error");
                        else if (LineNo > Lines) Server.Print("Line out of range (1 to " + Lines.ToString() + ")");
                        else EditLine(Server, Tempfile, LineNo);
                        break;
                }
                
            }
        }

        /// <summary>
        /// Lists lines from a file
        /// </summary>
        /// <param name="Server">Reference to the telnet server</param>
        /// <param name="Tempfile">The location of the temp file</param>
        /// <param name="Start">First line to show</param>
        /// <param name="End">Last line to show</param>
        private static void List(TelnetServer Server, string Tempfile, int Start, int End)
        {
            FileStream Stream = File.OpenRead(Tempfile);

            int LineNo = 1;
            string Buffer = "";
            while (true)
            {
                // Defines the buffer size (max. 255 bytes)
                long Length = Stream.Length - Stream.Position;
                if (Length == 0) break;
                if (Length > 255) Length = 255;
                // Reads out the buffer
                byte[] ReadBuffer = new byte[(int)Length];
                Stream.Read(ReadBuffer, 0, ReadBuffer.Length);
                Buffer += new string(Tools.Bytes2Chars(ReadBuffer));

                // Is this a line?
                while (true) 
                {
                    int Pos = Buffer.IndexOf("\n");
                    if (Pos < 0) break;
                    // New line found, lets split it
                    string Line = Buffer.Substring(0, Pos);
                    Buffer = Buffer.Substring(Pos + 1);
                    // Do we need to print this line?
                    if (LineNo >= Start && LineNo <= End)
                        Server.Print(Tools.ZeroFill(LineNo, 8, ' ') + ": " + Line.TrimEnd());
                    ++LineNo;
                } 
            }
            Stream.Close();

            // If there's no line ending at the end of the file, we need to show this line
            if (Buffer != "" && LineNo >= Start && LineNo <= End)
                Server.Print(Tools.ZeroFill(LineNo, 8, ' ') + ": " + Buffer.TrimEnd());
        }

        /// <summary>
        /// Edits a line
        /// </summary>
        /// <param name="Server">Reference to the telnet server</param>
        /// <param name="Tempfile">The location of the temp file</param>
        /// <param name="LineNo">Line number to edit</param>
        private static void EditLine(TelnetServer Server, string Tempfile, int LineNo)
        {
            List(Server, Tempfile, LineNo, LineNo);
            Server.Print(Tools.ZeroFill(LineNo, 8, ' ') + ":*", true);
            string NewLine = Server.Input();
            if (NewLine == "\x03") return;

            Edlin.UpdateLine(Tempfile, LineNo, NewLine);
        }

        /// <summary>
        /// Shows the EDLIN help
        /// </summary>
        /// <param name="Server">Reference to the telnet server</param>
        private static void EdlinHelp(TelnetServer Server)
        {
            Server.Print("Edit line                   line#");
            Server.Print("End (save file)             E");
            Server.Print("Quit (throw away changes)   Q");
            Server.Print("List                        [startline][,endline]L");
        }

    }
}
