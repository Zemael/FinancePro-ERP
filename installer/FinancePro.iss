#define MyAppName "FinancePro"
#ifndef MyAppVersion
  #define MyAppVersion "6.71.0"
#endif
#define MyAppPublisher "FinancePro"
#define MyAppExeName "FinancePro.UI.exe"
#ifndef PublishedRoot
  #define PublishedRoot "..\dist\FinancePro-v6.71.0-win-x64\FinancePro"
#endif

[Setup]
AppId={{D5439810-569D-4C57-B893-DA07A32FC842}
AppName={#MyAppName}
AppVersion={#MyAppVersion}
AppPublisher={#MyAppPublisher}
DefaultDirName={autopf}\FinancePro
DefaultGroupName=FinancePro
DisableProgramGroupPage=yes
OutputDir=..\dist\installer
OutputBaseFilename=FinancePro-Setup-v{#MyAppVersion}-win-x64
SetupIconFile=..\FinancePro.UI\Resources\app-v4.ico
UninstallDisplayIcon={app}\{#MyAppExeName}
Compression=lzma2
SolidCompression=yes
WizardStyle=modern
PrivilegesRequired=admin
ArchitecturesAllowed=x64compatible
ArchitecturesInstallIn64BitMode=x64compatible
CloseApplications=yes
RestartApplications=no
VersionInfoVersion={#MyAppVersion}.0
VersionInfoProductName={#MyAppName}
VersionInfoProductVersion={#MyAppVersion}

[Languages]
Name: "portuguese"; MessagesFile: "compiler:Languages\Portuguese.isl"

[Tasks]
Name: "desktopicon"; Description: "Criar atalho no Ambiente de Trabalho"; GroupDescription: "Atalhos adicionais:"; Flags: unchecked

[Files]
Source: "{#PublishedRoot}\*"; DestDir: "{app}"; Excludes: "appsettings.json"; Flags: ignoreversion recursesubdirs createallsubdirs
Source: "{#PublishedRoot}\appsettings.json"; DestDir: "{app}"; Flags: onlyifdoesntexist uninsneveruninstall

[Icons]
Name: "{autoprograms}\FinancePro"; Filename: "{app}\{#MyAppExeName}"; WorkingDir: "{app}"
Name: "{autodesktop}\FinancePro"; Filename: "{app}\{#MyAppExeName}"; WorkingDir: "{app}"; Tasks: desktopicon

[Run]
Filename: "{app}\{#MyAppExeName}"; Description: "Abrir o FinancePro"; Flags: nowait postinstall skipifsilent
