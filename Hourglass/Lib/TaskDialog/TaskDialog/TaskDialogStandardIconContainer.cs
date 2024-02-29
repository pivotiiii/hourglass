// ReSharper disable all

namespace KPreisser.UI;

internal class TaskDialogStandardIconContainer : TaskDialogIcon
{
    public TaskDialogStandardIconContainer(TaskDialogStandardIcon icon)
    {
        Icon = icon;
    }
        
    public TaskDialogStandardIcon Icon
    {
        get;
    }
}