; PTree Gold Inno Setup Script

#ifndef AppVersion
  #define AppVersion "0.0.0"
#endif

#ifndef SourceDir
  #define SourceDir "."
#endif

#define AppName "PTree Gold"
#define AppPublisher "Todd Whitehead"
#define AppURL "https://github.com/toddwhitehead/ptreegold"

[Setup]
AppId={{22DD7BBF-CE63-44D8-BFAA-0773122C111B}
AppName={#AppName}
AppVersion={#AppVersion}
AppPublisher={#AppPublisher}
AppPublisherURL={#AppURL}
AppSupportURL={#AppURL}
AppUpdatesURL={#AppURL}
DefaultDirName={autopf64}\PTreeGold
DisableProgramGroupPage=yes
OutputDir=Output
OutputBaseFilename=ptg-{#AppVersion}-win-x64-setup
Compression=lzma
SolidCompression=yes
ArchitecturesInstallIn64BitMode=x64compatible
ChangesEnvironment=yes

[Languages]
Name: "english"; MessagesFile: "compiler:Default.isl"

[Files]
Source: "{#SourceDir}\PTG.exe"; DestDir: "{app}"; Flags: ignoreversion
Source: "{#SourceDir}\appsettings.json"; DestDir: "{app}"; Flags: ignoreversion

[Tasks]
Name: modifypath; Description: "Add to system PATH (recommended for CLI use)"; GroupDescription: "Additional tasks:"; Flags: checkedonce

[Code]
const
  EnvironmentKey = 'SYSTEM\CurrentControlSet\Control\Session Manager\Environment';

procedure EnvAddPath(Path: string);
var
  Paths: string;
begin
  if not RegQueryStringValue(HKEY_LOCAL_MACHINE, EnvironmentKey, 'Path', Paths) then
    Paths := '';
  if Pos(';' + Uppercase(Path) + ';', ';' + Uppercase(Paths) + ';') > 0 then
    exit;
  while (Length(Paths) > 0) and (Paths[Length(Paths)] = ';') do
    SetLength(Paths, Length(Paths) - 1);
  if Length(Paths) > 0 then
    Paths := Paths + ';';
  RegWriteExpandStringValue(HKEY_LOCAL_MACHINE, EnvironmentKey, 'Path', Paths + Path);
end;

procedure EnvRemovePath(Path: string);
var
  Paths: string;
  SearchStr: string;
  P: Integer;
begin
  if not RegQueryStringValue(HKEY_LOCAL_MACHINE, EnvironmentKey, 'Path', Paths) then
    exit;
  if (Length(Paths) > 0) and (Paths[Length(Paths)] <> ';') then
    Paths := Paths + ';';
  SearchStr := Uppercase(Path) + ';';
  P := Pos(SearchStr, Uppercase(Paths));
  if P = 0 then
    exit;
  Delete(Paths, P, Length(SearchStr));
  if (Length(Paths) > 0) and (Paths[Length(Paths)] = ';') then
    SetLength(Paths, Length(Paths) - 1);
  RegWriteExpandStringValue(HKEY_LOCAL_MACHINE, EnvironmentKey, 'Path', Paths);
end;

procedure CurStepChanged(CurStep: TSetupStep);
begin
  if (CurStep = ssPostInstall) and WizardIsTaskSelected('modifypath') then
    EnvAddPath(ExpandConstant('{app}'));
end;

procedure CurUninstallStepChanged(CurUninstallStep: TUninstallStep);
begin
  if CurUninstallStep = usPostUninstall then
    EnvRemovePath(ExpandConstant('{app}'));
end;
