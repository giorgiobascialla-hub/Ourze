using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net;
using System.Runtime.InteropServices;
using System.Security.Cryptography.X509Certificates;
namespace HdrPilot {
public sealed class DlssLibrary {public string File,Path,Version,Relative;public override string ToString(){return File+" · "+Version+" · "+Relative;}}
public static class DlssLibraries {
 public const string Marker="hdr-unlock-dlls.json";
 public static readonly string[] Names={"nvngx_dlss.dll","nvngx_dlssd.dll","nvngx_dlssg.dll"};
 public static string Tpu(string file){return "https://www.techpowerup.com/download/"+(file=="nvngx_dlssd.dll"?"nvidia-dlss-3-ray-reconstruction-dll/":file=="nvngx_dlssg.dll"?"nvidia-dlss-3-frame-generation-dll/":"nvidia-dlss-dll/");}
 public static List<DlssLibrary> Scan(Game g){var found=new List<DlssLibrary>();Walk(g.Root,g.Root,found,0);return found;}
 static void Walk(string root,string dir,List<DlssLibrary> found,int depth){if(depth>32||found.Count>1000)throw new IOException("Scansione non completa: limite di sicurezza raggiunto.");foreach(string p in Directory.GetFiles(dir)){if(!Names.Contains(System.IO.Path.GetFileName(p),StringComparer.OrdinalIgnoreCase))continue;string rel=p.Substring(root.TrimEnd('\\').Length+1);DlssCore.Safe(root,rel);found.Add(new DlssLibrary{File=System.IO.Path.GetFileName(p),Path=p,Relative=rel,Version=FileVersionInfo.GetVersionInfo(p).FileVersion??"non disponibile"});}foreach(string sub in Directory.GetDirectories(dir)){if((File.GetAttributes(sub)&FileAttributes.ReparsePoint)!=0)continue;string name=System.IO.Path.GetFileName(sub);if(name.Equals("data",StringComparison.OrdinalIgnoreCase)||name.IndexOf("backup",StringComparison.OrdinalIgnoreCase)>=0||name.Equals(".git",StringComparison.OrdinalIgnoreCase))continue;Walk(root,sub,found,depth+1);}}
 public static string CopyState(List<DlssLibrary> files){
  if(files.Count<2)return "single";
  try{string first=DlssCore.Hash(files[0].Path);foreach(var file in files.Skip(1))if(DlssCore.Hash(file.Path)!=first)return "different";return "identical";}catch{return "unreadable";}
 }
 public static string CopyLabel(List<DlssLibrary> files){
  if(files.Count<2)return "";string state=CopyState(files);int lang=Array.IndexOf(new[]{"en","it","es","fr","de","pt"},Core.Pref.Language);if(lang<0)lang=0;
  string[] same={"identical files in {0} locations","file identici in {0} percorsi","archivos idénticos en {0} rutas","fichiers identiques dans {0} emplacements","identische Dateien an {0} Speicherorten","arquivos idênticos em {0} locais"};
  string[] different={"different files — review paths","file diversi — controlla i percorsi","archivos distintos — revisa las rutas","fichiers différents — vérifier les chemins","unterschiedliche Dateien — Pfade prüfen","arquivos diferentes — confira os caminhos"};
  string[] unknown={"comparison unavailable — review paths","confronto non disponibile — controlla i percorsi","comparación no disponible — revisa las rutas","comparaison indisponible — vérifier les chemins","Vergleich nicht verfügbar — Pfade prüfen","comparação indisponível — confira os caminhos"};
  return state=="identical"?String.Format(same[lang],files.Count):state=="different"?different[lang]:unknown[lang];
 }
 public static string CopyExplanation(){int lang=Array.IndexOf(new[]{"en","it","es","fr","de","pt"},Core.Pref.Language);if(lang<0)lang=0;return new[]{
 "Multiple locations do not mean multiple active effects. Identical means matching SHA-256 hashes, not verified loading or compatibility. Different files need review, not automatic deletion. Backups are excluded. Use the component buttons to inspect or update a specific path.",
 "Più percorsi non significano più effetti attivi. Identici indica hash SHA-256 uguali, non caricamento o compatibilità verificati. I file diversi richiedono un controllo, non una cancellazione automatica. I backup sono esclusi. Usa i pulsanti dei componenti per controllare o aggiornare il singolo percorso.",
 "Varias rutas no significan varios efectos activos. Idénticos indica hashes SHA-256 iguales, no carga ni compatibilidad verificadas. Revisa los archivos diferentes; no los borres automáticamente. Se excluyen las copias de seguridad. Los botones de componentes permiten revisar o actualizar una ruta.",
 "Plusieurs emplacements ne signifient pas plusieurs effets actifs. Identiques signifie des empreintes SHA-256 égales, sans confirmer le chargement ni la compatibilité. Vérifiez les fichiers différents sans suppression automatique. Les sauvegardes sont exclues. Les boutons des composants permettent de vérifier ou mettre à jour un chemin.",
 "Mehrere Speicherorte bedeuten nicht mehrere aktive Effekte. Identisch bedeutet gleiche SHA-256-Prüfsummen, keine bestätigte Ausführung oder Kompatibilität. Unterschiedliche Dateien prüfen, nicht automatisch löschen. Sicherungen sind ausgeschlossen. Über die Komponentenschaltflächen lassen sich einzelne Pfade prüfen oder aktualisieren.",
 "Vários caminhos não significam vários efeitos ativos. Idênticos indica hashes SHA-256 iguais, não carregamento ou compatibilidade verificados. Revise arquivos diferentes sem exclusão automática. Backups são excluídos. Os botões dos componentes permitem verificar ou atualizar um caminho."
 }[lang];}
 public static List<string> ParseTags(string json){return Core.Json.Deserialize<List<Dictionary<string,object>>>(json).Select(x=>Convert.ToString(x["name"])).Where(x=>System.Text.RegularExpressions.Regex.IsMatch(x,@"^v?[0-9]+(\.[0-9]+){1,3}$")).Distinct().OrderByDescending(x=>new Version(x.TrimStart('v'))).ToList();}
 static string Url(string tag,string file){if(!Names.Contains(file)||!System.Text.RegularExpressions.Regex.IsMatch(tag,@"^v?[0-9]+(\.[0-9]+){1,3}$"))throw new Exception("Versione o componente non valido.");return "https://raw.githubusercontent.com/NVIDIA/DLSS/"+tag+"/lib/Windows_x86_64/rel/"+file;}
 public static List<string> Versions(string file,Action<string> progress){
  ServicePointManager.SecurityProtocol|=SecurityProtocolType.Tls12;
  using(var web=new DlssCore.DownloadClient()){web.Headers[HttpRequestHeader.UserAgent]="HDR-Unlock/0.1";
   var tags=ParseTags(web.DownloadString("https://api.github.com/repos/NVIDIA/DLSS/tags?per_page=100"));var result=new List<string>();
   foreach(string tag in tags){progress("Controllo disponibilità "+tag+" · "+result.Count+"/10…");var request=(HttpWebRequest)WebRequest.Create(Url(tag,file));request.Method="HEAD";request.Timeout=12000;request.UserAgent="HDR-Unlock/0.1";
    try{using(var response=request.GetResponse()){}result.Add(tag);}catch(WebException ex){var response=ex.Response as HttpWebResponse;if(response==null||response.StatusCode!=HttpStatusCode.NotFound)throw;response.Close();}
    if(result.Count==10)break;
   }return result;
  }
 }
 public static string Latest(string file,Action<string> progress){var versions=Versions(file,progress);if(versions.Count==0)throw new Exception("Nessuna versione disponibile.");return Download(file,versions[0],progress);}
 public static string Download(string file,string tag,Action<string> progress){string url=Url(tag,file);ServicePointManager.SecurityProtocol|=SecurityProtocolType.Tls12;
  using(var web=new DlssCore.DownloadClient()){web.Headers[HttpRequestHeader.UserAgent]="HDR-Unlock/0.1";string dir=System.IO.Path.Combine(Core.Data,"dll-library",tag);Directory.CreateDirectory(dir);string dest=DlssCore.Safe(dir,file);progress("Scarico NVIDIA "+tag+" · "+file+"…");string temp=dest+".download";web.DownloadFile(url,temp);Validate(temp,file);File.Copy(temp,dest,true);File.Delete(temp);File.WriteAllText(dest+".source.txt",url+"\nSHA256 "+DlssCore.Hash(dest));return dest;}
 }
 [StructLayout(LayoutKind.Sequential,CharSet=CharSet.Unicode)]struct TrustFile {public uint Size;[MarshalAs(UnmanagedType.LPWStr)]public string Path;public IntPtr Handle,Subject;}
 [StructLayout(LayoutKind.Sequential,CharSet=CharSet.Unicode)]struct TrustData {public uint Size;public IntPtr Policy,Sip;public uint UI,Revocation,Choice;public IntPtr File;public uint Action;public IntPtr State;[MarshalAs(UnmanagedType.LPWStr)]public string Url;public uint Flags,Context;}
 [DllImport("wintrust.dll",ExactSpelling=true,CharSet=CharSet.Unicode)]static extern int WinVerifyTrust(IntPtr window,[In]ref Guid action,[In]ref TrustData data);
 public static void Validate(string path,string expected){if(!DlssCore.Pe64(path))throw new Exception("La DLL non è un file PE x64 valido.");var info=FileVersionInfo.GetVersionInfo(path);string product=(info.ProductName??"").ToLowerInvariant();bool component=expected=="nvngx_dlss.dll"?product.Contains("supersampling"):expected=="nvngx_dlssd.dll"?product.Contains("ray reconstruction"):expected=="nvngx_dlssg.dll"?product.Contains("dlss-g"):false;if(!component)throw new Exception("Il componente interno non corrisponde a "+expected+".");
  var f=new TrustFile{Size=(uint)Marshal.SizeOf(typeof(TrustFile)),Path=path};IntPtr mem=Marshal.AllocHGlobal(Marshal.SizeOf(typeof(TrustFile)));try{Marshal.StructureToPtr(f,mem,false);var d=new TrustData{Size=(uint)Marshal.SizeOf(typeof(TrustData)),UI=2,Choice=1,File=mem,Flags=0x1000};Guid id=new Guid("00AAC56B-CD44-11d0-8CC2-00C04FC295EE");int hr=WinVerifyTrust(new IntPtr(-1),ref id,ref d);if(hr!=0)throw new Exception("Firma digitale non verificabile (0x"+hr.ToString("X8")+"). DLL non applicata.");var cert=new X509Certificate2(X509Certificate.CreateFromSignedFile(path));if(cert.Subject.IndexOf("NVIDIA",StringComparison.OrdinalIgnoreCase)<0)throw new Exception("La DLL non risulta firmata da NVIDIA.");}finally{Marshal.DestroyStructure(mem,typeof(TrustFile));Marshal.FreeHGlobal(mem);}}
 public static void ValidateTarget(Game g,DlssLibrary target){
  if(g.ProtectedInstall)throw new Exception(g.Evidence);
  if(target==null||!Names.Contains(target.File,StringComparer.OrdinalIgnoreCase))throw new Exception("Seleziona una DLL DLSS 5 supportata.");
  string path=DlssCore.Safe(g.Root,target.Relative);
  if(!String.Equals(path,System.IO.Path.GetFullPath(target.Path),StringComparison.OrdinalIgnoreCase)||!String.Equals(System.IO.Path.GetFileName(path),target.File,StringComparison.OrdinalIgnoreCase))throw new Exception("La DLL selezionata non corrisponde al percorso del gioco.");
  if(!DlssCore.Pe64(path))throw new Exception("La DLL del gioco non è leggibile come componente x64. Nessuna modifica: verifica i permessi o ripara i file dal launcher.");
 }
 public static DlssPlan Prepare(Game g,DlssLibrary target,string source){ValidateTarget(g,target);var scan=DlssCore.Inspect(g,false);if(scan.Blockers.Count>0)throw new Exception(String.Join("\n",scan.Blockers));Validate(source,target.File);if(!File.Exists(target.Path))throw new Exception("DLL del gioco non trovata.");if(DlssCore.Hash(source)==DlssCore.Hash(target.Path))throw new Exception("Questa DLL è già identica a quella presente nel gioco: nessuna modifica necessaria.");var manifest=File.Exists(DlssCore.Safe(g.Root,Marker))?DlssCore.ReadManifest(g.Root,Marker):new DlssManifest{Game=g.Id,Folder=System.IO.Path.GetFullPath(g.Root),PreviousOwner="DLL presenti prima dello scambio",BackupRoot="dll-backups/"+Guid.NewGuid().ToString("N")};
  var p=new DlssPlan{Folder=System.IO.Path.GetFullPath(g.Root),NativeSwap=true,MarkerName=Marker,Manifest=manifest,MarkerExpected=File.Exists(DlssCore.Safe(g.Root,Marker))?DlssCore.Hash(DlssCore.Safe(g.Root,Marker)):null,Summary=g.Name+"\n"+target.Relative+"\nVersione presente: "+target.Version+"\nVersione scelta: "+FileVersionInfo.GetVersionInfo(source).FileVersion+"\nFirma NVIDIA verificata. La versione più recente può richiedere driver più recenti e non garantisce compatibilità col gioco.\nCambiare DLL non aggiunge funzionalità che il gioco non implementa."};p.Changes.Add(new DlssChange{Name=target.Relative,Source=source,SourceHash=DlssCore.Hash(source),Expected=DlssCore.Hash(target.Path)});return p;
 }
 public static DlssPlan Restore(Game g,DlssLibrary target=null){var m=DlssCore.ReadManifest(g.Root,Marker);var plan=new DlssPlan{Folder=System.IO.Path.GetFullPath(g.Root),NativeSwap=true,MarkerName=Marker,Uninstall=true,Summary="Ripristina le DLL presenti prima del primo scambio effettuato da HDLSS. Eventuali mod preesistenti sono parte di quello stato."};var chosen=m.Files.Where(f=>target==null||f.Name.Equals(target.Relative,StringComparison.OrdinalIgnoreCase)).ToList();if(chosen.Count==0)throw new Exception("Nessun backup gestito per questa DLL.");foreach(var f in chosen){string dst=DlssCore.Safe(g.Root,f.Name),src=DlssCore.Safe(Core.Data,f.Backup);if(!File.Exists(dst)||DlssCore.Hash(dst)!=f.After||!File.Exists(src)||DlssCore.Hash(src)!=f.Before)throw new Exception("DLL o backup modificato: "+f.Name);plan.Changes.Add(new DlssChange{Name=f.Name,Source=src,SourceHash=f.Before,RestoreAttributes=f.Attributes,Expected=f.After});}m.Files=m.Files.Except(chosen).ToList();plan.Summary=target==null?plan.Summary:"Ripristina solo "+target.Relative+" alla versione presente prima dello scambio. Le altre DLL restano invariate.";plan.Changes.Add(new DlssChange{Name=Marker,Expected=DlssCore.Hash(DlssCore.Safe(g.Root,Marker)),Delete=m.Files.Count==0,Bytes=m.Files.Count==0?null:new System.Text.UTF8Encoding(false).GetBytes(Core.Json.Serialize(m))});return plan;}
}
}
