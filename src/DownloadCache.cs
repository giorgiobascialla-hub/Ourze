using System;using System.IO;using System.Linq;using System.Collections.Generic;
namespace HdrPilot {
public static class DownloadCache {
 static readonly object Gate=new object();
 static readonly string[] Areas={"nr-models","packages","hdr-packages","dll-library","streamline","ue4ss-downloads","reframework-packages","asi-packages","effect-packages","mfg-packages"};
 public static string Get(string area,string key,Func<string,string> create){lock(Gate){
  if(!Areas.Contains(area)||!System.Text.RegularExpressions.Regex.IsMatch(key,"^[a-fA-F0-9]{64}$"))throw new Exception("Invalid cache key.");
  string root=Path.Combine(Core.Data,area,"verified-"+key.ToLowerInvariant()),record=Path.Combine(root,"cache.json");
  try{var entries=Core.Json.Deserialize<Dictionary<string,string>>(File.ReadAllText(record));if(entries.Count>0&&entries.All(x=>File.Exists(DlssCore.Safe(root,x.Key))&&DlssCore.Hash(DlssCore.Safe(root,x.Key))==x.Value)){Directory.SetLastWriteTimeUtc(root,DateTime.UtcNow);return DlssCore.Safe(root,File.ReadAllText(Path.Combine(root,"result.txt")));}}catch{}
  // A fresh staging directory also keeps a failed download out of the reusable cache.
  string staging=Path.Combine(Core.Data,area,Guid.NewGuid().ToString("N"));Directory.CreateDirectory(staging);
  string result=create(staging),relative=Path.GetFullPath(result).Substring(Path.GetFullPath(staging).Length+1);
  DlssCore.Safe(staging,relative);File.WriteAllText(Path.Combine(staging,"result.txt"),relative);
  var hashes=Files(staging).ToDictionary(f=>f.Substring(staging.Length+1),DlssCore.Hash);
  File.WriteAllText(Path.Combine(staging,"cache.json"),Core.Json.Serialize(hashes));
  if(Directory.Exists(root))Remove(root);Directory.Move(staging,root);return DlssCore.Safe(root,relative);
 }}
 static IEnumerable<string> Files(string dir){if((File.GetAttributes(dir)&FileAttributes.ReparsePoint)!=0)throw new IOException("Linked cache directory.");foreach(string f in Directory.GetFiles(dir)){if((File.GetAttributes(f)&FileAttributes.ReparsePoint)!=0)throw new IOException("Linked cache file.");yield return f;}foreach(string d in Directory.GetDirectories(dir))foreach(string f in Files(d))yield return f;}
 static void Remove(string dir){string data=Path.GetFullPath(Core.Data).TrimEnd(Path.DirectorySeparatorChar)+Path.DirectorySeparatorChar;string full=Path.GetFullPath(dir);if(!Areas.Any(a=>full.StartsWith(Path.Combine(data,a)+Path.DirectorySeparatorChar,StringComparison.OrdinalIgnoreCase)))throw new IOException("Outside download cache.");var files=Files(full).ToArray();foreach(string f in files){File.SetAttributes(f,FileAttributes.Normal);File.Delete(f);}foreach(string d in Directory.GetDirectories(full))Remove(d);Directory.Delete(full);}
 // Runs only before the interactive app starts, never while an installation plan uses these files.
 public static long Trim(bool clear){lock(Gate){long removed=0;var candidates=new List<Tuple<string,long,DateTime>>();
  var protectedPaths=Core.Pref.DlssChoices.Values.SelectMany(v=>new[]{v.Package,v.Runtime}).Where(p=>!String.IsNullOrWhiteSpace(p)).Select(Path.GetFullPath).ToArray();
  foreach(string area in Areas){string root=Path.Combine(Core.Data,area);if(!Directory.Exists(root))continue;try{if((File.GetAttributes(root)&FileAttributes.ReparsePoint)!=0)continue;foreach(string dir in Directory.GetDirectories(root)){try{string prefix=Path.GetFullPath(dir)+Path.DirectorySeparatorChar;if(protectedPaths.Any(p=>p.Equals(prefix.TrimEnd(Path.DirectorySeparatorChar),StringComparison.OrdinalIgnoreCase)||p.StartsWith(prefix,StringComparison.OrdinalIgnoreCase)))continue;var files=Files(dir).ToArray();candidates.Add(Tuple.Create(dir,files.Sum(f=>new FileInfo(f).Length),files.Length==0?Directory.GetLastWriteTimeUtc(dir):files.Select(f=>File.GetLastWriteTimeUtc(f)).Max()));}catch{}}}catch{}}
  long total=candidates.Sum(c=>c.Item2);foreach(var c in candidates.OrderBy(c=>c.Item3)){if(!clear&&total<=2L*1024*1024*1024)break;try{Remove(c.Item1);removed+=c.Item2;total-=c.Item2;}catch{}}
  return removed;
 }}
}
}
