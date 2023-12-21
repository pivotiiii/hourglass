using Hourglass.Extensions;

namespace Hourglass.Managers;

#pragma warning disable S3881
public sealed class TaskDialogManager : Manager
#pragma warning restore S3881
{
    public static readonly TaskDialogManager Instance = new ();

    private TaskDialogManager()
    {
    }

    protected override void Dispose(bool disposing)
    {
        WindowExtensions.Clean();
    }
}

