; Rigs of Rods Online Setup
; Created by Edgar (AnotherFoxGuy)

#define InstallerName "Rigs of Rods"
#define InstallerVersion "1.1"
#define InstallerPublisher "Rigs of Rods"
#define InstallerURL "https://www.rigsofrods.org"

[Setup]
; NOTE: The value of AppId uniquely identifies this application.
; Do not use the same AppId value in installers for other applications.
; (To generate a new GUID, click Tools | Generate GUID inside the IDE.)
AppId={{207CE13A-211A-4936-B294-9F99A531F04E}
AppName={#InstallerName}
AppVersion={#InstallerVersion}
VersionInfoVersion={#InstallerVersion}
AppPublisher={#InstallerPublisher}
AppPublisherURL={#InstallerURL}
AppSupportURL={#InstallerURL}
AppUpdatesURL={#InstallerURL}
DefaultDirName={commonpf}\Rigs of Rods
DefaultGroupName=Rigs of Rods
DisableProgramGroupPage=yes
LicenseFile=LICENSE
OutputDir=Build
OutputBaseFilename=RoR_Online_Setup
SetupIconFile=Icons\ror.ico
Compression=lzma2/ultra
SolidCompression=yes
; "ArchitecturesInstallIn64BitMode=x64" requests that the install be
; done in "64-bit mode" on x64, meaning it should use the native
; 64-bit Program Files directory and the 64-bit view of the registry.
; On all other architectures it will install in "32-bit mode".
ArchitecturesInstallIn64BitMode=x64
DisableWelcomePage=no
; Custom images
; 64x64
WizardSmallImageFile=Icons\ror-64.bmp
; 164x314
WizardImageFile=Icons\RoRSetupLarge.bmp
InternalCompressLevel=ultra
DisableFinishedPage=True
ShowLanguageDialog=no

[Tasks]
Name: "desktopicon"; Description: "{cm:CreateDesktopIcon}"; GroupDescription: "{cm:AdditionalIcons}"; Flags: unchecked

[Files]
Source: "Client\bin\Release\ror-updater.exe"; DestDir: "{app}"; Flags: ignoreversion
Source: "Icons\ror.ico"; DestDir: "{app}"
Source: "Client\bin\Release\*.dll"; DestDir: "{app}"; Flags: ignoreversion createallsubdirs recursesubdirs

[Icons]
; Start Menu
Name: "{group}\Rigs of Rods"; Filename: "{app}\RoR.exe"; IconFilename: "{app}\ror.ico"; IconIndex: 0
Name: "{group}\Rigs of Rods Updater"; Filename: "{app}\ror-updater.exe"
Name: "{group}\{cm:UninstallProgram,{#InstallerName}}"; Filename: "{uninstallexe}"
; Desktop
Name: "{commondesktop}\Rigs of Rods"; Filename: "{app}\RoR.exe"; IconFilename: "{app}\ror.ico"; IconIndex: 0; Tasks: desktopicon

[Run]
Filename: "{app}\ror-updater.exe"; WorkingDir: "{app}"; Flags: nowait
