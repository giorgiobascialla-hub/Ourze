using System;
using System.IO;
using System.Linq;
using System.Text;
namespace HdrPilot {
public sealed class RenoHdrProfile {
 public string Folder,EngineHash,UserHash;public byte[] Engine,User;
 static string HashOrNull(string p){return File.Exists(p)?DlssCore.Hash(p):null;}
 public void Validate(Game game){if(!String.Equals(Folder,Path.GetFullPath(game.Config??""),StringComparison.OrdinalIgnoreCase)||HashOrNull(DlssCore.Safe(Folder,"Engine.ini"))!=EngineHash||HashOrNull(DlssCore.Safe(Folder,"GameUserSettings.ini"))!=UserHash)throw new Exception("Impostazioni del gioco cambiate dopo l’anteprima. Prepara nuovamente RenoDX.");}
 public void Commit(Game game,string recovery){Validate(game);string backup=Core.WritePair(game,Engine,User);ActivityLog.Write("RenoDX: uscita HDR Unreal preparata; regolazioni dell’immagine nel menu RenoDX. Backup INI: "+backup);try{File.WriteAllText(Path.Combine(recovery,"renodx-config-backup.txt"),backup);}catch(Exception ex){ActivityLog.Write("Riferimento backup INI non salvato nel registro: "+ex.Message);}}
 public static void Attach(Game game,DlssPlan plan){
  if(!game.Confirmed||String.IsNullOrEmpty(game.Config))throw new Exception("Il profilo RenoDX richiede UE5 confermato e la cartella delle impostazioni riconosciuta.");
  string root=Path.GetDirectoryName(game.Exe);if(File.Exists(Path.Combine(root,"AutoHDR.addon64")))throw new Exception("Rimuovi prima l’add-on AutoHDR: RenoDX deve gestire da solo la conversione HDR.");
  string folder=Path.GetFullPath(game.Config),ep=DlssCore.Safe(folder,"Engine.ini"),up=DlssCore.Safe(folder,"GameUserSettings.ini");if(!File.Exists(up))throw new Exception("Avvia una volta il gioco per creare le impostazioni, poi riprova.");string e=File.Exists(ep)?File.ReadAllText(ep):"",u=File.ReadAllText(up);
  foreach(var kv in new[]{new[]{"r.AllowHDR","1"},new[]{"r.HDR.EnableHDROutput","1"},new[]{"r.HDR.Display.OutputDevice","3"},new[]{"r.HDR.Display.ColorGamut","2"},new[]{"r.HDR.UI.CompositeMode","1"}})e=Core.Set(e,"SystemSettings",kv[0],kv[1]);e=Core.Set(e,"/Script/Engine.RendererSettings","r.LUT.UpdateEveryFrame","1");
  var sections=Core.Sections(u);foreach(string section in sections)if(Core.Get(u,section,"bUseHDRDisplayOutput")!=null)u=Core.Set(u,section,"bUseHDRDisplayOutput","True");
  plan.HdrProfile=new RenoHdrProfile{Folder=folder,EngineHash=HashOrNull(ep),UserHash=HashOrNull(up),Engine=new UTF8Encoding(false).GetBytes(e),User=new UTF8Encoding(false).GetBytes(u)};
  var loaders=RenoDx.Loaders(root);if(loaders.Count!=1)throw new Exception("Loader ReShade non univoco.");string config="ReShade.ini",custom=Path.ChangeExtension(loaders[0],".ini");if(!File.Exists(Path.Combine(root,config))&&File.Exists(Path.Combine(root,custom)))config=custom;string path=DlssCore.Safe(root,config),ini=File.Exists(path)?File.ReadAllText(path):"";
  ini=Core.Set(ini,"renodx","Set_Path","0");int selected;if(!Int32.TryParse(Core.Get(ini,"renodx","SelectedProfile"),out selected)||selected<1||selected>3)selected=1;ini=Core.Set(ini,"renodx","SelectedProfile",selected.ToString());ini=Core.Set(ini,"renodx-preset"+selected,"ToneMapType","1");
  plan.Changes.Add(new DlssChange{Name=config,Expected=HashOrNull(path),Bytes=new UTF8Encoding(false).GetBytes(ini)});
  plan.Summary=plan.Summary.Replace("Non vengono cambiati i parametri HDR del motore.","I parametri di uscita HDR del motore vengono preparati dal profilo seguente.");
  plan.Summary+="\nPROFILO HDR AUTOMATICO — RenoDX Unreal Engine Extended\nEngine.ini: abilita uscita HDR10/Rec.2020 e aggiornamento LUT.\nGameUserSettings.ini: abilita le preferenze HDR già presenti.\nReShade: RenoDX attivo, Upgrade Path Off, tone mapper HDR.\nLuminosità, picco e colori si regolano in RenoDX; i valori RenoDX esistenti sono conservati.\nBackup INI separato recuperabile da Backup e dati.\nCartella impostazioni: "+folder+"\nRiavvia il gioco. La scrittura dei file non certifica l’HDR effettivamente attivo.";
 }
}
}
