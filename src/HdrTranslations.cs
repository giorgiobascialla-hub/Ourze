using System;
using System.Collections.Generic;
using System.IO;
using System.Text.RegularExpressions;
namespace HdrPilot {
// Complete messages, including their placeholders, belong to one language resource.
public static class HdrTranslations {
 sealed class Template { public Regex Pattern; public string[] Text; }
 static readonly Dictionary<string,string[]> Exact=new Dictionary<string,string[]>();
 static readonly List<Template> Templates=new List<Template>();
 static readonly string[] Languages={"it","en","es","fr","de","pt"};
 static HdrTranslations(){using(var r=new StreamReader(typeof(HdrTranslations).Assembly.GetManifestResourceStream("HdrTranslations.tsv"))){while(!r.EndOfStream){var line=r.ReadLine();if(String.IsNullOrWhiteSpace(line)||line.StartsWith("#"))continue;var cells=line.Split('\t');if(cells.Length!=6)throw new InvalidDataException("HDR translation requires six languages.");for(int i=0;i<6;i++){cells[i]=cells[i].Replace("\\n","\n");if(String.IsNullOrWhiteSpace(cells[i]))throw new InvalidDataException("Missing HDR translation.");}foreach(string cell in cells){if(!cell.Contains("{0}")){Exact[cell]=cells;continue;}string pattern=Regex.Escape(cell).Replace("\\{0}","(?<value>.+?)");Templates.Add(new Template{Pattern=new Regex("\\A"+pattern+"\\z",RegexOptions.CultureInvariant),Text=cells});}}}}
 public static bool TryTranslate(string text,string language,out string result){return TryTranslate(text,language,0,out result);}
 static bool TryTranslate(string text,string language,int depth,out string result){result=text;if(String.IsNullOrEmpty(text)||depth>3)return false;int index=Array.IndexOf(Languages,language);if(index<0)index=1;string[] row;if(Exact.TryGetValue(text,out row)){result=row[index];return true;}foreach(var t in Templates){var match=t.Pattern.Match(text);if(!match.Success)continue;string value=match.Groups["value"].Value,translated;if(TryTranslate(value,language,depth+1,out translated))value=translated;result=t.Text[index].Replace("{0}",value);return true;}return false;}
}
}
