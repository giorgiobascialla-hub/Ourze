using System;using System.IO;using System.Linq;using System.Collections.Generic;using System.Reflection;using System.Windows;using System.Windows.Controls;using System.Windows.Markup;using System.Windows.Media;using System.Windows.Media.Imaging;using HdrPilot;
class UiTests{
 static int checks;static List<string> report=new List<string>();
 static void Check(bool b,string m){if(!b)throw new Exception(m);checks++;}
 static void Set(object p,string n,object v){typeof(Pilot).GetField(n,BindingFlags.Instance|BindingFlags.NonPublic).SetValue(p,v);}
 static void Call(object p,string n,params object[] args){typeof(Pilot).GetMethod(n,BindingFlags.Instance|BindingFlags.NonPublic).Invoke(p,args);}
 static IEnumerable<DependencyObject> Tree(DependencyObject p){yield return p;for(int i=0;i<VisualTreeHelper.GetChildrenCount(p);i++)foreach(var q in Tree(VisualTreeHelper.GetChild(p,i)))yield return q;}
 static void Render(Window w,string path){w.UpdateLayout();var image=new RenderTargetBitmap((int)w.ActualWidth,(int)w.ActualHeight,96,96,PixelFormats.Pbgra32);image.Render(w);var enc=new PngBitmapEncoder();enc.Frames.Add(BitmapFrame.Create(image));using(var f=File.Create(path))enc.Save(f);}
 [STAThread] static int Main(string[] args){string root=Path.GetFullPath(args[0]);Directory.CreateDirectory(root);try{
 new Application{ShutdownMode=ShutdownMode.OnExplicitShutdown};Core.Data=Path.Combine(root,"data");Core.Init();var asm=typeof(Pilot).Assembly;
 Check(asm.GetType("HdrPilot.SceneOverlay")==null&&asm.GetType("HdrPilot.LiveOverlay")==null&&asm.GetType("HdrPilot.HdrCapture")==null,"overlay classes still in binary");
 string[] languages={"it","en","es","fr","de","pt"};using(var reader=new StreamReader(asm.GetManifestResourceStream("HdrTranslations.tsv"))){while(!reader.EndOfStream){var line=reader.ReadLine();if(line.StartsWith("#")||String.IsNullOrWhiteSpace(line))continue;var cells=line.Split('\t');for(int from=0;from<6;from++)for(int to=0;to<6;to++){string value;Check(HdrTranslations.TryTranslate(cells[from].Replace("{0}","1234").Replace("\\n","\n"),languages[to],out value),"missing translation "+cells[from]);Check(value==cells[to].Replace("{0}","1234").Replace("\\n","\n"),"translation round trip "+value);}}}
 string gameDir=Path.Combine(root,"game");Directory.CreateDirectory(gameDir);var game=new Game{Id="ui-mfg",Name="Test game · long title for layout",Root=gameDir,Exe=Path.Combine(gameDir,"FixtureGame.exe")};File.Copy(asm.Location,game.Exe,true);
 foreach(string lang in languages)foreach(int width in new[]{1180,3440}){
 Core.Pref.Language=lang;Window w;using(var s=asm.GetManifestResourceStream("Main.xaml"))w=(Window)XamlReader.Load(s);w.Width=width;w.Height=width==1180?740:1440;w.Left=-10000;w.Top=-10000;w.ShowActivated=false;
 var p=new Pilot();Set(p,"window",w);Set(p,"busy",true);Call(p,"Wire");w.Show();Set(p,"busy",false);Call(p,"Select",game);Appearance.Refresh();w.UpdateLayout();
 Check(w.FindName("HdrMethod")==null&&w.FindName("Overlay")==null&&w.FindName("Apply")==null,"removed controls remain");
 Render(w,Path.Combine(root,lang+"-"+width+"-home.png"));
 Call(p,"SwitchSection",false);Appearance.Refresh();w.UpdateLayout();var mfg=Tree(w).OfType<Button>().Single(b=>b.Name=="MfgInstall");
 Check(Convert.ToString(mfg.Content)==Appearance.Localize("Install MFG Unlock"),"MFG action translation");var pos=mfg.TransformToAncestor(w).Transform(new Point(0,0));Check(pos.X>=0&&pos.X+mfg.ActualWidth<=w.ActualWidth&&pos.Y>=0&&pos.Y+mfg.ActualHeight<=w.ActualHeight-90,"primary MFG action outside viewport "+lang+" "+width+" "+pos);
 var scroll=(ScrollViewer)w.FindName("HdrScroll");Render(w,Path.Combine(root,lang+"-"+width+"-mods.png"));scroll.ScrollToBottom();w.UpdateLayout();Render(w,Path.Combine(root,lang+"-"+width+"-mods-bottom.png"));
 var guide=Guide.Create(w);guide.ShowActivated=false;guide.Left=-10000;guide.Top=-10000;guide.Show();Appearance.Refresh();guide.UpdateLayout();Check(Tree(guide).OfType<TextBlock>().Any(t=>t.Text==Appearance.Localize("The UE5 HDR editor, engine reader and HDLSS luminance overlay have been removed. Existing game INIs, mods and backups are preserved. HDR is configured through ReShade and a compatible mod; follow its author if engine HDR must be enabled manually.")),"guide migration language");guide.Close();
 Call(p,"SwitchSection",true);Appearance.Refresh();w.UpdateLayout();Render(w,Path.Combine(root,lang+"-"+width+"-dlss.png"));Call(p,"ShowHome");w.Close();report.Add("PASS "+lang+" "+width+": home, mods, MFG action visible, guide, DLSS, navigation");
 }
 report.Add("PASS "+checks+" assertions; 48 WPF renders; no overlay types in binary.");File.WriteAllLines(Path.Combine(root,"results.txt"),report);Console.WriteLine(String.Join("\n",report));return 0;
 }catch(Exception ex){File.WriteAllText(Path.Combine(root,"error.txt"),ex.ToString());Console.WriteLine(ex);return 1;}}
}
