Imports System
Imports System.IO
Imports Toolbox.NETMF
Imports Toolbox.NETMF.NET

'  Copyright 2012-2014 Stefan Thoolen (http://www.netmftoolbox.com/)
'
'  Licensed under the Apache License, Version 2.0 (the "License");
'  you may not use this file except in compliance with the License.
'  You may obtain a copy of the License at
'
'      http://www.apache.org/licenses/LICENSE-2.0
'
'  Unless required by applicable law or agreed to in writing, software
'  distributed under the License is distributed on an "AS IS" BASIS,
'  WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
'  See the License for the specific language governing permissions and
'  limitations under the License.
Namespace Programs
    Public Module FileSystem
        ''' <summary>
        ''' Binds to the Shell Core
        ''' </summary>
        ''' <param name="Shell">The ShellCore object</param>
        Public Sub Bind(Shell As ShellCore)
            AddHandler Shell.OnCommandReceived, AddressOf Shell_OnCommandReceived
            Shell.Prompt = Directory.GetCurrentDirectory() + ">"
        End Sub

        ''' <summary>
        ''' Unbinds from the Shell Core
        ''' </summary>
        ''' <param name="Shell">The ShellCore object</param>
        Public Sub Unbind(Shell As ShellCore)
            RemoveHandler Shell.OnCommandReceived, AddressOf Shell_OnCommandReceived
        End Sub

        ''' <summary>
        ''' Triggered when a command has been given
        ''' </summary>
        ''' <param name="Shell">Reference to the shell</param>
        ''' <param name="Arguments">Command line arguments</param>
        ''' <param name="SuspressError">Set to 'true' if you could do anything with the command</param>
        ''' <param name="Time">Current timestamp</param>
        Private Sub Shell_OnCommandReceived(Shell As ShellCore, Arguments() As String, ByRef SuspressError As Boolean, Time As DateTime)
            Select Case Arguments(0).ToUpper()
                Case "CD"
                    If Arguments.Length = 1 Then
                        Shell.Telnetserver.Print(Directory.GetCurrentDirectory())
                    Else
                        ChangeDir(Shell.Telnetserver, Shell.LastCommandline.Substring(3).Trim())
                    End If
                    Shell.Prompt = Directory.GetCurrentDirectory() + ">"
                    SuspressError = True
                Case "MKDIR"
                    If Arguments.Length = 1 Then Throw New ArgumentException("Specify a directory name. See also HELP MKDIR")
                    CreateDir(Shell.Telnetserver, Shell.LastCommandline.Substring(6).Trim())
                    SuspressError = True
                Case "RMDIR"
                    If Arguments.Length = 1 Then Throw New ArgumentException("Specify a directory name. See also HELP RMDIR")
                    RemoveDir(Shell.Telnetserver, Shell.LastCommandline.Substring(6).Trim())
                    SuspressError = True
                Case "TOUCH"
                    If Arguments.Length = 1 Then Throw New ArgumentException("Specify a filename. See also HELP TOUCH")
                    CreateFile(Shell.Telnetserver, Shell.LastCommandline.Substring(6).Trim())
                    SuspressError = True
                Case "DEL"
                    If Arguments.Length = 1 Then Throw New ArgumentException("Specify a filename. See also HELP DEL")
                    RemoveFile(Shell.Telnetserver, Shell.LastCommandline.Substring(4).Trim())
                    SuspressError = True
                Case "TYPE"
                    If Arguments.Length = 1 Then Throw New ArgumentException("Specify a filename. See also HELP TYPE")
                    DisplayFile(Shell.Telnetserver, Shell.LastCommandline.Substring(5).Trim())
                    SuspressError = True
                Case "DIR"
                    If Arguments.Length <> 1 Then Throw New ArgumentException("DIR has no valid arguments")
                    ListDir(Shell.Telnetserver)
                    SuspressError = True
                Case "HELP"
                    Dim PageFound As Boolean = False
                    If Arguments.Length = 1 Then
                        PageFound = DoHelp(Shell.Telnetserver, "")
                    Else
                        PageFound = DoHelp(Shell.Telnetserver, Arguments(1).ToUpper())
                    End If
                    If PageFound Then SuspressError = True
            End Select
        End Sub

        ''' <summary>
        ''' Create a file
        ''' </summary>
        ''' <param name="Server">Reference to the telnet server</param>
        ''' <param name="Pathname">Filename</param>
        Private Sub CreateFile(Server As TelnetServer, Pathname As String)
            If File.Exists(Pathname) Then Exit Sub

            Dim Stream As FileStream = File.Create(Pathname)
            Stream.Close()
        End Sub

        ''' <summary>
        ''' Displays a file
        ''' </summary>
        ''' <param name="Server">Reference to the telnet server</param>
        ''' <param name="Pathname">Filename</param>
        Private Sub DisplayFile(Server As TelnetServer, Pathname As String)
            If Not File.Exists(Pathname) Then
                DisplayError(Server, "File not found")
                Exit Sub
            End If

            Dim Stream As FileStream = File.OpenRead(Pathname)
            Do
                Dim Length As Long = Stream.Length - Stream.Position
                If Length = 0 Then Exit Do
                If Length > 255 Then Length = 255
                Dim Buffer(CInt(Length)) As Byte
                Stream.Read(Buffer, 0, Buffer.Length)
                Server.Print(New String(Tools.Bytes2Chars(Buffer)), True)
            Loop
            Stream.Close()
            Server.Print("")
        End Sub

        ''' <summary>
        ''' Deletes a file
        ''' </summary>
        ''' <param name="Server">Reference to the telnet server</param>
        ''' <param name="Pathname">File to remove</param>
        Private Sub RemoveFile(Server As TelnetServer, Pathname As String)
            Try
                File.Delete(Pathname)
            Catch ex As System.IO.IOException
                DisplayError(Server, "An error occured while trying to remove file """ + Pathname + """")
            End Try
        End Sub

        ''' <summary>
        ''' Creates a dir
        ''' </summary>
        ''' <param name="Server">Reference to the telnet server</param>
        ''' <param name="Pathname">Path to create</param>
        Private Sub CreateDir(Server As TelnetServer, Pathname As String)
            Try
                Directory.CreateDirectory(Pathname)
            Catch ex As System.IO.IOException
                DisplayError(Server, "An error occured while trying to create directory """ + Pathname + """")
            End Try
        End Sub

        ''' <summary>
        ''' Removes a dir
        ''' </summary>
        ''' <param name="Server">Reference to the telnet server</param>
        ''' <param name="Pathname">Path to remove</param>
        Private Sub RemoveDir(Server As TelnetServer, Pathname As String)
            Try
                Directory.Delete(Pathname)
            Catch ex As System.IO.IOException
                DisplayError(Server, "An error occured while trying to remove directory """ + Pathname + """")
            End Try
        End Sub

        ''' <summary>
        ''' Lists all items in the current directory
        ''' </summary>
        ''' <param name="Server">Reference to the telnet server</param>
        Private Sub ListDir(Server As TelnetServer)
            Dim Cwd As String = Directory.GetCurrentDirectory()

            ' Tries the current and parent folder, but they're not always available
            Try : ShowDirEntry(Server, New DirectoryInfo(Cwd), ".") : Catch ex As Exception : End Try
            Try : ShowDirEntry(Server, New DirectoryInfo(Cwd).Parent, "..") : Catch ex As Exception : End Try
            ' Prints out all directories
            Dim Dirs() As String = Directory.GetDirectories(Cwd)
            For DirCnt As Integer = 0 To Dirs.Length - 1
                ShowDirEntry(Server, New DirectoryInfo(Dirs(DirCnt)))
            Next
            ' Prints out all files
            Dim Files() As String = Directory.GetFiles(Cwd)
            For FileCnt = 0 To Files.Length - 1
                ShowFileEntry(Server, New FileInfo(Files(FileCnt)))
            Next
        End Sub

        ''' <summary>
        ''' Shows a directory entry (child of ListDir)
        ''' </summary>
        ''' <param name="Server">Reference to the telnet server</param>
        ''' <param name="Path">Path</param>
        ''' <param name="Name">If filled, this name will be shown instead (useful for "." and "..")</param>
        Private Sub ShowDirEntry(Server As TelnetServer, Path As DirectoryInfo, Optional Name As String = "")
            If Name = "" Then Name = Path.Name
            Server.Print(Path.CreationTime.ToString() + "    <DIR>             " + Name)
        End Sub

        ''' <summary>
        ''' Shows a file entry (child of ListDir)
        ''' </summary>
        ''' <param name="Server">Reference to the telnet server</param>
        ''' <param name="Path">Path</param>
        Private Sub ShowFileEntry(Server As TelnetServer, Path As FileInfo)
            Server.Print(Path.CreationTime.ToString() + Tools.ZeroFill(Path.Length.ToString(), 20, CChar(" ")) + "  " + Path.Name)
        End Sub

        ''' <summary>
        ''' Changes to a directory
        ''' </summary>
        ''' <param name="Server">Reference to the telnet server</param>
        ''' <param name="Pathname">The name of the directory</param>
        Private Sub ChangeDir(Server As TelnetServer, Pathname As String)
            Try
                Dim Dir As String = Path.Combine(Directory.GetCurrentDirectory(), Pathname)
                Directory.SetCurrentDirectory(Dir)
            Catch ex As System.IO.IOException
                DisplayError(Server, "An error occured while changing directory to """ + Pathname + """")
            End Try
        End Sub

        ''' <summary>
        ''' Displays a line of text in red
        ''' </summary>
        ''' <param name="Server">Reference to the telnet server</param>
        ''' <param name="Text">Text to display</param>
        Private Sub DisplayError(Server As TelnetServer, Text As String)
            Server.Color(TelnetServer.Colors.LightRed)
            Server.Print(Text)
            Server.Color(TelnetServer.Colors.White)
        End Sub

        ''' <summary>
        ''' Shows a specific help page
        ''' </summary>
        ''' <param name="Server">The telnet server object</param>
        ''' <param name="Page">The page</param>
        ''' <returns>True when the page exists</returns>
        Private Function DoHelp(Server As TelnetServer, Page As String) As Boolean
            Select Case Page.ToUpper()
                Case ""
                    Server.Print("DIR                                Reads out a listing of the current directory")
                    Server.Print("CD [PATH]                          Changes the current directory")
                    Server.Print("MKDIR [PATH]                       Creates a directory")
                    Server.Print("RMDIR [PATH]                       Removes a directory")
                    Server.Print("DEL [PATH]                         Removes a file")
                    Server.Print("TYPE [PATH]                        Prints out a file")
                    Server.Print("TOUCH [PATH]                       Creates an empty file")
                    Return True
                Case "CD"
                    Server.Print("CD [PATH]                          Changes the current directory")
                    Server.Print("- [PATH]  The directory to go into")
                    Return True
                Case "TYPE"
                    Server.Print("TYPE [PATH]                        Prints out a file")
                    Server.Print("- [PATH]  The file to display")
                    Return True
                Case "DEL"
                    Server.Print("DEL [PATH]                         Removes a file")
                    Server.Print("- [PATH]  The file to remove")
                    Return True
                Case "TOUCH"
                    Server.Print("TOUCH [PATH]                       Creates an empty file")
                    Server.Print("- [PATH]  The file to create")
                    Return True
                Case "MKDIR"
                    Server.Print("MKDIR [PATH]                       Creates a directory")
                    Server.Print("- [PATH]  The directory to create")
                    Return True
                Case "RMDIR"
                    Server.Print("RMDIR [PATH]                       Removes a directory")
                    Server.Print("- [PATH]  The directory to remove")
                    Return True
                Case "DIR"
                    Server.Print("DIR                                Reads out a listing of the current directory")
                    Return True
                Case Else
                    Return False
            End Select
        End Function

    End Module
End Namespace

