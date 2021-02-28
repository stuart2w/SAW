; SAW
;

;--------------------------------

!include "MUI.nsh"

; The name of the installer
Name "SAW Prototype"

; The file to write
OutFile "setup prototype.exe"

; The default installation directory
InstallDir "$PROGRAMFILES\SAW Prototype"

; Registry key to check for directory (so if you install again, it will 
; overwrite the old one automatically)
InstallDirRegKey HKLM "Software\SAWPrototype" "Install_Dir"


;--------------------------------

; !include "FileAssociation.nsh"

;--------------------------------
;Interface Settings

;  !define MUI_ABORTWARNING

;--------------------------------
;Pages

; !insertmacro MUI_PAGE_LICENSE "License.txt"
;  !insertmacro MUI_PAGE_LICENSE "${NSISDIR}\Docs\Modern UI\License.txt"
;  !insertmacro MUI_PAGE_COMPONENTS
  !insertmacro MUI_PAGE_DIRECTORY
  !insertmacro MUI_PAGE_INSTFILES
  
  !insertmacro MUI_UNPAGE_CONFIRM
  !insertmacro MUI_UNPAGE_INSTFILES
  
;--------------------------------
;Languages
 
  !insertmacro MUI_LANGUAGE "English"
 ; !insertmacro MUI_LANGUAGE "German"
  !insertmacro MUI_LANGUAGE "Swedish"

; Pages

;Page components
;Page directory
;Page instfiles

;UninstPage uninstConfirm
;UninstPage instfiles

icon icon.ico

;--------------------------------

; The stuff to install
Section "SAW"

  ; install for all users
  SetShellVarContext all

  SectionIn RO

  SetRegView 64
  ClearErrors
  ; can't use ReadRegStr as that seems to return an error for default value if it's not set.  Just want to see if the folder exists really...
  EnumRegKey $0 HKLM "SOFTWARE\Microsoft\NET Framework Setup\NDP\v4\Client" 0
  IfErrors 0 NetCheckDone
  	MessageBox MB_OK|MB_ICONEXCLAMATION "Warning: SAW requires the '.Net 4.0 runtime', which does not appear to be present on the machine.  The installation will continue, but the software will probably not run correctly until this is also installed.  It can be downloaded from the microsoft web site.  Please contact support if you need a direct download link."
NetCheckDone:
  SetRegView 32

  ; remember where the installer ran from ; not needed?
  WriteRegStr HKLM "Software\SAWPrototype" InstallSource $EXEDIR

  ; Set output path to the installation directory.
  SetOutPath $INSTDIR
  
  ; Put files there
  File "Installer Files\*.*"
  File "..\SAW\bin\x86\release\SAW.exe"
  File "..\SAW\bin\x86\Release\SAW.exe.config"
  File "..\SAW\bin\x86\Release\SwitchCPP.dll"
  File "..\SAW\bin\x86\Release\NAudio.dll"
  File "..\SAW\bin\x86\Release\Svg.dll"
  File "..\SAW\bin\x86\Release\Newtonsoft.Json.dll"
  File "..\SAW\bin\x86\Release\Microsoft.DirectX.dll"
  File "..\SAW\bin\x86\Release\Microsoft.DirectX.DirectInput.dll"
  File "..\SAW\bin\x86\Release\System.ValueTuple.dll"
  File "..\SAW\bin\x86\Release\yapi.dll"
  File "..\SAW\strings.txt"
  File "Update.exe"
  ; copied here so that we can keep a signed one, rather than continuallyy resign it
  File "..\repoB.dll"

  SetOutPath $INSTDIR\Activities
  File "Installer Files\Activities\*.*"

  SetOutPath $INSTDIR\SAW6graphics
  File /r "Installer Files\graphics\*.*"
  SetOutPath $INSTDIR\SAW6sounds
  File "Installer Files\sounds\*.*"
  SetOutPath $INSTDIR\Deltas
  File /nonfatal "Installer Files\Deltas\*.*"

  

  ; write start menu shortcuts
  CreateDirectory "$SMPROGRAMS\SAWPrototype"
  CreateShortCut "$SMPROGRAMS\SAWPrototype\SAW Prototype.lnk" "$INSTDIR\SAW.exe" "" "$INSTDIR\SAW.exe" 0

  CreateShortCut "$DESKTOP\SAW Prototype.lnk" "$INSTDIR\SAW.exe" ""
  
  ; Write the installation path into the registry
  WriteRegStr HKLM SOFTWARE\SAWPrototype "Install_Dir" "$INSTDIR"
  
  ; Write the uninstall keys for Windows
  WriteRegStr HKLM "Software\Microsoft\Windows\CurrentVersion\Uninstall\SAWPrototype" "DisplayName" "SAW Prototype"
  WriteRegStr HKLM "Software\Microsoft\Windows\CurrentVersion\Uninstall\SAWPrototype" "UninstallString" '"$INSTDIR\uninstall.exe"'
  WriteRegDWORD HKLM "Software\Microsoft\Windows\CurrentVersion\Uninstall\SAWPrototype" "NoModify" 1
  WriteRegDWORD HKLM "Software\Microsoft\Windows\CurrentVersion\Uninstall\SAWPrototype" "NoRepair" 1
  WriteUninstaller "uninstall.exe"
  
  ;${RegisterExtension} "$INSTDIR\SAW.exe" ".saw7" "SAW 7 file"
SectionEnd


;--------------------------------

; Uninstaller

Section "Uninstall"
  
  SetShellVarContext all

  ; Remove registry keys
  DeleteRegKey HKLM "Software\Microsoft\Windows\CurrentVersion\Uninstall\SAWPrototype"

  ; Remove files and uninstaller
  Delete $INSTDIR\uninstall.exe

  ; Remove shortcuts, if any
  Delete "$SMPROGRAMS\SAWPrototype\*.*"
  RMDir "$SMPROGRAMS\SAWPrototype"

  ; Remove directories used
  Delete "$INSTDIR\*.*"
  RMDir "$INSTDIR"


SectionEnd
