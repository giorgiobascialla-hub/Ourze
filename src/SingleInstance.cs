using System;
using System.Diagnostics;
using System.Threading;
using System.Windows;
namespace HdrPilot {
public static class SingleInstance {
 static Mutex instance;
 const string Name="Local\\HDRUnlock.Desktop.Instance";
 public static bool Enter(){
  bool created;instance=new Mutex(false,Name,out created);
  if(!created){instance.Close();instance=null;Notify();return false;}
  int current=Process.GetCurrentProcess().Id;
  foreach(string name in new[]{"HDLSS","HDR-Unlock","HDR-Pilot"})foreach(var p in Process.GetProcessesByName(name))using(p){
   if(p.Id==current)continue;
   try{if(p.HasExited)continue;}catch{continue;}
   Leave();Notify();return false;
  }
  return true;
 }
 static void Notify(){MessageBox.Show("HDLSS è già aperto (anche una vecchia copia può usare F11). Chiudi la copia precedente prima di avviare questa. Impostazioni e backup restano conservati.","HDLSS · già in esecuzione");}
 public static void Leave(){if(instance!=null){instance.Close();instance=null;}}
 public static void Test(string file){string name=Name+".Test."+Guid.NewGuid();bool first,second,third;using(var a=new Mutex(false,name,out first)){using(var b=new Mutex(false,name,out second)){if(!first||second)throw new Exception("Duplicate instance guard failure");}}using(var c=new Mutex(false,name,out third)){if(!third)throw new Exception("Instance guard not released");}System.IO.File.WriteAllText(file,"PASS: duplicate instance detection and release on exit.");}
}
}
