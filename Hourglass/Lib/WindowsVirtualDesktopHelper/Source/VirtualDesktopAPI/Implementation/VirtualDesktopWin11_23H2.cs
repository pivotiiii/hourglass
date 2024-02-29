using System;
using System.Runtime.InteropServices;

// ReSharper disable once CheckNamespace

namespace WindowsVirtualDesktopHelper.VirtualDesktopAPI.Implementation;

#pragma warning disable S101
internal sealed class VirtualDesktopWin11_23H2(ImmersiveShellProvider immersiveShellProvider)
#pragma warning restore S101
    : VirtualDesktop<VirtualDesktopWin11_23H2.IVirtualDesktopManagerInternal>(immersiveShellProvider)
{
    protected override Guid GetCurrentDesktopId() =>
        VirtualDesktopManagerInternal!.GetCurrentDesktop().GetId();

    [ComImport]
    [InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
    [Guid("3F07F4BE-B107-441A-AF0F-39D82529072C")]
    internal interface IVirtualDesktop
    {
        bool IsViewVisible(IntPtr view);
        Guid GetId();
        string GetName();
        [return: MarshalAs(UnmanagedType.HString)]
        string GetWallpaperPath();
        bool IsRemote();
    }

    [ComImport]
    [InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
    [Guid("A3175F2D-239C-4BD2-8AA0-EEBA8B0B138E")]
    internal interface IVirtualDesktopManagerInternal
    {
        int GetCount();
        void MoveViewToDesktop(IntPtr view, IVirtualDesktop desktop);
        bool CanViewMoveDesktops(IntPtr view);
        IVirtualDesktop GetCurrentDesktop();
        void GetDesktops(out IntPtr desktops);
        [PreserveSig]
        int GetAdjacentDesktop(IVirtualDesktop from, int direction, out IVirtualDesktop desktop);
        void SwitchDesktop(IVirtualDesktop desktop);
        IVirtualDesktop CreateDesktop(IntPtr hWndOrMon);
        void MoveDesktop(IVirtualDesktop desktop, IntPtr hWndOrMon, int nIndex);
        void RemoveDesktop(IVirtualDesktop desktop, IVirtualDesktop fallback);
        IVirtualDesktop FindDesktop(ref Guid desktopId);
        void GetDesktopSwitchIncludeExcludeViews(IVirtualDesktop desktop, out IntPtr unknown1, out IntPtr unknown2);
        void SetDesktopName(IVirtualDesktop desktop, [MarshalAs(UnmanagedType.HString)] string name);
        void SetDesktopWallpaper(IVirtualDesktop desktop, [MarshalAs(UnmanagedType.HString)] string path);
        void UpdateWallpaperPathForAllDesktops([MarshalAs(UnmanagedType.HString)] string path);
        void CopyDesktopState(IntPtr pView0, IntPtr pView1);
        void CreateRemoteDesktop([MarshalAs(UnmanagedType.HString)] string path, out IVirtualDesktop desktop);
        void SwitchRemoteDesktop(IVirtualDesktop desktop);
        void SwitchDesktopWithAnimation(IVirtualDesktop desktop);
        void GetLastActiveDesktop(out IVirtualDesktop desktop);
        void WaitForAnimationToComplete();
    }
}