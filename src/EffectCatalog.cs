using System;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Net;
using System.Text;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;
using System.Threading.Tasks;
namespace HdrPilot {
public sealed class EffectPackage {public string Id,Name,Repo,Branch,Description;public bool Addon;}
public static class EffectCatalog {
 public static readonly EffectPackage[] All={
 new EffectPackage{Id="autohdr",Name="AutoHDR · uscita HDR",Repo="EndlesslyFlowering/AutoHDR-ReShade",Addon=true,Description="Add-on sperimentale: forza l’uscita HDR in alcuni giochi. Abbinalo a Pumbo oppure all’inverse tone mapping di Lilium. Non garantisce supporto universale; non usarlo con anti-cheat. Non combinarlo con RenoDX HDR o HDR del motore attivo."},
 new EffectPackage{Id="pumbo",Name="Pumbo · conversione SDR → HDR",Repo="Filoppi/PumboAutoHDR",Branch="master",Description="Shader: espande le luci SDR e offre regolazioni HDR. Richiede un’uscita scRGB/HDR già disponibile, per esempio tramite l’add-on AutoHDR. Da solo non abilita HDR. Attiva e regola AdvancedAutoHDR nel menu ReShade."},
 new EffectPackage{Id="lilium",Name="Lilium · analisi e strumenti HDR",Repo="EndlesslyFlowering/ReShade_HDR_shaders",Branch="master",Description="Shader per analizzare luminanza, regolare neri e tone mapping. Include conversione SDR → HDR, che richiede un’uscita HDR. L’analisi non equivale a una misura fisica del monitor. Seleziona solo gli effetti necessari in ReShade."},
 new EffectPackage{Id="quint",Name="qUINT · effetti visivi",Repo="martymcmodding/qUINT",Branch="master",Description="Shader per contrasto, nitidezza, profondità e altri effetti. Non converte da solo un gioco in HDR; compatibilità HDR da verificare per effetto. MXAO e profondità di campo richiedono accesso alla profondità della scena."},
 new EffectPackage{Id="sweetfx",Name="SweetFX · colori e nitidezza",Repo="CeeJayDK/SweetFX",Branch="master",Description="Raccolta di shader per colori, nitidezza e antialiasing. FakeHDR è un effetto estetico SDR, non un’uscita HDR reale. Per immagini HDR usa solo effetti compatibili e verifica il risultato."}};
 static string Marker(EffectPackage p){return "hdr-unlock-effects-"+p.Id+".json";}
 static string Prefix(EffectPackage p){return "reshade-shaders/HDRUnlock-"+p.Id;}
 public static void Extract(string archive,string output){
  long total=0;using(var stream=File.OpenRead(archive))using(var zip=new ZipArchive(stream,ZipArchiveMode.Read)){
   foreach(var e in zip.Entries){string n=e.FullName.Replace('\\','/');int cut=n.IndexOf('/');if(cut<0||n.EndsWith("/"))continue;n=n.Substring(cut+1);if(!(n.StartsWith("Shaders/",StringComparison.OrdinalIgnoreCase)||n.StartsWith("Textures/",StringComparison.OrdinalIgnoreCase)))continue;
    string extension=Path.GetExtension(n).ToLowerInvariant();if(!new[]{".fx",".fxh",".h",".png",".jpg",".jpeg",".dds",".bmp",".tga"}.Contains(extension))continue;
    total+=e.Length;if(total>250000000||e.Length>64000000)throw new Exception("Pacchetto shader troppo grande.");string dest=DlssCore.Safe(output,n);Directory.CreateDirectory(Path.GetDirectoryName(dest));using(var input=e.Open())using(var target=File.Create(dest))input.CopyTo(target);
   }
  }
 }
 static string ConfigFile(string root){var loaders=RenoDx.Loaders(root);if(loaders.Count!=1)throw new Exception("Installa prima una sola copia ReShade con supporto add-on.");string name="ReShade.ini",custom=Path.ChangeExtension(loaders[0],".ini");return !File.Exists(Path.Combine(root,name))&&File.Exists(Path.Combine(root,custom))?custom:name;}
 public static DlssPlan Prepare(Game game,EffectPackage p,Action<string> progress){
  Core.Closed(game);string root=Path.GetDirectoryName(game.Exe),config=ConfigFile(root),marker=Marker(p);if(p.Addon){
   bool engine=game.Config!=null&&File.Exists(Path.Combine(game.Config,"Engine.ini"))&&Core.Get(File.ReadAllText(Path.Combine(game.Config,"Engine.ini")),"SystemSettings","r.HDR.EnableHDROutput")=="1";
   if(engine||Directory.GetFiles(root,"renodx-*.addon64").Any(f=>Path.GetFileName(f).IndexOf("dlss",StringComparison.OrdinalIgnoreCase)<0))throw new Exception("AutoHDR: disattiva prima HDR del motore o rimuovi RenoDX HDR. Evita due conversioni HDR sovrapposte.");
  }
  string cache=Path.Combine(Core.Data,"effect-packages",Guid.NewGuid().ToString("N"));Directory.CreateDirectory(cache);string source=Path.Combine(cache,"files");Directory.CreateDirectory(source);string origin="";
  ServicePointManager.SecurityProtocol|=SecurityProtocolType.Tls12;using(var web=new DlssCore.DownloadClient()){web.Headers[HttpRequestHeader.UserAgent]="HDR-Unlock/0.1";progress("Scarico "+p.Name+" dal repository dell’autore…");
   if(p.Addon){var release=Core.Json.Deserialize<Dictionary<string,object>>(web.DownloadString("https://api.github.com/repos/"+p.Repo+"/releases/latest"));var assets=((System.Collections.IEnumerable)release["assets"]).Cast<Dictionary<string,object>>();var asset=assets.SingleOrDefault(a=>Convert.ToString(a["name"])=="AutoHDR.addon64");if(asset==null)throw new Exception("Release senza AutoHDR.addon64 diretto: formato non supportato.");origin=Convert.ToString(asset["browser_download_url"]);if(!origin.StartsWith("https://github.com/"+p.Repo+"/releases/download/",StringComparison.Ordinal))throw new Exception("Origine add-on non valida.");string dest=Path.Combine(source,"AutoHDR.addon64");web.DownloadFile(origin,dest);if(!DlssCore.Pe64(dest))throw new Exception("Add-on non x64.");string digest=asset.ContainsKey("digest")?Convert.ToString(asset["digest"]):"";if(digest.StartsWith("sha256:")&&DlssCore.Hash(dest)!=digest.Substring(7))throw new Exception("Hash add-on non valido.");
   }else{
    string standard=Path.Combine(cache,"standard.zip");web.DownloadFile("https://github.com/crosire/reshade-shaders/archive/slim.zip",standard);Extract(standard,source);
    origin="https://github.com/"+p.Repo+"/archive/refs/heads/"+p.Branch+".zip";string archive=Path.Combine(cache,"package.zip");web.DownloadFile(origin,archive);Extract(archive,source);
    if(!Directory.GetFiles(source,"*.fx",SearchOption.AllDirectories).Any())throw new Exception("Pacchetto senza shader.");
   }
  }
  var plan=new DlssPlan{Folder=root,MarkerName=marker,MarkerExpected=File.Exists(Path.Combine(root,marker))?DlssCore.Hash(Path.Combine(root,marker)):null,Summary=p.Name+"\n"+p.Description+"\nFonte: "+origin+"\nShader scaricati dal ramo corrente. Effetti da attivare nel menu ReShade; nessuna garanzia di HDR attivo.",Manifest=File.Exists(Path.Combine(root,marker))?DlssCore.ReadManifest(root,marker):new DlssManifest{Game=game.Id,Folder=root,BackupRoot="effect-backups/"+Guid.NewGuid().ToString("N"),PreviousOwner="File precedenti del pacchetto"}};
  foreach(string file in Directory.GetFiles(source,"*",SearchOption.AllDirectories)){string relative=file.Substring(source.Length+1),name=p.Addon?relative:Prefix(p)+"/"+relative;string dest=DlssCore.Safe(root,name);plan.Changes.Add(new DlssChange{Name=name,Source=file,SourceHash=DlssCore.Hash(file),Expected=File.Exists(dest)?DlssCore.Hash(dest):null});}
  if(!p.Addon){string path=Path.Combine(root,config),ini=File.Exists(path)?File.ReadAllText(path):"";foreach(string key in new[]{"EffectSearchPaths","TextureSearchPaths"}){string suffix=key=="EffectSearchPaths"?"Shaders":"Textures",entry=".\\"+Prefix(p).Replace('/','\\')+"\\"+suffix+"\\**";var values=(Core.Get(ini,"GENERAL",key)??"").Split(',').Where(x=>!String.IsNullOrWhiteSpace(x)).ToList();if(!values.Contains(entry,StringComparer.OrdinalIgnoreCase))values.Add(entry);ini=Core.Set(ini,"GENERAL",key,String.Join(",",values));}plan.Changes.Add(new DlssChange{Name=config,Bytes=new UTF8Encoding(false).GetBytes(ini),Expected=File.Exists(path)?DlssCore.Hash(path):null});}
  ActivityLog.Write("Pacchetto ReShade pronto: "+p.Name+" · "+origin);return plan;
 }
 public static DlssPlan Remove(Game game,EffectPackage p){
  string root=Path.GetDirectoryName(game.Exe),marker=Marker(p);var manifest=DlssCore.ReadManifest(root,marker);var plan=new DlssPlan{Folder=root,Uninstall=true,Summary="Rimuovi solo "+p.Name+". Conserva gli altri pacchetti. Copia di recupero prima di scrivere."};
  foreach(var f in manifest.Files){if(!p.Addon&&!f.Name.Replace('\\','/').StartsWith(Prefix(p)+"/",StringComparison.OrdinalIgnoreCase))continue;string dest=DlssCore.Safe(root,f.Name);if(!File.Exists(dest)||DlssCore.Hash(dest)!=f.After)throw new Exception("File modificato: "+f.Name+". Rimozione fermata.");string backup=f.Before==null?null:DlssCore.Safe(Core.Data,f.Backup);if(backup!=null&&(!File.Exists(backup)||DlssCore.Hash(backup)!=f.Before))throw new Exception("Backup non valido.");plan.Changes.Add(new DlssChange{Name=f.Name,Expected=f.After,Delete=backup==null,Source=backup,SourceHash=f.Before});}
  if(!p.Addon){string config=ConfigFile(root),path=Path.Combine(root,config);if(File.Exists(path)){string ini=File.ReadAllText(path);foreach(string key in new[]{"EffectSearchPaths","TextureSearchPaths"}){string expected=".\\"+Prefix(p).Replace('/','\\')+"\\"+(key=="EffectSearchPaths"?"Shaders":"Textures")+"\\**";ini=Core.Set(ini,"GENERAL",key,String.Join(",",(Core.Get(ini,"GENERAL",key)??"").Split(',').Where(x=>!String.Equals(x,expected,StringComparison.OrdinalIgnoreCase))));}plan.Changes.Add(new DlssChange{Name=config,Expected=DlssCore.Hash(path),Bytes=new UTF8Encoding(false).GetBytes(ini)});}}
  plan.Changes.Add(new DlssChange{Name=marker,Expected=DlssCore.Hash(Path.Combine(root,marker)),Delete=true});return plan;
 }
 public static FrameworkElement Panel(Game game,Window owner,Action<DlssPlan> review,TextBlock info){
  var panel=new StackPanel{Margin=new Thickness(0,16,0,0)};panel.Children.Add(Dialogs.Text("Add-on e shader ReShade",16));panel.Children.Add(Dialogs.Text("Selezione dai progetti degli autori e dal catalogo ReShade; non è una classifica dei download.",12));var choices=new WrapPanel();panel.Children.Add(choices);EffectPackage selected=All[0];var description=Dialogs.Text(selected.Description,14);panel.Children.Add(description);var buttons=new List<Button>();foreach(var entry in All){var item=entry;var button=HdrChoices.Option(item.Name);HdrChoices.Help(button,item.Description);buttons.Add(button);choices.Children.Add(button);button.Click+=(s,e)=>{selected=item;description.Text=item.Description;foreach(var b in buttons)b.Style=owner.TryFindResource(b==button?(object)"Primary":typeof(Button)) as Style;};}buttons[0].Style=owner.TryFindResource("Primary") as Style;var actions=new WrapPanel();panel.Children.Add(actions);var install=new Button{Content="Installa selezionato",Margin=new Thickness(0,8,8,0)};var remove=new Button{Content="Rimuovi selezionato",Margin=new Thickness(0,8,8,0)};actions.Children.Add(install);actions.Children.Add(remove);
  install.Click+=async(s,e)=>{if(RenoDxUi.IsBusy)return;RenoDxUi.IsBusy=true;panel.IsEnabled=false;try{var choice=selected;var plan=await Task.Run(()=>Prepare(game,choice,msg=>owner.Dispatcher.Invoke(new Action(()=>info.Text=msg))));review(plan);}catch(Exception ex){info.Text=ex.Message;}finally{RenoDxUi.IsBusy=false;panel.IsEnabled=true;}};
  remove.Click+=(s,e)=>{if(RenoDxUi.IsBusy)return;try{Core.Closed(game);review(Remove(game,selected));}catch(Exception ex){info.Text=ex.Message;}};return panel;
 }
}
}
