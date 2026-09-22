using System;
using System.IO;
using System.Text;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;
namespace HdrPilot {
public static class ActivityLog {
 static readonly object gate=new object();static readonly Queue<string> lines=new Queue<string>();static string file;public static event Action Changed;
 public static string FilePath {get{lock(gate){return file;}}}
 public static string Snapshot(){lock(gate){return String.Join(Environment.NewLine,lines);}}
 public static void Write(string message){Action update;lock(gate){string friendly=Readable(message);if(friendly!=null&&friendly!=lastFriendly){readable.Enqueue(DateTime.Now.ToString("HH:mm")+"   "+friendly);lastFriendly=friendly;while(readable.Count>40)readable.Dequeue();}string line=DateTime.Now.ToString("HH:mm:ss")+"  "+message;lines.Enqueue(line);while(lines.Count>300)lines.Dequeue();try{string dir=Path.Combine(Core.Data,"logs");Directory.CreateDirectory(dir);string next=Path.Combine(dir,"operazioni-"+DateTime.Now.ToString("yyyyMMdd")+".txt");File.AppendAllText(next,line+Environment.NewLine,new UTF8Encoding(false));file=next;}catch(Exception ex){lines.Enqueue("Registro su disco non disponibile: "+ex.Message);}update=Changed;}if(update!=null)update();}
 static string phase="Pronto",lastFriendly="";static bool active;static double progress;
 public static void Progress(string label,double value=-1){Action changed;lock(gate){phase=label;active=value<100;progress=value;changed=Changed;}if(changed!=null)changed();}
 public static string Readable(string message){
  if(message.StartsWith("Preferenze")||message.StartsWith("Connessione riuscita:")||message.StartsWith("Scrittura:")||message.StartsWith("Rimozione:")||message.StartsWith("Download / verifica:"))return null;
  if(message.StartsWith("INSTALLAZIONE"))return "Installazione avviata";
  if(message.StartsWith("RIPRISTINO"))return "Ripristino avviato";
  if(message.StartsWith("Verifica percorsi"))return "Controllo dei file e dei componenti";
  if(message.StartsWith("Backup e copia"))return "Backup creato: puoi recuperare i file precedenti";
  if(message.StartsWith("Hash finali"))return "File installati e verificati";
  if(message.StartsWith("ERRORE",StringComparison.OrdinalIgnoreCase)||message.StartsWith("Errore",StringComparison.OrdinalIgnoreCase))return "Operazione interrotta: "+message.Substring(message.IndexOf(' ')+1).Split('\n')[0];
  if(message.Length>160||message.Contains("https://")||message.Contains(@":\"))return null;
  return message;
 }
 static readonly Queue<string> readable=new Queue<string>();
 public static FrameworkElement Panel(){
  var root=new StackPanel();var heading=new DockPanel();var open=new Button{Content="Dettagli tecnici",Margin=new Thickness(8,0,0,0)};DockPanel.SetDock(open,Dock.Right);heading.Children.Add(open);var state=new TextBlock{FontSize=14,VerticalAlignment=VerticalAlignment.Center,Foreground=new System.Windows.Media.SolidColorBrush(System.Windows.Media.Color.FromRgb(230,172,80)),Text="Pronto"};heading.Children.Add(state);root.Children.Add(heading);
  var bar=new ProgressBar{Background=new System.Windows.Media.SolidColorBrush(System.Windows.Media.Color.FromRgb(37,46,56)),BorderThickness=new Thickness(0),Height=10,Minimum=0,Maximum=100,Margin=new Thickness(0,6,0,8),Foreground=new System.Windows.Media.SolidColorBrush(System.Windows.Media.Color.FromRgb(230,172,80))};root.Children.Add(bar);
  var expander=new Expander{Header="Attività recenti",IsExpanded=false};root.Children.Add(expander);var log=new DockPanel();expander.Content=log;var box=new TextBox{Height=80,IsReadOnly=true,TextWrapping=TextWrapping.Wrap,VerticalScrollBarVisibility=ScrollBarVisibility.Auto,FontSize=14,Padding=new Thickness(10),BorderThickness=new Thickness(0)};
  var grip=new System.Windows.Controls.Primitives.Thumb{Height=6,Cursor=System.Windows.Input.Cursors.SizeNS,Background=new System.Windows.Media.SolidColorBrush(System.Windows.Media.Color.FromRgb(230,172,80)),ToolTip="Trascina per ridimensionare il registro",Margin=new Thickness(0,0,0,5)};var gripVisual=new FrameworkElementFactory(typeof(Border));gripVisual.SetValue(Border.BackgroundProperty,new System.Windows.Media.SolidColorBrush(System.Windows.Media.Color.FromRgb(230,172,80)));grip.Template=new ControlTemplate(typeof(System.Windows.Controls.Primitives.Thumb)){VisualTree=gripVisual};DockPanel.SetDock(grip,Dock.Top);log.Children.Add(grip);log.Children.Add(box);grip.DragDelta+=(sender,e)=>{var host=Window.GetWindow(root);box.Height=Math.Max(60,Math.Min((host==null?740:host.ActualHeight)*0.45,box.Height-e.VerticalChange));};
  bool shown=false;Action refresh=()=>{if(!box.Dispatcher.HasShutdownStarted)box.Dispatcher.BeginInvoke(new Action(()=>{lock(gate){if(!shown&&readable.Count>0){expander.IsExpanded=true;shown=true;}box.Text=String.Join(Environment.NewLine+Environment.NewLine,readable);state.Text=phase;bar.IsIndeterminate=active&&progress<0;bar.Value=progress<0?0:progress;}box.ScrollToEnd();}));};root.Loaded+=(sender,e)=>{Changed+=refresh;refresh();};root.Unloaded+=(sender,e)=>Changed-=refresh;open.Click+=(sender,e)=>{try{if(FilePath!=null)System.Diagnostics.Process.Start(new System.Diagnostics.ProcessStartInfo(FilePath){UseShellExecute=true});}catch(Exception ex){state.Text=ex.Message;}};return root;
 }

}
public static class PreferenceStore {
 static bool AppFolder(string name){return name.StartsWith("HDR-Unlock",StringComparison.OrdinalIgnoreCase)||name.StartsWith("HDLSS",StringComparison.OrdinalIgnoreCase);}
 public static string Resolve(string app){string own=Path.Combine(app,"data");if(File.Exists(Path.Combine(own,"preferences.json"))||File.Exists(Path.Combine(own,"preferences.json.bak")))return own;
  var candidates=new List<string>();var dir=new DirectoryInfo(app);for(int depth=0;depth<3&&dir!=null;depth++,dir=dir.Parent){if(!AppFolder(dir.Name))break;string path=Path.Combine(dir.FullName,"data");if(File.Exists(Path.Combine(path,"preferences.json")))candidates.Add(path);foreach(var child in System.Linq.Enumerable.Where(dir.GetDirectories(),child=>AppFolder(child.Name))){path=Path.Combine(child.FullName,"data");if(File.Exists(Path.Combine(path,"preferences.json")))candidates.Add(path);}}
  // An explicit portable data link survives updates; only existing preference stores are used.
  string link=Path.Combine(app,"data-location.txt");if(File.Exists(link)){string target=Path.GetFullPath(Path.Combine(app,File.ReadAllText(link).Trim()));if(File.Exists(Path.Combine(target,"preferences.json")))return target;}
  // Discover adjacent portable builds, including builds already linked to an older data folder.
  var parent=Directory.GetParent(app);if(parent!=null)foreach(string sibling in Core.Dirs(parent.FullName)){
   if(!AppFolder(Path.GetFileName(sibling)))continue;
   string candidate=Path.Combine(sibling,"data");string pointer=Path.Combine(sibling,"data-location.txt");
   try{if(File.Exists(pointer)){string linked=Path.GetFullPath(Path.Combine(sibling,File.ReadAllText(pointer).Trim()));if(File.Exists(Path.Combine(linked,"preferences.json")))candidate=linked;}
    string pref=Path.Combine(candidate,"preferences.json");if(File.Exists(pref)&&Core.Json.Deserialize<Preferences>(File.ReadAllText(pref))!=null&&!candidates.Contains(candidate))candidates.Add(candidate);
   }catch{}
  }
  if(candidates.Count>0){string selected=System.Linq.Enumerable.First(System.Linq.Enumerable.OrderByDescending(candidates,x=>File.GetLastWriteTimeUtc(Path.Combine(x,"preferences.json"))));File.WriteAllText(link,selected);return selected;}return own;
 }
 public static Preferences Load(string dir){Directory.CreateDirectory(dir);string path=Path.Combine(dir,"preferences.json");Exception failure=null;foreach(string candidate in new[]{path,path+".bak"}){if(!File.Exists(candidate))continue;try{var pref=Core.Json.Deserialize<Preferences>(File.ReadAllText(candidate));if(pref==null)throw new Exception("Preferenze vuote.");if(Array.IndexOf(new[]{"en","it","es","fr","de","pt"},pref.Language)<0)pref.Language="en";if(pref.DlssChoices==null)pref.DlssChoices=new Dictionary<string,DlssOptions>();if(pref.HdrMethods==null)pref.HdrMethods=new Dictionary<string,int>();if(pref.MonitorModes==null)pref.MonitorModes=new Dictionary<string,int>();if(pref.Libraries==null)pref.Libraries=new List<string>();if(pref.Configs==null)pref.Configs=new Dictionary<string,string>();if(pref.Covers==null)pref.Covers=new Dictionary<string,string>();if(pref.Sections==null)pref.Sections=new Dictionary<string,string>();if(candidate!=path){if(File.Exists(path))File.Copy(path,path+".corrupt-"+DateTime.Now.ToString("yyyyMMddHHmmss"),true);File.Copy(candidate,path,true);ActivityLog.Write("Preferenze recuperate dalla copia di sicurezza.");}return pref;}catch(Exception ex){failure=ex;}}
  if(failure!=null)throw new Exception("Preferenze non leggibili: i file sono stati conservati, senza sostituirli con i valori iniziali. "+path,failure);return new Preferences();
 }
 public static void Save(){Directory.CreateDirectory(Core.Data);string p=Path.Combine(Core.Data,"preferences.json"),temp=p+".tmp";string json=Core.Json.Serialize(Core.Pref);File.WriteAllText(temp,json,new UTF8Encoding(false));if(File.Exists(p))File.Replace(temp,p,p+".bak");else{File.Move(temp,p);File.Copy(p,p+".bak",true);}if(File.ReadAllText(p)!=json)throw new IOException("Verifica preferenze fallita.");ActivityLog.Write("Preferenze salvate · overlay "+Shortcut.Label(Core.Pref.HotkeyMods,Core.Pref.HotkeyKey));}
}
}
