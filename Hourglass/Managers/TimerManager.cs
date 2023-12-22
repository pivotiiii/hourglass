﻿// --------------------------------------------------------------------------------------------------------------------
// <copyright file="TimerManager.cs" company="Chris Dziemborowicz">
//   Copyright (c) Chris Dziemborowicz. All rights reserved.
// </copyright>
// --------------------------------------------------------------------------------------------------------------------

namespace Hourglass.Managers;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;

using Properties;
using Timing;
using Windows;

/// <summary>
/// Manages timers.
/// </summary>
public sealed class TimerManager : Manager
{
    /// <summary>
    /// The maximum number of timers to persist in settings.
    /// </summary>
    public const int MaxSavedTimers = 10;

    /// <summary>
    /// Singleton instance of the <see cref="TimerManager"/> class.
    /// </summary>
    public static readonly TimerManager Instance = new();

    /// <summary>
    /// The currently loaded timers in reverse chronological order.
    /// </summary>
    private readonly List<Timer> _timers = new();

    /// <summary>
    /// Prevents a default instance of the <see cref="TimerManager"/> class from being created.
    /// </summary>
    private TimerManager()
    {
    }

    /// <summary>
    /// Gets a list of the currently loaded timers.
    /// </summary>
    public IList<Timer> Timers => _timers.AsReadOnly();

    /// <summary>
    /// Gets a list of the currently loaded timers that are not bound to any <see cref="TimerWindow"/> and are not
    /// <see cref="TimerState.Stopped"/>.
    /// </summary>
#pragma warning disable S2365
    public IList<Timer> ResumableTimers => _timers.Where(t => t.State != TimerState.Stopped && !IsBoundToWindow(t)).ToList();
#pragma warning restore S2365

    /// <summary>
    /// Gets a list of the currently loaded timers that are bound to any <see cref="TimerWindow"/> and are <see
    /// cref="TimerState.Running"/>.
    /// </summary>
#pragma warning disable S2365
    public IList<Timer> RunningTimers => _timers.Where(t => t.State == TimerState.Running && IsBoundToWindow(t)).ToList();
#pragma warning restore S2365

    /// <summary>
    /// Initializes the class.
    /// </summary>
    public override void Initialize()
    {
        _timers.Clear();
        _timers.AddRange(Settings.Default.Timers);
    }

    /// <summary>
    /// Persists the state of the class.
    /// </summary>
    public override void Persist()
    {
        Settings.Default.Timers = _timers
            .Where(t => t.State != TimerState.Stopped && t.State != TimerState.Expired)
            .Where(t => !t.Options.LockInterface)
            .Take(MaxSavedTimers)
            .ToList();
    }

    /// <summary>
    /// Add a new timer.
    /// </summary>
    /// <param name="timer">A <see cref="Timer"/>.</param>
    /// <exception cref="InvalidOperationException">If the <see cref="Timer"/> has already been added.
    /// </exception>
    public void Add(Timer timer)
    {
        if (_timers.Contains(timer))
        {
            throw new InvalidOperationException();
        }

        _timers.Insert(0, timer);
    }

    /// <summary>
    /// Remove an existing timer.
    /// </summary>
    /// <param name="timer">A <see cref="Timer"/>.</param>
    /// <exception cref="InvalidOperationException">If the timer had not been added previously or has already been
    /// removed.</exception>
    public void Remove(Timer timer)
    {
        if (!_timers.Contains(timer))
        {
            throw new InvalidOperationException();
        }

        _timers.Remove(timer);
        timer.Dispose();
    }

    /// <summary>
    /// Removes the timer elements of the specified collection.
    /// </summary>
    /// <param name="collection">A collection of timers to remove.</param>
    public void Remove(IEnumerable<Timer> collection)
    {
        foreach (Timer timer in collection)
        {
            Remove(timer);
        }
    }

    /// <summary>
    /// Clears the <see cref="ResumableTimers"/>.
    /// </summary>
    public void ClearResumableTimers()
    {
        Remove(ResumableTimers);
    }

    /// <summary>
    /// Returns a value indicating whether a timer is bound to any <see cref="TimerWindow"/>.</summary>
    /// <param name="timer">A <see cref="Timer"/>.</param>
    /// <returns>A value indicating whether the timer is bound to any <see cref="TimerWindow"/>. </returns>
    private static bool IsBoundToWindow(Timer timer)
    {
        return Application.Current?.Windows.OfType<TimerWindow>().Any(w => w.Timer == timer) == true;
    }
}