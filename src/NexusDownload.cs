using System;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using System.Diagnostics;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Threading;
using Microsoft.Win32;
namespace HdrPilot {
public static class NexusDownload {
 public static string Downloads(){if(!String.IsNullOrEmpty(Core.Pref.NexusDownloads)&&Directory.Exists(Core.Pref.NexusDownloads))return Core.Pref.NexusDownloads;try{using(var k=Registry.CurrentUser.OpenSubKey(@"Software\Microsoft\Windows\CurrentVersion\Explorer\User Shell Folders")){var p=k.GetValue("{374DE290-123F-4565-9164-39C4925E467B}") as string;if(!String.IsNullOrEmpty(p))return Environment.ExpandEnvironmentVariables(p);}}catch{}return Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.UserProfile),"Downloads");}
 public static bool Matches(string file,string modId){string name=Path.GetFileName(file);return Regex.IsMatch(modId??"",@"^\d+$")&&name.EndsWith(".zip",StringComparison.OrdinalIgnoreCase)&&name.IndexOf("renodx",StringComparison.OrdinalIgnoreCase)>=0&&Regex.IsMatch(name,@"(?:^|[ _-])"+Regex.Escape(modId)+@"(?=[ _-])");}
 public static List<string> Candidates(string folder,string id,string title){string key=Regex.Replace(title.ToLowerInvariant(),@"[^\p{L}\p{N}]","");return Core.Files(folder,"*.zip").Where(f=>Matches(f,id)&&key.Length>0&&Regex.Replace(Path.GetFileNameWithoutExtension(f).ToLowerInvariant(),@"[^\p{L}\p{N}]","").Contains(key)).OrderByDescending(File.GetLastWriteTimeUtc).ToList();}
 public static string FindDownloadFolder(string id,string title){
  var folders=new HashSet<string>(StringComparer.OrdinalIgnoreCase){Downloads()};
  foreach(var drive in DriveInfo.GetDrives())try{if(drive.IsReady&&drive.DriveType==DriveType.Fixed){folders.Add(Path.Combine(drive.RootDirectory.FullName,"Downloads"));folders.Add(Path.Combine(drive.RootDirectory.FullName,"Download"));}}catch{}
  var found=folders.Where(f=>Directory.Exists(f)&&Candidates(f,id,title).Count>0).ToList();
  return found.Count==1?found[0]:Downloads();
 }
 public static string Extract(string archive,string destination,string expected=null){
  using(var input=new FileStream(archive,FileMode.Open,FileAccess.Read,FileShare.None))using(var zip=new ZipArchive(input,ZipArchiveMode.Read)){
   if(input.Length>200000000||zip.Entries.Count>100)throw new Exception("Pacchetto troppo grande o complesso.");
   var addons=new List<ZipArchiveEntry>();
   foreach(var e in zip.Entries){string n=e.FullName.Replace('\\','/');if(n.StartsWith("/")||n.Contains(":")||n.Split('/').Any(x=>x=="..")||((e.ExternalAttributes>>16)&0xF000)==0xA000)throw new Exception("Percorso non sicuro nel pacchetto.");if(n.EndsWith("/"))continue;
    if(Regex.IsMatch(Path.GetFileName(n),@"^renodx-[a-zA-Z0-9_-]+\.addon64$",RegexOptions.IgnoreCase))addons.Add(e);
    else if(!new[]{".txt",".md",".png",".jpg",".pdf"}.Contains(Path.GetExtension(n).ToLowerInvariant()))throw new Exception("Il pacchetto contiene altri componenti da verificare: "+n+". Nessun file installato.");
   }
   if(addons.Count!=1||addons[0].Length>150000000)throw new Exception("Serve un solo add-on RenoDX x64 nel pacchetto.");
   string name=Path.GetFileName(addons[0].FullName.Replace('\\','/'));if(expected!=null&&!name.Equals(expected,StringComparison.OrdinalIgnoreCase))throw new Exception("Wrong add-on in archive: "+name);if(expected==null&&Regex.IsMatch(name,"dlss|neural",RegexOptions.IgnoreCase))throw new Exception("Il pacchetto contiene Neural Rendering, non la mod HDR.");
   Directory.CreateDirectory(destination);string output=DlssCore.Safe(destination,name);using(var src=addons[0].Open())using(var dst=File.Create(output))src.CopyTo(dst);if(!DlssCore.Pe64(output))throw new Exception("Add-on non x64 o non valido.");return output;
  }
 }
 public static void Show(Window owner,Game game,string page,Action<DlssPlan> review,string target=null,string sourceTitle=null,StackPanel inlineHost=null){
  var match=Regex.Match(page??"",@"^https://(?:www\.)?nexusmods\.com/[a-zA-Z0-9_-]+/mods/(\d+)/?$");if(!match.Success)throw new Exception("Pagina Nexus non valida.");
  string title=sourceTitle??game.Name;string id=match.Groups[1].Value,folder=FindDownloadFolder(id,title);var baseline=new Dictionary<string,string>(StringComparer.OrdinalIgnoreCase);Func<string,string> stamp=f=>{var i=new FileInfo(f);return i.Length+":"+i.LastWriteTimeUtc.Ticks;};Action baselineReset=()=>baseline.Clear();
  StackPanel body;var w=Dialogs.Create(owner,"RenoDX · download da Nexus Mods",out body);w.Width=780;
  body.Children.Add(Dialogs.Text(game.Name+"\nConferma il download manuale gratuito sul sito. Resta in questa sezione: al termine rileviamo lo ZIP RenoDX, anche se già scaricato, e prepariamo il backup. Nessuna password richiesta dall’app.",14));
  var status=Dialogs.Text("In attesa del download in: "+folder,14);body.Children.Add(status);var row=new WrapPanel();body.Children.Add(row);var open=new Button{Content="Apri download Nexus",Margin=new Thickness(0,8,8,8)};row.Children.Add(open);open.Click+=(s,e)=>Process.Start(new ProcessStartInfo(page+"?tab=files"){UseShellExecute=true});
  var choose=new Button{Content="Cartella download…",Margin=new Thickness(0,8,8,8)};row.Children.Add(choose);choose.Click+=(s,e)=>{using(var picker=new System.Windows.Forms.FolderBrowserDialog()){picker.SelectedPath=folder;if(picker.ShowDialog()==System.Windows.Forms.DialogResult.OK){folder=picker.SelectedPath;Core.Pref.NexusDownloads=folder;Core.SavePrefs();baselineReset();status.Text="Cerco il pacchetto in: "+folder;}}};
  var cancel=new Button{Content="Annulla",Margin=new Thickness(0,8,0,8)};row.Children.Add(cancel);cancel.Click+=(s,e)=>{if(inlineHost==null)w.Close();else inlineHost.Children.Remove(body);};
  body.Children.Add(Dialogs.Text("Rileva ZIP con RenoDX, il titolo del gioco e l’ID della mod nel nome, anche già presenti. Accetta nomi con spazi o trattini. Scarica nella cartella indicata mantenendo il nome originale. Archivi con componenti aggiuntivi o più add-on richiedono verifica. Il nome del file non certifica l’origine: nell’anteprima controlla il gioco e la mod.",12));
  var stable=new Dictionary<string,string>();var timer=new DispatcherTimer{Interval=TimeSpan.FromSeconds(2)};bool reading=false;DlssPlan pending=null;
  timer.Tick+=async(s,e)=>{if(reading||ModCenter.Busy||RenoDxUi.IsBusy)return;var files=Candidates(folder,id,title).Where(f=>{try{return !baseline.ContainsKey(f)||baseline[f]!=stamp(f);}catch{return false;}}).ToList();if(files.Count>1){status.Text="Più pacchetti per questa mod: lascia un solo ZIP nella cartella per evitare un abbinamento errato.";return;}if(files.Count==0){if(baseline.Count==0)status.Text="In attesa del pacchetto di "+game.Name+" in: "+folder;return;}string file=files[0];string now;try{now=stamp(file);}catch{return;}string old;if(!stable.TryGetValue(file,out old)||old!=now){stable[file]=now;return;}
   reading=true;row.IsEnabled=false;status.Text="Download rilevato. Verifica del pacchetto…";try{string addon=await System.Threading.Tasks.Task.Run(()=>Extract(file,Path.Combine(Core.Data,"hdr-packages",Guid.NewGuid().ToString("N")),target==null?null:Path.GetFileName(target)));Core.Closed(game);pending=target==null?RenoDx.InstallAddon(game,addon):RenoDx.ReplaceManualAddon(game,target,addon);pending.Summary+="\n\nDownload assistito Nexus: "+page+"\nArchivio locale: "+Path.GetFileName(file)+"\nAbbinamento dal nome e ID mod; origine non certificata. Segui le impostazioni HDR indicate dall’autore.";ActivityLog.Write("RenoDX · pacchetto Nexus rilevato e verificato: "+Path.GetFileName(file));timer.Stop();if(inlineHost==null)w.Close();else{inlineHost.Children.Remove(body);review(pending);}}catch(InvalidDataException ex){baseline[file]=now;status.Text="ZIP non valido: "+ex.Message;}catch(IOException){status.Text="Download ancora occupato: attendo il completamento…";}catch(Exception ex){baseline[file]=now;status.Text=ex.Message;ActivityLog.Write("RenoDX · download assistito: "+ex.Message);}finally{reading=false;row.IsEnabled=true;}};
  w.Closing+=(s,e)=>{if(reading&&pending==null)e.Cancel=true;};w.Closed+=(s,e)=>timer.Stop();Action start=()=>{timer.Start();if(Candidates(folder,id,title).Count>0){status.Text="Pacchetto già scaricato: verifica in corso…";return;}try{Process.Start(new ProcessStartInfo(page+"?tab=files"){UseShellExecute=true});}catch(Exception ex){status.Text=ex.Message;}};if(inlineHost==null){w.ContentRendered+=(s,e)=>start();w.ShowDialog();if(pending!=null)review(pending);}else{var parent=body.Parent as Panel;if(parent!=null)parent.Children.Remove(body);else{var border=body.Parent as Border;if(border!=null)border.Child=null;}body.Unloaded+=(s,e)=>timer.Stop();body.Children.RemoveAt(0);w.Close();inlineHost.Children.Add(body);body.BringIntoView();start();}
 }
}
}
