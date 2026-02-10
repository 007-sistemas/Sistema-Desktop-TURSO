[Setup]
AppName=iDev Sistemas
AppVersion=1.0
DefaultDirName={userappdata}\BiometricSystem
DefaultGroupName=BiometricSystem
OutputDir=.
OutputBaseFilename=BiometricSystemSetup
Compression=lzma
SolidCompression=yes
ArchitecturesInstallIn64BitMode=x64



[Files]
; ATENÇÃO: Use a pasta win-x64 (NÃO a publish) para garantir que o instalador replique o ambiente funcional.
Source: "..\bin\*"; DestDir: "{app}"; Flags: ignoreversion recursesubdirs createallsubdirs

[Icons]
Name: "{group}\BiometricSystem"; Filename: "{app}\BiometricSystem.exe"
Name: "{commondesktop}\BiometricSystem"; Filename: "{app}\BiometricSystem.exe"; Tasks: desktopicon

[Tasks]
Name: "desktopicon"; Description: "Criar atalho na Área de Trabalho"; GroupDescription: "Opções adicionais:"

[Run]
Filename: "{app}\BiometricSystem.exe"; Description: "Executar BiometricSystem"; Flags: nowait postinstall skipifsilent
