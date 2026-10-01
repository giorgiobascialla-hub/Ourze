using System;
using System.IO;
using System.Net;
using System.Collections.Generic;
namespace HdrPilot {
// Bundled metadata is a fallback, never evidence of an online version check.
public static class CatalogStore {
 static readonly object Gate=new object();
 static readonly Dictionary<string,DateTime> Checked=new Dictionary<string,DateTime>();
 public static string Local(string resource,Func<string,bool> valid){
  string file=Path.Combine(Core.Data,"catalogs",resource);
  try{if(File.Exists(file)){string json=File.ReadAllText(file);if(valid(json))return json;}}catch(IOException){}catch(UnauthorizedAccessException){}
  using(var stream=typeof(CatalogStore).Assembly.GetManifestResourceStream(resource))using(var reader=new StreamReader(stream)){string json=reader.ReadToEnd();if(!valid(json))throw new InvalidDataException("Invalid bundled catalog: "+resource);return json;}
 }
 public static string Refresh(string resource,string url,Func<string,bool> valid){lock(Gate){
  DateTime when;if(Checked.TryGetValue(resource,out when)&&DateTime.UtcNow-when<TimeSpan.FromMinutes(10))return Local(resource,valid);
  ServicePointManager.SecurityProtocol|=SecurityProtocolType.Tls12;string json;
  using(var web=new DlssCore.DownloadClient()){web.Headers[HttpRequestHeader.UserAgent]="HDLSS/1.0.8";json=web.DownloadString(url);}
  if(!valid(json))throw new InvalidDataException("Invalid online catalog; local catalog preserved.");
  string dir=Path.Combine(Core.Data,"catalogs");Directory.CreateDirectory(dir);string file=Path.Combine(dir,resource),temp=file+"."+Guid.NewGuid().ToString("N")+".tmp";
  try{File.WriteAllText(temp,json);if(File.Exists(file))File.Replace(temp,file,null);else File.Move(temp,file);}finally{if(File.Exists(temp))File.Delete(temp);}
  Checked[resource]=DateTime.UtcNow;return json;
 }}
}
}

