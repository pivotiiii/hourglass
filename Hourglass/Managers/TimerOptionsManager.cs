﻿// --------------------------------------------------------------------------------------------------------------------
// <copyright file="TimerOptionsManager.cs" company="Chris Dziemborowicz">
//   Copyright (c) Chris Dziemborowicz. All rights reserved.
// </copyright>
// --------------------------------------------------------------------------------------------------------------------

namespace Hourglass.Managers;

using System.Linq;
using System.Windows;

using Properties;
using Timing;
using Windows;

/// <summary>
/// Manages <see cref="TimerOptions"/>s.
/// </summary>
public sealed class TimerOptionsManager : Manager
{
    /// <summary>
    /// Singleton instance of the <see cref="TimerOptionsManager"/> class.
    /// </summary>
    public static readonly TimerOptionsManager Instance = new();

    /// <summary>
    /// The most recent <see cref="TimerOptions"/>.
    /// </summary>
    private TimerOptions _mostRecentOptions = new();

    /// <summary>
    /// Prevents a default instance of the <see cref="TimerOptionsManager"/> class from being created.
    /// </summary>
    private TimerOptionsManager()
    {
    }

    /// <summary>
    /// Gets the most recent <see cref="TimerOptions"/>.
    /// </summary>
    public TimerOptions MostRecentOptions
    {
        get
        {
            UpdateMostRecentOptions();
            return TimerOptions.FromTimerOptions(_mostRecentOptions);
        }
    }

    /// <summary>
    /// Initializes the class.
    /// </summary>
    public override void Initialize()
    {
        _mostRecentOptions = Settings.Default.MostRecentOptions ?? new TimerOptions();
    }

    /// <summary>
    /// Persists the state of the class.
    /// </summary>
    public override void Persist()
    {
        UpdateMostRecentOptions();
        Settings.Default.MostRecentOptions = _mostRecentOptions;
    }

    /// <summary>
    /// Updates the <see cref="MostRecentOptions"/> from the currently opened <see cref="TimerWindow"/>s.
    /// </summary>
    private void UpdateMostRecentOptions()
    {
        if (Application.Current is null)
        {
            return;
        }

        // Get the options most recently shown to the user from a window that is still open
        var q = from window in Application.Current.Windows.OfType<TimerWindow>()
            where window.IsVisible
            orderby window.Menu.LastShown descending
            select window.Options;

        _mostRecentOptions = TimerOptions.FromTimerOptions(q.FirstOrDefault()) ?? _mostRecentOptions;

        // Never save a title
        _mostRecentOptions.Title = string.Empty;

        // Never save shutting down when expired or lock interface options
        _mostRecentOptions.ShutDownWhenExpired = false;
        _mostRecentOptions.LockInterface = false;
    }
}