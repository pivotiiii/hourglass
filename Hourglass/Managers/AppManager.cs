﻿// --------------------------------------------------------------------------------------------------------------------
// <copyright file="AppManager.cs" company="Chris Dziemborowicz">
//   Copyright (c) Chris Dziemborowicz. All rights reserved.
// </copyright>
// --------------------------------------------------------------------------------------------------------------------

namespace Hourglass.Managers;

using System.Linq;

/// <summary>
/// Manages the app.
/// </summary>
public sealed class AppManager : Manager
{
    /// <summary>
    /// Singleton instance of the <see cref="AppManager"/> class.
    /// </summary>
    public static readonly AppManager Instance = new();

    /// <summary>
    /// The manager class singleton instances.
    /// </summary>
    private static readonly Manager[] Managers =
    {
        TaskDialogManager.Instance,
        ErrorManager.Instance,
        SettingsManager.Instance,
        UpdateManager.Instance,
        KeepAwakeManager.Instance,
        WakeUpManager.Instance,
        NotificationAreaIconManager.Instance,
        ThemeManager.Instance,
        SoundManager.Instance,
        TimerStartManager.Instance,
        TimerOptionsManager.Instance,
        TimerManager.Instance
    };

    /// <summary>
    /// Prevents a default instance of the <see cref="AppManager"/> class from being created.
    /// </summary>
    private AppManager()
    {
    }

    /// <summary>
    /// Initializes the class.
    /// </summary>
    public override void Initialize()
    {
        foreach (Manager manager in Managers)
        {
            manager.Initialize();
        }
    }

    /// <summary>
    /// Persists the state of the class.
    /// </summary>
    public override void Persist()
    {
        foreach (Manager manager in Managers.Reverse())
        {
            manager.Persist();
        }
    }

    /// <summary>
    /// Disposes the manager.
    /// </summary>
    /// <param name="disposing">A value indicating whether this method was invoked by an explicit call to <see
    /// cref="Dispose"/>.</param>
    protected override void Dispose(bool disposing)
    {
        if (Disposed)
        {
            return;
        }

        if (disposing)
        {
            foreach (Manager manager in Managers.Reverse())
            {
                manager.Dispose();
            }
        }

        base.Dispose(disposing);
    }
}