using System;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Net;
using System.Text;
using System.Text.RegularExpressions;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
namespace HdrPilot {
public static class RenoDx {
 public const string Marker="hdr-unlock-reshade.json",AddonMarker="hdr-unlock-renodx.json";
 public static bool IsReShade(string file){if(!File.Exists(file)||!DlssCore.Pe64(file))return false;var v=FileVersionInfo.GetVersionInfo(file);return ((v.ProductName??"")+" "+(v.FileDescription??"")).IndexOf("ReShade",StringComparison.OrdinalIgnoreCase)>=0;}
 public static List<string> Loaders(string folder){return DlssCore.Proxies.Concat(new[]{"ReShade64.dll","d3d11.dll","d3d9.dll","opengl32.dll","dinput8.dll"}).Where(n=>IsReShade(DlssCore.Safe(folder,n))).Distinct().ToList();}
 public static bool HasHdrAddon(Game g){string dir=Path.GetDirectoryName(g.Exe);return Directory.Exists(dir)&&Directory.GetFiles(dir,"renodx-*.addon64").Any(f=>!Regex.IsMatch(Path.GetFileName(f),"dlss|neural|mfg|framegen",RegexOptions.IgnoreCase)&&DlssCore.Pe64(f));}
 public static string InstalledChannel(Game g){try{string dir=Path.GetDirectoryName(g.Exe);if(!File.Exists(Path.Combine(dir,Marker)))return "";var m=DlssCore.ReadManifest(dir,Marker);if(m.Build!="stable"&&m.Build!="nightly")return "";var loaders=Loaders(dir);if(loaders.Count!=1)return "";var entry=m.Files.FirstOrDefault(f=>f.Name.Equals(loaders[0],StringComparison.OrdinalIgnoreCase));return entry!=null&&entry.After==DlssCore.Hash(Path.Combine(dir,loaders[0]))?m.Build:"";}catch{return "";}}
 public static string State(Game g){string dir=Path.GetDirectoryName(g.Exe);var loaders=Loaders(dir);var addons=Directory.GetFiles(dir,"renodx-*.addon64").Where(f=>!Regex.IsMatch(Path.GetFileName(f),"dlss|neural|mfg|framegen",RegexOptions.IgnoreCase)).Select(Path.GetFileName).ToList();
  string text=loaders.Count==0?"ReShade · non installato per questo eseguibile":String.Join("\n",loaders.Select(n=>"ReShade · installato · "+(FileVersionInfo.GetVersionInfo(Path.Combine(dir,n)).FileVersion??"versione non disponibile").Replace(",",".")));
  if(loaders.Count>1)text+="\nAttenzione: più loader ReShade presenti";
  if(loaders.Contains("ReShade64.dll")){string ini=DlssCore.ReadIni(dir);text+="\nCaricamento tramite OptiScaler: "+(Core.Get(ini,"Plugins","LoadReshade")=="true"?"configurato":"da configurare");}
  text+="\nRenoDX · "+(addons.Count==0?"non installato":String.Join(", ",addons));
  return text+"\n"+MfgAddon.State(g)+"\nStato letto dai file del gioco · attivazione in gioco da verificare";
 }

 static DlssPlan Plan(Game g,string marker,string summary){string root=Path.GetDirectoryName(Path.GetFullPath(g.Exe));string m=DlssCore.Safe(root,marker);return new DlssPlan{Folder=root,MarkerName=marker,MarkerExpected=File.Exists(m)?DlssCore.Hash(m):null,Summary=summary,Manifest=File.Exists(m)?DlssCore.ReadManifest(root,marker):new DlssManifest{Game=g.Id,Folder=root,PreviousOwner="Stato precedente del componente",BackupRoot="hdr-components/"+Guid.NewGuid().ToString("N")}};}
 static void Add(DlssPlan p,string name,string source,byte[] bytes=null){string dst=DlssCore.Safe(p.Folder,name);p.Changes.Add(new DlssChange{Name=name,Source=source,SourceHash=source==null?null:DlssCore.Hash(source),Bytes=bytes,Expected=File.Exists(dst)?DlssCore.Hash(dst):null});}
 public const string KeyMarker="hdr-unlock-reshade-key.json";
 public static DlssPlan MenuKey(Game g,string key){if(!InjectionSetup.MenuKeys.Contains(key)||key=="keep")throw new Exception("Scegli un tasto del menu ReShade.");string dir=Path.GetDirectoryName(g.Exe);var loaders=Loaders(dir);if(loaders.Count!=1)throw new Exception("Serve un solo loader ReShade riconosciuto per configurarne il menu.");string name="ReShade.ini";string custom=Path.ChangeExtension(loaders[0],".ini");if(!File.Exists(DlssCore.Safe(dir,name))&&File.Exists(DlssCore.Safe(dir,custom)))name=custom;uint vk=Convert.ToUInt32(key.Substring(2),16);string file=DlssCore.Safe(dir,name),text=File.Exists(file)?File.ReadAllText(file):"";var p=Plan(g,KeyMarker,"Tasto menu ReShade: "+InjectionSetup.KeyName(key)+"\nFile: "+name+"\nLe altre impostazioni vengono conservate.");Add(p,name,null,new UTF8Encoding(false).GetBytes(Core.Set(text,"INPUT","KeyOverlay",vk.ToString()+",0,0,0")));return p;}
 public static DlssPlan InstallReShade(Game g,string source,string origin){
  if(InjectionSetup.Profile(g).Api=="vulkan")throw new Exception("ReShade Vulkan richiede un layer dedicato non gestito da questo installer.");
  if(!IsReShade(source))throw new Exception("Il pacchetto non contiene una DLL ReShade x64 riconoscibile.");
  var scan=DlssCore.Inspect(g,true,false);if(scan.Blockers.Count>0)throw new Exception(String.Join("\n",scan.Blockers));
  var loaders=Loaders(scan.Folder);if(loaders.Count>1)throw new Exception("Più loader ReShade rilevati. Installazione fermata per evitare un doppio caricamento.");
  string target=loaders.FirstOrDefault();if(target==null)target=scan.Proxy!=""?"ReShade64.dll":"dxgi.dll";
  string dst=DlssCore.Safe(scan.Folder,target);if(File.Exists(dst)&&!IsReShade(dst))throw new Exception(target+" appartiene a un altro componente. Nessuna sovrascrittura automatica.");
  bool chain=target.Equals("ReShade64.dll",StringComparison.OrdinalIgnoreCase);
  if(chain&&scan.Proxy=="")throw new Exception("ReShade64.dll presente senza loader OptiScaler riconosciuto. Catena da verificare prima di procedere.");
  if(!chain&&scan.Proxy!=""&&String.Equals(Core.Get(scan.Ini,"Plugins","LoadReshade"),"true",StringComparison.OrdinalIgnoreCase))throw new Exception("ReShade usa già un loader diretto ma OptiScaler richiede un secondo caricamento. Correggi la catena prima di installare.");
  var p=Plan(g,Marker,g.Name+"\nReShade con supporto completo agli add-on · "+FileVersionInfo.GetVersionInfo(source).FileVersion+"\n"+origin+"\nCaricamento: "+(chain?"OptiScaler → ReShade64.dll":target)+"\nNessun preset o shader HDR viene attivato automaticamente. Configura RenoDX dal menu ReShade.");
  p.Manifest.Build=origin.StartsWith("Stabile ufficiale")?"stable":origin.StartsWith("Nightly dal repository")?"nightly":"";p.Manifest.Release=FileVersionInfo.GetVersionInfo(source).FileVersion;
  Add(p,target,source);
  if(chain){string ini=Core.Set(scan.Ini,"Plugins","LoadReshade","true");if(ini!=scan.Ini)Add(p,"OptiScaler.ini",null,new UTF8Encoding(false).GetBytes(ini));}
  return p;
 }
 public static DlssPlan InstallAddon(Game g,string source){
  string name=Path.GetFileName(source);if(!Regex.IsMatch(name,@"^renodx-[a-zA-Z0-9_-]+\.addon64$",RegexOptions.IgnoreCase)||!DlssCore.Pe64(source))throw new Exception("Scegli un add-on RenoDX x64 originale, con nome renodx-….addon64.");
  if(Regex.IsMatch(name,"dlss|neural|mfg|framegen",RegexOptions.IgnoreCase))throw new Exception("Questa sezione gestisce gli add-on HDR; non installa un secondo sistema DLSS 5/Neural Rendering.");
  var scan=DlssCore.Inspect(g,true,false);if(scan.Blockers.Count>0)throw new Exception(String.Join("\n",scan.Blockers));var loaders=Loaders(scan.Folder);if(loaders.Count!=1)throw new Exception("Installa prima una sola copia di ReShade con supporto completo agli add-on.");
  if(loaders[0]=="ReShade64.dll"&&(scan.Proxy==""||Core.Get(scan.Ini,"Plugins","LoadReshade")!="true"))throw new Exception("ReShade64.dll non è collegato a OptiScaler. Installa/configura ReShade prima dell’add-on.");
  var others=Directory.GetFiles(scan.Folder,"renodx-*.addon64").Select(Path.GetFileName).Where(n=>!n.Equals(name,StringComparison.OrdinalIgnoreCase)&&!Regex.IsMatch(n,"dlss|neural|mfg|framegen",RegexOptions.IgnoreCase));if(others.Any())throw new Exception("Altri add-on RenoDX presenti: "+String.Join(", ",others)+". Verifica la combinazione o ripristina il precedente prima di aggiungerne uno.");
  var p=Plan(g,AddonMarker,g.Name+"\nInstalla add-on HDR: "+name+"\nSHA256: "+DlssCore.Hash(source)+"\nMod selezionata manualmente: segui le istruzioni del suo autore per questo gioco. Non vengono cambiati i parametri HDR del motore. Regola l’HDR dal menu ReShade (normalmente Home).");Add(p,name,source);return p;
 }
 public static DlssPlan Restore(Game g,string marker){string root=Path.GetDirectoryName(Path.GetFullPath(g.Exe));var m=DlssCore.ReadManifest(root,marker);var p=new DlssPlan{Folder=root,MarkerName=marker,Uninstall=true,Summary="Ripristina lo stato precedente di "+(marker==Marker?"ReShade":marker==KeyMarker?"tasto ReShade":"RenoDX")+". Le modifiche successive di altre app non vengono sovrascritte."};
  foreach(var f in m.Files){string dst=DlssCore.Safe(root,f.Name),source=f.Before==null?null:DlssCore.Safe(Core.Data,f.Backup);if((File.Exists(dst)?DlssCore.Hash(dst):null)!=f.After||source!=null&&(!File.Exists(source)||DlssCore.Hash(source)!=f.Before))throw new Exception("File o backup modificato: "+f.Name+". Ripristino fermato.");p.Changes.Add(new DlssChange{Name=f.Name,Expected=f.After,Source=source,SourceHash=f.Before,Delete=source==null,RestoreAttributes=f.Attributes});}
  p.Changes.Add(new DlssChange{Name=marker,Expected=DlssCore.Hash(DlssCore.Safe(root,marker)),Delete=true});return p;
 }
 public static string Extract(string package,string folder){Directory.CreateDirectory(folder);byte[] bytes=File.ReadAllBytes(package);if(bytes.Length>100000000)throw new Exception("Pacchetto ReShade troppo grande.");
  // Installer is a self-extracting executable with an appended ZIP. Read data only.
  int start=0;if(bytes.Length>2&&bytes[0]==77&&bytes[1]==90){start=-1;for(int i=bytes.Length-22;i>=0;i--)if(bytes[i]==80&&bytes[i+1]==75&&bytes[i+2]==5&&bytes[i+3]==6){long offset=(long)i-BitConverter.ToUInt32(bytes,i+12)-BitConverter.ToUInt32(bytes,i+16);if(offset>=0&&offset<bytes.Length-4&&bytes[offset]==80&&bytes[offset+1]==75&&bytes[offset+2]==3&&bytes[offset+3]==4){start=(int)offset;break;}}if(start<0)throw new Exception("Archivio ReShade non trovato nell’installer.");}
  using(var stream=new MemoryStream(bytes,start,bytes.Length-start))using(var zip=new ZipArchive(stream,ZipArchiveMode.Read)){var files=zip.Entries.Where(e=>e.FullName=="ReShade64.dll").ToList();if(files.Count!=1||files[0].Length>80000000)throw new Exception("DLL ReShade x64 non univoca nel pacchetto.");string dest=DlssCore.Safe(folder,"ReShade64.dll");using(var input=files[0].Open())using(var output=File.Create(dest))input.CopyTo(output);if(!IsReShade(dest))throw new Exception("DLL ReShade non valida.");return dest;}
 }
 public const string CatalogUrl="https://raw.githubusercontent.com/wiki/clshortfuse/renodx/Mods.md";
 public class CatalogMod {public string Title,Url,Notes;public bool EngineRecipe;}
 static string TitleKey(string title){title=Regex.Replace(title,@"\[([^\]]+)\]\([^)]*\)","$1");title=WebUtility.HtmlDecode(title).Replace("™","").Replace("®","").Normalize(NormalizationForm.FormKC).ToLowerInvariant();return Regex.Replace(title,@"[^\p{L}\p{N}]","");}
 public static CatalogMod ResolveGame(string markdown,string title){
  if(String.IsNullOrWhiteSpace(title))throw new Exception("Nome del gioco non disponibile.");
  var matches=new List<CatalogMod>();bool found=false;string manualPage=null;
  CatalogMod extended=null;bool inExtended=false;string extendedUrl=null;
  foreach(string raw in markdown.Split('\n')){
   string line=raw.Trim();
   if(line.StartsWith("### Unreal Engine Extended ")){inExtended=true;var link=Regex.Match(line,@"https://[^\s)<>"" ]+/renodx-ue-extended\.addon64");extendedUrl=link.Success?link.Value:null;continue;}
   if(inExtended&&(line.StartsWith("### ")||line.StartsWith("## ")||line.StartsWith("<details>")))inExtended=false;
   if(!inExtended||extendedUrl==null||!line.StartsWith("|"))continue;
   var row=line.Split('|');if(row.Length<5||TitleKey(row[1])!=TitleKey(title))continue;
   extended=ResolveGame("| "+row[1]+" | author | [Download]("+extendedUrl+") | "+row[2]+" "+row[3]+" |",title);
   extended.EngineRecipe=String.Equals(row[3].Trim(),"Engine.ini",StringComparison.OrdinalIgnoreCase);
   extended.Notes="Unreal Engine Extended · "+WebUtility.HtmlDecode(row[2].Trim()+" · "+row[3].Trim())+". Segui le istruzioni HDR del catalogo; il solo add-on non configura Engine.ini.";
  }
  foreach(string line in markdown.Split('\n')){
   if(!line.TrimStart().StartsWith("|"))continue;var cells=line.Split('|');if(cells.Length<5||TitleKey(cells[1])!=TitleKey(title))continue;found=true;
   var page=Regex.Match(cells[3],@"https://(?:www\.)?nexusmods\.com/[a-zA-Z0-9_-]+/mods/[0-9]+/?");if(page.Success)manualPage=page.Value;
   foreach(Match link in Regex.Matches(cells[3],@"https://[^\s)<>""]+\.addon64")){
    Uri uri;if(!Uri.TryCreate(link.Value,UriKind.Absolute,out uri)||uri.Scheme!="https"||!String.IsNullOrEmpty(uri.UserInfo)||!uri.IsDefaultPort)continue;
    bool allowed=uri.Host.Equals("github.com",StringComparison.OrdinalIgnoreCase)&&Regex.IsMatch(uri.AbsolutePath,@"^/[^/]+/[^/]+/releases/download/[^/]+/renodx-[a-zA-Z0-9_-]+\.addon64$")||Regex.IsMatch(uri.Host,@"^[a-zA-Z0-9-]+\.github\.io$")&&Regex.IsMatch(uri.AbsolutePath,@"^/renodx/renodx-[a-zA-Z0-9_-]+\.addon64$");
    if(!allowed||Regex.IsMatch(Path.GetFileName(uri.AbsolutePath),"dlss|neural|mfg|framegen",RegexOptions.IgnoreCase))continue;
    matches.Add(new CatalogMod{Title=cells[1].Trim(),Url=uri.AbsoluteUri,Notes=WebUtility.HtmlDecode(cells[4].Trim())});
   }
  }
  var unique=matches.GroupBy(x=>x.Url,StringComparer.OrdinalIgnoreCase).Select(x=>x.First()).ToList();
  if(unique.Count==0&&extended!=null)return extended;
  if(unique.Count==0&&manualPage!=null)throw new Exception(title+": il catalogo pubblica questa mod su Nexus Mods, senza download diretto. Non è un errore x64. Scarica il pacchetto dalla pagina dell’autore, estrailo e usa Importa mod RenoDX locale.\nPagina della mod: "+manualPage);
  if(unique.Count==0)throw new Exception(found?"La mod è nel catalogo, ma non ha un download diretto x64 supportato. Apri Catalogo mod RenoDX per le istruzioni dell’autore.":"Nessuna mod RenoDX associata esattamente a «"+title+"» nel catalogo. Demo ed edizioni diverse non vengono abbinate automaticamente. Puoi consultare Catalogo mod RenoDX; nessun file modificato.");
  if(unique.Count>1)throw new Exception("Il catalogo propone più mod per questo titolo. Consulta Catalogo mod RenoDX per scegliere quella adatta; nessun file modificato.");return unique[0];
 }
 public static string FetchForGame(Game game,out string origin){CatalogMod ignored;return FetchForGame(game,out origin,out ignored);}
 public static string FetchForGame(Game game,out string origin,out CatalogMod selected){
  ServicePointManager.SecurityProtocol|=SecurityProtocolType.Tls12;
  using(var web=new DlssCore.DownloadClient()){
   web.Headers[HttpRequestHeader.UserAgent]="HDR-Unlock/0.1";
   ActivityLog.Write("RenoDX: ricerca nel catalogo per "+game.Name);
   var mod=ResolveGame(web.DownloadString(CatalogUrl),game.Name);selected=mod;
   ActivityLog.Write("RenoDX: mod associata "+mod.Title+" · "+mod.Url);
   string name=Path.GetFileName(new Uri(mod.Url).AbsolutePath);
   string dir=Path.Combine(Core.Data,"hdr-packages",Guid.NewGuid().ToString("N"));Directory.CreateDirectory(dir);
   string url=mod.Url;Dictionary<string,object> published=null;var uri=new Uri(url);
   if(uri.Host=="github.com"){
    var path=uri.AbsolutePath.Split('/');string repo=path[1]+"/"+path[2];
    published=PickAddon(web.DownloadString("https://api.github.com/repos/"+repo+"/releases?per_page=20"),name);
    if(published==null)throw new Exception("No current release asset found for this RenoDX mod. No cached package installed.");
    url=Convert.ToString(published["browser_download_url"]);
    if(!url.StartsWith("https://github.com/"+repo+"/releases/download/",StringComparison.Ordinal)||Path.GetFileName(new Uri(url).AbsolutePath)!=name)throw new Exception("Unexpected RenoDX release source.");
   }
   string file=DlssCore.Safe(dir,name);web.DownloadFile(url,file);
   if(published!=null){if(new FileInfo(file).Length!=Convert.ToInt64(published["size"]))throw new Exception("Incomplete RenoDX download.");string digest=published.ContainsKey("digest")?Convert.ToString(published["digest"]):"";if(digest.StartsWith("sha256:")&&!DlssCore.Hash(file).Equals(digest.Substring(7),StringComparison.OrdinalIgnoreCase))throw new Exception("RenoDX checksum mismatch.");}
   if(!DlssCore.Pe64(file))throw new Exception("Il download non contiene un add-on x64 valido. Nessun file del gioco modificato.");
   origin="Mod associata dal catalogo RenoDX: "+mod.Title+"\nUltima pubblicazione della mod, verificata ora.\n"+url+"\nStato / note del catalogo: "+mod.Notes+"\nConsulta le istruzioni dell’autore prima di applicare.\nSHA256: "+DlssCore.Hash(file);
   File.WriteAllText(Path.Combine(dir,"origine.txt"),origin);ActivityLog.Write("RenoDX: download x64 verificato; anteprima pronta.");return file;
  }
 }
 public static Dictionary<string,object> PickAddon(string json,string name){var parser=new System.Web.Script.Serialization.JavaScriptSerializer{MaxJsonLength=32000000};var releases=parser.Deserialize<List<Dictionary<string,object>>>(json);var candidates=new List<Dictionary<string,object>>();foreach(var release in releases){if(release.ContainsKey("draft")&&Convert.ToBoolean(release["draft"]))continue;foreach(var asset in ((System.Collections.IEnumerable)release["assets"]).Cast<Dictionary<string,object>>())if(String.Equals(Convert.ToString(asset["name"]),name,StringComparison.OrdinalIgnoreCase))candidates.Add(asset);}var chosen=candidates.OrderByDescending(a=>Convert.ToString(a["updated_at"]),StringComparer.Ordinal).FirstOrDefault();return chosen;}
 public static string FetchAddon(string name,out string origin){if(!Regex.IsMatch(name??"",@"^renodx-[a-zA-Z0-9_.-]+\.addon64$")||name.IndexOf("dlss",StringComparison.OrdinalIgnoreCase)>=0)throw new Exception("Scegli una mod RenoDX HDR valida.");ServicePointManager.SecurityProtocol|=SecurityProtocolType.Tls12;using(var web=new DlssCore.DownloadClient()){web.Headers[HttpRequestHeader.UserAgent]="HDR-Unlock/0.1";var chosen=PickAddon(web.DownloadString("https://api.github.com/repos/clshortfuse/renodx/releases?per_page=10"),name);if(chosen==null)throw new Exception("Mod non trovata nelle release recenti RenoDX. Consulta il catalogo del gioco; nessun vecchio file installato automaticamente.");string url=Convert.ToString(chosen["browser_download_url"]);if(!url.StartsWith("https://github.com/clshortfuse/renodx/releases/download/",StringComparison.OrdinalIgnoreCase))throw new Exception("Origine RenoDX non valida.");string dir=Path.Combine(Core.Data,"hdr-packages",Guid.NewGuid().ToString("N"));Directory.CreateDirectory(dir);string file=DlssCore.Safe(dir,name);web.DownloadFile(url,file);string digest=chosen.ContainsKey("digest")?Convert.ToString(chosen["digest"]):"";if(digest.StartsWith("sha256:")&&!DlssCore.Hash(file).Equals(digest.Substring(7),StringComparison.OrdinalIgnoreCase))throw new Exception("Hash RenoDX non corrispondente.");origin="RenoDX · ultima pubblicazione trovata per "+name+"\nAggiornata: "+Convert.ToString(chosen["updated_at"])+"\n"+url;File.WriteAllText(Path.Combine(dir,"origine.txt"),origin+"\nSHA256 "+DlssCore.Hash(file));return file;}}
 public static string Fetch(bool nightly,Action<string> progress,out string origin){ServicePointManager.SecurityProtocol|=SecurityProtocolType.Tls12;string url;string digest="";using(var web=new DlssCore.DownloadClient()){web.Headers[HttpRequestHeader.UserAgent]="HDR-Unlock/0.1";
  if(nightly){progress("Controllo la nightly del repository ufficiale…");var runs=Core.Json.Deserialize<Dictionary<string,object>>(web.DownloadString("https://api.github.com/repos/crosire/reshade/actions/workflows/build.yml/runs?branch=main&event=push&status=success&per_page=1"));var list=(System.Collections.IEnumerable)runs["workflow_runs"];var run=list.Cast<Dictionary<string,object>>().FirstOrDefault();if(run==null)throw new Exception("Nessuna nightly disponibile.");string id=Convert.ToString(run["id"]);var json=Core.Json.Deserialize<Dictionary<string,object>>(web.DownloadString("https://api.github.com/repos/crosire/reshade/actions/runs/"+id+"/artifacts"));var artifact=((System.Collections.IEnumerable)json["artifacts"]).Cast<Dictionary<string,object>>().FirstOrDefault(x=>Convert.ToString(x["name"])=="ReShade (64-bit)"&&!Convert.ToBoolean(x["expired"]));if(artifact==null)throw new Exception("Nightly scaduta o non disponibile.");url="https://nightly.link/crosire/reshade/actions/artifacts/"+Convert.ToString(artifact["id"])+".zip";if(artifact.ContainsKey("digest"))digest=Convert.ToString(artifact["digest"]);origin="Nightly dal repository crosire/reshade · commit "+Convert.ToString(run["head_sha"])+"\nDownload tramite nightly.link\n"+url;
  }else{progress("Controllo la versione stabile ufficiale…");string tag=DlssLibraries.ParseTags(web.DownloadString("https://api.github.com/repos/crosire/reshade/tags?per_page=20")).First();url="https://reshade.me/downloads/ReShade_Setup_"+tag.TrimStart('v')+"_Addon.exe";origin="Stabile ufficiale "+tag+" · supporto completo agli add-on\n"+url;}
  string dir=Path.Combine(Core.Data,"hdr-packages",Guid.NewGuid().ToString("N"));Directory.CreateDirectory(dir);string package=Path.Combine(dir,nightly?"nightly.zip":"stable.exe");progress("Scarico ReShade "+(nightly?"nightly":"stabile")+"…");web.DownloadFile(url,package);if(digest.StartsWith("sha256:",StringComparison.OrdinalIgnoreCase)&&!DlssCore.Hash(package).Equals(digest.Substring(7),StringComparison.OrdinalIgnoreCase))throw new Exception("Hash del pacchetto nightly non corrispondente.");string file=Extract(package,Path.Combine(dir,"files"));File.WriteAllText(Path.Combine(dir,"origine.txt"),origin+"\nSHA256 pacchetto "+DlssCore.Hash(package)+"\nSHA256 DLL "+DlssCore.Hash(file));return file;
 }}
}
public static class ComponentCleanup {
 public static DlssPlan Prepare(Game game,bool reshade){
  Core.Closed(game);var scan=DlssCore.Inspect(game);string root=scan.Folder;
  if(reshade&&(MfgAddon.Installed(game)||MfgAddon.HasRecord(game)))throw new Exception("Restore MFG Unlock before removing ReShade.");
  var plan=new DlssPlan{Folder=root,Uninstall=true,Summary=game.Name+"\nPulizia "+(reshade?"ReShade e add-on HDR RenoDX":"OptiScaler e Neural Rendering")+" anche da installazioni esterne.\nI file riconosciuti vengono rimossi dalla cartella del gioco e copiati in data/dlss-recovery.\nNon è un ripristino della precedente mod: i backup originali già conservati dall’app restano disponibili."};
  var names=new HashSet<string>(StringComparer.OrdinalIgnoreCase);
  Action<string> add=n=>{if(File.Exists(DlssCore.Safe(root,n)))names.Add(n);};
  var loaders=RenoDx.Loaders(root);
  if(reshade){
   foreach(string n in loaders){add(n);add(Path.ChangeExtension(n,".ini"));add(Path.ChangeExtension(n,".log"));}
   foreach(string n in new[]{"ReShade.ini","ReShade.log","ReShadePreset.ini",RenoDx.Marker,RenoDx.KeyMarker,RenoDx.AddonMarker})add(n);
   if(File.Exists(Path.Combine(root,"hdr-unlock-effects-autohdr.json"))&&DlssCore.Pe64(Path.Combine(root,"AutoHDR.addon64")))add("AutoHDR.addon64");
   foreach(string f in Directory.GetFiles(root,"hdr-unlock-effects-*.json"))add(Path.GetFileName(f));
   foreach(string path in Directory.GetFiles(root,"renodx-*.addon64")){string n=Path.GetFileName(path);if(!Regex.IsMatch(n,"dlss|neural|mfg|framegen",RegexOptions.IgnoreCase)&&DlssCore.Pe64(path))add(n);}
   foreach(string dir in new[]{"reshade-shaders","ReShade-shaders"}.Distinct(StringComparer.OrdinalIgnoreCase)){
    string path=DlssCore.Safe(root,dir);if(!Directory.Exists(path))continue;
    var pending=new Stack<string>();pending.Push(dir);while(pending.Count>0){string relative=pending.Pop(),folder=DlssCore.Safe(root,relative);foreach(string f in Directory.GetFiles(folder))add(relative+"/"+Path.GetFileName(f));foreach(string child in Directory.GetDirectories(folder)){string next=relative+"/"+Path.GetFileName(child);DlssCore.Safe(root,next);pending.Push(next);}}
   }
   if(scan.Proxy!=""&&Core.Get(scan.Ini,"Plugins","LoadReshade")=="true"){
    string ini=Core.Set(scan.Ini,"Plugins","LoadReshade","false");plan.Changes.Add(new DlssChange{Name="OptiScaler.ini",Expected=DlssCore.Hash(DlssCore.Safe(root,"OptiScaler.ini")),Bytes=new UTF8Encoding(false).GetBytes(ini)});
   }
   plan.Summary+="\nDisabilita il collegamento ReShade in OptiScaler, se presente. I preset con nomi personalizzati, percorsi shader esterni e add-on non identificati restano conservati. Le cartelle vuote possono restare.";
  }else{
   if(loaders.Contains("ReShade64.dll")&&Core.Get(scan.Ini,"Plugins","LoadReshade")=="true")throw new Exception("ReShade dipende da OptiScaler: usa prima Pulisci ReShade per rimuoverlo senza lasciare un caricamento incompleto.");
   foreach(string n in DlssCore.Proxies)if(DlssCore.IsOpti(DlssCore.Safe(root,n)))add(n);
   foreach(string n in new[]{"OptiScaler.dll","OptiScaler.ini","OptiScaler.log","nvngx_dlssnr.dll",DlssCore.Marker,NrModels.Marker}){if(n=="OptiScaler.dll"&&!DlssCore.IsOpti(DlssCore.Safe(root,n)))continue;if(n=="nvngx_dlssnr.dll"&&!DlssCore.Pe64(DlssCore.Safe(root,n)))continue;add(n);}
   foreach(string path in Directory.GetFiles(root,"*.addon64")){string n=Path.GetFileName(path);if(Regex.IsMatch(n,@"^renodx-.*(dlss|neural).*\.addon64$",RegexOptions.IgnoreCase)&&DlssCore.Pe64(path))add(n);}
   plan.Summary+="\nConserva ReShade HDR, UE4SS, MFG separato e DLL SR/RR/FG/Streamline che possono appartenere al gioco. I file non identificati e i registri esterni restano conservati: nessuna promessa di cartella completamente pulita per mod sconosciute.";
  }
  foreach(string n in names.OrderBy(x=>x))plan.Changes.Add(new DlssChange{Name=n,Delete=true,Expected=DlssCore.Hash(DlssCore.Safe(root,n))});
  if(plan.Changes.Count==0)throw new Exception("Nessun file del componente riconosciuto da pulire.");
  return plan;
 }
}
public static class RenoDxUi {
 public static bool IsBusy;
 public static FrameworkElement Embed(Window owner,Game game){var shell=Create(owner,game,true);var border=(Border)shell.Content;var content=border.Child;border.Child=null;shell.Close();return (FrameworkElement)content;}
 public static void Show(Window owner,Game game){Create(owner,game).ShowDialog();}
 public static Window Create(Window owner,Game game,bool embedded=false){StackPanel body;var d=Dialogs.Create(owner,"HDR · ReShade e RenoDX",out body);d.Width=950;d.FontSize=14;d.MaxHeight=SystemParameters.WorkArea.Height-40;if(embedded)body.Children.Clear();
  if(!embedded)body.Children.Add(Dialogs.Text(game.Name,16));
  body.Children.Add(Dialogs.Text("Componenti rilevati per questo eseguibile. Aggiorna quelli presenti oppure aggiungi la mod RenoDX del gioco.",14));
  var channels=new ComboBox{ItemsSource=new[]{"Stabile ufficiale · consigliata · full add-on","Nightly · sviluppo · full add-on"},SelectedIndex=0,Margin=new Thickness(0,0,0,12)};body.Children.Add(channels);channels.Visibility=Visibility.Collapsed;body.Children.Add(Dialogs.Text("Giallo: canale installato verificato · Blu: scelta per il prossimo aggiornamento",13));var channelRow=new WrapPanel{Margin=new Thickness(0,0,0,10)};body.Children.Add(channelRow);var stable=HdrChoices.Option("ReShade stabile");var nightlyButton=HdrChoices.Option("ReShade nightly");channelRow.Children.Add(stable);channelRow.Children.Add(nightlyButton);HdrChoices.Help(stable,"Scarica la versione stabile ufficiale con supporto completo agli add-on: il punto di partenza consigliato.");HdrChoices.Help(nightlyButton,"Scarica l’ultima compilazione di sviluppo riuscita del repository ReShade. Include modifiche più recenti che possono introdurre regressioni; non è una versione stabile.");Action channelStyle=()=>{string installed=RenoDx.InstalledChannel(game);stable.Style=d.TryFindResource(installed=="stable"?(object)"Installed":channels.SelectedIndex==0?(object)"Primary":typeof(Button)) as Style;nightlyButton.Style=d.TryFindResource(installed=="nightly"?(object)"Installed":channels.SelectedIndex==1?(object)"Primary":typeof(Button)) as Style;};stable.Click+=(s,e)=>{if(!IsBusy){channels.SelectedIndex=0;channelStyle();}};nightlyButton.Click+=(s,e)=>{if(!IsBusy){channels.SelectedIndex=1;channelStyle();}};channelStyle();
  var info=Dialogs.Text("Versioni controllate online al momento dell’installazione.",14);body.Children.Add(info);var advanced=new StackPanel{Margin=new Thickness(0,10,0,0)};var details=new StackPanel{Margin=new Thickness(0,12,0,0)};advanced.Children.Add(details);var state=Dialogs.Text(RenoDx.State(game),13);body.Children.Insert(0,state);state.FontSize=15;state.Foreground=new System.Windows.Media.SolidColorBrush(System.Windows.Media.Color.FromRgb(230,172,80));state.Margin=new Thickness(0,0,0,14);state.ToolTip="Stabile da reshade.me; nightly da crosire/reshade tramite nightly.link. Entrambe con supporto completo agli add-on.";var restores=new StackPanel{Margin=new Thickness(0,10,0,0)};restores.Children.Add(Dialogs.Text("Ripristina",15));var restoreButtons=new WrapPanel{Margin=new Thickness(0,12,0,0)};restores.Children.Add(restoreButtons);
  var actions=new WrapPanel();body.Children.Insert(body.Children.IndexOf(info),actions);IsBusy=false;
  Button reshadeInstall=null,renodxInstall=null,mfgInstall=null;Action refreshState=()=>{if(mfgInstall!=null){bool installed=MfgAddon.Installed(game);mfgInstall.Content=Appearance.Localize(installed?"Update MFG Unlock":"Install MFG Unlock");mfgInstall.Style=d.TryFindResource(installed?"Installed":"Primary") as Style;}channelStyle();state.Text=RenoDx.State(game);bool hasReShade=RenoDx.Loaders(Path.GetDirectoryName(game.Exe)).Count>0,hasReno=RenoDx.HasHdrAddon(game);if(reshadeInstall!=null){reshadeInstall.Content=hasReShade?"ReShade installato · aggiorna":"Installa ReShade";reshadeInstall.Style=d.TryFindResource(hasReShade?"Installed":"Primary") as Style;reshadeInstall.ToolTip=hasReShade?"ReShade rilevato nei file del gioco. Premi per aggiornare. Il caricamento in gioco va verificato.":"Scarica e installa ReShade.";}if(renodxInstall!=null){renodxInstall.Content=hasReno?"RenoDX installato · aggiorna":"Installa RenoDX";renodxInstall.Style=d.TryFindResource(hasReno?"Installed":"Primary") as Style;renodxInstall.ToolTip=hasReno?"Add-on HDR RenoDX x64 presente. Premi per aggiornare. La presenza non conferma il caricamento in gioco.":"Scarica e installa la mod HDR disponibile per questo gioco.";}};
  Action<DlssPlan> review=plan=>{StackPanel b;var w=Dialogs.Create(d,"Controlla la catena di caricamento",out b);w.Width=860;b.Children.Add(new TextBox{Text=plan.ToString(),IsReadOnly=true,TextWrapping=TextWrapping.Wrap,Height=360,VerticalScrollBarVisibility=ScrollBarVisibility.Auto});var apply=new Button{Content="Applica con backup",Margin=new Thickness(0,12,0,0)};b.Children.Add(apply);w.Closing+=(sender,args)=>{if(!apply.IsEnabled)args.Cancel=true;};apply.Click+=async(s,e)=>{apply.IsEnabled=false;try{await Task.Run(()=>DlssCore.Apply(game,plan));apply.IsEnabled=true;w.Close();refreshState();info.Text="Operazione completata. Backup verificato.";}catch(Exception ex){apply.IsEnabled=true;Dialogs.Notice(w,"Operazione non completata",ex.Message,null).ShowDialog();}};w.ShowDialog();};
  Action<string,Action> button=(label,action)=>{var b=new Button{Content=label,Margin=new Thickness(0,0,8,10)};var buttonStyle=d.TryFindResource(label=="Installa ReShade selezionato"?"Primary":(label.StartsWith("Ripristina")||label.StartsWith("Disinstalla"))?"RestoreAction":label.StartsWith("Catalogo")||(label.StartsWith("Importa")||label=="Install UE4SS for mods")?"InfoAction":"Unused") as Style;if(buttonStyle!=null)b.Style=buttonStyle;b.Click+=(s,e)=>{if(IsBusy)return;try{action();}catch(Exception ex){info.Text=Appearance.Localize(ex.Message);}};if(label.StartsWith("Ripristina"))restoreButtons.Children.Add(b);else if(label.StartsWith("Importa")||label=="Install UE4SS for mods")details.Children.Add(b);else if(label.StartsWith("Disinstalla")||label.StartsWith("Pulisci")||label.StartsWith("Catalogo")||label=="MFG Unlock · compatibility")details.Children.Add(b);else actions.Children.Add(b);};
  button("Installa ReShade selezionato",async()=>{if(IsBusy)return;bool nightly=channels.SelectedIndex==1;IsBusy=true;actions.IsEnabled=channels.IsEnabled=false;try{Core.Closed(game);string origin="";string file=await Task.Run(()=>RenoDx.Fetch(nightly,msg=>d.Dispatcher.Invoke(new Action(()=>info.Text=msg)),out origin));review(RenoDx.InstallReShade(game,file,origin));}catch(Exception ex){info.Text="Installazione non eseguita: "+ex.Message;}finally{IsBusy=false;actions.IsEnabled=channels.IsEnabled=true;}});
  reshadeInstall=actions.Children.OfType<Button>().First(b=>Convert.ToString(b.Content)=="Installa ReShade selezionato");refreshState();
  button("Disinstalla ReShade",()=>{Core.Closed(game);if(MfgAddon.Installed(game)||MfgAddon.HasRecord(game))throw new Exception("Restore MFG Unlock before removing ReShade.");string marker=Path.Combine(Path.GetDirectoryName(game.Exe),RenoDx.Marker);if(!File.Exists(marker))throw new Exception(RenoDx.Loaders(Path.GetDirectoryName(game.Exe)).Count==0?"ReShade non è installato per questo eseguibile.":"ReShade è presente, ma manca il registro di installazione di HDLSS. Non posso ripristinare automaticamente file originali senza un backup verificabile.");review(RenoDx.Restore(game,RenoDx.Marker));});
  var uninstallButton=details.Children.OfType<Button>().Last();HdrChoices.Help(uninstallButton,"Rimuove ReShade installato da HDLSS e ripristina il backup. Se ReShade era già presente prima, torna alla versione precedente. Le mod RenoDX rimangono su disco ma non funzionano senza ReShade. OptiScaler e MFG separato non vengono rimossi.");
  button("Pulisci ReShade",()=>review(ComponentCleanup.Prepare(game,true)));
  button("Catalogo mod RenoDX",()=>Process.Start(new ProcessStartInfo("https://github.com/clshortfuse/renodx/wiki/Mods"){UseShellExecute=true}));
  button("Importa mod RenoDX locale…",()=>{var pick=new Microsoft.Win32.OpenFileDialog{Filter="Add-on HDR RenoDX|renodx-*.addon64",Title="Scegli la mod HDR scaricata dall’autore per questo gioco"};if(pick.ShowDialog(d)==true)review(RenoDx.InstallAddon(game,pick.FileName));});
  button("Installa / aggiorna RenoDX",async()=>{IsBusy=true;actions.IsEnabled=channels.IsEnabled=false;try{Core.Closed(game);info.Text="Cerco la mod RenoDX per "+game.Name+" e scarico il download del catalogo…";string origin="";RenoDx.CatalogMod selected=null;string file=await Task.Run(()=>RenoDx.FetchForGame(game,out origin,out selected));string existing=Path.Combine(Path.GetDirectoryName(game.Exe),Path.GetFileName(file));if(File.Exists(existing)&&DlssCore.Hash(existing)==DlssCore.Hash(file)){info.Text="RenoDX già aggiornata.\n"+origin;refreshState();return;}var plan=RenoDx.InstallAddon(game,file);plan.Summary=plan.Summary.Replace("Mod selezionata manualmente:","Mod individuata nel catalogo per questo titolo:")+"\n\n"+origin;review(plan);}catch(Exception ex){info.Text="Installazione non eseguita: "+ex.Message;if(ex.Message.Contains("Pagina della mod:")){var link=Regex.Match(ex.Message,@"https://(?:www\.)?nexusmods\.com/[a-zA-Z0-9_-]+/mods/[0-9]+/?");if(link.Success){info.Text="Download Nexus assistito: conferma sul sito, poi attendi il rilevamento del pacchetto.";try{NexusDownload.Show(d,game,link.Value,review);}catch(Exception assistError){info.Text=assistError.Message;}}}}finally{IsBusy=false;actions.IsEnabled=channels.IsEnabled=true;}});
  renodxInstall=actions.Children.OfType<Button>().Last();refreshState();
  button("Install MFG Unlock",async()=>{IsBusy=true;actions.IsEnabled=channels.IsEnabled=false;try{Core.Closed(game);if(RenoDx.Loaders(Path.GetDirectoryName(game.Exe)).Count!=1)throw new Exception("Install one ReShade loader with full add-on support first.");info.Text=Appearance.Localize("Checking the latest stable MFG Unlock release…");MfgAddon.Release release=null;string file=await Task.Run(()=>MfgAddon.Fetch(out release));var plan=MfgAddon.Prepare(game,file,release);review(plan);}catch(Exception ex){info.Text=Appearance.Localize(ex.Message);}finally{IsBusy=false;actions.IsEnabled=channels.IsEnabled=true;Appearance.Refresh();}});
  mfgInstall=actions.Children.OfType<Button>().Last();mfgInstall.Name="MfgInstall";HdrChoices.Help(mfgInstall,MfgAddon.Requirements);refreshState();
  var mfgInfo=Dialogs.Text("MFG Unlock · RTX 2000 / 3000 / 4000. Native DLSS FG required.",13);body.Children.Insert(body.Children.IndexOf(info)+1,mfgInfo);
  button("Ripristina MFG Unlock",()=>review(MfgAddon.Restore(game)));
  button("MFG Unlock · compatibility",()=>Process.Start(new ProcessStartInfo("https://github.com/"+MfgAddon.Repo+"#game-compatibility"){UseShellExecute=true}));
  button("Install UE4SS for mods",async()=>{if(IsBusy)return;IsBusy=true;try{Core.Closed(game);var plan=await Task.Run(()=>Ue4ssInstaller.DownloadAndPrepare(game,false));review(plan);}catch(Exception ex){info.Text=Appearance.Localize(ex.Message);}finally{IsBusy=false;Appearance.Refresh();}});

  var menu=new ComboBox{ItemsSource=InjectionSetup.MenuKeys.Skip(1).Select(InjectionSetup.KeyName).ToArray(),SelectedIndex=1,Margin=new Thickness(0,8,0,8)};details.Children.Add(Dialogs.Text("Tasto overlay ReShade · senza modificatori",14));details.Children.Add(menu);var map=new Button{Content="Applica tasto ReShade",Margin=new Thickness(0,0,0,10)};details.Children.Add(map);map.Click+=(sender,args)=>{if(IsBusy)return;try{review(RenoDx.MenuKey(game,InjectionSetup.MenuKeys[menu.SelectedIndex+1]));}catch(Exception ex){info.Text=Appearance.Localize(ex.Message);}};
  button("Ripristina tasto ReShade",()=>review(RenoDx.Restore(game,RenoDx.KeyMarker)));
  button("Ripristina RenoDX",()=>review(RenoDx.Restore(game,RenoDx.AddonMarker)));

  button("Importa pacchetto ReShade…",()=>{var pick=new Microsoft.Win32.OpenFileDialog{Filter="Pacchetto ReShade full add-on|*.exe;*.zip",Title="Pacchetto ufficiale full add-on o nightly"};if(pick.ShowDialog(d)==true){string file=RenoDx.Extract(pick.FileName,Path.Combine(Core.Data,"hdr-packages",Guid.NewGuid().ToString("N")));review(RenoDx.InstallReShade(game,file,"Pacchetto scelto manualmente: "+Path.GetFileName(pick.FileName)));}});
  body.Children.Remove(actions);body.Children.Insert(0,actions);body.Children.Add(EffectCatalog.Panel(game,embedded?owner:d,review,info));
  body.Children.Add(restores);body.Children.Add(advanced);var help=new Button{Content="? · Compatibilità e caricamento",HorizontalAlignment=HorizontalAlignment.Left};HdrChoices.Help(help,"ReShade può essere collegato a OptiScaler tramite ReShade64.dll. I loader sconosciuti e duplicati fermano l’installazione. Segui le indicazioni della mod RenoDX per le impostazioni HDR del gioco. Le importazioni locali non certificano l’ultima versione.");body.Children.Add(help);if(!embedded)d.Closing+=(s,e)=>{if(IsBusy)e.Cancel=true;};if(embedded){var shell=d;var layout=new Grid();layout.ColumnDefinitions.Add(new ColumnDefinition());layout.ColumnDefinitions.Add(new ColumnDefinition());var primary=new StackPanel{Margin=new Thickness(0,0,24,0)};var secondary=new StackPanel{Margin=new Thickness(0,0,0,0)};Grid.SetColumn(secondary,1);layout.Children.Add(primary);layout.Children.Add(secondary);var entries=body.Children.Cast<UIElement>().ToArray();body.Children.Clear();foreach(var item in entries)if(item!=advanced&&item!=restores&&item!=help)primary.Children.Add(item);secondary.Children.Add(advanced);secondary.Children.Add(restores);secondary.Children.Add(help);Action arrange=()=>{bool compact=layout.ActualWidth<1050;layout.ColumnDefinitions[1].Width=compact?new GridLength(0):new GridLength(1,GridUnitType.Star);if(layout.RowDefinitions.Count==0){layout.RowDefinitions.Add(new RowDefinition{Height=GridLength.Auto});layout.RowDefinitions.Add(new RowDefinition{Height=GridLength.Auto});}Grid.SetColumn(secondary,compact?0:1);Grid.SetRow(secondary,compact?1:0);};layout.SizeChanged+=(s,e)=>arrange();((Border)shell.Content).Child=layout;d=owner;return shell;}var border=(Border)d.Content;border.Child=null;border.Child=new ScrollViewer{Content=body,VerticalScrollBarVisibility=ScrollBarVisibility.Auto,MaxHeight=SystemParameters.WorkArea.Height-100};return d;
 }
}
}
