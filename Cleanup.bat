@echo off
rem * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * *
rem *                           Deployment cleanup                            *
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
rem * The purpose of this script is to clean up a lot of 'useless' files      *
rem * before committing to a repository.                                      *
rem * It removes all temporarily files and user preferences. Also, it removes *
rem * images used by the .NET Micro Framework Emulator and other files that   *
rem * should not be in the online repository.                                 *
rem * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * * *

echo Are you sure you want to remove all unnecessary files?
echo Press ENTER to continue or Ctrl+C to stop
pause >nul

rem Visual Studio Solution User Options (Hidden files)
echo Removing *.suo...
del /s /ah *.suo

rem Visual Studio Project User Options
echo Removing *.user...
del /s *.user

rem Emulator memory (are created when forgetting to select a target device and
rem is crappy lost space)
echo Removing OnBoardFlash.dat...
del /s OnBoardFlash.dat

rem Some old build logs
echo Removing msbuild.log...
del /s msbuild.log

rem Binary folders (will be recreated after opening a project and removing them
rem saves bandwidth for downloaders)
echo Removing bin/...
for /f "delims=" %%a in ('dir /s /b /ad bin') do (
	echo Removing directory %%a ...
	rmdir /s /q "%%a"
)

rem Object folders (will be recreated after opening a project and removing them
rem saves bandwidth for downloaders)
echo Removing obj/...
for /f "delims=" %%a in ('dir /s /b /ad obj') do (
	echo Removing directory %%a ...
	rmdir /s /q "%%a"
)

rem Emulator folders (are created when forgetting to select a target device and
rem is crappy lost space)
echo Removing DOTNETMF_FS_EMULATION/...
for /f "delims=" %%a in ('dir /s /b /ad DOTNETMF_FS_EMULATION') do (
	echo Removing directory %%a ...
	rmdir /s /q "%%a"
)

rem These files are created when building the framework but aren't required
rem and are property of Secret Labs LLC. They are created because they're a
rem result of using the AnalogInput and PWM classes. Both could become obsolete
rem in a final 4.2 build.

echo Cleaning up Release-folders...
del /s SecretLabs.NETMF.Hardware.dll
del /s SecretLabs.NETMF.Hardware.pdb
del /s SecretLabs.NETMF.Hardware.pdbx
del /s SecretLabs.NETMF.Hardware.pe
del /s SecretLabs.NETMF.Hardware.Netduino.dll
del /s SecretLabs.NETMF.Hardware.Netduino.pdb
del /s SecretLabs.NETMF.Hardware.Netduino.pdbx
del /s SecretLabs.NETMF.Hardware.Netduino.pe
del /s SecretLabs.NETMF.Hardware.PWM.dll
del /s SecretLabs.NETMF.Hardware.PWM.pdb
del /s SecretLabs.NETMF.Hardware.PWM.pdbx
del /s SecretLabs.NETMF.Hardware.PWM.pe
del /s SecretLabs.NETMF.Hardware.AnalogInput.dll
del /s SecretLabs.NETMF.Hardware.AnalogInput.pdb
del /s SecretLabs.NETMF.Hardware.AnalogInput.pdbx
del /s SecretLabs.NETMF.Hardware.AnalogInput.pe
del /s GHIElectronics.NETMF.Hardware.dll
del /s GHIElectronics.NETMF.Hardware.pdb
del /s GHIElectronics.NETMF.Hardware.xml
del /s GHIElectronics.NETMF.Hardware.pdbx
del /s GHIElectronics.NETMF.Hardware.pe
del /s GHIElectronics.NETMF.System.dll
del /s GHIElectronics.NETMF.System.pdb
del /s GHIElectronics.NETMF.System.xml
del /s GHIElectronics.NETMF.System.pdbx
del /s GHIElectronics.NETMF.System.pe

echo Cleaning op setup builds...
del /q Installer\*Setup.exe
del /q Installer\InstallerV4?.nsi
del /q Installer\Publish\*.*
rmdir Installer\Publish

echo Finished cleaning up!
echo.
pause