using System;using System.IO;using System.Linq;using HdrPilot;
class Test102 {
 static void Check(bool ok,string name){if(!ok)throw new Exception(name);Console.WriteLine("PASS "+name);}
 static void Reject(Action a,string name){bool blocked=false;try{a();}catch{blocked=true;}Check(blocked,name);}
 static void Main(string[] args){
 string fixture=Path.GetFullPath(args[0]);Directory.CreateDirectory(fixture);Core.Data=Path.Combine(fixture,"data");Directory.CreateDirectory(Core.Data);
 string install=Path.Combine(fixture,"Game"),bin=Path.Combine(install,@"Project\Binaries\Win64");Directory.CreateDirectory(bin);Directory.CreateDirectory(Path.Combine(install,@"Project\Content"));Directory.CreateDirectory(Path.Combine(install,@"Engine\Plugins\DLSS"));
 string exe=Path.Combine(bin,"Fixture102.exe");File.Copy(typeof(Core).Assembly.Location,exe);File.WriteAllText(Path.Combine(install,@"Engine\Plugins\DLSS\nvngx_dlss.dll"),"fixture");
 string other=Path.Combine(fixture,"OtherGame");Directory.CreateDirectory(other);File.WriteAllText(Path.Combine(other,"nvngx_dlssg.dll"),"other");
 var g=Core.ManualGame(exe);Check(g.Root==install,"manual Unreal game root");Check(g.Project=="Project","project name preserved");Check(DlssLibraries.Scan(g).Count==1,"find plugin DLL and exclude other games");
 Check(Core.ManualRoot(Path.Combine(other,"Game.exe"))==other,"standalone root remains bounded");
 Check(Ue4ssInstaller.Check(g)=="","manual Unreal install available");
 string mod=Path.Combine(bin,@"Mods\Existing\main.lua");Directory.CreateDirectory(Path.GetDirectoryName(mod));File.WriteAllText(mod,"preserve");
 var plan=Ue4ssInstaller.Prepare(g,args[1]);Check(plan.Changes.Any(c=>c.Name=="UE4SS.dll"),"runtime included");
 string tx=DlssCore.Apply(g,plan);var state=Telemetry.Inspect(g);Check(state.Ue4ss&&state.Reader&&state.Enabled,"flat runtime and reader detected");Check(File.ReadAllText(mod)=="preserve","existing mod preserved");
 Telemetry.Disable(g);Check(!Telemetry.Inspect(g).Enabled,"reader disable");Telemetry.Enable(g);Check(Telemetry.Inspect(g).Enabled,"reader re-enable");
 DlssCore.Apply(g,DlssCore.Recovery(g,Path.Combine(tx,"transaction.json")));Check(!File.Exists(Path.Combine(bin,"UE4SS.dll"))&&File.Exists(mod),"runtime recovery preserves unrelated mod");
 string nested=Path.Combine(bin,"ue4ss");Directory.CreateDirectory(nested);File.Copy(exe,Path.Combine(nested,"UE4SS.dll"));Check(Telemetry.Inspect(g).Ue4ss&&Telemetry.ModFolder(g).StartsWith(nested),"nested runtime detected");
 File.Copy(exe,Path.Combine(bin,"UE4SS.dll"));Reject(()=>Ue4ssInstaller.RuntimeFolder(g),"duplicate runtime rejected");File.Delete(Path.Combine(bin,"UE4SS.dll"));
 File.WriteAllText(Path.Combine(bin,"override.txt"),"external");Reject(()=>Ue4ssInstaller.RuntimeFolder(g),"custom override preserved");File.Delete(Path.Combine(bin,"override.txt"));File.Delete(Path.Combine(nested,"UE4SS.dll"));
 File.WriteAllText(Path.Combine(bin,"dwmapi.dll"),"foreign loader");Reject(()=>Ue4ssInstaller.Prepare(g,args[1]),"foreign proxy preserved");Check(File.ReadAllText(Path.Combine(bin,"dwmapi.dll"))=="foreign loader","foreign loader unchanged");
 }
}
