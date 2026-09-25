using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Markup;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Threading;
using Forms=System.Windows.Forms;

namespace HdrPilot {
public class Pilot {
 DlssUi dlssPanel; bool dlssMode;bool homeMode=true; Window window; List<Game> games=new List<Game>(); Game current; bool busy,loading; string preview;
 T C<T>(string name) where T:class{return window.FindName(name) as T;}
 void SetStatus(string text){C<TextBlock>("Status").Text=Appearance.Localize(text);}
 void Fail(Exception e){ActivityLog.Write("ERRORE: "+e.Message);SetStatus(e.Message.Split('\n')[0]);Dialogs.Notice(window,"Operazione non completata",e.Message,null).ShowDialog();}
 void Act(Action a){try{a();}catch(Exception e){Fail(e);}}
 [STAThread] public static int Main(string[] args){
  AppContext.SetSwitch("Switch.System.IO.UseLegacyPathHandling",false);
  AppContext.SetSwitch("Switch.System.IO.BlockLongPaths",false);
  try{
   if(args.Length>2&&args[0]=="--prefs-smoke"){PreferencesTests.Run(args[1],args[2]);return 0;}
   if(args.Length>3&&args[0]=="--model-test"){ComponentTests.ModelTest(args[1],args[2],args[3]);return 0;}
   if(args.Length>3&&args[0]=="--catalog-test"){ComponentTests.Catalog(args[1],args[2],args[3]);return 0;}
   if(args.Length>3&&args[0]=="--individual-test"){ComponentTests.Individual(args[1],args[2],args[3]);return 0;}
   if(args.Length>3&&args[0]=="--component-test"){ComponentTests.Run(args[1],args[2],args[3]);return 0;}
   if(args.Length>2&&args[0]=="--native-test"){DlssTests.NativeTest(args[1],args[2]);return 0;}
   if(args.Length>2&&args[0]=="--native-inspect"){DlssTests.NativeInspect(args[1],args[2]);return 0;}
   if(args.Length>2&&args[0]=="--package-test"){DlssTests.Package(args[1],args[2]);return 0;}
   if(args.Length>1&&args[0]=="--dlss-test"){DlssTests.Run(args[1],args.Length>2?args[2]:null);return 0;}
   if(args.Length>2&&args[0]=="--dlss-preview"){DlssTests.Preview(args[1],args[2]);return 0;}
   if(args.Length>2&&args[0]=="--dlss-scan"){DlssTests.Scan(args[1],args[2]);return 0;}
   if(args.Length>1&&args[0]=="--instance-test"){SingleInstance.Test(args[1]);return 0;}
   if(args.Length>1&&args[0]=="--displays"){File.WriteAllText(args[1],Core.Json.Serialize(Displays.Scan()));return 0;}
   if(args.Length>0&&args[0]=="--self-test"){Test.Run(args.Length>1?args[1]:Path.Combine(AppDomain.CurrentDomain.BaseDirectory,"test-results.txt"));return 0;}
   if(args.Length==0&&!SingleInstance.Enter())return 0;
   if(args.Length==0)Core.Data=PreferenceStore.Resolve(AppDomain.CurrentDomain.BaseDirectory);Core.Init();
   if(args.Length>0&&args[0]=="--scan"){File.WriteAllText(args[1],Core.Json.Serialize(Core.Scan()));return 0;}
   var app=new Application();Appearance.Start();var p=new Pilot();if(args.Length>1&&args[0]=="--preview")p.preview=args[1];
   using(var s=Assembly.GetExecutingAssembly().GetManifestResourceStream("Main.xaml"))p.window=(Window)XamlReader.Load(s);
   using(var logo=Assembly.GetExecutingAssembly().GetManifestResourceStream("brand-logo.png")){var bitmap=new System.Windows.Media.Imaging.BitmapImage();bitmap.BeginInit();bitmap.CacheOption=System.Windows.Media.Imaging.BitmapCacheOption.OnLoad;bitmap.StreamSource=logo;bitmap.EndInit();bitmap.Freeze();p.C<Image>("BrandLogo").Source=bitmap;}
   using(var icon=Assembly.GetExecutingAssembly().GetManifestResourceStream("app.ico")){p.window.Icon=System.Windows.Media.Imaging.BitmapFrame.Create(icon,System.Windows.Media.Imaging.BitmapCreateOptions.None,System.Windows.Media.Imaging.BitmapCacheOption.OnLoad);}
   if(p.preview!=null&&args.Length>2&&args[2]=="maximized")p.window.WindowState=WindowState.Maximized;
   if(p.preview!=null&&args.Length>3&&new[]{"it","en","es","fr","de","pt"}.Contains(args[3]))Core.Pref.Language=args[3];
   p.Wire();app.DispatcherUnhandledException+=(o,e)=>{p.Fail(e.Exception);e.Handled=true;};app.Run(p.window);return 0;
  }catch(Exception ex){string dest=Path.Combine(AppDomain.CurrentDomain.BaseDirectory,"HDR-Unlock-error.txt");try{File.WriteAllText(dest,ex.ToString());}catch{}if(args.Length==0)MessageBox.Show(ex.Message,"HDLSS");return 1;}finally{SingleInstance.Leave();}
 }
 void Wire(){
  UpdateLibraryLayout();window.SizeChanged+=(s,e)=>UpdateLibraryLayout();
  C<ContentControl>("ActivityLogHost").Content=ActivityLog.Panel();
  C<Button>("MinimizeWindow").Click+=(s,e)=>SystemCommands.MinimizeWindow(window);
  C<Button>("MaximizeWindow").Click+=(s,e)=>{if(window.WindowState==WindowState.Maximized)SystemCommands.RestoreWindow(window);else SystemCommands.MaximizeWindow(window);};
  C<Button>("CloseWindow").Click+=(s,e)=>window.Close();
  C<Button>("InjectionSetup").Click+=(s,e)=>Act(()=>{if(current!=null&&(dlssPanel==null||!dlssPanel.IsBusy)&&Leave())InjectionSetup.Show(window,current,()=>Select(current));});
  C<Button>("AddGame").Click+=(s,e)=>Act(()=>{if(busy)return;if(!Leave())return;var dialog=new Microsoft.Win32.OpenFileDialog{Title="Add game",Filter="Game executable|*.exe"};if(dialog.ShowDialog(window)!=true)return;var game=Core.ManualGame(dialog.FileName);var existing=games.FirstOrDefault(g=>String.Equals(g.Exe,game.Exe,StringComparison.OrdinalIgnoreCase));if(existing!=null){C<ListBox>("Games").SelectedItem=existing;return;}if(Core.Pref.ManualGames==null)Core.Pref.ManualGames=new List<Game>();Core.Pref.ManualGames.Add(game);try{Core.SavePrefs();}catch{Core.Pref.ManualGames.Remove(game);throw;}games.Add(game);C<TextBox>("Search").Text="";C<CheckBox>("ShowUncertain").IsChecked=true;Filter();C<ListBox>("Games").SelectedItem=game;ShowHome();});
  C<Button>("Home").Click+=(s,e)=>Act(()=>ShowHome());C<Button>("Overview").Click+=(s,e)=>Act(()=>ShowHome());
  string[] languages={"it","en","es","fr","de","pt"};C<ComboBox>("HomeLanguage").ItemsSource=new[]{"Italiano","English","Español","Français","Deutsch","Português"};C<ComboBox>("HomeLanguage").SelectedIndex=Math.Max(0,Array.IndexOf(languages,Core.Pref.Language));
  C<ComboBox>("HomeLanguage").SelectionChanged+=(s,e)=>{string old=Core.Pref.Language;try{Core.Pref.Language=languages[C<ComboBox>("HomeLanguage").SelectedIndex];Core.SavePrefs();Appearance.Refresh();}catch(Exception ex){Core.Pref.Language=old;Fail(ex);}};
  string[] themes={"dark","light","system"};C<ComboBox>("HomeTheme").ItemsSource=new[]{"Dark","Light","System"};C<ComboBox>("HomeTheme").SelectedIndex=Math.Max(0,Array.IndexOf(themes,Core.Pref.Theme));C<ComboBox>("HomeTheme").SelectionChanged+=(s,e)=>{string old=Core.Pref.Theme;try{Core.Pref.Theme=themes[C<ComboBox>("HomeTheme").SelectedIndex];Core.SavePrefs();Appearance.Refresh();}catch(Exception ex){Core.Pref.Theme=old;Fail(ex);}};
  C<Button>("Scan").Click+=(s,e)=>{if(Leave())StartScan();};
  C<TextBox>("Search").TextChanged+=(s,e)=>Filter();C<CheckBox>("ShowUncertain").Checked+=(s,e)=>Filter();C<CheckBox>("ShowUncertain").Unchecked+=(s,e)=>Filter();
  C<ListBox>("Games").SelectionChanged+=(s,e)=>{if(loading)return;var g=C<ListBox>("Games").SelectedItem as Game;if(g==current)return;if(busy||RenoDxUi.IsBusy||(dlssPanel!=null&&dlssPanel.IsBusy)){loading=true;C<ListBox>("Games").SelectedItem=current;loading=false;SetStatus("Attendi il completamento dell’operazione.");return;}if(!Leave()){loading=true;C<ListBox>("Games").SelectedItem=current;loading=false;return;}Select(g);};
  C<Button>("SteamFolder").Click+=(s,e)=>Act(()=>{using(var d=new Forms.FolderBrowserDialog()){d.Description="Scegli SteamLibrary oppure la cartella Xbox / Game Pass (es. XboxGames)";if(d.ShowDialog()!=Forms.DialogResult.OK)return;string path=d.SelectedPath;if(String.Equals(Path.GetFileName(path),"steamapps",StringComparison.OrdinalIgnoreCase))path=Path.GetDirectoryName(path);if(!Core.Pref.Libraries.Contains(path))Core.Pref.Libraries.Add(path);Core.SavePrefs();StartScan();}});
  C<Button>("DataFolder").Click+=(s,e)=>Act(()=>Open(Core.Data));
  C<Button>("CoverButton").Click+=(s,e)=>Act(()=>{if(current==null)return;var d=new Microsoft.Win32.OpenFileDialog{Filter="Immagini|*.jpg;*.jpeg;*.png;*.bmp"};if(d.ShowDialog(window)!=true)return;ImageSource img=Image(d.FileName);if(img==null)throw new Exception("Immagine non leggibile.");string dir=Path.Combine(Core.Data,"covers");Directory.CreateDirectory(dir);string file=Path.Combine(dir,current.Id+Path.GetExtension(d.FileName));File.Copy(d.FileName,file,true);Core.Pref.Covers[current.Id]=Path.Combine("covers",Path.GetFileName(file));Core.SavePrefs();current.Cover=file;RefreshCards();});
  C<Button>("Dlss").Click+=(s,e)=>Act(()=>SwitchSection(true));
  C<Button>("HdrSection").Click+=(s,e)=>Act(()=>SwitchSection(false));
  C<Button>("Help").Click+=(s,e)=>Guide.Show(window);
  window.Closing+=(s,e)=>{if(busy||RenoDxUi.IsBusy||(dlssPanel!=null&&dlssPanel.IsBusy)){e.Cancel=true;return;}if(preview==null&&!Leave())e.Cancel=true;};
  window.Loaded+=(s,e)=>StartScan();
  C<Button>("Play").Click+=(s,e)=>Act(()=>{if(current!=null&&!busy&&!RenoDxUi.IsBusy&&(dlssPanel==null||!dlssPanel.IsBusy))InjectionSetup.Launch(current);});
 }

 void HdrMode(){var host=C<Grid>("RenoInline");if(!homeMode&&!dlssMode&&!RenoDxUi.IsBusy){host.Children.Clear();if(current!=null&&!String.IsNullOrEmpty(current.Exe)&&File.Exists(current.Exe))host.Children.Add(RenoDxUi.Embed(window,current));} }

 void RefreshHome(){
  bool valid=current!=null&&!String.IsNullOrEmpty(current.Exe)&&File.Exists(current.Exe);C<Button>("HdrSection").IsEnabled=C<Button>("Dlss").IsEnabled=valid;
  C<TextBlock>("HdrCardStatus").Text=C<TextBlock>("DlssCardStatus").Text=current==null?"Select a game":"Not installed";
  C<TextBlock>("HdrCardMethod").Text="ReShade · RenoDX · MFG Unlock";
  C<TextBlock>("DlssCardMethod").Text=valid?"OptiScaler setup for the selected game":"Select a valid game executable first.";
  bool hdr=false,dlss=false;
  if(valid){try{string folder=Path.GetDirectoryName(current.Exe);bool reshade=RenoDx.Loaders(folder).Count>0,reno=RenoDx.HasHdrAddon(current);hdr=reshade&&reno;if(hdr)C<TextBlock>("HdrCardStatus").Text="RenoDX installed";else if(reshade)C<TextBlock>("HdrCardStatus").Text="ReShade installed";
  }catch(Exception ex){C<TextBlock>("HdrCardStatus").Text="Check required";C<TextBlock>("HdrCardStatus").ToolTip=ex.Message;}try{
   var scan=DlssCore.Inspect(current,false,false);dlss=scan.Proxy!="";C<TextBlock>("DlssCardStatus").Text=dlss?"OptiScaler installed":"Not installed";if(dlss)C<TextBlock>("DlssCardMethod").Text="Detected files · verify activation in game";
  }catch(Exception ex){C<TextBlock>("DlssCardStatus").Text="Check required";C<TextBlock>("DlssCardStatus").ToolTip=ex.Message;}}
  C<Button>("HdrSection").Style=window.TryFindResource(hdr?"Installed":"Primary") as Style;C<Button>("Dlss").Style=window.TryFindResource(dlss?"Installed":"Primary") as Style;Appearance.Refresh();
 }
 void UpdateLibraryLayout(){double height=window.ActualHeight>0?window.ActualHeight:window.Height;double poster=Math.Round(Math.Max(200,Math.Min(320,200+(height-740)*0.18)));if(!homeMode)poster=Math.Round(Math.Max(110,Math.Min(230,110+(height-740)*0.18)));window.Resources["LibraryPosterHeight"]=poster;window.Resources["LibraryCardWidth"]=Math.Round(poster*0.70+24);bool compactDlss=!homeMode&&dlssMode;C<Grid>("LibrarySection").Visibility=compactDlss?Visibility.Collapsed:Visibility.Visible;C<RowDefinition>("LibraryRow").Height=new GridLength(compactDlss?0:poster+(homeMode?225:165));C<ListBox>("Games").LayoutTransform=Transform.Identity;}
 void ShowHome(){if(busy||RenoDxUi.IsBusy||(dlssPanel!=null&&dlssPanel.IsBusy))throw new Exception("Wait for the current operation to finish.");if(!homeMode&&!Leave())return;homeMode=true;C<Button>("Overview").Visibility=Visibility.Collapsed;UpdateLibraryLayout();C<ScrollViewer>("FeatureHome").Visibility=Visibility.Visible;C<Border>("HdrSettings").Visibility=Visibility.Collapsed;C<Grid>("DlssSection").Visibility=Visibility.Collapsed;RefreshHome();Appearance.Refresh();}
 void RenderDlss(){var host=C<Grid>("DlssSection");host.Children.Clear();host.RowDefinitions.Clear();dlssPanel=null;if(current==null)return;if(String.IsNullOrEmpty(current.Exe)||!File.Exists(current.Exe)){host.Children.Add(Dialogs.Text(current.ProtectedInstall?current.Evidence:"Scegli un target.exe valido dal pulsante Target.exe / API per configurare OptiScaler.",16));return;}dlssPanel=new DlssUi();dlssPanel.Create(window,current,true);dlssPanel.EmbeddedContent.Margin=new Thickness(18,16,18,16);host.Children.Add(dlssPanel.EmbeddedContent);}
 void SwitchSection(bool dlss){
  if(busy||RenoDxUi.IsBusy||(dlssPanel!=null&&dlssPanel.IsBusy))throw new Exception("Attendi il completamento dell’installazione.");
  if(dlss&&current==null)return;
  if(dlss&&!dlssMode&&!Leave())return;
  var host=C<Grid>("DlssSection");
  if(dlss&&(homeMode||!dlssMode))RenderDlss();homeMode=false;C<Button>("Overview").Visibility=Visibility.Visible;UpdateLibraryLayout();C<ScrollViewer>("FeatureHome").Visibility=Visibility.Collapsed;
  dlssMode=dlss;host.Visibility=dlss?Visibility.Visible:Visibility.Collapsed;
  UpdateLibraryLayout();
  C<Border>("HdrSettings").Visibility=dlss?Visibility.Collapsed:Visibility.Visible;
  HdrMode();

  RefreshHome();Appearance.Refresh();
 }
 bool Leave(){if(RenoDxUi.IsBusy||(dlssPanel!=null&&dlssPanel.IsBusy)){SetStatus("Attendi il completamento dell’operazione.");return false;}return true;}

 async void StartScan(){if(busy)return;busy=true;C<Button>("Scan").IsEnabled=C<Button>("SteamFolder").IsEnabled=false;SetStatus("Lettura di Steam e Xbox / Game Pass, motori e copertine locali…");try{games=await Task.Run(()=>Core.Scan());Filter();if(C<ListBox>("Games").Items.Count>0){loading=true;C<ListBox>("Games").SelectedItem=C<ListBox>("Games").Items[0];loading=false;Select(C<ListBox>("Games").SelectedItem as Game);}else Select(null);SetStatus("Scansione completata.");}catch(Exception ex){Fail(ex);}finally{busy=false;C<Button>("Scan").IsEnabled=C<Button>("SteamFolder").IsEnabled=true;}
 if(preview!=null){try{SavePreview(preview);if(current!=null){SwitchSection(false);SavePreview(preview+"-reshade.png");SwitchSection(true);SavePreview(preview+"-dlss.png");}}finally{window.Close();}}
 }
 void SavePreview(string file){Appearance.Refresh();window.UpdateLayout();var bitmap=new RenderTargetBitmap((int)window.ActualWidth,(int)window.ActualHeight,96,96,PixelFormats.Pbgra32);bitmap.Render(window);var png=new PngBitmapEncoder();png.Frames.Add(BitmapFrame.Create(bitmap));using(var output=File.Create(file))png.Save(output);}


 void Filter(){if(window==null)return;string q=C<TextBox>("Search").Text??"";bool all=C<CheckBox>("ShowUncertain").IsChecked==true;var filtered=games.Where(g=>(all||g.Confirmed)&&g.Name.IndexOf(q,StringComparison.CurrentCultureIgnoreCase)>=0).ToList();loading=true;C<ListBox>("Games").ItemsSource=filtered;if(current!=null&&filtered.Contains(current))C<ListBox>("Games").SelectedItem=current;loading=false;C<TextBlock>("Empty").Visibility=filtered.Count==0?Visibility.Visible:Visibility.Collapsed;}
 void RefreshCards(){C<ListBox>("Games").Items.Refresh();}
 static ImageSource Image(string path){if(String.IsNullOrEmpty(path)||!File.Exists(path))return null;try{var i=new BitmapImage();i.BeginInit();i.CacheOption=BitmapCacheOption.OnLoad;i.UriSource=new Uri(Path.GetFullPath(path));i.EndInit();i.Freeze();return i;}catch{return null;}}
 void Select(Game game){current=game;loading=true;try{
 C<TextBox>("TargetPath").Text=game==null?"Seleziona un gioco":game.ProtectedInstall?game.Evidence:String.IsNullOrEmpty(game.Exe)?"Select a valid game executable first.":Path.GetFullPath(game.Exe);
 C<TextBlock>("GameTitle").Text=game==null?"Seleziona un gioco":game.Name;
 foreach(string name in new[]{"InjectionSetup","Dlss","Play","CoverButton"})C<Button>(name).IsEnabled=game!=null;
 if(!homeMode){if(dlssMode)RenderDlss();else HdrMode();}RefreshHome();
 }finally{loading=false;}}

 static void Open(string target){System.Diagnostics.Process.Start(new System.Diagnostics.ProcessStartInfo(target){UseShellExecute=true});}
 void ShowText(string title,string text){var d=new Window{Owner=window,Title=title,Width=790,Height=600,WindowStartupLocation=WindowStartupLocation.CenterOwner};d.Content=new TextBox{Text=text,IsReadOnly=true,TextWrapping=TextWrapping.Wrap,VerticalScrollBarVisibility=ScrollBarVisibility.Auto,Padding=new Thickness(20),Background=new SolidColorBrush(Color.FromRgb(20,26,34)),Foreground=Brushes.White,FontFamily=new FontFamily("Consolas"),FontSize=13};d.ShowDialog();}

}
public static class Test {
 static void Check(bool ok,string message){if(!ok)throw new Exception("TEST: "+message);}
 public static void Run(string report){string root=Path.Combine(Path.GetDirectoryName(report),"fixture-"+Guid.NewGuid().ToString("N"));Directory.CreateDirectory(root);Core.Data=Path.Combine(root,"data");Core.Init();
  string section="/Script/Test.CustomUserSettings",user="["+section+"]\r\nbUseHDRDisplayOutput=False\r\nHDRDisplayOutputNits=1000\r\nResolutionSizeX=3440\r\n[/Script/Engine.GameUserSettings]\r\nbUseDesiredScreenHeight=False\r\n";string engine="[SystemSettings]\r\nKeep=7\r\n[Other]\r\nr.HDR.UI.Level=9\r\n";
  Check(Core.Field("\"path\" \"D:\\\\SteamLibrary\"","path")==@"D:\SteamLibrary","VDF escaped path");Check(Core.Sections(user).Count==2,"custom sections");
  string changed=Core.Set(engine,"SystemSettings","r.HDR.UI.Level","0.75");Check(Core.Get(changed,"Other","r.HDR.UI.Level")=="9","section isolation");Check(Core.Set(changed,"SystemSettings","r.HDR.UI.Level","0.75")==changed,"idempotent INI");
  foreach(string bad in new[]{"","1.5","1,5","-1","abc","99999999999999"}){bool failed=false;try{Core.Number(bad,"test",1,2000);}catch{failed=true;}Check(failed,"integer rejection "+bad);}
  long now=(long)(DateTime.UtcNow-new DateTime(1970,1,1)).TotalSeconds;var snapshot=Snapshot.Parse("HDRPILOT=1\ntime="+now+"\nmax=1060\nmid=NA\nsequence=5\ncomplete=1\n");Check(snapshot!=null&&snapshot.Fresh()&&snapshot.Values["max"]==1060&&!snapshot.Values.ContainsKey("mid"),"telemetry values and NA");Check(Snapshot.Parse("HDRPILOT=1\ntime="+now)==null,"partial telemetry rejected");snapshot.Time=now-10;Check(!snapshot.Fresh(),"stale telemetry rejected");
  var av=new AdvancedHdr{Output=3,Gamut=2,UiNits=200,ScenePercent=125,Compression=1,Composite=0}.Values();Check(av["r.HDR.Aces.SceneColorMultiplier"]=="1.25"&&av["r.HDR.UI.Luminance"]=="200","advanced HDR conversion");bool invalidFormat=false;try{new AdvancedHdr{Output=3,Gamut=0}.Values();}catch{invalidFormat=true;}Check(invalidFormat,"HDR format/gamut validation");
  string config=Path.Combine(root,"config");Directory.CreateDirectory(config);string ep=Path.Combine(config,"Engine.ini"),up=Path.Combine(config,"GameUserSettings.ini");File.WriteAllText(ep,engine,Encoding.Unicode);File.WriteAllText(up,user);byte[] before=File.ReadAllBytes(ep);File.SetAttributes(ep,FileAttributes.ReadOnly);var g=new Game{Id="123",Name="Test",Config=config,Exe="nonexistent-test-game.exe"};
  string backup=Core.Apply(g,section,1060,15,75,"-6");Check(File.ReadAllBytes(Path.Combine(backup,"Engine.ini")).SequenceEqual(before),"byte exact backup");Check((File.GetAttributes(ep)&FileAttributes.ReadOnly)!=0,"readonly preserved");Check(File.ReadAllBytes(ep)[0]==255,"UTF16 preserved");Check(Core.Get(File.ReadAllText(ep),"SystemSettings","r.HDR.UI.Level")=="0.75","percent conversion");Check(File.ReadAllText(up).Contains("ResolutionSizeX=3440"),"other settings preserved");
  byte[] state=File.ReadAllBytes(ep);using(var locked=File.Open(up,FileMode.Open,FileAccess.Read,FileShare.Read)){bool failed=false;try{Core.Apply(g,section,900,18,100,"-4");}catch{failed=true;}Check(failed,"locked second file");}Check(File.ReadAllBytes(ep).SequenceEqual(state),"first file unchanged on preflight failure");Check((File.GetAttributes(ep)&FileAttributes.ReadOnly)!=0,"readonly restored after failure");
  Core.Restore(g,backup);Check(File.ReadAllBytes(ep).SequenceEqual(before),"exact restore");Core.Apply(g,section,1060,15,75,"-6",new AdvancedHdr{Output=3,Gamut=2,UiNits=200,ScenePercent=125,Compression=1,Composite=0});Core.Apply(g,section,1060,15,100,"-4");string afterAdvanced=File.ReadAllText(ep);Check(Core.Get(afterAdvanced,"SystemSettings","r.HDR.Display.OutputDevice")=="3"&&Core.Get(afterAdvanced,"SystemSettings","r.HDR.Display.ColorGamut")=="2"&&Core.Get(afterAdvanced,"SystemSettings","r.HDR.UI.CompositeMode")=="0"&&Core.Get(afterAdvanced,"SystemSettings","r.HDR.UI.Luminance")=="200","advanced preservation when unchecked");Core.Restore(g,backup);File.SetAttributes(ep,FileAttributes.Normal);File.Delete(ep);backup=Core.Apply(g,section,1060,15,100,"-4");Check(File.Exists(ep),"create missing engine");Core.Restore(g,backup);Check(!File.Exists(ep),"restore missing engine");
  File.WriteAllText(report,"PASS: VDF parsing; section selection/isolation; INI idempotence; integer validation; advanced HDR conversions and preservation; exact backups; UTF16 preservation; readonly save/restore; locked-file preflight; rollback preservation; creation and restoration of absent Engine.ini.\nFixture: "+root); }
}
}
