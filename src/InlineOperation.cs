using System;using System.Threading.Tasks;using System.Windows.Controls;
namespace HdrPilot {
// Short-lived read snapshot used only while constructing a view. Never flows to
// asynchronous installation work: plans and writes read the files again.
public sealed class ViewReads {
 [ThreadStatic]static ViewReads active;
 public static ViewReads Current {get{return active;}}
 readonly System.Collections.Generic.Dictionary<string,object> values=new System.Collections.Generic.Dictionary<string,object>(StringComparer.OrdinalIgnoreCase);
 readonly System.Threading.CancellationToken token;
 static readonly object hardwareGate=new object();static DlssHardware recentHardware;static DateTime hardwareRead;
 public void WarmHardware(){lock(hardwareGate){Check();if(recentHardware==null||(DateTime.UtcNow-hardwareRead).TotalMinutes>=2){recentHardware=DlssHardware.Detect();hardwareRead=DateTime.UtcNow;}values["hardware"]=recentHardware;}}
 public ViewReads(System.Threading.CancellationToken cancellation){token=cancellation;}
 public static void Check(){if(active!=null)active.token.ThrowIfCancellationRequested();}
 public static T Read<T>(string key,Func<T> read){var scope=active;if(scope==null)return read();scope.token.ThrowIfCancellationRequested();object value;if(scope.values.TryGetValue(key,out value))return (T)value;T result=read();scope.values[key]=result;return result;}
 public System.Collections.Generic.List<ModItem> ReadInventory(Game game){using(Enter())return ModInventory.Scan(game);}
 public IDisposable Enter(){var old=active;active=this;return new Exit(()=>active=old);}
 sealed class Exit:IDisposable{Action restore;public Exit(Action action){restore=action;}public void Dispose(){if(restore!=null){restore();restore=null;}}}
}
public static class ButtonProgress {
 static readonly System.Threading.AsyncLocal<Session> current=new System.Threading.AsyncLocal<Session>();static bool started;
 static readonly System.Collections.Generic.Dictionary<Button,Session> sessions=new System.Collections.Generic.Dictionary<Button,Session>();
 public static void Start(){if(started)return;started=true;StatusFeedback.Start();System.Windows.EventManager.RegisterClassHandler(typeof(Button),Button.ClickEvent,new System.Windows.RoutedEventHandler((sender,args)=>{var b=(Button)sender;Session previous;if(sessions.TryGetValue(b,out previous))previous.Stop();var session=new Session(b);current.Value=session;b.Dispatcher.BeginInvoke(System.Windows.Threading.DispatcherPriority.Background,new Action(()=>{if(current.Value==session)current.Value=null;}));}),true);}
 public static void Report(string phase,double percent){var session=current.Value;if(session!=null)session.Report(phase,percent);}
 public static bool Terminal(string phase){return phase=="Operazione completata"||phase=="Operazione interrotta · consulta il registro"||phase=="Download non riuscito";}
 sealed class Session {
  readonly Button button;Overlay overlay;System.Windows.Documents.AdornerLayer layer;System.Windows.Threading.DispatcherTimer timer;bool finished;DateTime last,terminal;
  public Session(Button b){button=b;}
  public void Stop(){if(finished)return;finished=true;if(timer!=null)timer.Stop();button.Unloaded-=Unloaded;if(layer!=null&&overlay!=null)layer.Remove(overlay);Session s;if(sessions.TryGetValue(button,out s)&&s==this)sessions.Remove(button);}
  void Unloaded(object sender,System.Windows.RoutedEventArgs e){Stop();}
  public void Report(string phase,double value){if(finished||button.Dispatcher.HasShutdownStarted)return;button.Dispatcher.BeginInvoke(System.Windows.Threading.DispatcherPriority.Normal,new Action(()=>{
   if(finished)return;if(!button.IsLoaded){Stop();return;}last=DateTime.UtcNow;
   if(overlay==null){layer=System.Windows.Documents.AdornerLayer.GetAdornerLayer(button);if(layer==null)return;Session previous;if(sessions.TryGetValue(button,out previous)&&previous!=this)previous.Stop();sessions[button]=this;overlay=new Overlay(button);layer.Add(overlay);button.Unloaded+=Unloaded;timer=new System.Windows.Threading.DispatcherTimer{Interval=TimeSpan.FromMilliseconds(40)};timer.Tick+=(s,e)=>{
    // Completion must not depend on a button remaining enabled after its state changes.
    if(!button.IsLoaded||(terminal!=default(DateTime)&&(DateTime.UtcNow-terminal).TotalMilliseconds>=650)||(!RenoNr.Busy&&!RenoDxUi.IsBusy&&!ModCenter.Busy&&!InlineOperation.IsBusy&&button.IsEnabled&&(DateTime.UtcNow-last).TotalMilliseconds>1000)){Stop();return;}
    overlay.Tick();layer.Update(button);
   };timer.Start();}
   overlay.Phase=phase;overlay.Percent=value;overlay.Failed=StatusFeedback.IsError(phase);if(Terminal(phase))terminal=DateTime.UtcNow;else terminal=default(DateTime);overlay.InvalidateVisual();
  }));}
 }
 sealed class Overlay:System.Windows.Documents.Adorner {
  public string Phase="";public double Percent=-1;public bool Failed;double displayed,travel;
  public Overlay(System.Windows.UIElement target):base(target){IsHitTestVisible=false;}
  public void Tick(){displayed+=(Math.Max(0,Math.Min(100,Percent))-displayed)*0.25;travel=(travel+0.025)%1;InvalidateVisual();}
  protected override void OnRender(System.Windows.Media.DrawingContext dc){var size=AdornedElement.RenderSize;if(size.Width<1||size.Height<1)return;var rect=new System.Windows.Rect(size);var accent=Failed?System.Windows.Media.Color.FromRgb(255,115,115):System.Windows.Media.Color.FromRgb(240,184,81);dc.PushClip(new System.Windows.Media.RectangleGeometry(rect,6,6));dc.DrawRectangle(new System.Windows.Media.SolidColorBrush(System.Windows.Media.Color.FromRgb(32,43,55)),null,rect);
   double width=Percent<0?size.Width*0.32:size.Width*displayed/100;double x=Percent<0?(size.Width+width)*travel-width:0;
   dc.DrawRectangle(new System.Windows.Media.SolidColorBrush(System.Windows.Media.Color.FromArgb(35,accent.R,accent.G,accent.B)),null,new System.Windows.Rect(x,0,width,size.Height));
   dc.DrawRectangle(new System.Windows.Media.SolidColorBrush(accent),null,new System.Windows.Rect(x,Math.Max(0,size.Height-3),width,3));
   string phase=Phase.Split(new[]{'·'},2)[0].Trim();if(phase.Length>45)phase=phase.Substring(0,42)+"…";
   string label=Appearance.Localize(phase)+(Percent<0?" …":" · "+Math.Min(100,Math.Max(0,Percent)).ToString("0",System.Globalization.CultureInfo.InvariantCulture)+"%");
   var text=new System.Windows.Media.FormattedText(label,System.Globalization.CultureInfo.CurrentUICulture,System.Windows.FlowDirection.LeftToRight,new System.Windows.Media.Typeface("Segoe UI"),12,System.Windows.Media.Brushes.White,System.Windows.Media.VisualTreeHelper.GetDpi(this).PixelsPerDip);text.MaxTextWidth=Math.Max(1,size.Width-16);text.MaxTextHeight=Math.Max(1,size.Height-7);text.Trimming=System.Windows.TextTrimming.CharacterEllipsis;dc.DrawText(text,new System.Windows.Point(8,Math.Max(2,(size.Height-3-text.Height)/2)));dc.Pop();dc.DrawRoundedRectangle(null,new System.Windows.Media.Pen(new System.Windows.Media.SolidColorBrush(accent),1),rect,6,6);
  }
 }
}
public static class StatusFeedback {
 static bool started;
 static readonly System.Runtime.CompilerServices.ConditionalWeakTable<TextBlock,Watch> watches=new System.Runtime.CompilerServices.ConditionalWeakTable<TextBlock,Watch>();
 public static bool IsError(string text){if(String.IsNullOrWhiteSpace(text))return false;string first=text.Split('\n')[0];if(first.Length>550)return false;return System.Text.RegularExpressions.Regex.IsMatch(first,@"(^|[\s:·])(?:errore|error|failed|failure|impossibile|unable|cannot|invalid|non supportat\w*|non compatibil\w*|not supported|unsupported|incompatible|non riuscit\w*|non eseguit\w*|operazione interrotta|nessuna mod\b|nessun pacchetto\b|mod non trovata|backup mancante|no mod\b|nicht unterstützt|nicht kompatibel|nicht gefunden|no compatible|no disponible|non pris en charge|non compatible|não suportad\w*|não compatível)(\b|:)",System.Text.RegularExpressions.RegexOptions.IgnoreCase);}
 public static void Start(){if(started)return;started=true;System.Windows.EventManager.RegisterClassHandler(typeof(TextBlock),System.Windows.FrameworkElement.LoadedEvent,new System.Windows.RoutedEventHandler((sender,e)=>{var text=(TextBlock)sender;watches.GetValue(text,t=>new Watch(t)).Attach();}),true);}
 public static void Error(TextBlock text,string message){var watch=watches.GetValue(text,t=>new Watch(t));if(text.IsLoaded)watch.Attach();watch.Failure=message;text.Text=message;watch.Update();}
 public static void Refresh(TextBlock text){var watch=watches.GetValue(text,t=>new Watch(t));if(text.IsLoaded)watch.Attach();watch.Update();}
 sealed class Watch {
  public string Failure;readonly TextBlock text;bool attached,red;System.Windows.Media.Brush original;
  static readonly System.ComponentModel.DependencyPropertyDescriptor descriptor=System.ComponentModel.DependencyPropertyDescriptor.FromProperty(TextBlock.TextProperty,typeof(TextBlock));
  public Watch(TextBlock t){text=t;}
  public void Attach(){if(attached)return;attached=true;descriptor.AddValueChanged(text,Changed);text.Unloaded+=Unloaded;Update();}
  void Changed(object sender,EventArgs e){Update();}
  void Unloaded(object sender,System.Windows.RoutedEventArgs e){descriptor.RemoveValueChanged(text,Changed);text.Unloaded-=Unloaded;attached=false;}
  public void Update(){if(Failure!=text.Text)Failure=null;bool error=Failure!=null||IsError(text.Text);if(error){if(!red)original=text.Foreground;red=true;text.SetCurrentValue(TextBlock.ForegroundProperty,new System.Windows.Media.SolidColorBrush(Core.Pref.Theme=="light"?System.Windows.Media.Color.FromRgb(180,32,40):System.Windows.Media.Color.FromRgb(255,115,115)));}else if(red){red=false;text.SetCurrentValue(TextBlock.ForegroundProperty,original);}}
 }
}

public static class InlineOperation {
 public static bool IsBusy {get;private set;}
 public static async Task Apply(Game game,DlssPlan plan,TextBlock status,Action changed){
  if(IsBusy){status.Text=Appearance.Localize("An operation is already running.");return;}
  IsBusy=true;
  try{status.Text=Appearance.Localize("Installazione");ActivityLog.Write(plan.ToString());string recovery=await Task.Run(()=>DlssCore.Apply(game,plan));changed();status.Text=Appearance.Localize("Files verified. Recovery copy:")+" "+recovery;ModUpdates.AfterApply(game,plan);}
  catch(Exception ex){StatusFeedback.Error(status,Appearance.Localize(ex.Message));ActivityLog.Write(ex.Message);}
  finally{IsBusy=false;}
 }
}
}
