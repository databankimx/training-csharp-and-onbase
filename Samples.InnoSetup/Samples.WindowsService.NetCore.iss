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

; *Migration Note: packages Samples.WindowsService.NetCore (the modern
;   net10.0, Generic Host + BackgroundService + AddWindowsService()
;   service), demonstrating sc.exe create/delete registration, genuinely
;   simpler than Samples.WindowsService.iss's InstallUtil.exe mechanism,
;   no separate installer assembly needed at all. See LectureNotes.md for
;   the full comparison.
;
;   Before compiling, publish the service first, self-contained so the
;   target machine doesn't need the .NET runtime installed separately:
;     dotnet publish ..\Samples.WindowsService.NetCore\Samples.WindowsService.NetCore.csproj -c Release -r win-x64 --self-contained true
;   This script expects that output at the relative path below.

#define AppName "Samples Windows Service (.NET Core)"
#define AppVersion "1.0.0"
#define ServiceName "Samples.WindowsService.NetCore"
#define ServiceExe "Samples.WindowsService.NetCore.exe"
#define PublishDir "..\Samples.WindowsService.NetCore\bin\Release\net10.0\win-x64\publish"

[Setup]
AppId={{C7B2F3D4-5E60-4B7C-9D0E-1F2A3B4C5D6E}
AppName={#AppName}
AppVersion={#AppVersion}
AppPublisher=DataBank IMX
DefaultDirName={autopf}\DataBank\{#ServiceName}
DefaultGroupName=DataBank Samples
DisableProgramGroupPage=yes
OutputBaseFilename=Samples.WindowsService.NetCore.Setup
Compression=lzma
SolidCompression=yes
PrivilegesRequired=admin
ArchitecturesInstallIn64BitMode=x64compatible

[Files]
Source: "{#PublishDir}\*"; DestDir: "{app}"; Flags: ignoreversion recursesubdirs createallsubdirs

[Run]
; *Migration Note: sc.exe create's "binPath=" REQUIRES a space immediately
;   after the "=" sign, a well-known, easy-to-miss sc.exe quirk, "binPath="
;   (no space) is silently treated as an unrecognized parameter and the
;   whole command fails. The quoted path itself also needs its own
;   surrounding quotes if it contains spaces (as {app} typically does,
;   since it's under "Program Files"), hence the doubled quotes below.
Filename: "{sys}\sc.exe"; Parameters: "create ""{#ServiceName}"" binPath= ""\""{app}\{#ServiceExe}\"""" start= auto"; Flags: runhidden waituntilterminated; StatusMsg: "Registering {#ServiceName}..."
Filename: "{sys}\sc.exe"; Parameters: "start ""{#ServiceName}"""; Flags: runhidden waituntilterminated; StatusMsg: "Starting {#ServiceName}..."

[UninstallRun]
Filename: "{sys}\sc.exe"; Parameters: "stop ""{#ServiceName}"""; Flags: runhidden waituntilterminated; RunOnceId: "StopService"
Filename: "{sys}\sc.exe"; Parameters: "delete ""{#ServiceName}"""; Flags: runhidden waituntilterminated; RunOnceId: "DeleteService"
