using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using Microsoft.Win32;
namespace HdrPilot {
public class DisplayInfo {
 public string Id {get;set;} public string Name {get;set;} public string Device {get;set;} public bool Primary; public int Peak; public bool Hdr;
 public bool Alienware {get{return String.Equals(Name.Trim(),"Dell AW3423DW",StringComparison.OrdinalIgnoreCase)||String.Equals(Name.Trim(),"AW3423DW",StringComparison.OrdinalIgnoreCase)||String.Equals(Name.Trim(),"Alienware AW3423DW",StringComparison.OrdinalIgnoreCase);}}
 public override string ToString(){return Name+(Primary?" · principale":"");}
}
public static class Displays {
 [StructLayout(LayoutKind.Sequential,CharSet=CharSet.Unicode)]struct DD {public int cb;[MarshalAs(UnmanagedType.ByValTStr,SizeConst=32)]public string name;[MarshalAs(UnmanagedType.ByValTStr,SizeConst=128)]public string text;public uint flags;[MarshalAs(UnmanagedType.ByValTStr,SizeConst=128)]public string id;[MarshalAs(UnmanagedType.ByValTStr,SizeConst=128)]public string key;}
 [DllImport("user32.dll",CharSet=CharSet.Unicode)]static extern bool EnumDisplayDevices(string device,uint index,ref DD data,uint flags);
 static DD New(){return new DD{cb=Marshal.SizeOf(typeof(DD))};}
 public static List<DisplayInfo> Scan(){var list=new List<DisplayInfo>();for(uint a=0;a<32;a++){var adapter=New();if(!EnumDisplayDevices(null,a,ref adapter,0))break;if((adapter.flags&1)==0)continue;for(uint m=0;m<16;m++){var d=New();if(!EnumDisplayDevices(adapter.name,m,ref d,1))break;if((d.flags&1)==0)continue;var info=new DisplayInfo{Id=d.id,Name=d.text,Device=adapter.name,Primary=(adapter.flags&4)!=0};try{string[] parts=d.id.Split('#');if(parts.Length>=3)using(var k=Registry.LocalMachine.OpenSubKey(@"SYSTEM\CurrentControlSet\Enum\DISPLAY\"+parts[1]+"\\"+parts[2]+@"\Device Parameters")){if(k!=null)ReadEdid(info,k.GetValue("EDID") as byte[]);}}catch{}list.Add(info);}}return list.OrderByDescending(d=>d.Primary).ToList();}
 public static void ReadEdid(DisplayInfo info,byte[] b){if(b==null||b.Length<128||b[0]!=0||b[1]!=255)return;for(int o=54;o+18<=126;o+=18){if(b[o]==0&&b[o+1]==0&&b[o+3]==252){string name=Encoding.ASCII.GetString(b,o+5,13).Trim('\0','\n','\r',' ');if(name.Length>0)info.Name=name;}}
 for(int block=1;block<=b[126]&&(block+1)*128<=b.Length;block++){int start=block*128;if(b[start]!=2)continue;int end=b[start+2];if(end<4||end>127)continue;for(int p=start+4;p<start+end;){int len=b[p]&31,tag=b[p]>>5;if(p+len>=start+end)break;if(tag==7&&len>=3&&b[p+1]==6){info.Hdr=(b[p+2]&4)!=0;if(len>=4&&b[p+4]!=0){int value=(int)Math.Round(50*Math.Pow(2,b[p+4]/32.0));if(value>=200&&value<=10000)info.Peak=value;}}p+=len+1;}}}
}
}

