using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Net;
using System.Text;
using System.Text.RegularExpressions;
namespace HdrPilot {
public static class Streamline {
 public const string Repo="NVIDIA-RTX/Streamline", Marker="hdlss-streamline.json";
 public static readonly string[] Names={"sl.interposer.dll","sl.common.dll","sl.dlss.dll","sl.dlss_d.dll","sl.dlss_g.dll","sl.reflex.dll","sl.pcl.dll","sl.nis.dll","sl.deepdvc.dll","sl.directsr.dll","sl.nvperf.dll"};
 public const string Help="Choose a Streamline version and an existing game folder, then install with automatic backup. This does not add support to games. Only Streamline 2.x integrations are supported; major-version migrations are blocked. Use Removal to restore originals.";
 static string L(string s){return Appearance.Localize(s);}
 public static List<DlssLibrary> Scan(Game g){return DlssLibraries.ScanNames(g,Names);}
 public static string Summary(Game g){var files=Scan(g);if(files.Count==0)return L("Streamline: not detected");string root=Path.GetDirectoryName(g.Exe).TrimEnd('\\');return "Streamline · "+files.Select(f=>Path.GetDirectoryName(f.Path)).Distinct(StringComparer.OrdinalIgnoreCase).Count()+" "+L("folders")+"\n"+String.Join("\n",files.GroupBy(f=>Path.GetDirectoryName(f.Path),StringComparer.OrdinalIgnoreCase).Select(group=>{string folder=group.Key.Equals(root,StringComparison.OrdinalIgnoreCase)?".":group.Key.StartsWith(root+"\\",StringComparison.OrdinalIgnoreCase)?group.Key.Substring(root.Length+1):group.Key;return folder+" · "+String.Join(" / ",group.Select(f=>f.Version.Replace(",",".").Replace(" ","")).Distinct());}));}
 public static Version VersionOf(string path){var v=FileVersionInfo.GetVersionInfo(path);if(v.FileMajorPart<1)throw new Exception(L("Streamline version is unavailable."));return new Version(v.FileMajorPart,v.FileMinorPart,v.FileBuildPart,v.FilePrivatePart);}
 public static void Validate(string path,string name){
  if(!Names.Contains(name,StringComparer.OrdinalIgnoreCase)||!DlssCore.Pe64(path))throw new Exception(L("Invalid Streamline x64 component."));
  var v=FileVersionInfo.GetVersionInfo(path);string expected=Path.GetFileNameWithoutExtension(name);if(name.Equals("sl.dlss_d.dll",StringComparison.OrdinalIgnoreCase))expected="sl.dlss_rr";
  if(!String.Equals(v.InternalName,expected,StringComparison.OrdinalIgnoreCase)||(v.ProductName??"").IndexOf("STREAMLINE PRODUCTION",StringComparison.OrdinalIgnoreCase)<0)throw new Exception(L("Only matching NVIDIA production components are accepted."));
  DlssLibraries.VerifyNvidiaSignature(path);VersionOf(path);
 }
 public static void CheckTarget(Game g,string folder){
  Core.Closed(g);if(g.ProtectedInstall)throw new Exception(g.Evidence);string root=Path.GetFullPath(g.Root).TrimEnd('\\')+"\\",full=Path.GetFullPath(folder).TrimEnd('\\')+"\\";
  if(!full.StartsWith(root,StringComparison.OrdinalIgnoreCase))throw new Exception(L("Select a detected Streamline folder inside the game."));
  string interposer=Path.Combine(folder,"sl.interposer.dll"),common=Path.Combine(folder,"sl.common.dll");
  DlssCore.Safe(g.Root,interposer.Substring(root.Length));
  if(!File.Exists(interposer)||!File.Exists(common))throw new Exception(L("A complete existing integration requires sl.interposer.dll and sl.common.dll in the selected folder."));
  if(VersionOf(interposer).Major!=2)throw new Exception(L("Only existing Streamline 2.x integrations can be updated. Major-version migrations are blocked."));
 }
 public static string Extract(string zip){
  string dir=Path.Combine(Core.Data,"streamline","packages",Guid.NewGuid().ToString("N"));Directory.CreateDirectory(dir);
  using(var file=File.OpenRead(zip))using(var archive=new ZipArchive(file,ZipArchiveMode.Read)){
   var entries=archive.Entries.Where(e=>Regex.IsMatch(e.FullName.Replace('\\','/'),@"^(?:[^/]+/)?bin/x64/sl\.[^/]+\.dll$",RegexOptions.IgnoreCase)).ToList();
   if(entries.Count==0||entries.Any(e=>!Names.Contains(e.Name,StringComparer.OrdinalIgnoreCase))||entries.Select(e=>e.Name).Distinct(StringComparer.OrdinalIgnoreCase).Count()!=entries.Count)throw new Exception(L("Unsupported or ambiguous Streamline production package."));
   foreach(var e in entries){if(e.Length<1024||e.Length>64*1024*1024)throw new Exception(L("Invalid Streamline package size."));string dest=DlssCore.Safe(dir,e.Name);using(var input=e.Open())using(var output=File.Create(dest))input.CopyTo(output);Validate(dest,e.Name);}
  }
  string main=Path.Combine(dir,"sl.interposer.dll");if(!File.Exists(main)||!File.Exists(Path.Combine(dir,"sl.common.dll")))throw new Exception(L("Incomplete Streamline package."));
  var version=VersionOf(main);if(version.Major!=2||Directory.GetFiles(dir,"*.dll").Any(p=>VersionOf(p)!=version))throw new Exception(L("Mixed or unsupported Streamline versions."));return dir;
 }
 static bool HasPackage(Dictionary<string,object> release){try{string tag=Convert.ToString(release["tag_name"]);return ((System.Collections.IEnumerable)release["assets"]).Cast<Dictionary<string,object>>().Count(a=>Convert.ToString(a["name"])=="streamline-sdk-"+tag+".zip"&&a.ContainsKey("digest")&&Regex.IsMatch(Convert.ToString(a["digest"]),@"^sha256:[a-fA-F0-9]{64}$")&&Convert.ToString(a["browser_download_url"])=="https://github.com/"+Repo+"/releases/download/"+tag+"/streamline-sdk-"+tag+".zip")==1;}catch{return false;}}
 public static List<string> Tags(string json){return Core.Json.Deserialize<List<Dictionary<string,object>>>(json).Where(r=>HasPackage(r)&&!Convert.ToBoolean(r["draft"])&&!Convert.ToBoolean(r["prerelease"])&&Regex.IsMatch(Convert.ToString(r["tag_name"]),@"^v2\.\d+\.\d+$")).Select(r=>Convert.ToString(r["tag_name"])).OrderByDescending(v=>new Version(v.Substring(1))).Distinct().ToList();}
 public static bool ValidCatalog(string json){try{return Tags(json).Count>0;}catch{return false;}}
 public static List<string> LocalVersions(){return Tags(CatalogStore.Local("StreamlineSeed.json",ValidCatalog));}
 public static List<string> Versions(){return Tags(CatalogStore.Refresh("StreamlineSeed.json","https://api.github.com/repos/"+Repo+"/releases?per_page=20",ValidCatalog));}
 public static string Latest(Action<string> progress){return Download(null,progress);}
 public static string Download(string selectedTag,Action<string> progress){
  if(selectedTag!=null&&!Regex.IsMatch(selectedTag,@"^v2\.\d+\.\d+$"))throw new Exception("Invalid Streamline version.");
  ServicePointManager.SecurityProtocol|=SecurityProtocolType.Tls12;
  using(var web=new DlssCore.DownloadClient()){
   web.Headers[HttpRequestHeader.UserAgent]="HDLSS";web.Headers[HttpRequestHeader.CacheControl]="no-cache";progress(L("Checking the latest NVIDIA Streamline release…"));
   var r=Core.Json.Deserialize<Dictionary<string,object>>(web.DownloadString("https://api.github.com/repos/"+Repo+"/releases/"+(selectedTag==null?"latest":"tags/"+selectedTag)));string tag=Convert.ToString(r["tag_name"]);
   if(selectedTag!=null&&tag!=selectedTag)throw new Exception("Streamline release version mismatch.");
   if(!Regex.IsMatch(tag,@"^v2\.\d+\.\d+$")||Convert.ToBoolean(r["prerelease"])||Convert.ToBoolean(r["draft"]))throw new Exception(L("Unsupported Streamline release."));
   var assets=((System.Collections.IEnumerable)r["assets"]).Cast<Dictionary<string,object>>().Where(asset=>Convert.ToString(asset["name"])=="streamline-sdk-"+tag+".zip").ToList();if(assets.Count!=1)throw new Exception(L("Official x64 Streamline package not found."));
   var a=assets[0];string url=Convert.ToString(a["browser_download_url"]),digest=a.ContainsKey("digest")?Convert.ToString(a["digest"]):"";long size=Convert.ToInt64(a["size"]);
   if(url!="https://github.com/"+Repo+"/releases/download/"+tag+"/streamline-sdk-"+tag+".zip"||!Regex.IsMatch(digest,@"^sha256:[0-9a-fA-F]{64}$")||size<1024||size>1024L*1024*1024)throw new Exception(L("Streamline release integrity metadata is invalid."));
   string cache=Path.Combine(Core.Data,"streamline");Directory.CreateDirectory(cache);string zip=Path.Combine(cache,Guid.NewGuid().ToString("N")+".zip");
   try{progress(L("Downloading and verifying NVIDIA Streamline…"));web.DownloadFile(url,zip);if(new FileInfo(zip).Length!=size||!String.Equals(DlssCore.Hash(zip),digest.Substring(7),StringComparison.OrdinalIgnoreCase))throw new Exception(L("Streamline package checksum mismatch."));string folder=Extract(zip);if(VersionOf(Path.Combine(folder,"sl.interposer.dll")).ToString(3)!=tag.Substring(1))throw new Exception(L("Streamline release version mismatch."));return folder;}finally{if(File.Exists(zip))File.Delete(zip);}
  }
 }
 public static void ValidateBackup(DlssFile managed){if(managed!=null&&managed.Before!=null&&(!File.Exists(DlssCore.Safe(Core.Data,managed.Backup))||DlssCore.Hash(DlssCore.Safe(Core.Data,managed.Backup))!=managed.Before))throw new Exception(L("Streamline original backup missing or changed. Keep the original HDLSS data folder.")+" "+managed.Name);}
 public static DlssPlan Prepare(Game g,string folder,string package){
  CheckTarget(g,folder);if(Directory.GetFiles(folder,"sl.*.dll").Any(f=>!Names.Contains(Path.GetFileName(f),StringComparer.OrdinalIgnoreCase)))throw new Exception(L("Unknown Streamline component. Update stopped to avoid mixing versions."));var all=Scan(g);var target=all.Where(f=>String.Equals(Path.GetDirectoryName(f.Path),Path.GetFullPath(folder),StringComparison.OrdinalIgnoreCase)).ToList();
  var version=VersionOf(Path.Combine(package,"sl.interposer.dll"));
  if(version.Major!=2||target.Any(t=>VersionOf(t.Path).Major!=version.Major))throw new Exception(L("Mixed or unsupported Streamline versions."));
  var wanted=target.Select(f=>f.File).ToList();if((wanted.Contains("sl.reflex.dll",StringComparer.OrdinalIgnoreCase)||wanted.Contains("sl.dlss_g.dll",StringComparer.OrdinalIgnoreCase))&&!wanted.Contains("sl.pcl.dll",StringComparer.OrdinalIgnoreCase))wanted.Add("sl.pcl.dll");
  string marker=DlssCore.Safe(g.Root,Marker);var m=File.Exists(marker)?DlssCore.ReadManifest(g.Root,Marker):new DlssManifest{Game=g.Id,Folder=Path.GetFullPath(g.Root),PreviousOwner="Streamline before HDLSS",BackupRoot="streamline-backups/"+Guid.NewGuid().ToString("N")};
  if(m.Game!=g.Id)throw new Exception(L("Streamline backup belongs to another game."));
  var p=new DlssPlan{Folder=Path.GetFullPath(g.Root),NativeSwap=true,MarkerName=Marker,Manifest=m,MarkerExpected=File.Exists(marker)?DlssCore.Hash(marker):null,Summary=g.Name+"\nStreamline "+version+"\n"+folder+"\n"+L("Only this folder is updated. NVIDIA DLSS DLLs, JSON settings and other mods are preserved. Game compatibility is not guaranteed; restore if needed.")};
  foreach(string name in wanted){string source=DlssCore.Safe(package,name);if(!File.Exists(source))throw new Exception(L("The package is missing a component already used by the game.")+" "+name);Validate(source,name);if(VersionOf(source)!=version)throw new Exception(L("Mixed or unsupported Streamline versions."));string dest=Path.Combine(folder,name),rel=dest.Substring(p.Folder.TrimEnd('\\').Length+1);DlssCore.Safe(g.Root,rel);string before=File.Exists(dest)?DlssCore.Hash(dest):null,after=DlssCore.Hash(source);var managed=m.Files.FirstOrDefault(f=>f.Name.Equals(rel,StringComparison.OrdinalIgnoreCase));ValidateBackup(managed);if(managed!=null&&managed.After!=before){Validate(dest,name);p.Summary+="\n"+L("Changed Streamline file: current file will be backed up; original backup is preserved.")+" "+rel;}if(before!=after||managed!=null&&managed.After!=before)p.Changes.Add(new DlssChange{Name=rel,Source=source,SourceHash=after,Expected=before});}
  if(p.Changes.Count==0)throw new Exception(L("Streamline files are already identical. No changes needed."));m.Release=version.ToString();return p;
 }
 public static DlssPlan Restore(Game g,string folder){
  Core.Closed(g);if(g.ProtectedInstall)throw new Exception(g.Evidence);var m=DlssCore.ReadManifest(g.Root,Marker);if(m.Game!=g.Id)throw new Exception(L("Streamline backup belongs to another game."));
  var files=m.Files.Where(f=>String.Equals(Path.GetDirectoryName(DlssCore.Safe(g.Root,f.Name)),Path.GetFullPath(folder),StringComparison.OrdinalIgnoreCase)).ToList();if(files.Count==0)throw new Exception(L("No Streamline backup for this folder."));
  var p=new DlssPlan{Folder=Path.GetFullPath(g.Root),NativeSwap=true,MarkerName=Marker,Uninstall=true,Summary=L("Restore Streamline")+"\n"+folder};
  foreach(var f in files){if(!Names.Contains(Path.GetFileName(f.Name),StringComparer.OrdinalIgnoreCase))throw new Exception(L("Invalid Streamline backup."));string dest=DlssCore.Safe(g.Root,f.Name),backup=f.Before==null?null:DlssCore.Safe(Core.Data,f.Backup);if(!File.Exists(dest)||DlssCore.Hash(dest)!=f.After||(backup!=null&&(!File.Exists(backup)||DlssCore.Hash(backup)!=f.Before)))throw new Exception(L("Streamline files or backups changed outside HDLSS. Restore or inspect them first."));p.Changes.Add(new DlssChange{Name=f.Name,Source=backup,SourceHash=f.Before,Delete=backup==null,Expected=f.After,RestoreAttributes=f.Attributes});}
  m.Files=m.Files.Except(files).ToList();p.Changes.Add(new DlssChange{Name=Marker,Expected=DlssCore.Hash(DlssCore.Safe(g.Root,Marker)),Delete=m.Files.Count==0,Bytes=m.Files.Count==0?null:new UTF8Encoding(false).GetBytes(Core.Json.Serialize(m))});return p;
 }
}
}
