using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;
namespace HdrPilot {
public static class EnglishUi {
 static readonly Dictionary<string,string> entries=new Dictionary<string,string>();
 static EnglishUi(){
  using(var r=new System.IO.StreamReader(typeof(EnglishUi).Assembly.GetManifestResourceStream("English.tsv")))while(!r.EndOfStream){string line=r.ReadLine();int tab=line.IndexOf('\t');if(tab>0)entries[line.Substring(0,tab).Replace("\\n","\n")]=line.Substring(tab+1).Replace("\\n","\n");}
 }
 public static string Translate(string value){if(String.IsNullOrEmpty(value))return value;string found;if(entries.TryGetValue(value,out found))return found;
  if(value.IndexOf('\n')>=0)return String.Join("\n",Array.ConvertAll(value.Split('\n'),Translate));
  foreach(var pair in entries)if(pair.Key.EndsWith(": ")&&value.StartsWith(pair.Key,StringComparison.Ordinal))return pair.Value+value.Substring(pair.Key.Length);
  return value;
 }
}
}
