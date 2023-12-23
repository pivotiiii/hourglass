using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace Hourglass.Extensions;

public static class NotifyPropertyChangedExtensions
{
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static void Notify<T>(this PropertyChangedEventHandler propertyChanged, T sender, [CallerMemberName] string propertyName = null)
        where T: INotifyPropertyChanged =>
        propertyChanged?.Invoke(sender, new PropertyChangedEventArgs(propertyName));

    public static void Notify<T>(this PropertyChangedEventHandler propertyChanged, T sender, string firstPropertyName, params string[] propertyNames)
        where T : INotifyPropertyChanged
    {
        if (propertyChanged is null)
        {
            return;
        }

        propertyChanged.Notify(sender, firstPropertyName);

        foreach (var propertyName in propertyNames)
        {
            propertyChanged.Notify(sender, propertyName);
        }
    }
}
