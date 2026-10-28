using System;using System.IO;using System.Linq;using System.Windows;using System.Windows.Controls;using System.Windows.Markup;using System.Windows.Media;using System.Windows.Media.Imaging;using System.Windows.Threading;using HdrPilot;
class UiCheck {
 static void Alignment(UIElement content) {
  var root=(StackPanel)content;
  var walk=typeof(CardAlignment).GetMethod("Walk",System.Reflection.BindingFlags.NonPublic|System.Reflection.BindingFlags.Static);
  var match=typeof(CardAlignment).GetMethod("Matches",System.Reflection.BindingFlags.NonPublic|System.Reflection.BindingFlags.Static);
  foreach(var grid in root.Children.OfType<Grid>().Concat(root.Children.OfType<Expander>().Select(e=>e.Content).OfType<Grid>())) {
   if(!grid.IsVisible)continue;
   foreach(var row in grid.Children.OfType<Border>().GroupBy(c=>Grid.GetRow(c)))
    for(int phase=0;phase<2;phase++){
     var positions=row.Select(c=>((System.Collections.Generic.IEnumerable<FrameworkElement>)walk.Invoke(null,new object[]{c})).OfType<Button>().FirstOrDefault(b=>b.IsVisible&&(bool)match.Invoke(null,new object[]{b,phase}))).Where(b=>b!=null).Select(b=>b.TranslatePoint(new Point(),grid).Y).ToArray();
     if(positions.Length>1&&positions.Max()-positions.Min()>1)throw new Exception("Misaligned actions: "+string.Join(",",positions));
    }
  }
 }
 static void Pump(){var f=new DispatcherFrame();var t=new DispatcherTimer{Interval=TimeSpan.FromMilliseconds(250)};t.Tick+=(s,e)=>{t.Stop();f.Continue=false;};t.Start();Dispatcher.PushFrame(f);}
 static void Snap(Window w,string path){w.UpdateLayout();var b=new RenderTargetBitmap((int)w.ActualWidth,(int)w.ActualHeight,96,96,PixelFormats.Pbgra32);b.Render(w);var p=new PngBitmapEncoder();p.Frames.Add(BitmapFrame.Create(b));using(var stream=File.Create(path))p.Save(stream);}
 [STAThread]static int Main(string[] a){try{string root=a[0];Directory.CreateDirectory(root);Core.Data=Path.Combine(root,"data");Directory.CreateDirectory(Core.Data);string gameRoot=Path.Combine(root,"game");Directory.CreateDirectory(gameRoot);string exe=Path.Combine(gameRoot,"Game.exe");File.Copy(typeof(DlssCore).Assembly.Location,exe,true);var g=new Game{Id="uniform",Name="Gioco di prova",Root=gameRoot,Exe=exe};Core.Pref.Language="it";new Application();Appearance.Start();System.Threading.SynchronizationContext.SetSynchronizationContext(new DispatcherSynchronizationContext());ApiMetadata.Transport=u=>{throw new IOException("Offline UI test");};
 Window owner;using(var f=File.OpenRead(a[1]))owner=(Window)XamlReader.Load(f);owner.Left=-10000;owner.ShowActivated=false;owner.Show();
 var item=new ModItem{Game=g,Id="asi",Name="ASI Loader",Version="9.7.4"};var check=new ModItem{Latest="v10.0.0",Status="Update available",UpdateAvailable=true};if(!UpdatePresentation.Summary(item,check).Contains("9.7.4")||!UpdatePresentation.Summary(item,check).Contains("v10.0.0"))throw new Exception("Version status incomplete");if(Dialogs.Text("body",11).FontSize!=14||Dialogs.Text("heading",17).FontSize!=18)throw new Exception("Typography mismatch");
 File.Copy(typeof(DlssCore).Assembly.Location,Path.Combine(gameRoot,MfgAddon.Name),true);var mfg=ModInventory.Scan(g).First(i=>i.Id=="mfg");mfg.Latest="v2.0.0";mfg.Status="Update available";mfg.UpdateAvailable=true;ModUpdates.Items.Add(mfg);NavigationSession.Track("reshade-optional",new Expander{IsExpanded=true}).RaiseEvent(new RoutedEventArgs(Expander.ExpandedEvent));var w=RenoDxUi.Create(owner,g,true,true);var border=(Border)w.Content;var content=border.Child;border.Child=null;border.Child=new ScrollViewer{Content=content,VerticalScrollBarVisibility=ScrollBarVisibility.Auto};w.SizeToContent=SizeToContent.Manual;w.Width=1440;w.Height=810;w.Left=-10000;w.ShowActivated=false;w.Show();Pump();Pump();Alignment(content);Snap(w,Path.Combine(root,"reshade.png"));((ScrollViewer)border.Child).ScrollToVerticalOffset(580);Pump();Snap(w,Path.Combine(root,"optional.png"));w.Width=1920;w.Height=1080;((ScrollViewer)border.Child).ScrollToTop();Pump();Pump();Alignment(content);Snap(w,Path.Combine(root,"wide.png"));w.Width=960;Pump();Pump();Alignment(content);Snap(w,Path.Combine(root,"narrow.png"));w.Close();
 StackPanel body;var dll=Dialogs.Create(owner,"DLL e Streamline",out body);dll.SizeToContent=SizeToContent.Manual;dll.Width=1440;dll.Height=810;dll.Left=-10000;dll.ShowActivated=false;var host=new LibraryPanel(dll,g,()=>40,()=>{});body.Children.Add(new ScrollViewer{Content=host.View,Height=650});dll.Show();Pump();Snap(dll,Path.Combine(root,"dll.png"));dll.Close(); foreach(string language in new[]{"it","en","es","fr","de","pt"}){
  Core.Pref.Language=language;var guide=Guide.Create(owner);guide.Left=-10000;guide.ShowActivated=false;guide.Show();Pump();
  var gb=(StackPanel)((Border)guide.Content).Child;var gl=gb.Children.OfType<Grid>().Single();var gm=(StackPanel)((ScrollViewer)gl.Children[0]).Content;var gc=(StackPanel)((ScrollViewer)gl.Children[1]).Content;
  var labelPositions=gm.Children.OfType<Button>().Select(b=>((TextBlock)b.Content).TranslatePoint(new Point(),gm).X).ToArray();if(labelPositions.Max()-labelPositions.Min()>0.5)throw new Exception("Guide menu labels not aligned");if(gm.Children.Count!=15)throw new Exception("Guide chapter count");
  for(int i=0;i<15;i++){var button=(Button)gm.Children[i];button.RaiseEvent(new RoutedEventArgs(Button.ClickEvent));guide.UpdateLayout();if(!((TextBlock)gc.Children[0]).Text.StartsWith((i+1).ToString("00")))throw new Exception("Guide chapter navigation");if(gc.Children.OfType<TextBlock>().Any(t=>String.IsNullOrWhiteSpace(t.Text)))throw new Exception("Empty guide text");}
  ((Button)gm.Children[0]).RaiseEvent(new RoutedEventArgs(Button.ClickEvent));if(language=="it")Snap(guide,Path.Combine(root,"guide.png"));guide.Close();
 }
 Core.Pref.Language="it";
 var probe=new Button{Content="Aggiorna",IsEnabled=false};UpdatePresentation.ButtonStyle(probe,owner,false);probe.ApplyTemplate();
 if(probe.Opacity>0.51||((SolidColorBrush)probe.Background).Color.ToString()!="#FF1B222C")throw new Exception("Disabled update is not dimmed");
 probe.IsEnabled=true;UpdatePresentation.ButtonStyle(probe,owner,true);
 if(((SolidColorBrush)probe.Background).Color.ToString()!="#FFA82685"||probe.Opacity!=1)throw new Exception("Available update is not magenta");
 probe.IsEnabled=false;if(probe.Opacity>0.51)throw new Exception("Busy update is not dimmed");owner.Close();Console.WriteLine("PASS UI construction, shared status, fonts and 16:9 rendering");return 0;}catch(Exception e){Console.WriteLine(e);return 1;}}
}