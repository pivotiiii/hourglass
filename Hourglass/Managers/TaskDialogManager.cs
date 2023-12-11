using Hourglass.Extensions;

namespace Hourglass.Managers;

public class TaskDialogManager : Manager
{
    public static readonly TaskDialogManager Instance = new ();

    private TaskDialogManager()
    {
    }

    protected override void Dispose(bool disposing) =>
        WindowExtensions.Clean();
}

