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
rem * This script helps compiling the right list of DLL files for each .NETMF *
rem * version, and runs NSIS so it will compile the installation file.        *
rem * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * *

echo Looking for NSIS...
set nsis="%ProgramFiles(x86)%\NSIS\MakeNSIS.exe"
if not exist %nsis% set nsis="%ProgramFiles%\NSIS\MakeNSIS.exe"
if not exist %nsis% (
	echo Nullsoft Scriptable Install System has not been found.
	echo Please download and install NSIS from http://nsis.sourceforge.net/
	pause
	goto :eof
)

echo Looking for the NSIS Script File...
if not exist Installer.nsi (
	echo Installer.nsi not found
	pause
	goto :eof
)

echo Cleaning up from a previous run...
del /q *Setup.exe
del /q InstallerV4?.nsi

echo Generating lists of DLL files...
for /R "..\release (4.1)\" %%f in (*.dll) do (
	echo !insertmacro AddAssembly "%%~nf" "..\Release (4.1)" "${NETMFVersion1}">>InstallerV41.nsi
)
for /R "..\release (4.2)\" %%f in (*.dll) do (
	echo !insertmacro AddAssembly "%%~nf" "..\Release (4.2)" "${NETMFVersion2}">>InstallerV42.nsi
)
for /R "..\release (4.3)\" %%f in (*.dll) do (
	echo !insertmacro AddAssembly "%%~nf" "..\Release (4.3)" "${NETMFVersion3}">>InstallerV43.nsi
)

echo Compiling installer...
%nsis% /V2 Installer.nsi

rem If an error occurred, pause so the user can see it
if %errorlevel% neq 0 (
	pause
	goto :eof
)

echo Done!
