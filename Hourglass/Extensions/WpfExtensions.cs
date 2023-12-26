using System.Globalization;
using System.Windows.Input;

namespace Hourglass.Extensions;

internal static class WpfExtensions
{
    private static readonly KeyGestureConverter _keyGestureConverter = new();

    public static string ToInputGestureText(this KeyGesture keyGesture) =>
        (string) _keyGestureConverter.ConvertTo(null, CultureInfo.InvariantCulture, keyGesture, typeof(string));
}
