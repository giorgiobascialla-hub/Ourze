using System;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Text.RegularExpressions;
using System.Collections.Generic;
namespace HdrPilot {
// MFG Unlock is independent of RenoDX HDR and OptiScaler's frame-generation settings.
public static class MfgAddon {
 public const string Repo="nefh/MFGAmpereUnlock-RenoDx",Name="renodx-mfgunlock.addon64",Marker="hdlss-mfg-unlock.json";
 public const string Requirements="RTX 2000 / 3000 / 4000. Requires ReShade with full add-on support and native DLSS Frame Generation in the game. Enable FG and choose the multiplier in the game or in ReShade > MFG Unlock. Compatibility depends on the game, driver and DLSS-G / Streamline versions.";
 public sealed class Release {public string Tag,Url,Digest;public long Size;}
 public static Release ParseRelease(string json){
  var r=Core.Json.Deserialize<Dictionary<string,object>>(json);
  if(r==null||Convert.ToBoolean(r["draft"])||Convert.ToBoolean(r["prerelease"]))throw new Exception("MFG: no stable release available.");
  var assets=((System.Collections.IEnumerable)r["assets"]).Cast<Dictionary<string,object>>().Where(a=>Convert.ToString(a["name"])==Name).ToArray();
  if(assets.Length!=1)throw new Exception("MFG: unsupported release package. No game files changed.");
  var a1=assets[0];var result=new Release{Tag=Convert.ToString(r["tag_name"]),Url=Convert.ToString(a1["browser_download_url"]),Digest=a1.ContainsKey("digest")?Convert.ToString(a1["digest"]):"",Size=Convert.ToInt64(a1["size"])};
  Uri uri;if(!Uri.TryCreate(result.Url,UriKind.Absolute,out uri)||uri.Scheme!="https"||uri.Host!="github.com"||!uri.IsDefaultPort||uri.UserInfo!=""||!uri.AbsolutePath.StartsWith("/"+Repo+"/releases/download/",StringComparison.Ordinal)||Path.GetFileName(uri.AbsolutePath)!=Name||uri.Query!=""||uri.Fragment!=""||result.Size<4096||result.Size>32000000||!Regex.IsMatch(result.Digest,@"^sha256:[a-fA-F0-9]{64}$"))throw new Exception("MFG: invalid release origin or checksum.");
  return result;
 }
 public static void Verify(string file,Release release){if(new FileInfo(file).Length!=release.Size||!DlssCore.Pe64(file)||!DlssCore.Hash(file).Equals(release.Digest.Substring(7),StringComparison.OrdinalIgnoreCase))throw new Exception("MFG: download verification failed. No game files changed.");}
 public static string Fetch(out Release release){
  ServicePointManager.SecurityProtocol|=SecurityProtocolType.Tls12;
  using(var web=new DlssCore.DownloadClient()){
   web.Headers[HttpRequestHeader.UserAgent]="HDLSS";web.Headers[HttpRequestHeader.CacheControl]="no-cache";
   release=ParseRelease(web.DownloadString("https://api.github.com/repos/"+Repo+"/releases/latest"));
   string dir=Path.Combine(Core.Data,"mfg-packages",Guid.NewGuid().ToString("N"));Directory.CreateDirectory(dir);string file=Path.Combine(dir,Name);
   web.DownloadFile(release.Url,file);Verify(file,release);File.WriteAllText(Path.Combine(dir,"source.json"),Core.Json.Serialize(release));return file;
  }
 }
 // ReShade INI arrays use commas; doubled commas escape a literal comma.
 public static List<string> Entries(string value){var list=new List<string>();var item=new StringBuilder();value=value??"";for(int i=0;i<value.Length;i++){if(value[i]==','){if(i+1<value.Length&&value[i+1]==','){item.Append(',');i++;}else{if(item.Length>0)list.Add(item.ToString().Trim());item.Clear();}}else item.Append(value[i]);}if(item.Length>0)list.Add(item.ToString().Trim());return list;}
 static bool IsOurs(string value){return Path.GetFileName(value.Replace('/',Path.DirectorySeparatorChar)).Equals(Name,StringComparison.OrdinalIgnoreCase);}
 public static string ConfigName(string root){var loaders=RenoDx.Loaders(root);string custom=loaders.Count==1?Path.ChangeExtension(loaders[0],".ini"):"ReShade.ini";return !File.Exists(DlssCore.Safe(root,"ReShade.ini"))&&File.Exists(DlssCore.Safe(root,custom))?custom:"ReShade.ini";}
 public static string AddonName(string root,string ini){string folder=Core.Get(ini,"ADDON","AddonPath");if(String.IsNullOrWhiteSpace(folder))return Name;var dirs=Entries(folder);if(dirs.Count!=1)throw new Exception("MFG: ambiguous ReShade AddonPath.");string dest=Path.GetFullPath(Path.Combine(root,dirs[0],Name)),prefix=Path.GetFullPath(root).TrimEnd('\\','/')+Path.DirectorySeparatorChar;if(!dest.StartsWith(prefix,StringComparison.OrdinalIgnoreCase))throw new Exception("MFG: external ReShade AddonPath. Use the author's manual installation for this shared folder.");string relative=dest.Substring(prefix.Length);DlssCore.Safe(root,relative);return relative;}
 static string InstalledFile(string root){string config=DlssCore.Safe(root,ConfigName(root));return DlssCore.Safe(root,AddonName(root,File.Exists(config)?File.ReadAllText(config):""));}
 public static string EarlyLoad(string ini,bool enable){var entries=Entries(Core.Get(ini,"ADDON","LoadFromDllMain"));if(enable){if(entries.Any(IsOurs))return ini;entries.Add(Name);}else{if(!entries.Any(IsOurs))return ini;entries.RemoveAll(IsOurs);}return Core.Set(ini,"ADDON","LoadFromDllMain",String.Join(",",entries.Select(x=>x.Replace(",",",,"))));}
 static byte[] Encode(string path,string text){Encoding enc=new UTF8Encoding(false);if(File.Exists(path)){var b=File.ReadAllBytes(path);if(b.Length>1&&b[0]==255&&b[1]==254)enc=Encoding.Unicode;else if(b.Length>1&&b[0]==254&&b[1]==255)enc=Encoding.BigEndianUnicode;else if(b.Length>2&&b[0]==239&&b[1]==187&&b[2]==191)enc=new UTF8Encoding(true);}return enc.GetPreamble().Concat(enc.GetBytes(text)).ToArray();}
 public static string State(Game game){string root=Path.GetDirectoryName(game.Exe),file;try{file=InstalledFile(root);}catch(Exception ex){return ex.Message;}if(!File.Exists(file))return "MFG Unlock · not installed";if(!DlssCore.Pe64(file))return "MFG Unlock · invalid file";try{var m=DlssCore.ReadManifest(root,Marker);if(m.Files.Any(f=>IsOurs(f.Name)&&f.After==DlssCore.Hash(file)))return "MFG Unlock · "+m.Release+" · "+Appearance.Localize("Installed files; verify activation in game.");}catch{}return "MFG Unlock · "+Appearance.Localize("External installation; version not verified.");}
 public static bool Installed(Game game){try{return DlssCore.Pe64(InstalledFile(Path.GetDirectoryName(game.Exe)));}catch{return false;}}
 public static bool HasRecord(Game game){return File.Exists(DlssCore.Safe(Path.GetDirectoryName(game.Exe),Marker));}
 public static DlssPlan Prepare(Game game,string source,Release release){
  Core.Closed(game);Verify(source,release);string root=Path.GetDirectoryName(Path.GetFullPath(game.Exe));
  var scan=DlssCore.Inspect(game,true,false);if(scan.Blockers.Count>0)throw new Exception(String.Join("\n",scan.Blockers));
  var loaders=RenoDx.Loaders(root);if(loaders.Count!=1)throw new Exception("Install one ReShade loader with full add-on support first.");
  if(!Encoding.ASCII.GetString(File.ReadAllBytes(DlssCore.Safe(root,loaders[0]))).Contains("LoadFromDllMain"))throw new Exception("Install one ReShade loader with full add-on support first.");
  if(loaders[0]=="ReShade64.dll"&&(scan.Proxy==""||Core.Get(scan.Ini,"Plugins","LoadReshade")!="true"))throw new Exception("ReShade64.dll is not connected to OptiScaler. Configure ReShade first.");
  string config=ConfigName(root),path=DlssCore.Safe(root,config),ini=File.Exists(path)?File.ReadAllText(path):"",addonName=AddonName(root,ini),addonFolder=Path.GetDirectoryName(DlssCore.Safe(root,addonName));
  if(Entries(Core.Get(ini,"ADDON","LoadFromDllMain")).Where(IsOurs).Any(x=>!Path.GetFullPath(Path.Combine(addonFolder,x)).Equals(DlssCore.Safe(root,addonName),StringComparison.OrdinalIgnoreCase)))throw new Exception("MFG: early-load entry points to another installation. Resolve the duplicate first.");
  if(Entries(Core.Get(ini,"ADDON","DisabledAddons")).Any(x=>x.Split('@')[0]=="MFG Unlock"||x.IndexOf(Name,StringComparison.OrdinalIgnoreCase)>=0))throw new Exception("MFG Unlock is disabled in ReShade. Enable it in the ReShade add-ons menu first.");
  var conflicts=(Directory.Exists(addonFolder)?Directory.GetFiles(addonFolder,"*.addon64"):new string[0]).Concat(Directory.GetFiles(root,"*.addon64")).Distinct(StringComparer.OrdinalIgnoreCase).Where(f=>!f.Equals(DlssCore.Safe(root,addonName),StringComparison.OrdinalIgnoreCase)&&Regex.IsMatch(Path.GetFileName(f),"mfg|framegen",RegexOptions.IgnoreCase)).Select(Path.GetFileName).ToArray();
  if(conflicts.Length>0)throw new Exception("Another MFG add-on is present: "+String.Join(", ",conflicts));
  string marker=DlssCore.Safe(root,Marker);
  var manifest=File.Exists(marker)?DlssCore.ReadManifest(root,Marker):new DlssManifest{Game=game.Id,Folder=root,BackupRoot="mfg-backups/"+Guid.NewGuid().ToString("N"),PreviousOwner="Before MFG Unlock"};
  if(manifest.Files.Any(f=>IsOurs(f.Name)&&!f.Name.Equals(addonName,StringComparison.OrdinalIgnoreCase)))throw new Exception("MFG: add-on folder changed. Restore the previous installation first.");
  manifest.Build=Repo;manifest.Release=release.Tag;
  var plan=new DlssPlan{Folder=root,MarkerName=Marker,MarkerExpected=File.Exists(marker)?DlssCore.Hash(marker):null,Manifest=manifest,Summary=game.Name+"\nMFG Unlock · "+release.Tag+"\n"+release.Url+"\nSHA256: "+release.Digest.Substring(7)+"\n\n"+Appearance.Localize(Requirements)+"\n"+Appearance.Localize("NVIDIA / Streamline files and existing HDR settings are preserved.")};
  string dest=DlssCore.Safe(root,addonName);plan.Changes.Add(new DlssChange{Name=addonName,Source=source,SourceHash=DlssCore.Hash(source),Expected=File.Exists(dest)?DlssCore.Hash(dest):null});
  string updated=EarlyLoad(ini,true);
  if(updated!=ini)plan.Changes.Add(new DlssChange{Name=config,Expected=File.Exists(path)?DlssCore.Hash(path):null,Bytes=Encode(path,updated)});
  return plan;
 }
 public static DlssPlan Restore(Game game){
  Core.Closed(game);string root=Path.GetDirectoryName(Path.GetFullPath(game.Exe));var m=DlssCore.ReadManifest(root,Marker);
  var plan=new DlssPlan{Folder=root,MarkerName=Marker,Uninstall=true,Summary=Appearance.Localize("Restore MFG Unlock. Later ReShade settings and other add-ons are preserved.")};
  foreach(var f in m.Files){
   string path=DlssCore.Safe(root,f.Name),hash=File.Exists(path)?DlssCore.Hash(path):null,backup=f.Before==null?null:DlssCore.Safe(Core.Data,f.Backup);
   if(backup!=null&&(!File.Exists(backup)||DlssCore.Hash(backup)!=f.Before))throw new Exception("MFG: backup missing or changed: "+f.Name);
   if(!IsOurs(f.Name)&&!f.Name.EndsWith(".ini",StringComparison.OrdinalIgnoreCase))throw new Exception("MFG: unexpected file in installation record.");
   if(hash==f.After){plan.Changes.Add(new DlssChange{Name=f.Name,Expected=hash,Source=backup,SourceHash=f.Before,Delete=backup==null,RestoreAttributes=f.Attributes});continue;}
   if(IsOurs(f.Name))throw new Exception("MFG: add-on changed after installation. Restore stopped.");
   // ReShade writes its INI during normal gameplay; undo only our early-load entry.
   if(!File.Exists(path))continue;
   string original=backup==null?"":File.ReadAllText(backup);if(Entries(Core.Get(original,"ADDON","LoadFromDllMain")).Any(IsOurs))continue;
   string current=File.ReadAllText(path),updated=EarlyLoad(current,false);if(current!=updated)plan.Changes.Add(new DlssChange{Name=f.Name,Expected=hash,Bytes=Encode(path,updated)});
  }
  plan.Changes.Add(new DlssChange{Name=Marker,Expected=DlssCore.Hash(DlssCore.Safe(root,Marker)),Delete=true});return plan;
 }
}
}
