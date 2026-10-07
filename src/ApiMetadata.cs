using System;using System.IO;using System.Net;using System.Security.Cryptography;using System.Text;using System.Collections.Generic;
namespace HdrPilot {
public static class ApiMetadata {
 public sealed class Reply {public int Status;public string Text,Remaining,Reset,Retry;}
 public sealed class Entry {public string Url,Text;public long Checked;}
 static readonly object Gate=new object();
 public static Func<string,Reply> Transport;
 public static Func<DateTime> Clock=()=>DateTime.UtcNow;
 static long Seconds(DateTime time){return (long)(time.ToUniversalTime()-new DateTime(1970,1,1,0,0,0,DateTimeKind.Utc)).TotalSeconds;}
 static string Cache(string url){using(var hash=SHA256.Create())return Path.Combine(Core.Data,"api-cache",BitConverter.ToString(hash.ComputeHash(Encoding.UTF8.GetBytes(url))).Replace("-","")+".json");}
 static string PauseFile {get{return Path.Combine(Core.Data,"api-cache","github-pause.txt");}}
 public static bool Handles(string url){Uri u;return Uri.TryCreate(url,UriKind.Absolute,out u)&&u.Scheme=="https"&&u.Host=="api.github.com";}
 static Reply Fetch(string url){var request=(HttpWebRequest)WebRequest.Create(url);request.UserAgent="HDLSS/1.0.8";request.Accept="application/vnd.github+json";request.Timeout=10000;request.ReadWriteTimeout=10000;HttpWebResponse response;
  try{response=(HttpWebResponse)request.GetResponse();}catch(WebException ex){response=ex.Response as HttpWebResponse;if(response==null)throw;}
  using(response)using(var reader=new StreamReader(response.GetResponseStream()))return new Reply{Status=(int)response.StatusCode,Text=reader.ReadToEnd(),Remaining=response.Headers["x-ratelimit-remaining"],Reset=response.Headers["x-ratelimit-reset"],Retry=response.Headers["retry-after"]};
 }
 public static string Get(string url){if(!Handles(url))throw new ArgumentException("Not a GitHub API URL.");lock(Gate){
  long now=Seconds(Clock());string path=Cache(url);Entry cached=null;
  try{if(File.Exists(path))cached=Core.Json.Deserialize<Entry>(File.ReadAllText(path));}catch{}
  if(cached!=null&&cached.Url==url&&ValidJson(cached.Text)&&now>=cached.Checked&&now-cached.Checked<1800)return cached.Text;
  long pause=0;try{if(File.Exists(PauseFile))Int64.TryParse(File.ReadAllText(PauseFile),out pause);}catch{}
  if(pause>now)throw new IOException(PauseMessage(pause));
  ServicePointManager.SecurityProtocol|=SecurityProtocolType.Tls12;Reply reply=Transport==null?Fetch(url):Transport(url);
  long reset=0,retry=0;Int64.TryParse(reply.Reset,out reset);Int64.TryParse(reply.Retry,out retry);
  bool limited=reply.Remaining=="0"||reply.Status==429||((reply.Status==403)&&(retry>0||(reply.Text??"").IndexOf("rate limit",StringComparison.OrdinalIgnoreCase)>=0));
  if(limited){pause=Math.Max(reply.Remaining=="0"&&reset>now?reset:now+60,now+Math.Min(Math.Max(0,retry),31536000));Directory.CreateDirectory(Path.GetDirectoryName(PauseFile));File.WriteAllText(PauseFile,pause.ToString(System.Globalization.CultureInfo.InvariantCulture));}
  if(reply.Status!=200){if(limited)throw new IOException(PauseMessage(pause));throw new IOException("GitHub HTTP "+reply.Status+". "+Appearance.Localize("Version check unavailable"));}
  Core.Json.DeserializeObject(reply.Text);Directory.CreateDirectory(Path.GetDirectoryName(path));string tmp=path+".tmp";File.WriteAllText(tmp,Core.Json.Serialize(new Entry{Url=url,Text=reply.Text,Checked=now}));if(File.Exists(path))File.Replace(tmp,path,null);else File.Move(tmp,path);return reply.Text;
 }}
 static bool ValidJson(string text){try{return !String.IsNullOrWhiteSpace(text)&&Core.Json.DeserializeObject(text)!=null;}catch{return false;}}
 static string PauseMessage(long epoch){return Appearance.Localize("GitHub request limit reached. Try again after {0}.").Replace("{0}",new DateTime(1970,1,1,0,0,0,DateTimeKind.Utc).AddSeconds(epoch).ToLocalTime().ToString("HH:mm"));}
}
}
