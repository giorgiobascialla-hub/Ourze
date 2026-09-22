using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
namespace HdrPilot {
public sealed class AdvancedHdr {
 public int Output=5,Gamut=0,UiNits=300,ScenePercent=150,Compression=0,Composite=1;
 public Dictionary<string,string> Values(){if(!new[]{3,4,5,6}.Contains(Output)||!new[]{0,2}.Contains(Gamut)||(Output<=4&&Gamut!=2)||(Output>=5&&Gamut!=0))throw new Exception("Formato HDR e spazio colore non coerenti.");if(UiNits<50||UiNits>1000||ScenePercent<25||ScenePercent>300||Compression<0||Compression>1||Composite<0||Composite>1)throw new Exception("Parametro HDR avanzato fuori intervallo.");return new Dictionary<string,string>{{"r.HDR.Display.OutputDevice",Output.ToString()},{"r.HDR.Display.ColorGamut",Gamut.ToString()},{"r.HDR.UI.Luminance",UiNits.ToString()},{"r.HDR.Aces.SceneColorMultiplier",(ScenePercent/100m).ToString(CultureInfo.InvariantCulture)},{"r.HDR.Aces.GamutCompression",Compression.ToString()},{"r.HDR.UI.CompositeMode",Composite.ToString()}};}
}
}
