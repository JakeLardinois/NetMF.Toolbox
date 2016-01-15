@echo off
rem * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * *
rem *                     NSIS Compiler helper script                         *
rem * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * *
rem * Copyright 2012-2014 Stefan Thoolen (http://www.netmftoolbox.com/)       *
rem *                                                                         *
rem * Licensed under the Apache License, Version 2.0 (the "License");         *
rem * you may not use this file except in compliance with the License.        *
rem * You may obtain a copy of the License at                                 *
rem *                                                                         *
rem *     http://www.apache.org/licenses/LICENSE-2.0                          *
rem *                                                                         *
rem * Unless required by applicable law or agreed to in writing, software     *
rem * distributed under the License is distributed on an "AS IS" BASIS,       *
rem * WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.*
rem * See the License for the specific language governing permissions and     *
rem * limitations under the License.                                          *
rem * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * *
rem * This script helps preparing for a distribution of the .NETMF Toolbox    *
rem * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * *

rem Looking for an archiver; for compatibility sake, I try to locate several
rem archivers...
set zip=
if exist "%ProgramFiles(x86)%\WinRAR\WinRAR.exe" set zip="%ProgramFiles(x86)%\WinRAR\WinRAR.exe" a -afzip -r
if exist "%ProgramFiles%\WinRAR\WinRAR.exe" set zip="%ProgramFiles%\WinRAR\WinRAR.exe" a -afzip -r
if exist "%ProgramFiles(x86)%\7-Zip\7z.exe" set zip="%ProgramFiles(x86)%\7-Zip\7z.exe" a -tzip -r
if exist "%ProgramFiles%\7-Zip\7z.exe" set zip="%ProgramFiles%\7-Zip\7z.exe" a -tzip -r

if not defined zip (
	echo No archiver has been found.
	echo Please download and install 7-Zip from http://sourceforge.net/projects/sevenzip/
	pause
	goto :eof
)

if exist Publish\nul (
	echo Cleaning up old data...
	del /q Publish\*.*
)

if not exist Publish\nul (
	echo Creating Publish folder...
	mkdir Publish
)

if not exist Publish\nul (
	echo Can't create folder
	pause
	goto :eof
)

echo Copying the installation file...
copy /b *Setup.exe "publish\NETMF Toolbox Setup.exe"
if errorlevel 1 (
	echo No setup executable found.
	echo Please build the setup first!
	pause
	goto :eof
)

echo Compressing sourcecode...
cd ..\Framework
%zip% "..\Installer\publish\NETMF Toolbox Source.zip" *.* 

echo Compressing example codes...
cd ..\Samples
%zip% "..\Installer\publish\NETMF Toolbox Samples.zip" *.* 

cd ..\Installer
echo Done!
