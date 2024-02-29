using System.ComponentModel;

// ReSharper disable all

namespace KPreisser.UI;

/// <summary>
/// 
/// </summary>
public class TaskDialogClosingEventArgs : CancelEventArgs
{
    /// <summary>
    /// 
    /// </summary>
    internal TaskDialogClosingEventArgs(TaskDialogButton closeButton)
    {
        CloseButton = closeButton;
    }

    /// <summary>
    /// Gets the <see cref="TaskDialogButton"/> that is causing the task dialog
    /// to close.
    /// </summary>
    public TaskDialogButton CloseButton
    {
        get;
    }
}