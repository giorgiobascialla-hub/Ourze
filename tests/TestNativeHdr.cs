using System;using System.IO;using System.Linq;using HdrPilot;
class TestNativeHdr {
 static void Check(bool b,string name){if(!b)throw new Exception(name);Console.WriteLine("PASS "+name);}
 static void Main(string[] args){string root=Path.GetFullPath(args[0]);Directory.CreateDirectory(root);Core.Data=Path.Combine(root,"data");Directory.CreateDirectory(Core.Data);
 foreach(string format in new[]{"0","3","5"})foreach(bool mod in new[]{false,true}){
 string dir=Path.Combine(root,format+mod);Directory.CreateDirectory(dir);string config=Path.Combine(dir,"Config");Directory.CreateDirectory(config);
 var g=new Game{Id=Path.GetFileName(dir),Name="Fixture",Exe=Path.Combine(dir,"NotRunningFixture.exe"),Root=dir,Config=config,Confirmed=true};
 string original="[SystemSettings]\nr.HDR.Display.OutputDevice="+format+"\nr.HDR.Display.ColorGamut=0\nother.setting=preserve\n";
 File.WriteAllText(Path.Combine(config,"Engine.ini"),original);File.WriteAllText(Path.Combine(config,"GameUserSettings.ini"),"[/Script/Engine.GameUserSettings]\nbUseHDRDisplayOutput=False\n");
 if(mod){File.WriteAllText(Path.Combine(dir,"ReShade.ini"),"user preset");File.WriteAllText(Path.Combine(dir,"renodx.addon64"),"foreign mod");}
 var before=Directory.GetFiles(dir).ToArray();string tx=Core.Apply(g,"/Script/Engine.GameUserSettings",1060,15,100,"-6");
 Check(Directory.GetFiles(dir).SequenceEqual(before),"INI-only, no injected files "+g.Id);
 string ini=File.ReadAllText(Path.Combine(config,"Engine.ini"));Check(Core.Get(ini,"SystemSettings","r.HDR.Display.OutputDevice")== (format=="0"?"5":format),"HDR output active/preserved "+g.Id);
 Check(Core.Get(ini,"SystemSettings","r.HDR.Display.ColorGamut")== (format=="3"?"2":"0"),"matching HDR gamut "+g.Id);
 Check(Core.Get(ini,"SystemSettings","r.HDR.Display.MaxLuminance")=="1060","requested peak saved "+g.Id);
 if(mod)Check(File.ReadAllText(Path.Combine(dir,"ReShade.ini"))=="user preset"&&File.ReadAllText(Path.Combine(dir,"renodx.addon64"))=="foreign mod","existing HDR mod untouched");
 Core.Restore(g,tx);Check(File.ReadAllText(Path.Combine(config,"Engine.ini"))==original,"INI restore "+g.Id);
 }
 }
}

