using System;using System.IO;using System.Linq;using System.Text;using System.Collections.Generic;
namespace HdrPilot {
// Remove only activation of the retired HDLSS shader, not the user's ReShade installation.
public static class LegacyHdr {
 public static string Strip(string list){return String.Join(",",(list??"").Split(',').Where(t=>!String.Equals(t.Trim(),"HDLSS_PeakLimit",StringComparison.OrdinalIgnoreCase)&&!String.Equals(t.Trim(),"HDLSS_PeakLimit@HDLSS_PeakLimit.fx",StringComparison.OrdinalIgnoreCase)));}
 static string Get(string text,string key){return Core.Get("[root]\n"+text,"root",key);}
 static string Set(string text,string key,string value){string s=Core.Set("[root]\n"+text,"root",key,value);return s.Substring(s.IndexOf('\n')+1);}
 public static DlssPlan Prepare(Game g){
  if(g==null||String.IsNullOrWhiteSpace(g.Exe))return new DlssPlan();
  string root=Path.GetDirectoryName(Path.GetFullPath(g.Exe));var p=new DlssPlan{Folder=root,MarkerName="Retired HDLSS HDR filter",Summary="Disable only the retired HDLSS_PeakLimit technique. Preserve other effects, ReShade, RenoDX and DLSS. The previous preset is saved in transaction recovery."};
  if(!File.Exists(DlssCore.Safe(root,"hdlss-peak-limit.json"))&&!File.Exists(DlssCore.Safe(root,"reshade-shaders/HDLSS/HDLSS_PeakLimit.fx")))return p;
  var paths=new HashSet<string>(StringComparer.OrdinalIgnoreCase){"HDLSS-HDR.ini"};
  foreach(string config in new[]{"ReShade.ini","dxgi.ini","d3d11.ini","ReShade64.ini"}){string c=DlssCore.Safe(root,config);if(!File.Exists(c))continue;string ini=File.ReadAllText(c);foreach(string key in new[]{"PresetPath","StartupPresetPath"}){string path=Core.Get(ini,"GENERAL",key);if(String.IsNullOrWhiteSpace(path))continue;path=path.Trim().Replace('\\','/');while(path.StartsWith("./"))path=path.Substring(2);if(Path.IsPathRooted(path)){if(!Core.Under(path,root))throw new Exception("The retired HDLSS filter uses an external preset. Disable HDLSS_PeakLimit in that preset before applying native HDR.");path=Path.GetFullPath(path).Substring(root.TrimEnd('\\').Length+1);}DlssCore.Safe(root,path);paths.Add(path);}}
  foreach(string rel in paths){string path=DlssCore.Safe(root,rel);if(!File.Exists(path))continue;string before=File.ReadAllText(path),after=before;foreach(string key in new[]{"Techniques","TechniqueSorting"}){string list=Get(after,key);if(list!=null&&Strip(list)!=list)after=Set(after,key,Strip(list));}if(after!=before)p.Changes.Add(new DlssChange{Name=rel,Expected=DlssCore.Hash(path),Bytes=new UTF8Encoding(false).GetBytes(after)});}
  return p;
 }
 public static string Disable(Game g){var p=Prepare(g);return p.Changes.Count==0?null:DlssCore.Apply(g,p);}
}
}
