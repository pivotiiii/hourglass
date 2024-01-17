using System;
using System.Runtime.InteropServices;

namespace WindowsVirtualDesktopHelper.VirtualDesktopAPI.Implementation;

#pragma warning disable S3881
internal class ImmersiveShellProvider : IDisposable
#pragma warning restore S3881
{
    private IServiceProvider10 _serviceProvider;

    public ImmersiveShellProvider()
    {
        try
        {
            _serviceProvider = (IServiceProvider10)Activator.CreateInstance(Type.GetTypeFromCLSID(new("C2F03A33-21F5-47FA-B4BB-156362A2F239") /* CLSID_ImmersiveShell */));
        }
        catch
        {
            // Ignore.
        }
    }

    public T QueryService<T>(Guid service) where T : class
    {
        var obj = _serviceProvider?.QueryService(service, typeof(T).GUID);
        try
        {
            return (T)obj;
        }
        catch
        {
            if (obj is not null)
            {
                Marshal.ReleaseComObject(obj);
            }
            return null;
        }
    }

    public void Dispose()
    {
        if (_serviceProvider is null)
        {
            return;
        }

        Marshal.ReleaseComObject(_serviceProvider);
        _serviceProvider = null;
    }

    [ComImport]
    [InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
    [Guid("6D5140C1-7436-11CE-8034-00AA006009FA")]
    internal interface IServiceProvider10
    {
        [return: MarshalAs(UnmanagedType.IUnknown)]
        object QueryService(ref Guid service, ref Guid riid);
    }
}
