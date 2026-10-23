using System;using System.IO;using System.Linq;using System.Text;using System.Collections.Generic;using System.Diagnostics;
namespace HdrPilot {
public static class VulkanSupport {
 public const string DirectoryName="hdlss-vulkan",Dll="hdlss-vulkan/ReShade64.dll",Manifest="hdlss-vulkan/ReShade64.json",FeedDll="hdlss-vulkan/VkLayer_feed_vk.dll",FeedManifest="hdlss-vulkan/VkLayer_feed_vk.json";
 public static byte[] Layer(string name,string library,bool reshade){var layer=new Dictionary<string,object>{{"name",name},{"type","GLOBAL"},{"library_path",".\\"+library},{"api_version","1.3.268"},{"implementation_version","1"},{"description",reshade?"ReShade - HDLSS per-game Vulkan":"DLSS5 Feeder - HDLSS per-game Vulkan"}};if(reshade)layer["device_extensions"]=new[]{new Dictionary<string,object>{{"name","VK_EXT_tooling_info"},{"spec_version","1"},{"entrypoints",new[]{"vkGetPhysicalDeviceToolPropertiesEXT"}}}};else layer["functions"]=new Dictionary<string,string>{{"vkNegotiateLoaderLayerInterfaceVersion","vkNegotiateLoaderLayerInterfaceVersion"}};return Encoding.UTF8.GetBytes(Core.Json.Serialize(new{file_format_version="1.2.0",layer=layer}));}
 public static bool NativeDlss(Game g){return InjectionSetup.PackagedRenderer(g)!=null&&File.Exists(Path.Combine(Path.GetDirectoryName(g.Exe),"bbport_dlss.dll"))&&File.Exists(Path.Combine(Path.GetDirectoryName(g.Exe),"nvngx_dlss.dll"));}
 public static bool Installed(Game g){return g!=null&&!String.IsNullOrEmpty(g.Exe)&&File.Exists(DlssCore.Safe(Path.GetDirectoryName(g.Exe),Manifest));}
 public static DlssPlan Prepare(Game g,string source,string origin){
  Core.Closed(g);var scan=DlssCore.Inspect(g,true,false);if(g.ProtectedInstall||scan.Blockers.Count>0)throw new Exception(String.Join("\n",scan.Blockers));if(scan.Proxy!="")throw new Exception("Rimuovi OptiScaler prima di configurare questo percorso Vulkan.");
  if(!RenoDx.IsReShade(source)||!DlssCore.ContainsAscii(source,"ReShadeRegisterAddon"))throw new Exception("Serve ReShade x64 con supporto completo add-on.");
  if(RenoDx.Loaders(scan.Folder).Any(n=>n!=Dll))throw new Exception("ReShade diretto gia presente: rimuovilo prima di installare il layer Vulkan.");
  string marker=DlssCore.Safe(scan.Folder,RenoDx.Marker);var m=File.Exists(marker)?DlssCore.ReadManifest(scan.Folder,RenoDx.Marker):new DlssManifest{Game=g.Id,Folder=scan.Folder,BackupRoot="hdr-components/"+Guid.NewGuid().ToString("N"),PreviousOwner="Before Vulkan ReShade"};
  if(m.Files.Count>0&&!m.Build.StartsWith("vulkan-"))throw new Exception("Rimuovi la precedente installazione ReShade prima di cambiare API.");m.Build="vulkan-stable";m.Release=FileVersionInfo.GetVersionInfo(source).FileVersion;
  var p=new DlssPlan{Folder=scan.Folder,Manifest=m,MarkerName=RenoDx.Marker,MarkerExpected=File.Exists(marker)?DlssCore.Hash(marker):null,Summary=g.Name+"\nReShade Vulkan x64. Avvia il gioco dal pulsante Avvia con Vulkan di HDLSS5.\n"+origin};
  Add(p,Dll,source,null);Add(p,Manifest,null,Layer("VK_LAYER_reshade","ReShade64.dll",true));
  string config=DlssCore.Safe(scan.Folder,"ReShade.ini");if(!File.Exists(config))Add(p,"ReShade.ini",null,Encoding.UTF8.GetBytes("[ADDON]\r\nAddonPath=.\\\r\n"));return p;
 }
 static void Add(DlssPlan p,string n,string source,byte[] bytes){string dst=DlssCore.Safe(p.Folder,n),hash=File.Exists(dst)?DlssCore.Hash(dst):null;var f=p.Manifest.Files.FirstOrDefault(x=>x.Name==n);if(hash!=null&&(f==null||hash!=f.After))throw new IOException("File esterno o modificato: "+n);p.Changes.Add(new DlssChange{Name=n,Source=source,SourceHash=source==null?null:DlssCore.Hash(source),Bytes=bytes,Expected=hash});}
 public static DlssPlan Restore(Game g){Core.Closed(g);string root=Path.GetDirectoryName(g.Exe);var m=DlssCore.ReadManifest(root,RenoDx.Marker);var p=new DlssPlan{Folder=root,MarkerName=RenoDx.Marker,Uninstall=true,Summary="Rimuovi ReShade Vulkan; conserva preset e configurazione"};foreach(var f in m.Files.Where(f=>f.Name!="ReShade.ini")){string dst=DlssCore.Safe(root,f.Name),backup=f.Before==null?null:DlssCore.Safe(Core.Data,f.Backup);if(!File.Exists(dst)||DlssCore.Hash(dst)!=f.After||backup!=null&&(!File.Exists(backup)||DlssCore.Hash(backup)!=f.Before))throw new IOException("File o backup modificato: "+f.Name);p.Changes.Add(new DlssChange{Name=f.Name,Expected=f.After,Source=backup,SourceHash=f.Before,Delete=backup==null,RestoreAttributes=f.Attributes});}p.Changes.Add(new DlssChange{Name=RenoDx.Marker,Expected=DlssCore.Hash(DlssCore.Safe(root,RenoDx.Marker)),Delete=true});return p;}
 public static ProcessStartInfo StartInfo(Game g){
  InjectionSetup.AutoSelect(g);Core.Closed(g);if(InjectionSetup.EffectiveApi(g)!="vulkan"||!Installed(g))throw new Exception("Installa prima ReShade Vulkan per questo gioco.");string root=Path.GetDirectoryName(g.Exe);
  foreach(string marker in new[]{RenoDx.Marker,NrFeeder.Marker})if(File.Exists(DlssCore.Safe(root,marker))){var m=DlssCore.ReadManifest(root,marker);foreach(var f in m.Files.Where(f=>f.Name.StartsWith(DirectoryName+"/",StringComparison.OrdinalIgnoreCase))){string path=DlssCore.Safe(root,f.Name);if(!File.Exists(path)||DlssCore.Hash(path)!=f.After)throw new IOException("Layer modificato: "+f.Name);}}
  string exe=g.Exe,working=root;if(InjectionSetup.PackagedRenderer(g)!=null){exe=DlssCore.Safe(g.Root,"Bloodborne.exe");working=g.Root;if(!DlssCore.Pe64(exe))throw new IOException("Avviatore del port non trovato.");}
  var psi=new ProcessStartInfo(exe){WorkingDirectory=working,UseShellExecute=false};string layers="VK_LAYER_reshade";if(NrFeeder.Installed(g)){if(!File.Exists(DlssCore.Safe(root,FeedManifest)))throw new IOException("Reinstalla NR Feeder per completare il layer Vulkan.");layers+=";VK_LAYER_feed_vk";}
  // A child-process environment avoids machine-wide layer registration and DLL replacement.
  string prior=psi.EnvironmentVariables["VK_LAYER_PATH"];psi.EnvironmentVariables["VK_LAYER_PATH"]=Path.Combine(root,DirectoryName)+(String.IsNullOrEmpty(prior)?"":";"+prior);
  prior=psi.EnvironmentVariables["VK_INSTANCE_LAYERS"];psi.EnvironmentVariables["VK_INSTANCE_LAYERS"]=layers+(String.IsNullOrEmpty(prior)?"":";"+prior);
  return psi;
 }
 public static void Launch(Game g){Process.Start(StartInfo(g));}
}
}
