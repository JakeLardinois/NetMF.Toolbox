; .NET Micro Framework Driver Installation Script
; Matt Isenhower, Komodex Systems LLC
; http://komodex.com
; Released under the Apache License 2.0.

!include "Macros.nsh"

;--------------------------------
; MAIN SETTINGS
;--------------------------------

;--------------------------------
; Company and Product Information

  !define ShortCompanyName "Netmftoolbox.com"
  !define FullCompanyName "${ShortCompanyName}"
  !define ProductName ".NET Micro Framework Toolbox"
  !define InfoURL "http://www.netmftoolbox.com/"
  !define Copyright "Copyright 2011-2014 Stefan Thoolen"
  !define FullProductName "${ProductName}"
  ;Version number must be in X.X.X.X format
  !define ProductVersion "0.3.0.0"
  !define Version "${ProductVersion}"

;--------------------------------
; EULA (Optional)

  ;Uncomment the following line to include a EULA page. The specified plain or rich text file will be used for the EULA terms.
  !define EULAFilename "..\License.txt"
  
;--------------------------------
; Readme (Optional)
  !define MUI_FINISHPAGE_SHOWREADME $INSTDIR\Readme.txt

;--------------------------------
; More info link (Optional)
  ;Link shown on the installer's Finish page (optional)
  !define MUI_FINISHPAGE_LINK "Learn more about the .NET Micro Framework Toolbox"
  !define MUI_FINISHPAGE_LINK_LOCATION "http://www.netmftoolbox.com/?ref=installer"

;--------------------------------
; .NET Micro Framework Version (in vX.Y format)
  !define NETMFVersion1 "v4.1"
  !define NETMFVersion2 "v4.2"
  !define NETMFVersion3 "v4.3"

;--------------------------------
; Source Directory and Assemblies

  ;AddAssembly macro parameters:
  ; - The assembly output file name, without the .dll extension
  ; - The assembly output directory, relative to the location of this script

  ;Insert the macro once for each assembly you need to include in your installer. All necessary files
  ;will be included automatically (including the endian-specific files in the be/le folders).

  ;!insertmacro AddAssembly "NETMFClassLibraryName" "..\NETMFClassLibraryName\bin\Release"

  ;The dlls per NETMF version may differ.
  !ifdef NETMFVersion1
  	!include InstallerV41.nsi
  !endif
  !ifdef NETMFVersion2
  	!include InstallerV42.nsi
  !endif
  !ifdef NETMFVersion3
  	!include InstallerV43.nsi
  !endif

;--------------------------------
; Installer/Uninstaller Filenames

  !define InstallerFilename "${FullProductName} ${Version} Setup.exe"
  !define UninstallerFilename "${FullProductName} Uninstall.exe"

  ;Custom installer/uninstaller icons (optional)
  ;!define MUI_ICON "Icons\XPUI-install.ico"
  ;!define MUI_UNICON "Icons\XPUI-uninstall.ico"
  !define MUI_ICON "Installer.ico"

;--------------------------------
; Installation Directory and Registry Keys

  ;Default installation directory (created within the Program Files directory)
  !define InstallationDirectory "${ShortCompanyName}\${ProductName}"
  
  ;Assemblies will be installed to a .NET Micro Framework version-specific directory within the assembly subdirectory.
  !define AssemblySubdirectory "Assemblies"

  ;Registry keys for storing the installation location and adding this to the "Add/Remove Programs" control panel
  !define RegKey "Software\${ShortCompanyName}\${ProductName}"
  !define RegUninstall "Software\Microsoft\Windows\CurrentVersion\Uninstall\${FullProductName}"

;--------------------------------
; INSTALLATION SCRIPT
;--------------------------------
; It is not typically necessary to edit anything below this line.

;--------------------------------
; Include Modern UI

  !include "MUI2.nsh"

;--------------------------------
; General

  ;Name and file
  Name "${FullProductName}"
  BrandingText "${FullProductName} ${Version}"
  OutFile "${InstallerFilename}"

  ;Default installation folder
  InstallDir "$PROGRAMFILES\${InstallationDirectory}"

  ;Get installation folder from registry if available
  InstallDirRegKey HKLM "${RegKey}" "InstallDir"

  ;Request application privileges for Windows Vista/7
  RequestExecutionLevel admin

  ;Show installation details
  ShowInstDetails show
  ShowUnInstDetails show

;--------------------------------
; Interface Settings

  !define MUI_ABORTWARNING
  
;--------------------------------
; Installer Pages

  ;Welcome page
  !define MUI_PAGE_CUSTOMFUNCTION_PRE CheckAdmin
  !insertmacro MUI_PAGE_WELCOME

  ;License page
  !ifdef EULAFilename
    !insertmacro MUI_PAGE_LICENSE ${EULAFilename}
  !endif

  ;Directory page
  !insertmacro MUI_PAGE_DIRECTORY

  ;Instfiles page
  !define MUI_FINISHPAGE_NOAUTOCLOSE
  !insertmacro MUI_PAGE_INSTFILES

  ;Finish page
  !insertmacro MUI_PAGE_FINISH

;--------------------------------
; Uninstaller Pages

  ;Welcome page
  !define MUI_PAGE_CUSTOMFUNCTION_PRE un.CheckAdmin
  !insertmacro MUI_UNPAGE_WELCOME

  ;Confirmation page
  !insertmacro MUI_UNPAGE_CONFIRM

  ;Instfiles page
  !define MUI_UNFINISHPAGE_NOAUTOCLOSE
  !insertmacro MUI_UNPAGE_INSTFILES

;--------------------------------
; Languages

  !insertmacro MUI_LANGUAGE "English"

;--------------------------------
; Installer version information

  VIProductVersion "${Version}"
  VIAddVersionKey /LANG=${LANG_ENGLISH} "FileVersion" "${FileVersion}"
  VIAddVersionKey /LANG=${LANG_ENGLISH} "ProductName" "${FullProductName}"
  VIAddVersionKey /LANG=${LANG_ENGLISH} "CompanyName" "${FullCompanyName}"
  VIAddVersionKey /LANG=${LANG_ENGLISH} "LegalCopyright" "${Copyright}"
  VIAddVersionKey /LANG=${LANG_ENGLISH} "ProductVersion" "${ProductVersion}"
  VIAddVersionKey /LANG=${LANG_ENGLISH} "FileDescription" "${FullProductName} Setup"

;--------------------------------
; Installer Sections

  Section "${FullProductName}"

    ;This section is required
    SectionIn RO

    SetOverwrite on

    ;Add the assemblies
    !include "${InstallAssemblies}"

    ;Store installation folder
    DetailPrint "Writing registry keys..."
    WriteRegStr HKLM "${RegKey}" "InstallDir" "$INSTDIR"

    ;Add registry key so the assembly appears in Visual Studio's "Add Reference" window
    !ifdef NETMFVersion1
           WriteRegStr HKLM "Software\Microsoft\.NETMicroFramework\${NETMFVersion1}\AssemblyFoldersEx\${FullProductName}" "" "$INSTDIR\${AssemblySubdirectory}\${NETMFVersion1}"
    !endif
    !ifdef NETMFVersion2
           WriteRegStr HKLM "Software\Microsoft\.NETMicroFramework\${NETMFVersion2}\AssemblyFoldersEx\${FullProductName}" "" "$INSTDIR\${AssemblySubdirectory}\${NETMFVersion2}"
    !endif
    !ifdef NETMFVersion3
           WriteRegStr HKLM "Software\Microsoft\.NETMicroFramework\${NETMFVersion3}\AssemblyFoldersEx\${FullProductName}" "" "$INSTDIR\${AssemblySubdirectory}\${NETMFVersion3}"
    !endif

	; Adds the readme.txt
    SetOutPath "$INSTDIR"
    File "Readme.txt"
    
    ;Write the uninstall keys for Windows
    WriteRegStr HKLM "${RegUninstall}" "DisplayName" "${FullProductName}"
    WriteRegStr HKLM "${RegUninstall}" "Publisher" "${FullCompanyName}"
    WriteRegStr HKLM "${RegUninstall}" "URLInfoAbout" "${InfoUrl}"
    WriteRegStr HKLM "${RegUninstall}" "UninstallString" "$INSTDIR\${UninstallerFilename}"
    WriteRegStr HKLM "${RegUninstall}" "DisplayVersion" "${Version}"
    
    WriteRegDWORD HKLM "${RegUninstall}" "NoModify" 1
    WriteRegDWORD HKLM "${RegUninstall}" "NoRepair" 1

    ;Create uninstaller
    WriteUninstaller "$INSTDIR\${UninstallerFilename}"
    
  SectionEnd

;--------------------------------
; Uninstaller Sections

  Section "Uninstall"

    ;Remove files
    ;NOTE: Each file is deleted individually.  It is possible to recursively delete the
    ;entire installation directory and its contents, but this is not usually recommended
    ;since the user may have added other files to the directory, installed the drivers to
    ;an existing or shared directory, etc.
    !include "${UninstallAssemblies}"

	; Removes the readme.txt
	Delete "$INSTDIR\Readme.txt"
    
    ;Remove directories
    !ifdef NETMFVersion1
           RMDir "$INSTDIR\${AssemblySubdirectory}\${NETMFVersion1}\be"
           RMDir "$INSTDIR\${AssemblySubdirectory}\${NETMFVersion1}\le"
           RMDir "$INSTDIR\${AssemblySubdirectory}\${NETMFVersion1}"
           
    !endif
    !ifdef NETMFVersion2
           RMDir "$INSTDIR\${AssemblySubdirectory}\${NETMFVersion2}\be"
           RMDir "$INSTDIR\${AssemblySubdirectory}\${NETMFVersion2}\le"
           RMDir "$INSTDIR\${AssemblySubdirectory}\${NETMFVersion2}"

    !endif
    !ifdef NETMFVersion3
           RMDir "$INSTDIR\${AssemblySubdirectory}\${NETMFVersion3}\be"
           RMDir "$INSTDIR\${AssemblySubdirectory}\${NETMFVersion3}\le"
           RMDir "$INSTDIR\${AssemblySubdirectory}\${NETMFVersion3}"

    !endif
    RMDir "$INSTDIR\${AssemblySubdirectory}"
    RMDir "$INSTDIR"

    ;Remove uninstaller
    Delete "$INSTDIR\${UninstallerFilename}"

    ;Remove installation directory
    RMDir "$INSTDIR"

    ;Remove registry keys
    DetailPrint "Removing registry keys..."
    !ifdef NETMFVersion1
           DeleteRegKey HKLM "Software\Microsoft\.NETMicroFramework\${NETMFVersion1}\AssemblyFoldersEx\${FullProductName}"
    !endif
    !ifdef NETMFVersion2
           DeleteRegKey HKLM "Software\Microsoft\.NETMicroFramework\${NETMFVersion2}\AssemblyFoldersEx\${FullProductName}"
    !endif
    !ifdef NETMFVersion3
           DeleteRegKey HKLM "Software\Microsoft\.NETMicroFramework\${NETMFVersion3}\AssemblyFoldersEx\${FullProductName}"
    !endif
    DeleteRegKey HKLM "${RegUninstall}"
    DeleteRegKey HKLM "${RegKey}"

  SectionEnd

;--------------------------------
; Cleanup
!insertmacro CleanTempFiles
