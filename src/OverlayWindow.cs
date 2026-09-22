using System;
using System.IO;
using System.Windows;
using System.Windows.Interop;
using System.Runtime.InteropServices;
namespace HdrPilot {
public static class OverlayWindow {
 [DllImport("user32.dll")]static extern bool SetWindowPos(IntPtr h,IntPtr after,int x,int y,int cx,int cy,uint flags);
 [DllImport("user32.dll")]public static extern bool IsWindowVisible(IntPtr h);
 public static void Show(Window window){window.Opacity=1;window.ShowActivated=false;window.Show();BringAbove(window);}
 public static void BringAbove(Window window){if(window.IsVisible)SetWindowPos(new WindowInteropHelper(window).Handle,new IntPtr(-1),0,0,0,0,0x13);}
 public static void Toggle(Window window){if(window.IsVisible)window.Hide();else Show(window);}
}
public static class OverlayDiagnostics {
 static readonly object Gate=new object();
 public static string FilePath {get{return Path.Combine(Core.Data,"diagnostics","hdr-scene.log");}}
 public static void Log(string message){try{lock(Gate){Directory.CreateDirectory(Path.GetDirectoryName(FilePath));if(File.Exists(FilePath)&&new FileInfo(FilePath).Length>65536)File.WriteAllText(FilePath,"");File.AppendAllText(FilePath,DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss.fff")+" | "+message+Environment.NewLine);}}catch{}}
 public static string Read(){try{return File.Exists(FilePath)?File.ReadAllText(FilePath):"Nessuna diagnostica disponibile. Apri l’overlay e prova ad attivarlo nel gioco.";}catch(Exception e){return e.Message;}}
}
public sealed class CaptureDpi:IDisposable {
 [DllImport("user32.dll")]static extern IntPtr SetThreadDpiAwarenessContext(IntPtr context);
 IntPtr previous;
 public CaptureDpi(){try{previous=SetThreadDpiAwarenessContext(new IntPtr(-4));}catch(EntryPointNotFoundException){}}
 public void Dispose(){if(previous!=IntPtr.Zero)SetThreadDpiAwarenessContext(previous);}
}
}
