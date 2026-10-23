using System;using System.IO;using System.IO.Compression;using System.Linq;using System.Collections.Generic;using System.Text;using System.Text.RegularExpressions;using System.Net;
namespace HdrPilot {
public static class NrFeeder {
 public const string Marker="hdlss-nr-feeder.json",Prefix="reshade-shaders/HDLSS-Feeder",Preset="HDLSS-Feeder.ini";
 public const string Repo="jlrouzies-fr/DLSS5-Feeder";
 public static string Help {get{return RenoNr.T(
 "Senza DLSS nativo · sperimentale. Installazione automatica per giochi x64 DirectX 11/12/Vulkan: completa prima ReShade. Installa Feeder, LumeniteFX, RenoDX NR 8.5 e le DLL SR/NR per la GPU. ShortFuse e altri iniettori NR non vanno combinati. Nel gioco verifica Generic Depth, poi LUMENITE: Kernel sopra DLSS 5 Feed; disattiva MSAA/SSAA. I vettori stimati possono produrre scie e modificare l’HUD. Per Vulkan usa Avvia con Vulkan dall’app; disattiva NVIDIA Smooth Motion per il gioco. File installati non significa NR attivo: verifica nel pannello NR e nei log. DX9/10, OpenGL e giochi a 32 bit richiedono altri loader/helper: non sono installati automaticamente da questa versione.",
 "Without native DLSS · experimental. Automatic setup for x64 DirectX 11/12/Vulkan games: install ReShade first. Installs Feeder, LumeniteFX, RenoDX NR 8.5 and GPU-specific SR/NR DLLs. Do not combine with ShortFuse or other NR injectors. In game verify Generic Depth, then LUMENITE: Kernel before DLSS 5 Feed; disable MSAA/SSAA. Estimated motion can cause ghosting and affect the HUD. For Vulkan use Launch with Vulkan in the app; disable NVIDIA Smooth Motion for the game. Installed files do not prove active NR: check its panel and logs. DX9/10, OpenGL and 32-bit games need additional loaders/helpers not installed automatically by this version.",
 "Sin DLSS · experimental. Instalación automática x64 DX11/12/Vulkan tras ReShade. Incluye Feeder, LumeniteFX, RenoDX NR 8.5 y DLL SR/NR. No combinar con ShortFuse. Verifica profundidad y Kernel antes de Feed; desactiva MSAA. DX9/10, OpenGL y 32 bits requieren otros componentes no automatizados.",
 "Sans DLSS · expérimental. Installation x64 DX11/12/Vulkan après ReShade : Feeder, LumeniteFX, RenoDX NR 8.5 et DLL SR/NR. Ne pas combiner avec ShortFuse. Vérifiez profondeur et Kernel avant Feed, désactivez MSAA. DX9/10, OpenGL et 32 bits nécessitent d’autres composants non automatisés.",
 "Ohne DLSS · experimentell. Automatisch für x64 DX11/12/Vulkan nach ReShade: Feeder, LumeniteFX, RenoDX NR 8.5 und SR/NR-DLLs. Nicht mit ShortFuse kombinieren. Tiefe und Kernel vor Feed prüfen, MSAA aus. DX9/10, OpenGL und 32 Bit benötigen weitere, nicht automatisierte Komponenten.",
 "Sem DLSS · experimental. Instalação x64 DX11/12/Vulkan após ReShade: Feeder, LumeniteFX, RenoDX NR 8.5 e DLL SR/NR. Não combinar com ShortFuse. Verifique profundidade e Kernel antes de Feed, desative MSAA. DX9/10, OpenGL e 32 bits exigem componentes não automatizados.");}}
 public static bool Installed(Game g){return g!=null&&!String.IsNullOrEmpty(g.Exe)&&File.Exists(Path.Combine(Path.GetDirectoryName(g.Exe),Marker));}
 public static bool Owns(Game g,string path){return Installed(g)&&DlssCore.ReadManifest(Path.GetDirectoryName(g.Exe),Marker).Files.Any(f=>String.Equals(DlssCore.Safe(Path.GetDirectoryName(g.Exe),f.Name),path,StringComparison.OrdinalIgnoreCase));}
 public static void Check(Game g){
  Core.Closed(g);if(VulkanSupport.NativeDlss(g))throw new Exception("Questo renderer usa DLSS Vulkan nativo: seleziona Con DLSS nativo. Feeder puo interferire con Frame Generation.");var scan=DlssCore.Inspect(g,true,false);
  if(g.ProtectedInstall||scan.Blockers.Count>0)throw new Exception(String.Join("\n",scan.Blockers));
  string api=InjectionSetup.EffectiveApi(g);if(api!="dx11"&&api!="dx12"&&api!="vulkan")throw new Exception("Feeder: installazione automatica disponibile solo per x64 DX11/DX12/Vulkan. Verifica l’API in Eseguibile e grafica; non forzare un’API diversa da quella del gioco.");
  if(scan.Proxy!="")throw new Exception("Rimuovi prima OptiScaler: questo percorso usa RenoDX NR 8.5.");
  var loaders=RenoDx.Loaders(scan.Folder);if(loaders.Count!=1||!DlssCore.ContainsAscii(Path.Combine(scan.Folder,loaders[0]),"LoadFromDllMain"))throw new Exception("Completa prima il passaggio 1: ReShade con supporto add-on.");
  if(File.Exists(Path.Combine(scan.Folder,RenoNr.Marker)))throw new Exception("Rimuovi prima il percorso NR precedente, poi installa Senza DLSS.");
  foreach(string file in RenoDx.AllAddons(g)){string name=Path.GetFileName(file).ToLowerInvariant();if(name.Contains("shortfuse")||name=="renodx-dlss.addon64"||name.Contains("deep-fried")||name.Contains("bridge")||name.Contains("feed-helper")||name.Contains("toolkit")||name.Contains("dlss")&&!(Installed(g)&&(name==RenoNr.Name||name=="dlss5-feed.addon64")))throw new Exception("Iniettore incompatibile o non gestito: "+name);}
 }
 static string Fetch(string url,string hash,string name){
  return DownloadCache.Get("feeder-packages",hash,dir=>{string file=Path.Combine(dir,name);using(var w=new DlssCore.DownloadClient{MaxDownloadBytes=64000000})w.DownloadFile(url,file);if(!DlssCore.Hash(file).Equals(hash,StringComparison.OrdinalIgnoreCase))throw new IOException("Hash diverso dalla fonte: "+name);return file;});
 }
 public static string Latest(out string version){
  ServicePointManager.SecurityProtocol|=SecurityProtocolType.Tls12;
  using(var w=new DlssCore.DownloadClient()){w.Headers[HttpRequestHeader.UserAgent]=AppIdentity.UserAgent;var r=Core.Json.Deserialize<Dictionary<string,object>>(w.DownloadString("https://api.github.com/repos/"+Repo+"/releases/latest"));version=Convert.ToString(r["tag_name"]);
   if(!Regex.IsMatch(version,@"^v\d+\.\d+\.\d+$")||Convert.ToBoolean(r["draft"])||Convert.ToBoolean(r["prerelease"]))throw new IOException("Release Feeder non stabile.");
   string name="DLSS5-Feeder-"+version.Substring(1)+".zip";var a=((System.Collections.IEnumerable)r["assets"]).Cast<Dictionary<string,object>>().Single(x=>Convert.ToString(x["name"])==name);string url=Convert.ToString(a["browser_download_url"]),digest=a.ContainsKey("digest")?Convert.ToString(a["digest"]):"";
   if(url!="https://github.com/"+Repo+"/releases/download/"+version+"/"+name||!Regex.IsMatch(digest,@"^sha256:[a-fA-F0-9]{64}$")||Convert.ToInt64(a["size"])>16000000)throw new IOException("Origine Feeder non valida.");
   return Fetch(url,digest.Substring(7),name);
  }
 }
 public static void Extract(string archive,string output,bool vulkan=false){
  Directory.CreateDirectory(output);using(var z=new ZipArchive(File.OpenRead(archive),ZipArchiveMode.Read)){
   foreach(string name in new[]{"dlss5-feed.addon64","reshade-shaders/Shaders/DLSS5_Feed.fx"}.Concat(vulkan?new[]{"layer-x64/VkLayer_feed_vk.dll"}:new string[0])){
    var entries=z.Entries.Where(e=>e.FullName.Replace('\\','/')==name).ToArray();if(entries.Length!=1)throw new IOException("Pacchetto Feeder incompleto o ambiguo.");
    string target=DlssCore.Safe(output,name);Directory.CreateDirectory(Path.GetDirectoryName(target));using(var s=LimitedStream.Open(entries[0],16000000))using(var d=File.Create(target))s.CopyTo(d);
   }
  }if(!DlssCore.Pe64(Path.Combine(output,"dlss5-feed.addon64")))throw new IOException("Feeder non x64.");
 }
 public static DlssPlan DownloadAndPrepare(Game game,Action<string> progress){
  Check(game);var hardware=DlssHardware.Detect();hardware.Validate(hardware.Series,"");var model=NrModels.Supplied().FirstOrDefault(m=>m.GpuSeries==hardware.Series);if(model==null)throw new Exception("Modello NR automatico disponibile per RTX 4000/5000.");NrModels.Compatible(model,hardware.Series);
  progress("Scarico Feeder e dipendenze verificate…");string version,archive=Latest(out version),source=Path.Combine(Core.Data,"feeder-packages",Guid.NewGuid().ToString("N"));Extract(archive,source,InjectionSetup.EffectiveApi(game)=="vulkan");
  string lumenite=Fetch("https://github.com/umar-afzaal/LumeniteFX/archive/25f40a2ac70bc90bd3dc53bb2fc9d5fa50536b8b.zip","06d40e49b54b7e2fcae61b1f489b25a0c1779796160955760a3739b28cd103cc","lumenite.zip");
  EffectCatalog.Extract(lumenite,Path.Combine(source,"shader"));
  string[] headers={"ReShade.fxh","ReShadeUI.fxh"},hashes={"6dabfbbaf968c3871905d2ea17f96572ff7b1cec01310b5d0e5252b66b30174f","78adf672df47460297eb9fe6dd238d2aafa24510b52b84feb1a745dff70eb901"};
  for(int i=0;i<headers.Length;i++)File.Copy(Fetch("https://raw.githubusercontent.com/crosire/reshade-shaders/fd0022170615ce0d8162d219bff07232fa6dd84f/Shaders/"+headers[i],hashes[i],headers[i]),Path.Combine(source,"shader","Shaders",headers[i]),true);
  var sr=DllCatalog.Fetch("nvngx_dlss.dll").FirstOrDefault();if(sr==null)throw new Exception("Catalogo SR vuoto.");
  return Prepare(game,source,DllCatalog.Download(sr,progress),NrModels.Download(model,progress),RenoNr.Bundled(),version);
 }
 public static DlssPlan Prepare(Game g,string source,string sr,string nr,string addon,string version){
  Check(g);DlssLibraries.Validate(sr,"nvngx_dlss.dll");string nrHash=DlssCore.Hash(nr);if(!DlssCore.Pe64(nr)||!NrModels.Supplied().Any(x=>x.Digest==nrHash))throw new IOException("Modello NR non verificato.");if(DlssCore.Hash(addon)!=RenoNr.CurrentHash)throw new Exception("Feeder richiede RenoDX NR 8.5 verificato; ShortFuse non compatibile.");
  string root=Path.GetDirectoryName(g.Exe),marker=DlssCore.Safe(root,Marker);
  var m=Installed(g)?DlssCore.ReadManifest(root,Marker):new DlssManifest{Game=g.Id,Folder=root,BackupRoot="feeder-backups/"+Guid.NewGuid().ToString("N"),PreviousOwner="Before NR Feeder"};
  m.Build="NR Feeder";m.Release=version;
  var p=new DlssPlan{Folder=root,Manifest=m,MarkerName=Marker,MarkerExpected=File.Exists(marker)?DlssCore.Hash(marker):null,Summary="Neural Rendering senza DLSS · "+version+"\n"+g.Name+"\nReShade + Feeder + LumeniteFX + RenoDX NR 8.5. Verifica profondità e attivazione in gioco."};
  Add(p,"dlss5-feed.addon64",Path.Combine(source,"dlss5-feed.addon64"));
  if(InjectionSetup.EffectiveApi(g)=="vulkan"){string layer=Path.Combine(source,"layer-x64","VkLayer_feed_vk.dll");if(!DlssCore.Pe64(layer))throw new IOException("Layer Feeder non x64.");Add(p,VulkanSupport.FeedDll,layer);Add(p,VulkanSupport.FeedManifest,null,VulkanSupport.Layer("VK_LAYER_feed_vk","VkLayer_feed_vk.dll",false));}
  Add(p,RenoNr.Name,addon);Add(p,"nvngx_dlss.dll",sr);Add(p,"nvngx_dlssnr.dll",nr);
  foreach(string f in Directory.GetFiles(Path.Combine(source,"shader"),"*",SearchOption.AllDirectories)){string relative=f.Substring(Path.Combine(source,"shader").Length+1).Replace('\\','/');if(relative.EndsWith(".fx",StringComparison.OrdinalIgnoreCase)&&!relative.EndsWith("lumenite_Kernel.fx",StringComparison.OrdinalIgnoreCase))continue;Add(p,Prefix+"/"+relative,f);}
  Add(p,Prefix+"/Shaders/DLSS5_Feed.fx",Path.Combine(source,"reshade-shaders","Shaders","DLSS5_Feed.fx"));
  if(!p.Changes.Any(c=>c.Name.EndsWith("/lumenite_Kernel.fx"))||!p.Changes.Any(c=>c.Name.EndsWith("/lumenite_bluenoise256.png")))throw new IOException("Dipendenze motion vectors incomplete.");
  string config=MfgAddon.ConfigName(root),path=DlssCore.Safe(root,config),text=File.Exists(path)?File.ReadAllText(path):"";
  string addonPath=Core.Get(text,"ADDON","AddonPath");if(!String.IsNullOrEmpty(addonPath)&&addonPath!="."&&addonPath!=".\\")throw new Exception("AddonPath personalizzato: usa la cartella del gioco per il percorso Feeder.");
  string oldPreset=Core.Get(text,"GENERAL","PresetPath"),preset="";
  if(!Installed(g)&&!String.IsNullOrEmpty(oldPreset)){string rel=oldPreset.Replace('/','\\');while(rel.StartsWith(".\\"))rel=rel.Substring(2);if(Path.IsPathRooted(rel))throw new Exception("Preset esterno: copialo nella cartella del gioco prima di installare Feeder.");string old=DlssCore.Safe(root,rel);if(File.Exists(old))preset=File.ReadAllText(old);}
  if(Installed(g)&&File.Exists(DlssCore.Safe(root,Preset)))preset=File.ReadAllText(DlssCore.Safe(root,Preset));
  foreach(string key in new[]{"Techniques","TechniqueSorting"})preset=SetGlobal(preset,key,Join(GetGlobal(preset,key),"Lumenite_Kernel@lumenite_Kernel.fx","DLSS5_Feed@DLSS5_Feed.fx"));
  preset=Core.Set(preset,"DLSS5_Feed.fx","PreprocessorDefinitions",String.Join(",",(Core.Get(preset,"DLSS5_Feed.fx","PreprocessorDefinitions")??"").Split(',').Where(x=>x!=""&&!x.StartsWith("DLSS5_MV_PROVIDER=")).Concat(new[]{"DLSS5_MV_PROVIDER=3"})));
  Add(p,Preset,null,Core.EncodeLike(DlssCore.Safe(root,Preset),preset));
  text=Core.Set(text,"GENERAL","EffectSearchPaths",Join(Core.Get(text,"GENERAL","EffectSearchPaths"),".\\"+Prefix.Replace('/','\\')+"\\Shaders\\**"));
  text=Core.Set(text,"GENERAL","TextureSearchPaths",Join(Core.Get(text,"GENERAL","TextureSearchPaths"),".\\"+Prefix.Replace('/','\\')+"\\Textures\\**"));
  text=Core.Set(text,"GENERAL","PresetPath",".\\"+Preset);text=RenoNr.Early(text,true);text=RenoNr.Early(text,true,"dlss5-feed.addon64");
  Add(p,config,null,Core.EncodeLike(path,text));return p;
 }
 static void Add(DlssPlan p,string name,string source,byte[] bytes=null){
  string path=DlssCore.Safe(p.Folder,name);var prior=p.Manifest.Files.FirstOrDefault(f=>f.Name.Equals(name,StringComparison.OrdinalIgnoreCase));string current=File.Exists(path)?DlssCore.Hash(path):null;
  if(prior!=null&&!name.EndsWith(".ini",StringComparison.OrdinalIgnoreCase)&&current!=prior.After)throw new Exception("File modificato dopo l’installazione: "+name);
  if(prior!=null&&source!=null&&current==prior.After&&DlssCore.Hash(source)==current)return;
  if(prior==null&&current!=null&&(name==RenoNr.Name||name=="dlss5-feed.addon64"||name.StartsWith(Prefix)||name.StartsWith(VulkanSupport.DirectoryName+"/")))throw new Exception("File esterno già presente: "+name);
  p.Changes.Add(new DlssChange{Name=name,Source=source,SourceHash=source==null?null:DlssCore.Hash(source),Bytes=bytes,Expected=current});
 }
 static string Join(string value,params string[] tail){return String.Join(",",(value??"").Split(',').Where(s=>s!=""&&!tail.Contains(s)).Concat(tail));}
 static string GetGlobal(string text,string key){var head=Regex.Split(text,@"(?m)^\s*\[")[0];var m=Regex.Match(head,@"(?im)^\s*"+Regex.Escape(key)+@"=(.*)$");return m.Success?m.Groups[1].Value.Trim():"";}
 static string SetGlobal(string text,string key,string value){int start=Regex.Match(text,@"(?m)^\s*\[").Success?Regex.Match(text,@"(?m)^\s*\[").Index:text.Length;string head=text.Substring(0,start);head=Regex.Replace(head,@"(?im)^\s*"+Regex.Escape(key)+@"=.*(?:\r?\n|$)","");return key+"="+value+"\r\n"+head+text.Substring(start);}
 public static DlssPlan Restore(Game g){
  Core.Closed(g);string root=Path.GetDirectoryName(g.Exe);var m=DlssCore.ReadManifest(root,Marker);
  var p=new DlssPlan{Folder=root,MarkerName=Marker,Uninstall=true,Summary="Ripristina configurazione precedente a NR Feeder"};
  foreach(var f in m.Files){
   string dst=DlssCore.Safe(root,f.Name),backup=f.Before==null?null:DlssCore.Safe(Core.Data,f.Backup),hash=File.Exists(dst)?DlssCore.Hash(dst):null;
   if(backup!=null&&(!File.Exists(backup)||DlssCore.Hash(backup)!=f.Before))throw new IOException("Backup non valido: "+f.Name);
   if(f.Name.EndsWith(".ini",StringComparison.OrdinalIgnoreCase)&&f.Name!=Preset){
    if(hash==null)continue;
    string current=File.ReadAllText(dst),prior=backup==null?"":File.ReadAllText(backup),updated=current;
    foreach(string key in new[]{"EffectSearchPaths","TextureSearchPaths"}){
     string own=".\\"+Prefix.Replace('/','\\')+(key=="EffectSearchPaths"?"\\Shaders\\**":"\\Textures\\**");
     if(!MfgAddon.Entries(Core.Get(prior,"GENERAL",key)).Contains(own))
      updated=Core.Set(updated,"GENERAL",key,String.Join(",",MfgAddon.Entries(Core.Get(updated,"GENERAL",key)).Where(x=>x!=own).Select(x=>x.Replace(",",",,"))));
    }
    if(Core.Get(updated,"GENERAL","PresetPath")==".\\"+Preset)updated=Core.Set(updated,"GENERAL","PresetPath",Core.Get(prior,"GENERAL","PresetPath")??"");
    foreach(string addon in new[]{RenoNr.Name,"dlss5-feed.addon64"})if(!MfgAddon.Entries(Core.Get(prior,"ADDON","LoadFromDllMain")).Contains(addon))updated=RenoNr.Early(updated,false,addon);
    if(updated!=current)p.Changes.Add(new DlssChange{Name=f.Name,Expected=hash,Bytes=Core.EncodeLike(dst,updated)});
   }else{
    // A preset edited by ReShade belongs to the user now; keep it as an inactive preset.
    if(f.Name==Preset&&hash!=f.After)continue;
    if(hash!=f.After)throw new IOException("File modificato dopo l’installazione: "+f.Name);
    p.Changes.Add(new DlssChange{Name=f.Name,Expected=hash,Source=backup,SourceHash=f.Before,Delete=backup==null,RestoreAttributes=f.Attributes});
   }
  }
  p.Changes.Add(new DlssChange{Name=Marker,Expected=DlssCore.Hash(DlssCore.Safe(root,Marker)),Delete=true});return p;
 }
}
}
