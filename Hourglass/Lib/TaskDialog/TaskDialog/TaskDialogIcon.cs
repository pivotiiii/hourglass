using System;
using System.Collections.Generic;
using System.Drawing;
// ReSharper disable ExceptionNotDocumented

// ReSharper disable all

namespace KPreisser.UI;

/// <summary>
/// 
/// </summary>
public abstract class TaskDialogIcon
{
    private static readonly IReadOnlyDictionary<TaskDialogStandardIcon, TaskDialogStandardIconContainer> s_standardIcons
        = new Dictionary<TaskDialogStandardIcon, TaskDialogStandardIconContainer>() {
            { TaskDialogStandardIcon.None, new TaskDialogStandardIconContainer(TaskDialogStandardIcon.None) },
            { TaskDialogStandardIcon.Information, new TaskDialogStandardIconContainer(TaskDialogStandardIcon.Information) },
            { TaskDialogStandardIcon.Warning, new TaskDialogStandardIconContainer(TaskDialogStandardIcon.Warning) },
            { TaskDialogStandardIcon.Error, new TaskDialogStandardIconContainer(TaskDialogStandardIcon.Error) },
            { TaskDialogStandardIcon.SecurityShield, new TaskDialogStandardIconContainer(TaskDialogStandardIcon.SecurityShield) },
            { TaskDialogStandardIcon.SecurityShieldBlueBar, new TaskDialogStandardIconContainer(TaskDialogStandardIcon.SecurityShieldBlueBar) },
            { TaskDialogStandardIcon.SecurityShieldGrayBar, new TaskDialogStandardIconContainer(TaskDialogStandardIcon.SecurityShieldGrayBar) },
            { TaskDialogStandardIcon.SecurityWarningYellowBar, new TaskDialogStandardIconContainer(TaskDialogStandardIcon.SecurityWarningYellowBar) },
            { TaskDialogStandardIcon.SecurityErrorRedBar, new TaskDialogStandardIconContainer(TaskDialogStandardIcon.SecurityErrorRedBar) },
            { TaskDialogStandardIcon.SecuritySuccessGreenBar, new TaskDialogStandardIconContainer(TaskDialogStandardIcon.SecuritySuccessGreenBar) },
        };

    private protected TaskDialogIcon()
    {
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="icon"></param>
    public static implicit operator TaskDialogIcon(TaskDialogStandardIcon icon) =>
        s_standardIcons.TryGetValue(icon, out TaskDialogStandardIconContainer result)
            ? result
            : throw new InvalidCastException();

#if !NET_STANDARD
    /// <summary>
    /// 
    /// </summary>
    /// <param name="icon"></param>
    public static implicit operator TaskDialogIcon(Icon icon)
    {
        return new TaskDialogIconHandle(icon);
    }
#endif

    /// <summary>
    /// 
    /// </summary>
    /// <param name="icon"></param>
    /// <returns></returns>
    public static TaskDialogIcon Get(TaskDialogStandardIcon icon)
    {
        if (!s_standardIcons.TryGetValue(icon, out TaskDialogStandardIconContainer result))
            throw new ArgumentOutOfRangeException(nameof(icon));

        return result;
    }
}