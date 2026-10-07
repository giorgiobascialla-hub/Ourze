using System;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using System.Windows;
using System.Windows.Controls;
namespace HdrPilot {
public static class ModRemoval {
 public const string Help="Remove mods opens one panel for ReShade, RenoDX HDR and DLSS 5 / OptiScaler. Select components, review the detected files and apply once with a recovery copy. Removing ReShade also removes detected HDR add-ons and MFG Unlock. When ReShade is loaded through OptiScaler, removing OptiScaler also requires selecting ReShade. Native DLSS, Streamline, ASI mods and UE4SS are preserved. Unknown files are not deleted.";
 static string L(string s){return Appearance.Localize(s);}
 public static bool Linked(Game g){string root=Path.GetDirectoryName(g.Exe);return RenoDx.Loaders(root).Contains("ReShade64.dll")&&String.Equals(Core.Get(DlssCore.ReadIni(root),"Plugins","LoadReshade"),"true",StringComparison.OrdinalIgnoreCase);}
 public static DlssPlan Prepare(Game g,bool reshade,bool renodx,bool dlss){Core.Closed(g);if(g.ProtectedInstall)throw new Exception(g.Evidence);if(!reshade&&!renodx&&!dlss)throw new Exception(L("Select at least one component."));if(dlss&&!reshade&&Linked(g))throw new Exception(L("ReShade depends on OptiScaler. Select ReShade too to remove this chain in one operation."));string root=Path.GetDirectoryName(Path.GetFullPath(g.Exe));var p=new DlssPlan{Folder=root,Uninstall=true,Summary=g.Name+"\n"+L(Help)};
  Action<DlssChange> merge=c=>{var old=p.Changes.FirstOrDefault(x=>x.Name.Equals(c.Name,StringComparison.OrdinalIgnoreCase));if(old!=null)p.Changes.Remove(old);p.Changes.Add(c);};
  Action<string> remove=n=>{string path=DlssCore.Safe(root,n);if(File.Exists(path))merge(new DlssChange{Name=n,Delete=true,Expected=DlssCore.Hash(path)});};
  if(reshade||renodx){string config=MfgAddon.ConfigName(root),configPath=DlssCore.Safe(root,config),ini=File.Exists(configPath)?File.ReadAllText(configPath):"";string addonFolder=Path.GetDirectoryName(DlssCore.Safe(root,MfgAddon.AddonName(root,ini)));if(Directory.Exists(addonFolder))foreach(string path in Directory.GetFiles(addonFolder,"renodx-*.addon64"))if(!Regex.IsMatch(Path.GetFileName(path),"dlss|neural|mfg|framegen",RegexOptions.IgnoreCase)&&DlssCore.Pe64(path))remove(path.Substring(root.TrimEnd('\\').Length+1));}
  if(reshade){var cleanup=ComponentCleanup.Prepare(g,true,true);foreach(var c in cleanup.Changes)merge(c);string config=MfgAddon.ConfigName(root),path=DlssCore.Safe(root,config),ini=File.Exists(path)?File.ReadAllText(path):"";remove(MfgAddon.AddonName(root,ini));remove(MfgAddon.Name);remove(MfgAddon.Marker);remove(config);}
  else if(renodx){foreach(string path in Directory.GetFiles(root,"renodx-*.addon64"))if(!Regex.IsMatch(Path.GetFileName(path),"dlss|neural|mfg|framegen",RegexOptions.IgnoreCase)&&DlssCore.Pe64(path))remove(Path.GetFileName(path));remove(RenoDx.AddonMarker);}
  if(dlss){foreach(var c in ComponentCleanup.Prepare(g,false,reshade).Changes)merge(c);string proxy=DlssCore.Safe(root,"nvngx.dll_dlssnr.dll");if(DlssCore.Pe64(proxy))remove("nvngx.dll_dlssnr.dll");}
  if(p.Changes.Count==0)throw new Exception(L("No recognized files for the selected components."));return p;
 }
 public static void Show(Window owner,Game game,Action<DlssPlan> review){StackPanel b;var d=Dialogs.Create(owner,L("Remove mods"),out b);d.Width=740; b.Children.Add(Dialogs.Text(L(Help),13));var reshade=new CheckBox{Content="ReShade + RenoDX HDR + MFG Unlock",Margin=new Thickness(0,12,0,8)};var renodx=new CheckBox{Content="RenoDX HDR",Margin=new Thickness(0,0,0,8)};var dlss=new CheckBox{Content="DLSS 5 / OptiScaler",Margin=new Thickness(0,0,0,12)};b.Children.Add(reshade);b.Children.Add(renodx);b.Children.Add(dlss);var info=Dialogs.Text("",13);b.Children.Add(info);bool linked=Linked(game);Action sync=()=>{if(dlss.IsChecked==true&&linked){reshade.IsChecked=true;info.Text=L("ReShade depends on OptiScaler and is included in this removal.");}else info.Text="";reshade.IsEnabled=!(dlss.IsChecked==true&&linked);renodx.IsEnabled=reshade.IsChecked!=true;};dlss.Checked+=(s,e)=>sync();dlss.Unchecked+=(s,e)=>sync();reshade.Checked+=(s,e)=>sync();reshade.Unchecked+=(s,e)=>sync();var next=new Button{Content=L("Review removal"),Margin=new Thickness(0,12,0,0)};b.Children.Add(next);next.Click+=(s,e)=>{try{var p=Prepare(game,reshade.IsChecked==true,renodx.IsChecked==true,dlss.IsChecked==true);d.Close();review(p);}catch(Exception ex){info.Text=ex.Message;}};d.ShowDialog();}
}
}
