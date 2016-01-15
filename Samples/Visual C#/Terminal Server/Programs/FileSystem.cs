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
    /// Access to the SD storage
    /// </summary>
    public static class FileSystem
    {
        /// <summary>
        /// Binds to the Shell Core
        /// </summary>
        /// <param name="Shell">The ShellCore object</param>
        public static void Bind(ShellCore Shell)
        {
            Shell.OnCommandReceived += new ShellCore.CommandReceived(Shell_OnCommandReceived);
            Shell.Prompt = Directory.GetCurrentDirectory() + ">";
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
            switch (Arguments[0].ToUpper())
            {
                case "CD":
                    if (Arguments.Length == 1) Shell.TelnetServer.Print(Directory.GetCurrentDirectory());
                    else ChangeDir(Shell.TelnetServer, Shell.LastCommandline.Substring(3).Trim());
                    Shell.Prompt = Directory.GetCurrentDirectory() + ">";
                    SuspressError = true;
                    break;
                case "MKDIR":
                    if (Arguments.Length == 1) throw new ArgumentException("Specify a directory name. See also HELP MKDIR");
                    CreateDir(Shell.TelnetServer, Shell.LastCommandline.Substring(6).Trim());
                    SuspressError = true;
                    break;
                case "RMDIR":
                    if (Arguments.Length == 1) throw new ArgumentException("Specify a directory name. See also HELP RMDIR");
                    RemoveDir(Shell.TelnetServer, Shell.LastCommandline.Substring(6).Trim());
                    SuspressError = true;
                    break;
                case "TOUCH":
                    if (Arguments.Length == 1) throw new ArgumentException("Specify a filename. See also HELP TOUCH");
                    CreateFile(Shell.TelnetServer, Shell.LastCommandline.Substring(6).Trim());
                    SuspressError = true;
                    break;
                case "DEL":
                    if (Arguments.Length == 1) throw new ArgumentException("Specify a filename. See also HELP DEL");
                    RemoveFile(Shell.TelnetServer, Shell.LastCommandline.Substring(4).Trim());
                    SuspressError = true;
                    break;
                case "TYPE":
                    if (Arguments.Length == 1) throw new ArgumentException("Specify a filename. See also HELP TYPE");
                    DisplayFile(Shell.TelnetServer, Shell.LastCommandline.Substring(5).Trim());
                    SuspressError = true;
                    break;
                case "DIR":
                    if (Arguments.Length != 1) throw new ArgumentException("DIR has no valid arguments");
                    ListDir(Shell.TelnetServer);
                    SuspressError = true;
                    break;
                case "HELP":
                    bool PageFound = false;
                    if (Arguments.Length == 1) PageFound = FileSystem.DoHelp(Shell.TelnetServer, "");
                    else PageFound = FileSystem.DoHelp(Shell.TelnetServer, Arguments[1].ToUpper());
                    if (PageFound) SuspressError = true;
                    break;
            }
        }

        /// <summary>
        /// Create a file
        /// </summary>
        /// <param name="Server">Reference to the telnet server</param>
        /// <param name="Pathname">Filename</param>
        private static void CreateFile(TelnetServer Server, string Pathname)
        {
            if (File.Exists(Pathname)) return;

            FileStream Stream = File.Create(Pathname);
            Stream.Close();
        }

        /// <summary>
        /// Displays a file
        /// </summary>
        /// <param name="Server">Reference to the telnet server</param>
        /// <param name="Pathname">Filename</param>
        private static void DisplayFile(TelnetServer Server, string Pathname)
        {
            if (!File.Exists(Pathname))
            {
                DisplayError(Server, "File not found");
                return;
            }

            FileStream Stream = File.OpenRead(Pathname);
            while (true)
            {
                long Length = Stream.Length - Stream.Position;
                if (Length == 0) break;
                if (Length > 255) Length = 255;
                byte[] Buffer = new byte[(int)Length];
                Stream.Read(Buffer, 0, Buffer.Length);
                Server.Print(new String(Tools.Bytes2Chars(Buffer)), true);
            }
            Stream.Close();
            Server.Print("");
        }

        /// <summary>
        /// Deletes a file
        /// </summary>
        /// <param name="Server">Reference to the telnet server</param>
        /// <param name="Pathname">File to remove</param>
        private static void RemoveFile(TelnetServer Server, string Pathname)
        {
            try
            {
                File.Delete(Pathname);
            }
            catch (System.IO.IOException)
            {
                DisplayError(Server, "An error occured while trying to remove file \"" + Pathname + "\"");
            }
        }

        /// <summary>
        /// Creates a dir
        /// </summary>
        /// <param name="Server">Reference to the telnet server</param>
        /// <param name="Pathname">Path to create</param>
        private static void CreateDir(TelnetServer Server, string Pathname)
        {
            try
            {
                Directory.CreateDirectory(Pathname);
            }
            catch (System.IO.IOException)
            {
                DisplayError(Server, "An error occured while trying to create directory \"" + Pathname + "\"");
            }
        }

        /// <summary>
        /// Removes a dir
        /// </summary>
        /// <param name="Server">Reference to the telnet server</param>
        /// <param name="Pathname">Path to remove</param>
        private static void RemoveDir(TelnetServer Server, string Pathname)
        {
            try
            {
                Directory.Delete(Pathname);
            }
            catch (System.IO.IOException)
            {
                DisplayError(Server, "An error occured while trying to remove directory \"" + Pathname + "\"");
            }
        }

        /// <summary>
        /// Lists all items in the current directory
        /// </summary>
        /// <param name="Server">Reference to the telnet server</param>
        private static void ListDir(TelnetServer Server)
        {
            string Cwd = Directory.GetCurrentDirectory();

            // Tries the current and parent folder, but they're not always available
            try { ShowDirEntry(Server, new DirectoryInfo(Cwd), "."); } catch { }
            try { ShowDirEntry(Server, new DirectoryInfo(Cwd).Parent, ".."); } catch { }
            // Prints out all directories
            string[] Dirs = Directory.GetDirectories(Cwd);
            for (int DirCnt = 0; DirCnt < Dirs.Length; ++DirCnt)
                ShowDirEntry(Server, new DirectoryInfo(Dirs[DirCnt]));
            // Prints out all files
            string[] Files = Directory.GetFiles(Cwd);
            for (int FileCnt = 0; FileCnt < Files.Length; ++FileCnt)
                ShowFileEntry(Server, new FileInfo(Files[FileCnt]));
        }

        /// <summary>
        /// Shows a directory entry (child of ListDir)
        /// </summary>
        /// <param name="Server">Reference to the telnet server</param>
        /// <param name="Path">Path</param>
        /// <param name="Name">If filled, this name will be shown instead (useful for "." and "..")</param>
        private static void ShowDirEntry(TelnetServer Server, DirectoryInfo Path, string Name = "")
        {
            if (Name == "") Name = Path.Name;
            Server.Print(Path.CreationTime.ToString() + "    <DIR>             " + Name);
        }

        /// <summary>
        /// Shows a file entry (child of ListDir)
        /// </summary>
        /// <param name="Server">Reference to the telnet server</param>
        /// <param name="Path">Path</param>
        private static void ShowFileEntry(TelnetServer Server, FileInfo Path)
        {
            Server.Print(Path.CreationTime.ToString() + Tools.ZeroFill(Path.Length.ToString(), 20, ' ') + "  " + Path.Name);
        }

        /// <summary>
        /// Changes to a directory
        /// </summary>
        /// <param name="Server">Reference to the telnet server</param>
        /// <param name="Pathname">The name of the directory</param>
        private static void ChangeDir(TelnetServer Server, string Pathname)
        {
            try
            {
                string Dir = Path.Combine(Directory.GetCurrentDirectory(), Pathname);
                Directory.SetCurrentDirectory(Dir);
            }
            catch (System.IO.IOException)
            {
                DisplayError(Server, "An error occured while changing directory to \"" + Pathname + "\"");
            }
        }

        /// <summary>
        /// Displays a line of text in red
        /// </summary>
        /// <param name="Server">Reference to the telnet server</param>
        /// <param name="Text">Text to display</param>
        private static void DisplayError(TelnetServer Server, string Text)
        {
            Server.Color(TelnetServer.Colors.LightRed);
            Server.Print(Text);
            Server.Color(TelnetServer.Colors.White);
        }

        /// <summary>
        /// Shows a specific help page
        /// </summary>
        /// <param name="Server">The telnet server object</param>
        /// <param name="Page">The page</param>
        /// <returns>True when the page exists</returns>
        private static bool DoHelp(TelnetServer Server, string Page)
        {
            switch (Page)
            {
                case "":
                    Server.Print("DIR                                Reads out a listing of the current directory");
                    Server.Print("CD [PATH]                          Changes the current directory");
                    Server.Print("MKDIR [PATH]                       Creates a directory");
                    Server.Print("RMDIR [PATH]                       Removes a directory");
                    Server.Print("DEL [PATH]                         Removes a file");
                    Server.Print("TYPE [PATH]                        Prints out a file");
                    Server.Print("TOUCH [PATH]                       Creates an empty file");
                    return true;
                case "CD":
                    Server.Print("CD [PATH]                          Changes the current directory");
                    Server.Print("- [PATH]  The directory to go into");
                    return true;
                case "TYPE":
                    Server.Print("TYPE [PATH]                        Prints out a file");
                    Server.Print("- [PATH]  The file to display");
                    return true;
                case "DEL":
                    Server.Print("DEL [PATH]                         Removes a file");
                    Server.Print("- [PATH]  The file to remove");
                    return true;
                case "TOUCH":
                    Server.Print("TOUCH [PATH]                       Creates an empty file");
                    Server.Print("- [PATH]  The file to create");
                    return true;
                case "MKDIR":
                    Server.Print("MKDIR [PATH]                       Creates a directory");
                    Server.Print("- [PATH]  The directory to create");
                    return true;
                case "RMDIR":
                    Server.Print("RMDIR [PATH]                       Removes a directory");
                    Server.Print("- [PATH]  The directory to remove");
                    return true;
                case "DIR":
                    Server.Print("DIR                                Reads out a listing of the current directory");
                    return true;
                default:
                    return false;
            }
        }
    }
}
