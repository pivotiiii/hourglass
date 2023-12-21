// --------------------------------------------------------------------------------------------------------------------
// <copyright file="Settings.cs" company="Chris Dziemborowicz">
//   Copyright (c) Chris Dziemborowicz. All rights reserved.
// </copyright>
// --------------------------------------------------------------------------------------------------------------------

namespace Hourglass.Properties;

using System.Collections.Generic;
using System.Linq;

using Serialization;
using Timing;
using Windows;

/// <summary>
/// Application settings.
/// </summary>
#if PORTABLE
[System.Configuration.SettingsProvider(typeof(PortableSettingsProvider))]
#endif
internal sealed partial class Settings
{
    /// <summary>
    /// Gets or sets the most recent <see cref="TimerOptions"/>.
    /// </summary>
    public TimerOptions MostRecentOptions
    {
        get => TimerOptions.FromTimerOptionsInfo(MostRecentOptionsInfo);
        set => MostRecentOptionsInfo = TimerOptionsInfo.FromTimerOptions(value);
    }

    /// <summary>
    /// Gets or sets the <see cref="Timer"/>s.
    /// </summary>
    public IList<Timer> Timers
    {
        get
        {
            IEnumerable<TimerInfo> timerInfos = TimerInfos ?? new TimerInfoList();
#pragma warning disable S2365
            return timerInfos.Select(Timer.FromTimerInfo).ToList();
#pragma warning restore S2365
        }

        set
        {
            IEnumerable<TimerInfo> timerInfos = value.Select(TimerInfo.FromTimer);
            TimerInfos = new(timerInfos);
        }
    }

    /// <summary>
    /// Gets or sets the <see cref="TimerStart"/>s.
    /// </summary>
    public IList<TimerStart> TimerStarts
    {
        get
        {
            IEnumerable<TimerStartInfo> timerStartInfos = TimerStartInfos ?? new TimerStartInfoList();
#pragma warning disable S2365
            return timerStartInfos.Select(TimerStart.FromTimerStartInfo).ToList();
#pragma warning restore S2365
        }

        set
        {
            IEnumerable<TimerStartInfo> timerStartInfos = value.Select(TimerStartInfo.FromTimerStart);
            TimerStartInfos = new(timerStartInfos);
        }
    }

    /// <summary>
    /// Gets or sets the collection of the themes defined by the user.
    /// </summary>
    public IList<Theme> UserProvidedThemes
    {
        get
        {
            IEnumerable<ThemeInfo> userProvidedThemeInfos = UserProvidedThemeInfos ?? new ThemeInfoList();
#pragma warning disable S2365
            return userProvidedThemeInfos.Select(Theme.FromThemeInfo).ToList();
#pragma warning restore S2365
        }

        set
        {
            IEnumerable<ThemeInfo> userProvidedThemeInfos = value.Select(ThemeInfo.FromTheme);
            UserProvidedThemeInfos = new(userProvidedThemeInfos);
        }
    }

    /// <summary>
    /// Gets or sets the <see cref="WindowSize"/>.
    /// </summary>
    public WindowSize WindowSize
    {
        get => WindowSize.FromWindowSizeInfo(WindowSizeInfo);
        set => WindowSizeInfo = WindowSizeInfo.FromWindowSize(value);
    }
}