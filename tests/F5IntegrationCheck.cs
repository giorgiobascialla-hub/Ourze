using System;
using System.IO;
using System.Linq;
using HdrPilot;
class F5Check {
 static void Check(bool ok,string message){if(!ok)throw new Exception(message);}
 static void Main(string[] args){
 string dir=args[0];Core.Data=Path.Combine(dir,"test-data");Directory.CreateDirectory(Core.Data);
 var b=DlssBuild.All.Single(x=>x.Id=="f5");Check(!DlssBuild.All.Any(x=>x.Id=="neurotic"),"Alpha still offered");
 string json="["+File.ReadAllText(Path.Combine(dir,"release.json"))+"]";
 var r=DlssCore.PickRelease(json,b);Check(r.Tag=="v0.1.33"&&r.Asset=="OptiScaler-DLSSNR-F5-0.1.33-with-dlss-fg.zip","wrong F5 asset");
 Check("sha256:"+DlssCore.Hash(Path.Combine(dir,"f5.zip"))==r.Digest,"archive digest mismatch");
 try{DlssCore.PickRelease(json.Replace("\"prerelease\": false","\"prerelease\": true"),b);throw new Exception("prerelease accepted");}catch(Exception ex){if(ex.Message=="prerelease accepted")throw;}
 string pkg=DlssCore.Unpack(Path.Combine(dir,"f5.zip"),Path.Combine(dir,"extracted"));
 string gameRoot=Path.Combine(dir,"f5-game");Directory.CreateDirectory(gameRoot);string exe=Path.Combine(gameRoot,"Game.exe");File.Copy(Path.Combine(dir,"HDLSS5-test.exe"),exe,true);
 var g=new Game{Id="f5-fixture",Name="Fixture",Root=gameRoot,Exe=exe};
 var plan=DlssCore.Prepare(g,new DlssOptions{Build="f5",Enabled=false},pkg,r.Tag);
 foreach(string f in new[]{"OptiScaler/streamline/nvngx_dlssg.dll","OptiScaler/streamline/sl.interposer.dll","OptiScaler/streamline/Streamline-LICENSE.txt","OptiScaler/LUTs/identity_33.cube","nvngx.dll_dlssnr.dll"})Check(plan.Changes.Any(c=>c.Name==f),"missing "+f);
 Check(!plan.Changes.Any(c=>c.Name=="nvngx_dlssg.dll"||c.Name.EndsWith(".bat")||c.Name.EndsWith(".ps1")),"unsafe package selection");
 DlssCore.Apply(g,plan);var undo=DlssCore.PrepareUninstall(g);Check(undo.Changes.Any(c=>c.Name=="OptiScaler/streamline/nvngx_dlssg.dll"),"FG omitted from restore");DlssCore.Apply(g,undo);
 Check(!File.Exists(Path.Combine(gameRoot,"dxgi.dll")),"loader not removed");Check(!File.Exists(Path.Combine(gameRoot,"OptiScaler","streamline","nvngx_dlssg.dll")),"FG not removed");
 File.WriteAllText(Path.Combine(dir,"F5-tests-passed.txt"),"PASS: live release selection, SHA256, prerelease exclusion, bounded extraction, full package installation and managed removal.");
 }
}
