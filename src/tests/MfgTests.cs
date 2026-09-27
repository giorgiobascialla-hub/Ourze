using System;
using System.IO;
using System.Linq;
using System.Text;
using System.Collections.Generic;
using HdrPilot;
class Tests {
 static List<string> results=new List<string>();
 static void Check(bool yes,string text){if(!yes)throw new Exception("FAIL: "+text);results.Add("PASS: "+text);}
 static void Reject(Action action,string text){bool rejected=false;try{action();}catch{rejected=true;}Check(rejected,text);}
 [STAThread] static int Main(string[] args){string root=Path.GetFullPath(args[0]);Directory.CreateDirectory(root);try{
 Core.Data=Path.Combine(root,"data");Core.Init();
 MfgAddon.Release release;string source,reshade,origin;
 if(args.Length>1){var data=Core.Json.Deserialize<Dictionary<string,object>>(File.ReadAllText(args[1]));source=Convert.ToString(data["Mfg"]);reshade=Convert.ToString(data["ReShade"]);release=Core.Json.Deserialize<MfgAddon.Release>(Core.Json.Serialize(data["Release"]));origin="Previously verified official download";MfgAddon.Verify(source,release);}
 else{source=MfgAddon.Fetch(out release);reshade=RenoDx.Fetch(false,s=>Console.WriteLine(s),out origin);}
 results.Add("MFG release: "+release.Tag+" "+release.Digest);results.Add("ReShade: "+origin);
 File.WriteAllText(Path.Combine(root,"downloads.json"),Core.Json.Serialize(new {Mfg=source,ReShade=reshade,Release=release}));
 string initial="[ADDON]\r\nLoadFromDllMain=folder/other,,name.addon64,another.addon64\r\n[INPUT]\r\nKeyOverlay=36,0,0,0\r\n[RenoDX.MFGUnlock]\r\nForceMultiplier=3\r\n";
 string merged=MfgAddon.EarlyLoad(initial,true);Check(MfgAddon.Entries(Core.Get(merged,"ADDON","LoadFromDllMain")).SequenceEqual(new[]{"folder/other,name.addon64","another.addon64",MfgAddon.Name}),"preserve escaped commas and existing add-ons");
 Check(MfgAddon.EarlyLoad(merged,true)==merged,"early-load idempotence");Check(MfgAddon.EarlyLoad(merged,false)==initial,"remove only MFG early-load entry");
 Check(MfgAddon.EarlyLoad("[ADDON]\nLoadFromDllMain=C:\\Mods\\RENODX-MFGUNLOCK.ADDON64\n",true).IndexOf(MfgAddon.Name,StringComparison.Ordinal)==-1,"existing path and case respected");
 string bad=Path.Combine(root,"bad.addon64");File.WriteAllText(bad,"not a DLL");Reject(()=>MfgAddon.Verify(bad,release),"bad download rejected");
 var assets=new[]{new Dictionary<string,object>{{"name",MfgAddon.Name},{"browser_download_url",release.Url},{"digest",release.Digest},{"size",release.Size}}};
 var metadata=new Dictionary<string,object>{{"draft",false},{"prerelease",false},{"tag_name",release.Tag},{"assets",assets}};
 Check(MfgAddon.ParseRelease(Core.Json.Serialize(metadata)).Tag==release.Tag,"live-shaped release parsed");metadata["draft"]=true;Reject(()=>MfgAddon.ParseRelease(Core.Json.Serialize(metadata)),"draft rejected");metadata["draft"]=false;metadata["prerelease"]=true;Reject(()=>MfgAddon.ParseRelease(Core.Json.Serialize(metadata)),"prerelease rejected");metadata["prerelease"]=false;assets[0]["browser_download_url"]="https://example.com/"+MfgAddon.Name;Reject(()=>MfgAddon.ParseRelease(Core.Json.Serialize(metadata)),"foreign download rejected");assets[0]["browser_download_url"]=release.Url;assets[0]["digest"]="";Reject(()=>MfgAddon.ParseRelease(Core.Json.Serialize(metadata)),"missing digest rejected");
 string gameRoot=Path.Combine(root,"game");Directory.CreateDirectory(gameRoot);var game=new Game{Id="mfg-fixture",Name="MFG fixture",Root=gameRoot,Exe=Path.Combine(gameRoot,"FixtureGame.exe")};File.Copy(typeof(MfgAddon).Assembly.Location,game.Exe);
 Reject(()=>MfgAddon.Prepare(game,source,release),"missing ReShade rejected");
 File.Copy(reshade,Path.Combine(gameRoot,"dxgi.dll"));File.WriteAllText(Path.Combine(gameRoot,"ReShade.ini"),initial,Encoding.Unicode);byte[] original=File.ReadAllBytes(Path.Combine(gameRoot,"ReShade.ini"));
 File.Copy(source,Path.Combine(gameRoot,"different-mfg.addon64"));Reject(()=>MfgAddon.Prepare(game,source,release),"duplicate MFG rejected");File.Delete(Path.Combine(gameRoot,"different-mfg.addon64"));
 var plan=MfgAddon.Prepare(game,source,release);Check(plan.Changes.All(c=>c.Name==MfgAddon.Name||c.Name=="ReShade.ini"),"no NVIDIA, Streamline or engine INI writes");
 DlssCore.Apply(game,plan);Check(MfgAddon.Installed(game)&&!RenoDx.HasHdrAddon(game),"MFG present, not misidentified as HDR");Check(File.ReadAllBytes(Path.Combine(gameRoot,"ReShade.ini"))[0]==255,"UTF16 preserved");Check(Core.Get(File.ReadAllText(Path.Combine(gameRoot,"ReShade.ini")),"RenoDX.MFGUnlock","ForceMultiplier")=="3","user multiplier preserved");
 Reject(()=>ComponentCleanup.Prepare(game,true),"ReShade cleanup protects MFG dependency");
 string hdr=Path.Combine(root,"renodx-fixture.addon64");File.Copy(source,hdr);Check(RenoDx.InstallAddon(game,hdr).Changes.Count==1,"HDR installer can coexist with MFG");
 DlssCore.Apply(game,MfgAddon.Prepare(game,source,release));Check(DlssCore.ReadManifest(gameRoot,MfgAddon.Marker).Files.Count==2,"repeat update keeps original backup");DlssCore.Apply(game,MfgAddon.Restore(game));Check(!MfgAddon.Installed(game)&&File.ReadAllBytes(Path.Combine(gameRoot,"ReShade.ini")).SequenceEqual(original),"byte-exact initial restore after update");
 DlssCore.Apply(game,MfgAddon.Prepare(game,source,release));File.AppendAllText(Path.Combine(gameRoot,"ReShade.ini"),"\r\n[NEW]\r\nUserChoice=7\r\n",Encoding.Unicode);DlssCore.Apply(game,MfgAddon.Restore(game));string after=File.ReadAllText(Path.Combine(gameRoot,"ReShade.ini"));Check(Core.Get(after,"NEW","UserChoice")=="7"&&!MfgAddon.Entries(Core.Get(after,"ADDON","LoadFromDllMain")).Contains(MfgAddon.Name),"restore preserves subsequent ReShade edits");
 plan=MfgAddon.Prepare(game,source,release);File.AppendAllText(Path.Combine(gameRoot,"ReShade.ini"),"; change after review\r\n",Encoding.Unicode);Reject(()=>DlssCore.Apply(game,plan),"stale preview rejected");Check(!MfgAddon.Installed(game),"stale preview writes no add-on");
 File.WriteAllText(Path.Combine(gameRoot,MfgAddon.Name),"external baseline");byte[] external=File.ReadAllBytes(Path.Combine(gameRoot,MfgAddon.Name));DlssCore.Apply(game,MfgAddon.Prepare(game,source,release));DlssCore.Apply(game,MfgAddon.Restore(game));Check(File.ReadAllBytes(Path.Combine(gameRoot,MfgAddon.Name)).SequenceEqual(external),"external pre-existing file restored");
 File.Delete(Path.Combine(gameRoot,MfgAddon.Name));
 File.WriteAllText(Path.Combine(gameRoot,"ReShade.ini"),"[ADDON]\nAddonPath=mods/addons\n");plan=MfgAddon.Prepare(game,source,release);Check(plan.Changes.Any(c=>c.Name.Replace('\\','/')=="mods/addons/"+MfgAddon.Name),"custom add-on directory respected");DlssCore.Apply(game,plan);Check(MfgAddon.Installed(game),"custom location detected");DlssCore.Apply(game,MfgAddon.Restore(game));Check(!MfgAddon.Installed(game),"custom location restored");
 File.WriteAllText(Path.Combine(gameRoot,"ReShade.ini"),"[ADDON]\nAddonPath=../shared\n");Reject(()=>MfgAddon.Prepare(game,source,release),"external shared add-on folder protected");
 File.WriteAllText(Path.Combine(gameRoot,"ReShade.ini"),"[ADDON]\nDisabledAddons=MFG Unlock@"+MfgAddon.Name+"\n");Reject(()=>MfgAddon.Prepare(game,source,release),"disabled addon reported instead of false activation");
 File.WriteAllText(Path.Combine(gameRoot,"ReShade.ini"),initial);plan=MfgAddon.Prepare(game,source,release);using(var locked=File.Open(Path.Combine(gameRoot,"ReShade.ini"),FileMode.Open,FileAccess.ReadWrite,FileShare.Read)){Reject(()=>DlssCore.Apply(game,plan),"locked INI install rejected");}Check(!MfgAddon.Installed(game),"locked INI causes no partial install");
 DlssCore.Apply(game,MfgAddon.Prepare(game,source,release));File.AppendAllText(Path.Combine(gameRoot,MfgAddon.Name),"modified");Reject(()=>MfgAddon.Restore(game),"changed addon blocks destructive restore");
 File.WriteAllLines(Path.Combine(root,"results.txt"),results);Console.WriteLine(String.Join("\n",results));return 0;
 }catch(Exception ex){results.Add(ex.ToString());File.WriteAllLines(Path.Combine(root,"results.txt"),results);Console.WriteLine(ex);return 1;}}
}
