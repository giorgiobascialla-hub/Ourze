using System;using System.Threading.Tasks;using System.Windows.Controls;
namespace HdrPilot {
public static class InlineOperation {
 public static bool IsBusy {get;private set;}
 public static async Task Apply(Game game,DlssPlan plan,TextBlock status,Action changed){
  if(IsBusy){status.Text=Appearance.Localize("An operation is already running.");return;}
  IsBusy=true;
  try{status.Text=Appearance.Localize("Installing with backup…");ActivityLog.Write(plan.ToString());string recovery=await Task.Run(()=>DlssCore.Apply(game,plan));changed();status.Text=Appearance.Localize("Files verified. Recovery copy:")+" "+recovery;var refresh=ModUpdates.Refresh(new[]{game});}
  catch(Exception ex){status.Text=Appearance.Localize(ex.Message);ActivityLog.Write(ex.Message);}
  finally{IsBusy=false;}
 }
}
}
