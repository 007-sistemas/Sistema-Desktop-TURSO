[Setup]
AppName=iDev Sistemas
AppVersion=1.0
DefaultDirName={userappdata}\iDev Sistemas
DefaultGroupName=iDev Sistemas
OutputDir=.
OutputBaseFilename=iDev SistemasSetup
Compression=lzma
SolidCompression=yes
ArchitecturesInstallIn64BitMode=x64



[Files]
; ATENÇÃO: Use a pasta win-x64 (NÃO a publish) para garantir que o instalador replique o ambiente funcional.
Source: "..\bin\*"; DestDir: "{app}"; Flags: ignoreversion recursesubdirs createallsubdirs

[Icons]
Name: "{group}\iDev Sistemas"; Filename: "{app}\iDev Sistemas.exe"
Name: "{commondesktop}\iDev Sistemas"; Filename: "{app}\iDev Sistemas.exe"; Tasks: desktopicon

[Tasks]
Name: "desktopicon"; Description: "Criar atalho na Área de Trabalho"; GroupDescription: "Opções adicionais:"

[Run]
Filename: "{app}\iDev Sistemas.exe"; Description: "Executar iDev Sistemas"; Flags: nowait postinstall skipifsilent
