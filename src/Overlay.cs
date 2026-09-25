using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Interop;
using System.Windows.Media;
using System.Windows.Shapes;
using System.Windows.Threading;

namespace HdrPilot {
public class Snapshot {
 public Dictionary<string,double> Values=new Dictionary<string,double>();public long Time;public long Sequence;
 public static Snapshot Parse(string text){var s=new Snapshot();var lines=text.Split('\n').Select(x=>x.Trim()).ToArray();if(!lines.Contains("HDRPILOT=1")||!lines.Contains("complete=1"))return null;foreach(string line in lines){int i=line.IndexOf('=');if(i<0)continue;string key=line.Substring(0,i),val=line.Substring(i+1);long l;double d;if(key=="time"&&Int64.TryParse(val,out l))s.Time=l;else if(key=="sequence"&&Int64.TryParse(val,out l))s.Sequence=l;else if(Double.TryParse(val,NumberStyles.Float,CultureInfo.InvariantCulture,out d)&&!Double.IsNaN(d)&&!Double.IsInfinity(d))s.Values[key]=d;}return s.Time>0?s:null;}
 public bool Fresh(){double now=(DateTime.UtcNow-new DateTime(1970,1,1)).TotalSeconds;return now-Time>=-2&&now-Time<=4;}
}
public sealed class ReaderState {
 public bool Ue4ss,Reader,Enabled,Listed,Fresh;public string Folder,Error="",Last="";
 public string Description {get{return Error!=""?Error:(Ue4ss?"UE4SS presente":"UE4SS non rilevato")+"\n"+(Reader?"Lettore HDLSS già installato (HDRPilotTelemetry)":"Lettore HDLSS non installato")+"\n"+(Listed?"Voce presente in mods.txt: controlla anche questa abilitazione.":Enabled?"Abilitazione configurata con enabled.txt":"Nessun file enabled.txt presente")+"\n"+(Fresh?"Dati recenti ricevuti dal lettore":"Nessun dato recente ricevuto")+(Last==""?"":"\nUltimo file di lettura: "+Last)+"\n"+(Folder??"");}}
}
public static class Telemetry {
 public static string ModFolder(Game g){return System.IO.Path.Combine(Ue4ssInstaller.RuntimeFolder(g),@"Mods\HDRPilotTelemetry");}
 public static ReaderState Inspect(Game g){var state=new ReaderState();if(g==null||String.IsNullOrEmpty(g.Exe)){state.Error="Seleziona un gioco e un target.exe valido.";return state;}try{state.Folder=ModFolder(g);string ue=System.IO.Path.GetDirectoryName(System.IO.Path.GetDirectoryName(state.Folder));state.Ue4ss=File.Exists(System.IO.Path.Combine(ue,"UE4SS.dll"));state.Reader=File.Exists(System.IO.Path.Combine(state.Folder,@"Scripts\main.lua"));state.Enabled=File.Exists(System.IO.Path.Combine(state.Folder,"enabled.txt"));string list=System.IO.Path.Combine(ue,@"Mods\mods.txt");state.Listed=File.Exists(list)&&System.Text.RegularExpressions.Regex.IsMatch(File.ReadAllText(list),@"(?im)^\s*HDRPilotTelemetry\s*:");string path=System.IO.Path.Combine(state.Folder,"telemetry.txt");if(File.Exists(path)){state.Last=File.GetLastWriteTime(path).ToString("g");using(var f=File.Open(path,FileMode.Open,FileAccess.Read,FileShare.ReadWrite|FileShare.Delete))using(var r=new StreamReader(f)){var snap=Snapshot.Parse(r.ReadToEnd());state.Fresh=snap!=null&&snap.Fresh();}}}catch(Exception ex){state.Error="Stato lettore non verificabile: "+ex.Message;}return state;}
 public static string Payload(){using(var s=Assembly.GetExecutingAssembly().GetManifestResourceStream("telemetry.lua"))using(var r=new StreamReader(s))return r.ReadToEnd();}
 public static void Install(Game g){Core.Closed(g);string ue=Ue4ssInstaller.RuntimeFolder(g);if(!File.Exists(System.IO.Path.Combine(ue,"UE4SS.dll")))throw new Exception("Installa prima una versione di UE4SS compatibile con questo gioco. HDLSS aggiunge solo il lettore di parametri; non sostituisce UE4SS o altre mod.");string folder=ModFolder(g),script=System.IO.Path.Combine(folder,@"Scripts\main.lua");if(Directory.Exists(folder))throw new Exception("Il modulo HDRPilotTelemetry è già presente. Nessun file sovrascritto.");Directory.CreateDirectory(System.IO.Path.GetDirectoryName(script));File.WriteAllText(script,Payload(),new UTF8Encoding(false));File.WriteAllText(System.IO.Path.Combine(folder,"enabled.txt"),"");}
 public static void Disable(Game g){Core.Closed(g);string f=System.IO.Path.Combine(ModFolder(g),"enabled.txt"),target=System.IO.Path.Combine(ModFolder(g),"disabled-by-hdr-pilot.txt");if(!File.Exists(f))return;if(File.Exists(target))throw new Exception("Il lettore risulta già disattivato.");File.Move(f,target);}
 public static void Enable(Game g){Core.Closed(g);string folder=ModFolder(g),f=System.IO.Path.Combine(folder,"enabled.txt"),disabled=System.IO.Path.Combine(folder,"disabled-by-hdr-pilot.txt");if(File.Exists(f))return;if(File.Exists(disabled)){File.Move(disabled,f);return;}Install(g);}
}
}
