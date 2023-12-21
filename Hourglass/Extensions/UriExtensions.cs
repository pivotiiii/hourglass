using System;
using System.Diagnostics;

using Hourglass.Properties;

namespace Hourglass.Extensions;

internal static class UriExtensions
{
    public static readonly Uri FAQUri = new(Resources.FAQUrl);

    public static void Navigate(this Uri uri)
    {
        Process.Start(uri.ToString());
    }
}
