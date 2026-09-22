using System.Linq;
using System;

using System.Windows;

using System.Windows.Controls;

using System.Windows.Interop;

using System.Windows.Media;

using System.Runtime.InteropServices;

namespace HdrPilot {

public class Shortcut:IDisposable {

 [DllImport("user32.dll")]static extern bool RegisterHotKey(IntPtr h,int id,uint mods,uint key);

 [DllImport("user32.dll")]static extern bool UnregisterHotKey(IntPtr h,int id);

 [DllImport("user32.dll")]static extern short GetAsyncKeyState(int key);

 System.Windows.Threading.DispatcherTimer poll;bool held;DateTime lastFire;IntPtr handle;HwndSource source;int id=7021;bool registered;uint currentMods,currentKey;Action action;

 public Shortcut(Window window,Action trigger){handle=new WindowInteropHelper(window).Handle;source=HwndSource.FromHwnd(handle);action=trigger;source.AddHook(Hook);poll=new System.Windows.Threading.DispatcherTimer{Interval=TimeSpan.FromMilliseconds(40)};poll.Tick+=(s,e)=>Poll();poll.Start();}

 public bool Set(uint mods,uint key){if(registered&&mods==currentMods&&key==currentKey)return true;int next=id==7021?7022:7021;if(!RegisterHotKey(handle,next,mods|0x4000,key))return false;if(registered)UnregisterHotKey(handle,id);id=next;registered=true;currentMods=mods;currentKey=key;held=false;return true;}

 IntPtr Hook(IntPtr h,int msg,IntPtr w,IntPtr l,ref bool handled){if(msg==0x312&&w.ToInt32()==id){handled=true;Fire();}return IntPtr.Zero;}

 void Fire(){if((DateTime.UtcNow-lastFire).TotalMilliseconds<250)return;lastFire=DateTime.UtcNow;action();}

 static bool Down(int key){return (GetAsyncKeyState(key)&0x8000)!=0;}

 void Poll(){if(!registered)return;bool pressed=Down((int)currentKey)&&Down(0x11)==((currentMods&2)!=0)&&Down(0x10)==((currentMods&4)!=0)&&Down(0x12)==((currentMods&1)!=0);if(pressed&&!held)Fire();held=pressed;}

 public void Dispose(){poll.Stop();if(registered)UnregisterHotKey(handle,id);source.RemoveHook(Hook);}

 public static readonly uint[] NavigationKeys={0x24,0x23,0x21,0x22,0x2D,0x2E};
 public static bool Allowed(uint mods,uint key){bool nav=Array.IndexOf(NavigationKeys,key)>=0;bool letter=key>=65&&key<=90;bool function=key>=112&&key<=122;return (nav||letter||function)&&(mods!=0||nav||(function&&key!=121));}
 public static string Label(uint mods,uint key){string name;switch(key){case 0x24:name="Home";break;case 0x23:name="End / Fine";break;case 0x21:name="PgUp / Pag su";break;case 0x22:name="PgDn / Pag giù";break;case 0x2D:name="Ins";break;case 0x2E:name="Del / Canc";break;default:name=key>=112&&key<=122?"F"+(key-111):((char)key).ToString();break;}return ((mods&2)!=0?"Ctrl + ":"")+((mods&4)!=0?"Shift + ":"")+((mods&1)!=0?"Alt + ":"")+name;}

}

public static class HdrChoices {
 public static readonly DependencyProperty AppliedOptionsProperty=DependencyProperty.RegisterAttached("AppliedOptions",typeof(int),typeof(HdrChoices),new PropertyMetadata(0,(d,e)=>{var c=d as ComboBox;if(c!=null&&c.Resources.Contains("AppliedRefresh"))((Action)c.Resources["AppliedRefresh"])();}));
 public static void SetApplied(ComboBox c,params int[] indices){int bits=0;foreach(int i in indices)if(i>=0&&i<30)bits|=1<<i;c.SetValue(AppliedOptionsProperty,bits);}

 public static Button Option(object label){return new Button{Content=label,Padding=new Thickness(10,7,10,7),Margin=new Thickness(0,0,6,6),FontSize=13};}
 public static void Help(Button button,string text){button.ToolTip=new ToolTip{Content=new TextBlock{Text=text,TextWrapping=TextWrapping.Wrap,MaxWidth=440,FontSize=14},Padding=new Thickness(12)};ToolTipService.SetShowOnDisabled(button,true);ToolTipService.SetInitialShowDelay(button,300);ToolTipService.SetShowDuration(button,120000);}

 public static void Apply(Window window){foreach(string name in new[]{"HdrMethod","Black","OutputFormat"}){var select=window.FindName(name) as ComboBox;if(select==null||select.Visibility==Visibility.Collapsed)continue;var parent=select.Parent as Panel;if(parent==null)continue;int position=parent.Children.IndexOf(select);var wrap=new WrapPanel{Margin=select.Margin};Grid.SetColumn(wrap,Grid.GetColumn(select));Grid.SetRow(wrap,Grid.GetRow(select));Grid.SetColumnSpan(wrap,Grid.GetColumnSpan(select));parent.Children.Insert(position+1,wrap);select.Visibility=Visibility.Collapsed;var buttons=new System.Collections.Generic.List<Button>();for(int i=0;i<select.Items.Count;i++){int index=i;var item=select.Items[i] as ComboBoxItem;var button=Option(item==null?select.Items[i]:item.Content);button.SetBinding(Button.IsEnabledProperty,new System.Windows.Data.Binding("IsEnabled"){Source=item==null?(object)select:item});string explanation=name=="HdrMethod"?(index==0?"Richiede HDR al motore UE5 tramite i file di configurazione. Disponibile per titoli riconosciuti; non tutti applicano ogni parametro.":"Usa ReShade e una mod RenoDX specifica per il gioco. Le regolazioni HDR si effettuano nel menu ReShade; la selezione non installa o rimuove componenti."):name=="Black"?(index==0?"Mantiene il riferimento di nero previsto dal motore Unreal.":index==2?"Conserva il valore personalizzato già letto dal file. Non lo sostituisce con un preset.":"Richiede un livello molto vicino a zero. Non corregge necessariamente neri sollevati dal gioco; il parametro interno è logaritmico."):(index<2?"Richiede scRGB lineare con spazio Rec.709. ":"Richiede HDR10 PQ con spazio Rec.2020. ")+"La curva "+(index%2==0?"1000":"2000")+" è un preset del motore e non sostituisce il picco personalizzato. Il valore richiesto non certifica il formato effettivamente in uso.";Help(button,explanation);button.Click+=(s,e)=>select.SelectedIndex=index;buttons.Add(button);wrap.Children.Add(button);}Action update=()=>{for(int i=0;i<buttons.Count;i++)buttons[i].Style=window.TryFindResource((((int)select.GetValue(AppliedOptionsProperty)&(1<<i))!=0)?(object)"Installed":i==select.SelectedIndex?(object)"Primary":typeof(Button)) as Style;};select.Resources["AppliedRefresh"]=update;select.SelectionChanged+=(s,e)=>update();update();}
 }
}
public static class Dialogs {

 static Brush B(string color){return (Brush)new BrushConverter().ConvertFromString(color);}

 public static TextBlock Text(string text,int size){return new TextBlock{Text=text,FontSize=size,TextWrapping=TextWrapping.Wrap,Foreground=B("#ADBACA"),Margin=new Thickness(0,0,0,14)};}

 public static Window Create(Window owner,string title,out StackPanel body){var w=new Window{Title=title,Owner=owner,Width=650,SizeToContent=SizeToContent.Height,MaxHeight=850,WindowStartupLocation=WindowStartupLocation.CenterOwner,ResizeMode=ResizeMode.NoResize,WindowStyle=WindowStyle.None,Background=B("#141A22"),Foreground=B("#F0F3F5"),FontFamily=new FontFamily("Cascadia Mono, Consolas"),FontSize=13};if(owner!=null)w.Resources=owner.Resources;var border=new Border{BorderBrush=B("#35404D"),BorderThickness=new Thickness(1),Padding=new Thickness(26)};body=new StackPanel();var heading=new DockPanel();var close=new Button{Content="×",Padding=new Thickness(10,4,10,4),HorizontalAlignment=HorizontalAlignment.Right};DockPanel.SetDock(close,Dock.Right);close.Click+=(s,e)=>w.Close();heading.Children.Add(close);var h=Text(title,23);h.Foreground=B("#E6AC50");h.FontWeight=FontWeights.SemiBold;heading.Children.Add(h);heading.MouseLeftButtonDown+=(s,e)=>{if(e.ButtonState==System.Windows.Input.MouseButtonState.Pressed)w.DragMove();};body.Children.Add(heading);border.Child=body;w.Content=border;w.PreviewKeyDown+=(s,e)=>{if(e.Key==System.Windows.Input.Key.Escape)w.Close();};return w;}

 public static Window Notice(Window owner,string title,string message,string backup){StackPanel body;var w=Create(owner,title,out body);body.Children.Add(Text(message,14));if(backup!=null){body.Children.Add(Text("BACKUP CREATO · SOLA LETTURA RIPRISTINATA",10));var path=Text(backup,11);body.Children.Add(path);var open=new Button{Content="Apri backup",Margin=new Thickness(0,0,0,12)};open.Click+=(s,e)=>{try{System.Diagnostics.Process.Start(new System.Diagnostics.ProcessStartInfo(backup){UseShellExecute=true});}catch(Exception ex){path.Text=ex.Message;}};body.Children.Add(open);}var ok=new Button{Content="Ho capito",IsDefault=true};if(owner!=null)ok.Style=owner.TryFindResource("Primary") as Style;ok.Click+=(s,e)=>w.Close();body.Children.Add(ok);return w;}

}

}

namespace HdrPilot {

public static class UiTest {

 [System.Runtime.InteropServices.DllImport("user32.dll")]static extern System.IntPtr SendMessage(System.IntPtr h,int msg,System.IntPtr w,System.IntPtr l);

 static void Check(bool ok,string msg){if(!ok)throw new System.Exception(msg);}

 static void Render(System.Windows.Window w,string path){w.Show();w.UpdateLayout();var b=new System.Windows.Media.Imaging.RenderTargetBitmap((int)w.ActualWidth,(int)w.ActualHeight,96,96,System.Windows.Media.PixelFormats.Pbgra32);b.Render(w);var p=new System.Windows.Media.Imaging.PngBitmapEncoder();p.Frames.Add(System.Windows.Media.Imaging.BitmapFrame.Create(b));using(var f=System.IO.File.Create(path))p.Save(f);}

 public static void Run(string folder){System.IO.Directory.CreateDirectory(folder);System.Windows.Window owner;using(var s=System.Reflection.Assembly.GetExecutingAssembly().GetManifestResourceStream("Main.xaml"))owner=(System.Windows.Window)System.Windows.Markup.XamlReader.Load(s);owner.Show();int called=0;var h=new Shortcut(owner,()=>called++);Check(h.Set(7,120),"test shortcut registration");Check(h.Set(7,120),"unchanged shortcut");var competitorWindow=new System.Windows.Window();competitorWindow.Show();var competitor=new Shortcut(competitorWindow,()=>{});Check(!competitor.Set(7,120),"conflict rejection");SendMessage(new System.Windows.Interop.WindowInteropHelper(owner).Handle,0x312,new System.IntPtr(7022),System.IntPtr.Zero);Check(called==1,"shortcut action");Check(h.Set(7,119),"remap");Check(competitor.Set(7,120),"old shortcut released");System.Threading.Thread.Sleep(270);SendMessage(new System.Windows.Interop.WindowInteropHelper(owner).Handle,0x312,new System.IntPtr(7021),System.IntPtr.Zero);Check(called==2,"remapped action");competitor.Dispose();foreach(uint key in Shortcut.NavigationKeys){Check(Shortcut.Allowed(0,key)&&Shortcut.Allowed(2,key),"navigation key allowed");Check(Shortcut.Label(0,key).Length>1,"navigation key label");Check(h.Set(0,key),"standalone navigation key registration");var stored=Core.Json.Deserialize<Preferences>(Core.Json.Serialize(new Preferences{HotkeyMods=0,HotkeyKey=key}));Check(stored.HotkeyKey==key&&stored.HotkeyMods==0,"navigation preference roundtrip");}Check(!Shortcut.Allowed(0,65)&&!Shortcut.Allowed(0,121)&&!Shortcut.Allowed(0,123),"reserved shortcuts");h.Dispose();competitorWindow.Close();

 var prefs=Core.Json.Deserialize<Preferences>("{}");Check(prefs.HotkeyMods==6&&prefs.HotkeyKey==72,"legacy preference defaults");prefs.HotkeyMods=7;prefs.HotkeyKey=119;prefs.MonitorModes["screen"]=2;var saved=Core.Json.Deserialize<Preferences>(Core.Json.Serialize(prefs));Check(saved.HotkeyKey==119&&saved.MonitorModes["screen"]==2,"preference roundtrip");Check(new DisplayInfo{Name="Dell AW3423DW"}.Alienware&&!new DisplayInfo{Name="Dell AW3423DWF"}.Alienware,"exact monitor model");byte[] edid=new byte[256];edid[1]=255;edid[126]=1;edid[128]=2;edid[130]=10;edid[132]=0xE5;edid[133]=6;edid[134]=4;edid[136]=141;var display=new DisplayInfo{Name="Test"};Displays.ReadEdid(display,edid);Check(display.Hdr&&display.Peak==1060,"EDID luminance decode");edid[130]=7;display.Peak=0;display.Hdr=false;Displays.ReadEdid(display,edid);Check(display.Peak==0&&!display.Hdr,"truncated block rejection");

 var notice=Dialogs.Notice(owner,"Profilo HDR salvato","ANTEPRIMA · dati dimostrativi\n\nPicco: 1060 nit   ·   Grigio 18%: 15 nit\nInterfaccia: 100%   ·   Nero: Base Unreal\n\nFile aggiornati e riletti. Avvia il gioco da Steam. Apri l’overlay per confrontare i parametri effettivi del motore.",@"data\backups\5184670\esempio");Render(notice,System.IO.Path.Combine(folder,"conferma.png"));notice.Close();

 Check(owner.FindName("Options")==null && owner.FindName("HomeLanguage") is ComboBox && owner.FindName("HomeTheme") is ComboBox,"home preferences retained");Check(owner.FindName("FeatureHome") is ScrollViewer && owner.FindName("HdrOverlayKey") is Button && owner.FindName("Hotkey") is Button,"feature cards and HDR shortcut");var manual=Core.ManualGame(System.Reflection.Assembly.GetExecutingAssembly().Location);Check(manual.Store=="Manual"&&manual.ArtworkLabel=="PC","manual store identity");Check(Core.ManualGame(manual.Exe).Id==manual.Id,"stable manual identity");var manualPrefs=Core.Json.Deserialize<Preferences>(Core.Json.Serialize(new Preferences{ManualGames=new System.Collections.Generic.List<Game>{manual}}));Check(manualPrefs.ManualGames[0].Exe==manual.Exe,"manual game persistence");bool missingRejected=false;try{Core.ManualGame(System.IO.Path.Combine(folder,"missing.exe"));}catch{missingRejected=true;}Check(missingRejected,"missing manual executable rejected");Check(owner.FindName("AddGame") is Button,"add game action");Check(owner.WindowStyle==WindowStyle.None,"dark custom titlebar");

 foreach(string name in new[]{"Play","Apply","Overlay"}) { DependencyObject node=(DependencyObject)owner.FindName(name); while(node!=null){Check(!(node is ScrollViewer),"fixed footer outside scrolling: "+name);node=VisualTreeHelper.GetParent(node);} }

 Check(owner.FindName("GamutCompression") is CheckBox && owner.FindName("UiComposite") is CheckBox,"advanced toggles");

 owner.Width=1180;owner.Height=740;Render(owner,System.IO.Path.Combine(folder,"compatto.png"));

 Check(new Preferences().Language=="en","English default");Check(Core.Json.Deserialize<Preferences>("{\"Language\":\"it\"}").Language=="it","saved Italian language preserved");
 Core.Pref.Language="en";Check(Appearance.ToEnglish("Installa ReShade")=="Install ReShade","English component text");
 var englishGuide=Guide.Create(owner);Render(englishGuide,System.IO.Path.Combine(folder,"help-english.png"));var englishBody=(StackPanel)((Border)englishGuide.Content).Child;var englishLayout=(Grid)englishBody.Children[1];var englishMenu=(StackPanel)((ScrollViewer)englishLayout.Children[0]).Content;Check(englishMenu.Children.OfType<Button>().Any(b=>Convert.ToString(b.Content)=="Uninstall and Steam originals"),"English uninstall guide");englishMenu.Children.OfType<Button>().First(b=>Convert.ToString(b.Content)=="DLLs and models").RaiseEvent(new RoutedEventArgs(Button.ClickEvent));var englishContent=(StackPanel)((ScrollViewer)englishLayout.Children[1]).Content;Check(((TextBlock)englishContent.Children[0]).Text=="DLLs and models","English guide navigation");englishGuide.Close();Core.Pref.Language="it";
 var guide=Guide.Create(owner);Render(guide,System.IO.Path.Combine(folder,"guida.png"));

 var body=(StackPanel)((Border)guide.Content).Child;var layout=(Grid)body.Children[1];var menu=(StackPanel)((ScrollViewer)layout.Children[0]).Content;

 Check(menu.Children.OfType<Button>().Any(b=>Convert.ToString(b.Content)=="Xbox / Game Pass"),"Xbox guide topic");

 menu.Children.OfType<Button>().First(b=>Convert.ToString(b.Content)=="HDR avanzato").RaiseEvent(new RoutedEventArgs(Button.ClickEvent));guide.UpdateLayout();

 var content=(StackPanel)((ScrollViewer)layout.Children[1]).Content;Check(((TextBlock)content.Children[0]).Text=="HDR avanzato","guide navigation");

 Render(guide,System.IO.Path.Combine(folder,"guida-avanzato.png"));foreach(Button topic in menu.Children)if(Convert.ToString(topic.Content)=="Frame Generation / MFG")topic.RaiseEvent(new RoutedEventArgs(Button.ClickEvent));guide.UpdateLayout();Check(((TextBlock)content.Children[0]).Text=="Frame Generation / MFG","MFG guide navigation");Render(guide,System.IO.Path.Combine(folder,"guida-mfg.png"));guide.Close();owner.Close();System.IO.File.WriteAllText(System.IO.Path.Combine(folder,"test.txt"),"PASS: global shortcut registration, same shortcut, conflict rejection, remap and callback dispatch, released keys; legacy preferences, saved mapping and monitor mode; exact monitor match, EDID peak decode and truncated block rejection; themed dialog render; dark titlebar; fixed footer; advanced toggles; Xbox guide topic and navigation; compact window render.");}

}

}

