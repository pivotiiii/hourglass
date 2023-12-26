using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;

using Hourglass.Timing;
using Hourglass.Windows;

namespace Hourglass.Extensions;

public static class TimerWindowExtensions
{
    private static readonly Comparer<TimerWindow> TimeComparer = Comparer<TimerWindow>.Create(CompareTime);
    private static readonly StringComparer TitleComparer = StringComparer.CurrentCultureIgnoreCase;

    public static IEnumerable<TimerWindow> Arrange(this IEnumerable<TimerWindow> windows) =>
        windows.OrderBy(static window => window, TimeComparer).ThenBy(Title, TitleComparer);

    public static IEnumerable<TimerWindow> ArrangeDescending(this IEnumerable<TimerWindow> windows) =>
        windows.OrderByDescending(static window => window, TimeComparer).ThenByDescending(Title, TitleComparer);

    public static void BringNextToFrontAndActivate(this TimerWindow thisWindow)
    {
        if (thisWindow.DoNotActivateNextWindow)
        {
            thisWindow.DoNotActivateNextWindow = false;
            return;
        }

        var nextWindow = GetNextWindow();
        if (nextWindow is not null)
        {
            nextWindow.Dispatcher.BeginInvoke(nextWindow.BringToFrontAndActivate);
        }

        TimerWindow GetNextWindow()
        {
            var allWindows = Application.Current.Windows.OfType<TimerWindow>().Arrange().ToList();

            return GetNextApplicableWindow(allWindows.SkipWhile(NotThisWindow).Skip(1)) ??
                   GetNextApplicableWindow(allWindows.TakeWhile(NotThisWindow));

            bool NotThisWindow(TimerWindow window) =>
                !ReferenceEquals(thisWindow, window);

            static TimerWindow GetNextApplicableWindow(IEnumerable<TimerWindow> windows) =>
                windows.FirstOrDefault(static window => window.IsVisible && window.WindowState != WindowState.Minimized);
        }
    }

    private static string Title(TimerWindow window) =>
        window.Timer.Options.Title;

    private static int CompareTime(TimerWindow x, TimerWindow y)
    {
        var rankCompare = ToRank(x.Timer.State).CompareTo(ToRank(y.Timer.State));
        if (rankCompare != 0)
        {
            return rankCompare;
        }

        return IsNotRunning(x.Timer.TimeLeft, y.Timer.TimeLeft)
            ? CompareTimeSpan(x.Timer.TotalTime, y.Timer.TotalTime)
            : CompareTimeSpan(x.Timer.TimeLeft, y.Timer.TimeLeft);

        int CompareTimeSpan(TimeSpan? xTimeSpan, TimeSpan? yTimeSpan) =>
            CompareTimeSpanValue(ToTimeSpanValue(xTimeSpan), ToTimeSpanValue(yTimeSpan));

        int CompareTimeSpanValue(TimeSpan xTimeSpan, TimeSpan yTimeSpan)
        {
            var timeSpanCompare = xTimeSpan.CompareTo(yTimeSpan);

            return timeSpanCompare == 0
                ? y.ID.CompareTo(x.ID)
                : timeSpanCompare;
        }

        static bool IsNotRunning(TimeSpan? x, TimeSpan? y) =>
            x is null ||
            y is null ||
            x == TimeSpan.Zero ||
            y == TimeSpan.Zero;

        static int ToRank(TimerState timerState) =>
            timerState switch
            {
                TimerState.Stopped => 0,
                TimerState.Expired => 1,
                TimerState.Paused  => 2,
                TimerState.Running => 3,
                _ => int.MaxValue
            };

        static TimeSpan ToTimeSpanValue(TimeSpan? timeSpan) =>
            TimeSpan.FromSeconds(Math.Round((timeSpan ?? TimeSpan.Zero).TotalSeconds));
    }
}
