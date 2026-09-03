; ************************************************************************ ;
;                    Copyright (C) 2026, DataBank IMX                      ;
;                                                                          ;
; All rights reserved                                                     ;
;                                                                          ;
; For further information consult:                                        ;
;  - The DataBank IMX End User License Agreement (EULA)                    ;
;    or                                                                    ;
;  - DataBank IMX Intellectual Property Statement                          ;
;                                                                          ;
; Above referenced documents available upon request from:                  ;
;     development@databankimx.com                                          ;
;                                                                          ;
; ************************************************************************ ;

; *Migration Note: packages Samples.WindowsService (the classic net48,
;   System.ServiceProcess.ServiceBase-based service), demonstrating the
;   InstallUtil.exe registration mechanism that classic Windows Service
;   deserves, as opposed to Samples.WindowsService.NetCore.iss's sc.exe
;   create-based approach. See LectureNotes.md for the full comparison.
;
;   Before compiling, publish the service first:
;     dotnet publish ..\Samples.WindowsService\Samples.WindowsService.csproj -c Release
;   This script expects that output at the relative path below.

#define AppName "Samples Windows Service (Classic)"
#define AppVersion "1.0.0"
#define ServiceName "Samples.WindowsService"
#define ServiceExe "Samples.WindowsService.exe"
#define PublishDir "..\Samples.WindowsService\bin\Release\net48"

; *Migration Note: InstallUtil.exe lives under the .NET Framework install
;   directory, not something this installer bundles, the path below is the
;   standard net48 (v4.0.30319) location on 64-bit Windows. Adjust if
;   targeting a different Framework version or a 32-bit OS.
#define InstallUtilPath "{sys}\..\Microsoft.NET\Framework64\v4.0.30319\InstallUtil.exe"

[Setup]
AppId={{B6A1E2C3-4D5F-4A6B-8C9D-0E1F2A3B4C5D}
AppName={#AppName}
AppVersion={#AppVersion}
AppPublisher=DataBank IMX
DefaultDirName={autopf}\DataBank\{#ServiceName}
DefaultGroupName=DataBank Samples
DisableProgramGroupPage=yes
OutputBaseFilename=Samples.WindowsService.Setup
Compression=lzma
SolidCompression=yes
; A service installer genuinely needs to run elevated, both to write to
;   Program Files and to register the service itself.
PrivilegesRequired=admin
ArchitecturesInstallIn64BitMode=x64compatible

[Files]
Source: "{#PublishDir}\*"; DestDir: "{app}"; Flags: ignoreversion recursesubdirs createallsubdirs

[Run]
; *Migration Note: this is the InstallUtil.exe mechanism, see
;   Samples.WindowsService\ProjectInstaller.cs for the [RunInstaller(true)]
;   class InstallUtil.exe actually finds and runs, this Run entry is what
;   INVOKES installutil.exe, the actual registration logic lives in that
;   compiled assembly, not here.
Filename: "{#InstallUtilPath}"; Parameters: """{app}\{#ServiceExe}"""; Flags: runhidden waituntilterminated; StatusMsg: "Registering {#ServiceName}..."
Filename: "{sys}\sc.exe"; Parameters: "start ""{#ServiceName}"""; Flags: runhidden waituntilterminated; StatusMsg: "Starting {#ServiceName}..."

[UninstallRun]
; *Migration Note: RunOnceId is required on [UninstallRun] entries, without
;   it Inno Setup would refuse to compile this script at all, it's what
;   lets the uninstaller track which cleanup steps already ran if the
;   uninstall process is interrupted and resumed.
Filename: "{sys}\sc.exe"; Parameters: "stop ""{#ServiceName}"""; Flags: runhidden waituntilterminated; RunOnceId: "StopService"
Filename: "{#InstallUtilPath}"; Parameters: "/u ""{app}\{#ServiceExe}"""; Flags: runhidden waituntilterminated; RunOnceId: "UnregisterService"
