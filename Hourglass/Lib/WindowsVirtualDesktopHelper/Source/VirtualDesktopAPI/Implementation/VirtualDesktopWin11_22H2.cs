using System;
using System.Runtime.InteropServices;

namespace WindowsVirtualDesktopHelper.VirtualDesktopAPI.Implementation;

internal class VirtualDesktopWin11_22H2(ImmersiveShellProvider immersiveShellProvider)
    : VirtualDesktop<VirtualDesktopWin11_22H2.IVirtualDesktopManagerInternal>(immersiveShellProvider)
{
    protected override Guid GetCurrentDesktopId() =>
        VirtualDesktopManagerInternal.GetCurrentDesktop(IntPtr.Zero).GetId();

    [ComImport]
    [InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
    [Guid("536D3495-B208-4CC9-AE26-DE8111275BF8")]
    internal interface IVirtualDesktop
    {
        bool IsViewVisible(IntPtr view);
        Guid GetId();
        IntPtr Unknown1();
        [return: MarshalAs(UnmanagedType.HString)]
        string GetName();
        [return: MarshalAs(UnmanagedType.HString)]
        string GetWallpaperPath();
    }

    [ComImport]
    [InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
    [Guid("B2F925B9-5A0F-4D2E-9F4D-2B1507593C10")]
    internal interface IVirtualDesktopManagerInternal
    {
        int GetCount(IntPtr hWndOrMon);
        void MoveViewToDesktop(IntPtr view, IVirtualDesktop desktop);
        bool CanViewMoveDesktops(IntPtr view);
        IVirtualDesktop GetCurrentDesktop(IntPtr hWndOrMon);
        IntPtr GetAllCurrentDesktops();
        void GetDesktops(IntPtr hWndOrMon, out IntPtr desktops);
        [PreserveSig]
        int GetAdjacentDesktop(IVirtualDesktop from, int direction, out IVirtualDesktop desktop);
        void SwitchDesktop(IntPtr hWndOrMon, IVirtualDesktop desktop);
        IVirtualDesktop CreateDesktop(IntPtr hWndOrMon);
        void MoveDesktop(IVirtualDesktop desktop, IntPtr hWndOrMon, int nIndex);
        void RemoveDesktop(IVirtualDesktop desktop, IVirtualDesktop fallback);
        IVirtualDesktop FindDesktop(ref Guid desktopId);
        void GetDesktopSwitchIncludeExcludeViews(IVirtualDesktop desktop, out IntPtr unknown1, out IntPtr unknown2);
        void SetDesktopName(IVirtualDesktop desktop, [MarshalAs(UnmanagedType.HString)] string name);
        void SetDesktopWallpaper(IVirtualDesktop desktop, [MarshalAs(UnmanagedType.HString)] string path);
        void UpdateWallpaperPathForAllDesktops([MarshalAs(UnmanagedType.HString)] string path);
        void CopyDesktopState(IntPtr pView0, IntPtr pView1);
        int GetDesktopIsPerMonitor();
        void SetDesktopIsPerMonitor(bool state);
    }
}