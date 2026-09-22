using System;
using System.Diagnostics;
using System.IO;
using System.Runtime.InteropServices;
using System.Text.RegularExpressions;
namespace HdrPilot {
public sealed class DlssHardware {
 public string Name="GPU non rilevata",Driver="non rilevato";public int Series;
 public static DlssHardware Detect(){var result=new DlssHardware();try{string path=Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.System),"nvidia-smi.exe");if(File.Exists(path)){var psi=new ProcessStartInfo(path,"--query-gpu=name,driver_version --format=csv,noheader"){UseShellExecute=false,CreateNoWindow=true,RedirectStandardOutput=true,RedirectStandardError=true};using(var p=Process.Start(psi)){var text=p.StandardOutput.ReadToEndAsync();var err=p.StandardError.ReadToEndAsync();if(!p.WaitForExit(5000)){p.Kill();return result;}foreach(string line in text.Result.Split('\n')){var m=Regex.Match(line,@"^(.+RTX\s+(40|50)\d\d[^,]*),\s*([0-9.]+)");if(m.Success){result.Name=m.Groups[1].Value.Trim();result.Series=Int32.Parse(m.Groups[2].Value);result.Driver=m.Groups[3].Value;break;}}}}}catch{}return result;}
 public string Advice(){Version driver;string extra="";if(Version.TryParse(Driver,out driver)&&driver<new Version(616,56))extra="\nDriver sotto il minimo 616.56 indicato dalla build NR: aggiorna prima di attivarla.";else extra="\n616.56 è il requisito minimo documentato, non una garanzia per ogni build/gioco. Verifica le note della release; non cambiamo il driver.";return Name+" · driver "+Driver+"\n"+(Series==40?"RTX 4000: usa il runtime NR di compatibilità per Ada.":Series==50?"RTX 5000: usa il runtime NR originale NVIDIA.":"Serie GPU non verificata: seleziona RTX 4000 o 5000 nelle opzioni.")+extra;}
 public void Validate(int series,string runtime){if(series!=40&&series!=50)throw new Exception("Scegli la serie NVIDIA RTX 4000 o 5000.");if(Series!=0&&Series!=series)throw new Exception("La serie selezionata non corrisponde alla GPU rilevata.");Version driver;if(Version.TryParse(Driver,out driver)&&driver<new Version(616,56))throw new Exception("Driver "+Driver+": la build NR richiede almeno 616.56.");if(File.Exists(runtime)){string hash=DlssCore.Hash(runtime);if(series==40&&hash=="e16bcf15e16e13f527491cdf7845b2fe6521a738d8f7c9c721866a8496e1fc8e")throw new Exception("Questo è il runtime NR 310.8 originale per RTX 5000: scegli quello di compatibilità per RTX 4000.");}}
}
}
