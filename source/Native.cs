using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading;
using System.Web.Script.Serialization;
using System.Windows;
using System.Windows.Input;
using System.Windows.Interop;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Threading;

namespace AstralCodex {
public sealed class Config {
    public Config() {
        VideoHoldAt = 7.5;
        VideoTransitionStart = 8.25;
        VideoTransitionEnd = 9.9;
    }
    public double VideoHoldAt { get; set; }
    public double VideoTransitionStart { get; set; }
    public double VideoTransitionEnd { get; set; }
    public string AppLaunch { get; set; }
    public string ProcessPathContains { get; set; }
    public string[] ProcessNames { get; set; }
    public double Volume { get; set; }
    public double IntroSeconds { get; set; }
    public double TransitionSeconds { get; set; }
    public double MaxWaitSeconds { get; set; }
}
 [StructLayout(LayoutKind.Sequential)]
 public struct Bounds {
  public int Left,Top,Right,Bottom;
  public int Width { get { return Right-Left; } }
  public int Height { get { return Bottom-Top; } }
  public Bounds(int x,int y,int w,int h) { Left=x;Top=y;Right=x+w;Bottom=y+h; }
 }
 public static class Geometry {
  public static double Clamp(double x) { return Math.Max(0,Math.Min(1,x)); }
  public static double Ease(double x) { x=Clamp(x);return x*x*(3-2*x); }
  public static Bounds Lerp(Bounds a,Bounds b,double t) {
   return new Bounds((int)Math.Round(a.Left+(b.Left-a.Left)*t),(int)Math.Round(a.Top+(b.Top-a.Top)*t),
    (int)Math.Round(a.Width+(b.Width-a.Width)*t),(int)Math.Round(a.Height+(b.Height-a.Height)*t));
  }
 }
 [ComImport,Guid("2e941141-7f97-4756-ba1d-9decde894a3d"),InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
 internal interface IApplicationActivationManager {
  [PreserveSig]
  int ActivateApplication([MarshalAs(UnmanagedType.LPWStr)] string appUserModelId,
   [MarshalAs(UnmanagedType.LPWStr)] string arguments,uint options,out uint processId);
 }
 internal static class Native {
  public delegate bool EnumWindowsProc(IntPtr h,IntPtr l);
  [StructLayout(LayoutKind.Sequential)] public struct Point { public int X,Y; }
  [StructLayout(LayoutKind.Sequential)] public struct ThumbProperties {
   public uint Flags; public Bounds Destination; public Bounds Source;
   public byte Opacity;
   [MarshalAs(UnmanagedType.Bool)] public bool Visible;
   [MarshalAs(UnmanagedType.Bool)] public bool ClientOnly;
  }
  [DllImport("user32.dll")] public static extern bool EnumWindows(EnumWindowsProc cb,IntPtr param);
  [DllImport("user32.dll")] public static extern bool IsWindowVisible(IntPtr h);
  [DllImport("user32.dll")] public static extern bool IsWindow(IntPtr h);
  [DllImport("user32.dll")] public static extern bool IsIconic(IntPtr h);
  [DllImport("user32.dll")] public static extern bool IsZoomed(IntPtr h);
  [DllImport("user32.dll")] public static extern bool GetWindowRect(IntPtr h,out Bounds b);
  [DllImport("user32.dll")] public static extern bool GetClientRect(IntPtr h,out Bounds b);
  [DllImport("user32.dll")] public static extern bool ClientToScreen(IntPtr h,ref Point p);
  [DllImport("user32.dll")] public static extern uint GetWindowThreadProcessId(IntPtr h,out uint pid);
  [DllImport("user32.dll")] public static extern IntPtr GetWindow(IntPtr h,uint cmd);
  [DllImport("user32.dll")] public static extern int GetWindowLong(IntPtr h,int index);
  [DllImport("user32.dll")] public static extern bool ShowWindow(IntPtr h,int how);
  [DllImport("user32.dll")] public static extern bool SetForegroundWindow(IntPtr h);
  [DllImport("user32.dll")] public static extern bool SetWindowPos(IntPtr h,IntPtr after,int x,int y,int w,int ht,uint flags);
  [DllImport("user32.dll")] public static extern IntPtr SendMessageTimeout(IntPtr h,uint msg,IntPtr w,IntPtr l,uint flags,uint ms,out IntPtr result);
  [DllImport("user32.dll")] public static extern bool SetProcessDPIAware();
  [DllImport("kernel32.dll")] public static extern IntPtr OpenProcess(uint access,bool inherit,int pid);
  [DllImport("kernel32.dll",CharSet=CharSet.Unicode)] public static extern bool QueryFullProcessImageName(IntPtr h,uint flags,StringBuilder path,ref int length);
  [DllImport("kernel32.dll")] public static extern bool CloseHandle(IntPtr h);
  [DllImport("dwmapi.dll")] public static extern int DwmRegisterThumbnail(IntPtr dest,IntPtr source,out IntPtr thumb);
  [DllImport("dwmapi.dll")] public static extern int DwmUnregisterThumbnail(IntPtr thumb);
  [DllImport("dwmapi.dll")] public static extern int DwmUpdateThumbnailProperties(IntPtr thumb,ref ThumbProperties props);
  [DllImport("dwmapi.dll")] public static extern int DwmGetWindowAttribute(IntPtr h,int attr,out int val,int size);
  [DllImport("dwmapi.dll")] public static extern int DwmGetWindowAttribute(IntPtr h,int attr,out Bounds val,int size);
  public static Bounds ClientBounds(IntPtr h) {
   Bounds r;Point p=new Point();
   if(!GetClientRect(h,out r)||!ClientToScreen(h,ref p))return new Bounds();
   return new Bounds(p.X,p.Y,r.Width,r.Height);
  }
  public static bool Responsive(IntPtr h) {
   IntPtr result;
   return h!=IntPtr.Zero&&SendMessageTimeout(h,0,IntPtr.Zero,IntPtr.Zero,2,80,out result)!=IntPtr.Zero;
  }
  // Align the visible WHOLE app window to the animation host, including native chrome.
  // Extended frame bounds exclude invisible Windows resize borders.
  public static bool AlignWholeWindow(IntPtr h,Bounds desired) {
   if(IsIconic(h)||IsZoomed(h))ShowWindow(h,9);
   for(int attempt=0;attempt<3;attempt++) {
    Bounds outer,visual;
    if(!GetWindowRect(h,out outer))return false;
    int size=Marshal.SizeOf(typeof(Bounds));
    if(DwmGetWindowAttribute(h,9,out visual,size)!=0||visual.Width<=0||visual.Height<=0)visual=outer;
    int leftInset=visual.Left-outer.Left,topInset=visual.Top-outer.Top;
    int rightInset=outer.Right-visual.Right,bottomInset=outer.Bottom-visual.Bottom;
    Bounds requested=new Bounds(desired.Left-leftInset,desired.Top-topInset,
     desired.Width+leftInset+rightInset,desired.Height+topInset+bottomInset);
    if(!SetWindowPos(h,IntPtr.Zero,requested.Left,requested.Top,requested.Width,requested.Height,0x4|0x10|0x200))return false;
   }
   Bounds after;
   if(DwmGetWindowAttribute(h,9,out after,Marshal.SizeOf(typeof(Bounds)))!=0&&!GetWindowRect(h,out after))return false;
   return Math.Abs(after.Left-desired.Left)<=3&&Math.Abs(after.Top-desired.Top)<=3
    &&Math.Abs(after.Width-desired.Width)<=3&&Math.Abs(after.Height-desired.Height)<=3;
  }
  public static IntPtr FindApp(Config cfg) {
   HashSet<int> pids=new HashSet<int>();
   foreach(string name in cfg.ProcessNames)foreach(Process p in Process.GetProcessesByName(name)) {
    try {
     string path="";
     IntPtr process=OpenProcess(0x1000,false,p.Id);
     if(process!=IntPtr.Zero) {
      try { StringBuilder buffer=new StringBuilder(4096);int length=buffer.Capacity;if(QueryFullProcessImageName(process,0,buffer,ref length))path=buffer.ToString(); }
      finally { CloseHandle(process); }
     }
     if(path.IndexOf(cfg.ProcessPathContains,StringComparison.OrdinalIgnoreCase)>=0)pids.Add(p.Id);
    } catch {} finally { p.Dispose(); }
   }
   IntPtr found=IntPtr.Zero;
   EnumWindows(delegate(IntPtr h,IntPtr unused) {
    uint pid;GetWindowThreadProcessId(h,out pid);
    if(!pids.Contains((int)pid)||!IsWindowVisible(h)||GetWindow(h,4)!=IntPtr.Zero||(GetWindowLong(h,-20)&0x80)!=0)return true;
    int cloaked;
    if(DwmGetWindowAttribute(h,14,out cloaked,4)==0&&cloaked!=0)return true;
    Bounds b=ClientBounds(h);
    if(b.Width<450||b.Height<280)return true;
    found=h;return false; // Frontmost eligible main window; no title or screenshot is recorded.
   },IntPtr.Zero);
   return found;
  }
 }
}
