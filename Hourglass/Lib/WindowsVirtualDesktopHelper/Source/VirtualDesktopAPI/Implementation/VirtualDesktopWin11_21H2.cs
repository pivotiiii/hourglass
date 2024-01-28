using System;
using System.Runtime.InteropServices;

namespace WindowsVirtualDesktopHelper.VirtualDesktopAPI.Implementation;

#pragma warning disable S101
internal sealed class VirtualDesktopWin11_21H2(ImmersiveShellProvider immersiveShellProvider)
#pragma warning restore S101
    : VirtualDesktop<VirtualDesktopWin11_21H2.IVirtualDesktopManagerInternal>(immersiveShellProvider)
{
    protected override Guid GetCurrentDesktopId() =>
        VirtualDesktopManagerInternal!.GetCurrentDesktop(IntPtr.Zero).GetId();

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
        int GetCount(IntPtr hWnd);
        void MoveViewToDesktop(IntPtr view, IVirtualDesktop desktop);
        bool CanViewMoveDesktops(IntPtr view);
        IVirtualDesktop GetCurrentDesktop(IntPtr hWnd);
        void GetDesktops(IntPtr hWnd, out IntPtr desktops);

        [PreserveSig]
        int GetAdjacentDesktop(IVirtualDesktop from, int direction, out IVirtualDesktop desktop);

        void SwitchDesktop(IntPtr hWnd, IVirtualDesktop desktop);
        IVirtualDesktop CreateDesktop(IntPtr hWnd);
        void MoveDesktop(IVirtualDesktop desktop, IntPtr hWnd, int nIndex);
        void RemoveDesktop(IVirtualDesktop desktop, IVirtualDesktop fallback);
        IVirtualDesktop FindDesktop(ref Guid desktopId);
        void Unknown1(IVirtualDesktop desktop, out IntPtr unknown1, out IntPtr unknown2);
        void SetName(IVirtualDesktop desktop, [MarshalAs(UnmanagedType.HString)] string name);
        void SetWallpaperPath(IVirtualDesktop desktop, [MarshalAs(UnmanagedType.HString)] string path);
        void SetAllWallpaperPaths([MarshalAs(UnmanagedType.HString)] string path);
        void Unknown2(IntPtr pView0, IntPtr pView1);
        int Unknown3();
        void RemoveAll(bool remove);
    }
}