using System;
using System.IO;
using System.Linq;
using System.Text;
using System.Globalization;
using System.Text.RegularExpressions;
namespace HdrPilot {
// The limiter changes the HDR image in ReShade. It never changes captured measurements.
public static class PeakLimiter {
 public const string Marker="hdlss-peak-limit.json",Effect="HDLSS_PeakLimit.fx",Technique="HDLSS_PeakLimit@HDLSS_PeakLimit.fx";
 public static string RootGet(string text,string key){return Core.Get("[root]\n"+text,"root",key);}
 static string RootSet(string text,string key,string value){string t=Core.Set("[root]\n"+text,"root",key,value);return t.Substring(t.IndexOf('\n')+1);}
 static string Last(string list){return String.Join(",",(list??"").Split(',').Select(x=>x.Trim()).Where(x=>x.Length>0&&x!=Technique&&x!="HDLSS_PeakLimit").Concat(new[]{Technique}));}
 public static string Shader(int peak){if(peak<200||peak>10000)throw new Exception("HDR peak must be between 200 and 10000 nit.");return @"
// HDLSS final HDR peak limiter. Keep this technique LAST and effects enabled.
// scRGB: 1 = 80 nit. HDR10: ST2084, Rec.2020. Unsupported outputs refuse compilation.
#if BUFFER_COLOR_SPACE != 2 && BUFFER_COLOR_SPACE != 3
#error HDLSS peak limiter requires detected scRGB or HDR10 PQ output.
#endif
texture HDLSSColor : COLOR;
sampler HDLSSSampler { Texture = HDLSSColor; SRGBTexture = false; };
static const float HDLSSLimit = "+(peak*0.997).ToString("0.000",CultureInfo.InvariantCulture)+@"; // quantization headroom
float3 HDLSSDecodePQ(float3 v) {
 float3 p=pow(saturate(v),1.0/78.84375);
 return 10000.0*pow(max(p-0.8359375,0.0)/max(18.8515625-18.6875*p,0.000001),1.0/0.1593017578125);
}
float3 HDLSSEncodePQ(float3 v) {
 float3 p=pow(saturate(v/10000.0),0.1593017578125);
 return pow((0.8359375+18.8515625*p)/(1.0+18.6875*p),78.84375);
}
float4 HDLSSLimitColor(float4 c) {
#if BUFFER_COLOR_SPACE == 2
 float3 n=max(c.rgb*80.0,0.0);
#else
 float3 n=HDLSSDecodePQ(c.rgb);
#endif
 float high=max(n.r,max(n.g,n.b));
 n*=min(1.0,HDLSSLimit/max(high,0.000001));
#if BUFFER_COLOR_SPACE == 2
 return float4(n/80.0,c.a);
#else
 // Quantize downward to 10-bit PQ so output rounding cannot exceed the ceiling.
 return float4(floor(HDLSSEncodePQ(n)*1023.0)/1023.0,c.a);
#endif
}
void HDLSSVS(uint id:SV_VertexID,out float4 position:SV_Position,out float2 uv:TEXCOORD) {
 uv=float2((id<<1)&2,id&2);position=float4(uv*float2(2,-2)+float2(-1,1),0,1);
}
float4 HDLSSPS(float4 position:SV_Position,float2 uv:TEXCOORD):SV_Target { return HDLSSLimitColor(tex2D(HDLSSSampler,uv)); }
technique HDLSS_PeakLimit < ui_label=""HDLSS - final HDR peak limit""; ui_tooltip=""Keep LAST. Unknown/SDR formats are rejected. Verify output using the live meter.""; > {
 pass { VertexShader=HDLSSVS; PixelShader=HDLSSPS; SRGBWriteEnable=false; }
}
";}
 public static DlssPlan Prepare(Game g,int peak,string reshadeSource=null,string origin=null){
  Core.Closed(g);string shader=Shader(peak);var scan=DlssCore.Inspect(g,true,false);if(scan.Blockers.Count>0)throw new Exception(String.Join("\n",scan.Blockers));
  var loaders=RenoDx.Loaders(scan.Folder);if(loaders.Count>1)throw new Exception("Multiple ReShade loaders: resolve them before applying an HDR limit.");
  DlssPlan p;if(loaders.Count==0){if(reshadeSource==null)throw new Exception("The final HDR limiter requires ReShade. No output limit has been applied.");p=RenoDx.InstallReShade(g,reshadeSource,origin??"Official ReShade");}
  else {string marker=DlssCore.Safe(scan.Folder,Marker);p=new DlssPlan{Folder=scan.Folder,MarkerName=Marker,MarkerExpected=File.Exists(marker)?DlssCore.Hash(marker):null,Manifest=File.Exists(marker)?DlssCore.ReadManifest(scan.Folder,Marker):new DlssManifest{Folder=scan.Folder,Game=g.Id,PreviousOwner="Before HDR peak limiter",BackupRoot="hdr-components/"+Guid.NewGuid().ToString("N")}};
   if(loaders[0]=="ReShade64.dll"&&(scan.Proxy==""||Core.Get(scan.Ini,"Plugins","LoadReshade")!="true"))throw new Exception("ReShade64.dll is not linked to OptiScaler. Configure ReShade first.");
  }
  // Use a single component manifest for all files on a fresh ReShade installation.
  string config="ReShade.ini";if(loaders.Count==1&&!File.Exists(DlssCore.Safe(p.Folder,config))&&File.Exists(DlssCore.Safe(p.Folder,Path.ChangeExtension(loaders[0],".ini"))))config=Path.ChangeExtension(loaders[0],".ini");
  string cp=DlssCore.Safe(p.Folder,config),ini=File.Exists(cp)?File.ReadAllText(cp):"";
  string preset=Core.Get(ini,"GENERAL","PresetPath");if(String.IsNullOrWhiteSpace(preset))preset="HDLSS-HDR.ini";
  preset=preset.Trim().Replace('\\','/');while(preset.StartsWith("./"))preset=preset.Substring(2);
  if(Path.IsPathRooted(preset)){string full=Path.GetFullPath(preset),prefix=p.Folder.TrimEnd('\\')+"\\";if(!full.StartsWith(prefix,StringComparison.OrdinalIgnoreCase))throw new Exception("External ReShade preset: select a preset inside the game folder before applying the HDR limit.");preset=full.Substring(prefix.Length);}
  string pp=DlssCore.Safe(p.Folder,preset),pt=File.Exists(pp)?File.ReadAllText(pp):"";
  pt=RootSet(pt,"Techniques",Last(RootGet(pt,"Techniques")));pt=RootSet(pt,"TechniqueSorting",Last(RootGet(pt,"TechniqueSorting")));
  ini=Core.Set(ini,"GENERAL","PresetPath",".\\"+preset.Replace('/','\\'));ini=Core.Set(ini,"GENERAL","NoReloadOnInit","0");
  string entry=".\\reshade-shaders\\HDLSS",paths=Core.Get(ini,"GENERAL","EffectSearchPaths")??".\\reshade-shaders\\Shaders\\**";
  if(!paths.Split(',').Any(x=>String.Equals(x.Trim(),entry,StringComparison.OrdinalIgnoreCase)))paths+=","+entry;
  ini=Core.Set(ini,"GENERAL","EffectSearchPaths",paths);
  Add(p,config,ini);Add(p,preset,pt);Add(p,"reshade-shaders/HDLSS/"+Effect,shader);
  p.Summary="Final HDR peak limit: "+peak+" nit\nReShade must load successfully in HDR scRGB/PQ and this effect must remain enabled and LAST. Later injectors or overlays can still change the image. A small quantization margin keeps shader output below the requested ceiling.\nOriginal preset effects are preserved. All changed files have recovery copies. Verify the actual output with the live meter after restarting the game.";return p;
 }
 static void Add(DlssPlan p,string name,string text){string file=DlssCore.Safe(p.Folder,name);p.Changes.Add(new DlssChange{Name=name,Expected=File.Exists(file)?DlssCore.Hash(file):null,Bytes=new UTF8Encoding(false).GetBytes(text)});}
 public static DlssPlan DownloadAndPrepare(Game g,int peak,Action<string> progress){string source=null,origin=null;if(RenoDx.Loaders(Path.GetDirectoryName(g.Exe)).Count==0)source=RenoDx.Fetch(false,progress,out origin);return Prepare(g,peak,source,origin);}
}
}
