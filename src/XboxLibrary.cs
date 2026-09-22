using System;
using System.IO;
using System.Linq;
using System.Collections.Generic;
using System.Xml;
using System.Text;
using System.Text.RegularExpressions;
using System.Runtime.InteropServices;
using Microsoft.Win32;
using Microsoft.Win32.SafeHandles;
namespace HdrPilot {
public static class XboxLibrary {
 [DllImport("kernel32.dll",CharSet=CharSet.Unicode,SetLastError=true)] static extern SafeFileHandle CreateFile(string p,uint a,uint s,IntPtr sec,uint mode,uint flags,IntPtr template);
 [DllImport("kernel32.dll",CharSet=CharSet.Unicode,SetLastError=true)] static extern uint GetFinalPathNameByHandle(SafeFileHandle h,StringBuilder b,uint size,uint flags);
 static string Resolve(string p){if(p.StartsWith(@"\\?\"))p=p.Substring(4);using(var h=CreateFile(p,0,7,IntPtr.Zero,3,0x02000000,IntPtr.Zero)){if(!h.IsInvalid){var b=new StringBuilder(32768);uint n=GetFinalPathNameByHandle(h,b,(uint)b.Capacity,0);if(n>0&&n<b.Capacity)p=b.ToString();}}return p.StartsWith(@"\\?\")?p.Substring(4):p;}
 static XmlDocument Read(string p){var doc=new XmlDocument{XmlResolver=null};using(var r=XmlReader.Create(p,new XmlReaderSettings{DtdProcessing=DtdProcessing.Prohibit,XmlResolver=null}))doc.Load(r);return doc;}
 static string Attr(XmlNode n,string a){return n==null||n.Attributes[a]==null?"":n.Attributes[a].Value;}
 static string Asset(string root,string relative){if(String.IsNullOrEmpty(relative)||Path.IsPathRooted(relative))return "";string p=Path.GetFullPath(Path.Combine(root,relative));if(!Core.Under(p,root))return "";return Core.Files(Path.GetDirectoryName(p),Path.GetFileNameWithoutExtension(p)+"*"+Path.GetExtension(p)).OrderByDescending(f=>new FileInfo(f).Length).FirstOrDefault()??"";}
 // Compare decoded pixel dimensions, never compressed file sizes. Only inspect artwork folders.
 public static string BestCover(string root,XmlNode shell,XmlNode visual){
  var paths=new HashSet<string>(StringComparer.OrdinalIgnoreCase);
  foreach(var node in new[]{shell,visual})foreach(string key in new[]{"Square150x150Logo","Square310x310Logo","SplashScreenImage","StoreLogo"}){
   string asset=Asset(root,Attr(node,key));if(asset=="")continue;paths.Add(asset);
   foreach(string ext in new[]{"*.png","*.jpg","*.jpeg"})foreach(string file in Core.Files(Path.GetDirectoryName(asset),ext))paths.Add(file);
  }
  string best="";double bestScore=0;
  foreach(string file in paths)try{
   using(var stream=File.OpenRead(file)){
    var decoder=System.Windows.Media.Imaging.BitmapDecoder.Create(stream,System.Windows.Media.Imaging.BitmapCreateOptions.DelayCreation,System.Windows.Media.Imaging.BitmapCacheOption.None);
    var frame=decoder.Frames[0];double width=frame.PixelWidth,height=frame.PixelHeight;
    // Resolution remaining after filling a portrait card. Prefer portrait art at comparable quality.
    double usable=Math.Min(height,width/.75);
    double score=Math.Min(usable,1200)*(width/height<.95?1.2:1);
    if(score>bestScore){bestScore=score;best=file;}
   }
  }catch{}
  return best;
 }
 public static Game Parse(string root,string package){root=Path.GetFullPath(root);string mf=Path.Combine(root,"AppxManifest.xml"),gdk=Path.Combine(root,"MicrosoftGame.config");XmlDocument doc=File.Exists(mf)?Read(mf):null,config=File.Exists(gdk)?Read(gdk):null;if(doc==null&&config==null)return null;
 var identity=doc==null?config.SelectSingleNode("//*[local-name()='Identity']"):doc.SelectSingleNode("//*[local-name()='Identity']");string name=Attr(identity,"Name");if(name=="")return null;
 string family="";if(!String.IsNullOrEmpty(package)){int i=package.LastIndexOf('_');if(package.StartsWith(name+"_",StringComparison.Ordinal)&&i>name.Length)family=name+"_"+package.Substring(i+1);}
 var app=doc==null?null:doc.SelectSingleNode("//*[local-name()='Applications']/*[local-name()='Application']");var visual=app==null?null:app.SelectSingleNode("*[local-name()='VisualElements']");var shell=config==null?null:config.SelectSingleNode("//*[local-name()='ShellVisuals']");string title=Attr(shell,"DefaultDisplayName");if(title=="")title=Attr(visual,"DisplayName");if(title==""&&doc!=null){var n=doc.SelectSingleNode("//*[local-name()='Properties']/*[local-name()='DisplayName']");if(n!=null)title=n.InnerText;}if(title==""||title.StartsWith("ms-resource:"))title=Path.GetFileName(root)=="Content"?Directory.GetParent(root).Name:name;
 string cover=BestCover(root,shell,visual);
 var g=new Game{Id="xbox-"+(family==""?name:family),Name=title,Root=root,Store="Xbox / Game Pass",LaunchId=family==""?"":family+"!"+Attr(app,"Id"),Cover=cover,Hero=Asset(root,Attr(shell,"SplashScreenImage"))};
 g.ProtectedInstall=root.Split('\\').Any(x=>x.Equals("WindowsApps",StringComparison.OrdinalIgnoreCase));bool unreal=Core.Detect(g);if(!unreal)Core.DetectOther(g);g.Config=unreal?Core.FindConfig(g):"";if(g.ProtectedInstall){g.Exe="";g.Evidence="Installazione WindowsApps protetta: selezione e avvio disponibili, installazione delle mod non disponibile. Usa Gestisci nell’app Xbox per verificare se puoi spostarla in una cartella modificabile.";}else try{InjectionSetup.RestoreTarget(g);}catch(Exception ex){g.Exe="";g.Evidence=ex.Message;}
 string custom;if(Core.Pref.Covers.TryGetValue(g.Id,out custom)){if(!Path.IsPathRooted(custom))custom=Path.Combine(Core.Data,custom);if(File.Exists(custom))g.Cover=custom;}return g;
 }
 static void RegistryRoots(RegistryKey key,List<Tuple<string,string>> entries,int depth){if(key==null||depth>4)return;var root=key.GetValue("Root") as string;var package=key.GetValue("Package") as string;if(!String.IsNullOrEmpty(root))entries.Add(Tuple.Create(root,package??""));foreach(string n in key.GetSubKeyNames())try{using(var child=key.OpenSubKey(n))RegistryRoots(child,entries,depth+1);}catch{}}
 public static List<Game> Scan(){var entries=new List<Tuple<string,string>>();try{using(var h=RegistryKey.OpenBaseKey(RegistryHive.LocalMachine,RegistryView.Registry64))using(var k=h.OpenSubKey(@"SOFTWARE\Microsoft\GamingServices\PackageRepository\Root"))RegistryRoots(k,entries,0);}catch(Exception ex){Core.Warnings.Add("Xbox: "+ex.Message);}
 var libs=new HashSet<string>(Core.Pref.Libraries,StringComparer.OrdinalIgnoreCase);foreach(var drive in DriveInfo.GetDrives())try{if(!drive.IsReady)continue;libs.Add(Path.Combine(drive.RootDirectory.FullName,"XboxGames"));string marker=Path.Combine(drive.RootDirectory.FullName,".GamingRoot");if(File.Exists(marker)){byte[] b=File.ReadAllBytes(marker);if(b.Length>8&&Encoding.ASCII.GetString(b,0,4)=="RGBX"){string rel=Encoding.Unicode.GetString(b,8,b.Length-8).TrimEnd('\0');string path=Path.GetFullPath(Path.Combine(drive.RootDirectory.FullName,rel));if(Core.Under(path,drive.RootDirectory.FullName))libs.Add(path);}}}catch{}
 foreach(string lib in libs){entries.Add(Tuple.Create(lib,""));foreach(string d in Core.Dirs(lib))entries.Add(Tuple.Create(Directory.Exists(Path.Combine(d,"Content"))?Path.Combine(d,"Content"):d,""));}
 var seen=new HashSet<string>(StringComparer.OrdinalIgnoreCase);var ids=new HashSet<string>();var games=new List<Game>();foreach(var e in entries)try{string root=Resolve(e.Item1).TrimEnd('\\');if(!Directory.Exists(root)||!seen.Add(root))continue;var g=Parse(root,e.Item2);if(g!=null&&ids.Add(g.Id))games.Add(g);}catch(Exception ex){Core.Warnings.Add("Xbox · "+e.Item1+": "+ex.Message);}return games;}
 public static bool IsGameX64(Game g,string file){
  if(DlssCore.Pe64(file))return true;
  if(!g.IsXbox||g.ProtectedInstall||!File.Exists(file))return false;
  try{using(File.OpenRead(file)){}return false;}catch(UnauthorizedAccessException){}catch(IOException){}catch{return false;}
  try{
   if(!Core.Under(file,g.Root))return false;
   var manifest=Read(Path.Combine(g.Root,"AppxManifest.xml"));
   var identity=manifest.SelectSingleNode("//*[local-name()='Identity']");
   if(!Attr(identity,"ProcessorArchitecture").Equals("x64",StringComparison.OrdinalIgnoreCase))return false;
   string config=Path.Combine(g.Root,"MicrosoftGame.config");
   if(!File.Exists(config))return false;
   var doc=Read(config);
   if(Attr(doc.SelectSingleNode("//*[local-name()='Identity']"),"Name")!=Attr(identity,"Name"))return false;
   foreach(XmlNode exe in doc.SelectNodes("//*[local-name()='ExecutableList']/*[local-name()='Executable']")){
    string relative=Attr(exe,"Name");if(String.IsNullOrEmpty(relative)||Path.IsPathRooted(relative))continue;
    string candidate=DlssCore.Safe(g.Root,relative);
    if(String.Equals(candidate,Path.GetFullPath(file),StringComparison.OrdinalIgnoreCase)&&!Attr(exe,"IsDevOnly").Equals("true",StringComparison.OrdinalIgnoreCase))return true;
   }
  }catch{}return false;
 }
 public static string LaunchArgument(Game g){if(!Regex.IsMatch(g.LaunchId??"",@"^[A-Za-z0-9.\-_]+![A-Za-z0-9.\-_]+$"))throw new Exception("Avvio Xbox non disponibile: apri il gioco dall’app Xbox.");return "shell:AppsFolder\\"+g.LaunchId;}
}
}
