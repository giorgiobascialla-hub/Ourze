using System;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Net;
using System.Text;
using System.Collections.Generic;
using System.Text.RegularExpressions;
namespace HdrPilot {
public static class Ue4ssInstaller {
 public const string Marker="hdlss-ue4ss.json";
 public static string RuntimeFolder(Game g){
  string root=Path.GetDirectoryName(Path.GetFullPath(g.Exe));
  if(File.Exists(DlssCore.Safe(root,"override.txt")))throw new Exception("UE4SS override.txt detected. This custom installation must be configured manually; its files are preserved.");
  bool nested=File.Exists(DlssCore.Safe(root,"ue4ss/UE4SS.dll")),flat=File.Exists(DlssCore.Safe(root,"UE4SS.dll"));
  if(nested&&flat)throw new Exception("Two UE4SS runtimes detected. Resolve the duplicate installation before adding the reader.");
  return nested?Path.Combine(root,"ue4ss"):root;
 }
 public static string Check(Game g){
  if(g==null||String.IsNullOrEmpty(g.Exe)||!File.Exists(g.Exe))return "Select the actual game executable first.";
  if(g.ProtectedInstall)return "The game folder is protected. Choose a writable installation.";
  if(!DlssCore.Pe64(g.Exe))return "UE4SS requires a supported 64-bit Unreal Engine game.";
  string folder=RuntimeFolder(g);if(File.Exists(Path.Combine(folder,"UE4SS.dll")))return "";
  var bin=Directory.GetParent(Path.GetDirectoryName(g.Exe));
  if(!g.Confirmed&&(bin==null||!bin.Name.Equals("Binaries",StringComparison.OrdinalIgnoreCase)))return "Unreal Engine was not identified. Select the real executable in Binaries/Win64.";
  if(File.Exists(Path.Combine(folder,"dwmapi.dll")))return "dwmapi.dll already exists. Its loader is preserved; configure a compatible UE4SS installation manually.";
  return "";
 }
 public static DlssPlan Prepare(Game g,string zip){
  Core.Closed(g);string error=Check(g);if(error!="")throw new Exception(error);
  string root=Path.GetDirectoryName(Path.GetFullPath(g.Exe));
  if(File.Exists(Path.Combine(RuntimeFolder(g),"UE4SS.dll")))throw new Exception("UE4SS is already installed. Keep the game's existing version and install only the reader.");
  if(File.Exists(DlssCore.Safe(root,Marker)))throw new Exception("A previous UE4SS installation record exists. Recover that operation before reinstalling.");
  var p=new DlssPlan{Folder=root,MarkerName=Marker,Manifest=new DlssManifest{Game=g.Id,Folder=root,PreviousOwner="Before UE4SS installation",BackupRoot="ue4ss-backups/"+Guid.NewGuid().ToString("N")},Summary="Install official UE4SS (stable) and the HDLSS engine reader. Game-specific compatibility is not guaranteed: check a fresh UE4SS.log after launching. Existing mod files are preserved. Recover operation can undo this installation using its transaction record."};
  long total=0;var names=new HashSet<string>(StringComparer.OrdinalIgnoreCase);
  using(var stream=File.OpenRead(zip))using(var archive=new ZipArchive(stream,ZipArchiveMode.Read))foreach(var e in archive.Entries){
   string name=e.FullName.Replace('\\','/');if(name.EndsWith("/"))continue;
   if(name=="README.md"||name=="Changelog.md")continue;
   if(!(name=="UE4SS.dll"||name=="dwmapi.dll"||name=="UE4SS-settings.ini"||name.StartsWith("Mods/",StringComparison.Ordinal)))throw new Exception("Unsupported UE4SS package layout: "+name);
   string dest=DlssCore.Safe(root,name);total+=e.Length;
   if(!names.Add(name)||names.Count>2000||total>100*1024*1024||e.Length>50*1024*1024||((e.ExternalAttributes>>16)&0xF000)==0xA000)throw new Exception("Unsafe UE4SS archive.");
   // Existing mod configuration belongs to the user, even without an installation manifest.
   if(File.Exists(dest)||Directory.Exists(dest)){if(name=="UE4SS.dll"||name=="dwmapi.dll")throw new Exception("Existing loader preserved: "+name);continue;}
   using(var input=e.Open())using(var output=new MemoryStream()){input.CopyTo(output);p.Changes.Add(new DlssChange{Name=name,Bytes=output.ToArray()});}
  }
  if(!p.Changes.Any(c=>c.Name=="UE4SS.dll")||!p.Changes.Any(c=>c.Name=="dwmapi.dll"))throw new Exception("The package does not contain the required UE4SS runtime and loader.");
  string reader="Mods/HDRPilotTelemetry/";
  if(Directory.Exists(DlssCore.Safe(root,"Mods/HDRPilotTelemetry")))throw new Exception("An existing HDLSS reader folder must be reviewed before installation.");
  string modList=DlssCore.Safe(root,"Mods/mods.txt");if(File.Exists(modList)&&Regex.IsMatch(File.ReadAllText(modList),@"(?im)^\s*HDRPilotTelemetry\s*:"))throw new Exception("An existing HDRPilotTelemetry entry in mods.txt must be reviewed before installation.");
  p.Changes.Add(new DlssChange{Name=reader+"Scripts/main.lua",Bytes=new UTF8Encoding(false).GetBytes(Telemetry.Payload())});
  p.Changes.Add(new DlssChange{Name=reader+"enabled.txt",Bytes=new byte[0]});return p;
 }
 public static DlssPlan DownloadAndPrepare(Game g){
  string error=Check(g);if(error!="")throw new Exception(error);
  ServicePointManager.SecurityProtocol|=SecurityProtocolType.Tls12;
  using(var web=new DlssCore.DownloadClient()){
   web.Headers[HttpRequestHeader.UserAgent]="HDLSS/1.0.3";
   var release=Core.Json.Deserialize<Dictionary<string,object>>(web.DownloadString("https://api.github.com/repos/UE4SS-RE/RE-UE4SS/releases/latest"));
   if(Convert.ToBoolean(release["prerelease"])||Convert.ToBoolean(release["draft"]))throw new Exception("A stable UE4SS release is required.");
   var assets=((System.Collections.IEnumerable)release["assets"]).Cast<Dictionary<string,object>>();
   var asset=assets.Single(a=>Regex.IsMatch(Convert.ToString(a["name"]),@"^UE4SS_v[0-9]+\.[0-9]+\.[0-9]+\.zip$"));
   string url=Convert.ToString(asset["browser_download_url"]);
   if(!url.StartsWith("https://github.com/UE4SS-RE/RE-UE4SS/releases/download/",StringComparison.Ordinal))throw new Exception("Unexpected UE4SS download source.");
   string folder=Path.Combine(Core.Data,"ue4ss-downloads",Guid.NewGuid().ToString("N"));Directory.CreateDirectory(folder);string zip=Path.Combine(folder,"UE4SS.zip");
   web.DownloadFile(url,zip);if(new FileInfo(zip).Length!=Convert.ToInt64(asset["size"]))throw new Exception("Incomplete UE4SS download.");
   object digest;if(asset.TryGetValue("digest",out digest)&&Convert.ToString(digest).StartsWith("sha256:"))if(!String.Equals(Convert.ToString(digest).Substring(7),DlssCore.Hash(zip),StringComparison.OrdinalIgnoreCase))throw new Exception("UE4SS checksum mismatch.");
   File.WriteAllText(zip+".source.txt",url+"\nSHA256 "+DlssCore.Hash(zip));var p=Prepare(g,zip);p.Summary="UE4SS "+release["tag_name"]+"\n"+p.Summary;return p;
  }
 }
 public static string InstallLabel(bool runtime){int i=Array.IndexOf(new[]{"en","it","es","fr","de","pt"},Core.Pref.Language);if(i<0)i=0;return (runtime?new[]{"Install UE4SS + reader","Installa UE4SS + lettore","Instalar UE4SS + lector","Installer UE4SS + lecteur","UE4SS + Leser installieren","Instalar UE4SS + leitor"}:new[]{"Enable UE4SS reader","Attiva lettore UE4SS","Activar lector UE4SS","Activer le lecteur UE4SS","UE4SS-Leser aktivieren","Ativar leitor UE4SS"})[i];}
}
}
