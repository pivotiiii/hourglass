﻿// --------------------------------------------------------------------------------------------------------------------
// <copyright file="NotificationAreaIcon.cs" company="Chris Dziemborowicz">
//   Copyright (c) Chris Dziemborowicz. All rights reserved.
// </copyright>
// --------------------------------------------------------------------------------------------------------------------

namespace Hourglass.Windows;

using System;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Forms;
using System.Windows.Threading;

using Extensions;
using Managers;
using Properties;
using Timing;

using Application = System.Windows.Application;

/// <summary>
/// Displays an icon for the app in the notification area of the taskbar.
/// </summary>
public class NotificationAreaIcon : IDisposable
{
    /// <summary>
    /// The timeout in milliseconds for the balloon tip that is showed when a timer has expired.
    /// </summary>
    private const int TimerExpiredBalloonTipTimeout = 10000;

    /// <summary>
    /// A <see cref="NotifyIcon"/>.
    /// </summary>
    private readonly NotifyIcon _notifyIcon;

    /// <summary>
    /// A <see cref="DispatcherTimer"/> used to raise events.
    /// </summary>
    private readonly DispatcherTimer _dispatcherTimer;

    /// <summary>
    /// Indicates whether this object has been disposed.
    /// </summary>
    private bool _disposed;

    private DateTime _lastClickTime;

    /// <summary>
    /// Initializes a new instance of the <see cref="NotificationAreaIcon"/> class.
    /// </summary>
    public NotificationAreaIcon()
    {
        _notifyIcon = new()
        {
            Icon = new(Resources.TrayIcon, SystemInformation.SmallIconSize),
            ContextMenu = new()
        };

        _notifyIcon.MouseUp += NotifyIconMouseUp;
        _notifyIcon.MouseMove += NotifyIconMouseMove;

        _notifyIcon.BalloonTipClicked += BalloonTipClicked;

        _notifyIcon.ContextMenu.Popup += ContextMenuPopup;
        _notifyIcon.ContextMenu.Collapse += ContextMenuCollapse;

        _dispatcherTimer = new(DispatcherPriority.Normal)
        {
            Interval = TimeSpan.FromSeconds(1)
        };
        _dispatcherTimer.Tick += DispatcherTimerTick;

        Settings.Default.PropertyChanged += SettingsPropertyChanged;
        IsVisible = Settings.Default.ShowInNotificationArea;
    }

    /// <summary>
    /// Gets or sets a value indicating whether the icon is visible in the notification area of the taskbar.
    /// </summary>
    public bool IsVisible
    {
        get => _notifyIcon.Visible;
        set => _notifyIcon.Visible = value;
    }

    /// <summary>
    /// Displays a balloon tip notifying that a timer has expired.
    /// </summary>
    public void ShowBalloonTipForExpiredTimer()
    {
        _notifyIcon.ShowBalloonTip(
            TimerExpiredBalloonTipTimeout,
            Resources.NotificationAreaIconTimerExpired,
            Resources.NotificationAreaIconYourTimerHasExpired,
            ToolTipIcon.Info);
    }

    /// <summary>
    /// Disposes the <see cref="NotificationAreaIcon"/>.
    /// </summary>
    public void Dispose()
    {
        Dispose(true /* disposing */);
        GC.SuppressFinalize(this);
    }

    /// <summary>
    /// Disposes the <see cref="NotificationAreaIcon"/>.
    /// </summary>
    /// <param name="disposing">A value indicating whether this method was invoked by an explicit call to <see
    /// cref="Dispose"/>.</param>
    protected virtual void Dispose(bool disposing)
    {
        if (_disposed)
        {
            return;
        }

        _disposed = true;

        if (disposing)
        {
            _dispatcherTimer.Stop();
            _notifyIcon.Dispose();

            Settings.Default.PropertyChanged -= SettingsPropertyChanged;
        }
    }

    /// <summary>
    /// Restores all <see cref="TimerWindow"/>s.
    /// </summary>
    private void RestoreAllTimerWindows()
    {
        if (Application.Current is not null)
        {
            foreach (TimerWindow window in Application.Current.Windows.OfType<TimerWindow>().ArrangeDescending())
            {
                window.BringToFrontAndActivate();
            }
        }
    }

    /// <summary>
    /// Restores all <see cref="TimerWindow"/>s that show expired timers.
    /// </summary>
    private void RestoreAllExpiredTimerWindows()
    {
        if (Application.Current is not null)
        {
            foreach (TimerWindow window in Application.Current.Windows.OfType<TimerWindow>().Where(w => w.Timer.State == TimerState.Expired))
            {
                window.BringToFrontAndActivate();
            }
        }
    }

    /// <summary>
    /// Invoked after the value of an application settings property is changed.
    /// </summary>
    /// <param name="sender">The settings object.</param>
    /// <param name="e">The event data.</param>
    private void SettingsPropertyChanged(object sender, PropertyChangedEventArgs e)
    {
        if (IsVisible != Settings.Default.ShowInNotificationArea)
        {
            if (Settings.Default.ShowInNotificationArea)
            {
                IsVisible = true;
            }
            else
            {
                IsVisible = false;
                RestoreAllTimerWindows();
            }
        }
    }

    private bool IsDoubleClick(MouseEventArgs e)
    {
        if (e.Button != MouseButtons.Left)
        {
            return false;
        }

        DateTime clickTime = DateTime.UtcNow;
        if (_lastClickTime == DateTime.MinValue)
        {
            _lastClickTime = clickTime;
            return false;
        }

        if ((clickTime - _lastClickTime).Duration() > TimeSpan.FromMilliseconds(SystemInformation.DoubleClickTime))
        {
            _lastClickTime = clickTime;
            return false;
        }

        _lastClickTime = DateTime.MinValue;
        return true;
    }

    private void NotifyIconMouseUp(object sender, MouseEventArgs e)
    {
        if (Application.Current is null)
        {
            return;
        }

        if (!IsDoubleClick(e))
        {
            return;
        }

        TimerWindow[] windows = Application.Current.Windows.OfType<TimerWindow>()
            .Where(window => window.WindowState != WindowState.Minimized)
            .ToArray();

        if (windows.Any())
        {
            foreach (TimerWindow window in windows)
            {
                window.DoNotActivateNextWindow = true;
                window.WindowState = WindowState.Minimized;
            }

            return;
        }

        RestoreAllTimerWindows();
    }

    /// <summary>
    /// Invoked when the user moves the mouse while the pointer is over the icon in the notification area of the
    /// taskbar.
    /// </summary>
    /// <param name="sender">The <see cref="NotifyIcon"/>.</param>
    /// <param name="e">The event data.</param>
    private void NotifyIconMouseMove(object sender, MouseEventArgs e)
    {
        if (Application.Current is null)
        {
            return;
        }

        TimerWindow[] windows = Application.Current.Windows.OfType<TimerWindow>()
            .Where(window => window.Timer.State == TimerState.Running)
            .OrderBy(window => window.Timer.TimeLeft ?? TimeSpan.MaxValue)
            .ToArray();

        if (!windows.Any())
        {
            _notifyIcon.Text = Resources.NoTimersAreCurrentlyRunningNotificationAreText;
            return;
        }

        const int maxSize = 63;

        StringBuilder builder = new(maxSize);

        foreach (string windowString in Application.Current.Windows.OfType<TimerWindow>().Where(window => window.Timer.State == TimerState.Running)
                     .OrderBy(window => window.Timer.TimeLeft ?? TimeSpan.MaxValue)
                     .Select(window => window.ToString())
                     .Where(windowString => !string.IsNullOrWhiteSpace(windowString)))
        {
            if (builder.Length == 0)
            {
                builder.Append(windowString);
                continue;
            }

            builder.AppendLine();
            builder.Append(windowString);

            if (builder.Length > maxSize)
            {
                break;
            }
        }

        if (builder.Length > maxSize)
        {
            const string dots = "...";

            int maxTextSize = maxSize - dots.Length;

            builder.Remove(maxTextSize, builder.Length - maxTextSize);
            builder.Append(dots);
        }

        _notifyIcon.Text = builder.ToString();
    }

    /// <summary>
    /// Invoked when the balloon tip is clicked.
    /// </summary>
    /// <param name="sender">The <see cref="NotifyIcon"/>.</param>
    /// <param name="e">The event data.</param>
    private void BalloonTipClicked(object sender, EventArgs e)
    {
        RestoreAllExpiredTimerWindows();
    }

    /// <summary>
    /// Invoked before the notify icon context menu is displayed.
    /// </summary>
    /// <param name="sender">The notify icon context menu.</param>
    /// <param name="e">The event data.</param>
    private void ContextMenuPopup(object sender, EventArgs e)
    {
        _notifyIcon.ContextMenu.MenuItems.Clear();

        bool hasApplication = Application.Current is not null;

        if (hasApplication)
        {
            MenuItem newTimerMenuItem = new(Resources.NotificationAreaIconNewTimerMenuItem);
            newTimerMenuItem.Click += NewTimerMenuItemClick;
            _notifyIcon.ContextMenu.MenuItems.Add(newTimerMenuItem);

            _notifyIcon.ContextMenu.MenuItems.Add("-" /* separator */);

            foreach (TimerWindow window in Application.Current.Windows.OfType<TimerWindow>().Arrange())
            {
                MenuItem windowMenuItem = new(window.ToString());
                windowMenuItem.Tag = window;
                windowMenuItem.Click += WindowMenuItemClick;
                _notifyIcon.ContextMenu.MenuItems.Add(windowMenuItem);
            }

            _notifyIcon.ContextMenu.MenuItems.Add("-" /* separator */);

            MenuItem commandLineMenuItem = new(Resources.NotificationAreaIconCommandLineMenuItem);
            commandLineMenuItem.Click += CommandLineMenuItemClick;
            _notifyIcon.ContextMenu.MenuItems.Add(commandLineMenuItem);

            _notifyIcon.ContextMenu.MenuItems.Add("-" /* separator */);
        }

        MenuItem exitMenuItem = new(Resources.NotificationAreaIconExitMenuItem);
        exitMenuItem.Click += ExitMenuItemClick;
        _notifyIcon.ContextMenu.MenuItems.Add(exitMenuItem);

        if (hasApplication)
        {
            _dispatcherTimer.Start();
        }
    }

    private void CommandLineMenuItemClick(object sender, EventArgs e)
    {
        CommandLineArguments.ShowUsage();
    }

    /// <summary>
    /// Invoked when the <see cref="_dispatcherTimer"/> interval has elapsed.
    /// </summary>
    /// <param name="sender">The <see cref="DispatcherTimer"/>.</param>
    /// <param name="e">The event data.</param>
    private void DispatcherTimerTick(object sender, EventArgs e)
    {
        foreach (MenuItem menuItem in _notifyIcon.ContextMenu.MenuItems)
        {
            if (menuItem.Tag is TimerWindow window)
            {
                if (!window.Timer.Disposed)
                {
                    window.Timer.Update();
                }

                menuItem.Text = window.ToString();
            }
        }
    }

    /// <summary>
    /// Invoked when the shortcut menu collapses.
    /// </summary>
    /// <remarks>
    /// The Microsoft .NET Framework does not call this method consistently.
    /// </remarks>
    /// <param name="sender">The notify icon context menu.</param>
    /// <param name="e">The event data.</param>
    private void ContextMenuCollapse(object sender, EventArgs e)
    {
        _dispatcherTimer.Stop();
    }

    /// <summary>
    /// Invoked when the "New timer" <see cref="MenuItem"/> is clicked.
    /// </summary>
    /// <param name="sender">The <see cref="MenuItem"/> where the event handler is attached.</param>
    /// <param name="e">The event data.</param>
    private void NewTimerMenuItemClick(object sender, EventArgs e)
    {
        TimerWindow window = new();
        window.RestoreFromSibling();
        window.Show();
    }

    /// <summary>
    /// Invoked when a <see cref="MenuItem"/> for a <see cref="TimerWindow"/> is clicked.
    /// </summary>
    /// <param name="sender">The <see cref="MenuItem"/> where the event handler is attached.</param>
    /// <param name="e">The event data.</param>
    private void WindowMenuItemClick(object sender, EventArgs e)
    {
        MenuItem windowMenuItem = (MenuItem)sender;
        TimerWindow window = (TimerWindow)windowMenuItem.Tag;
        window.BringToFrontAndActivate();
    }

    /// <summary>
    /// Invoked when the "Exit" <see cref="MenuItem"/> is clicked.
    /// </summary>
    /// <param name="sender">The <see cref="MenuItem"/> where the event handler is attached.</param>
    /// <param name="e">The event data.</param>
    private void ExitMenuItemClick(object sender, EventArgs e)
    {
        if (Application.Current is null)
        {
            AppManager.Instance.Dispose();
            Environment.Exit(0);
            return;
        }

        if (Application.Current.Windows.OfType<TimerWindow>()
            .Any(window => window.Options.PromptOnExit &&
                           window.Timer.State != TimerState.Stopped &&
                           window.Timer.State != TimerState.Expired))
        {
            MessageBoxResult result = ((Window)null).ShowTaskDialog(
                Resources.ExitMenuTaskDialogInstruction,
                Resources.StopAndExitMenuTaskDialogCommand);

            if (result != MessageBoxResult.Yes)
            {
                return;
            }
        }

        foreach (Window window in Application.Current.Windows)
        {
            if (window is TimerWindow timerWindow)
            {
                timerWindow.DoNotActivateNextWindow = true;
                timerWindow.DoNotPromptOnExit = true;
            }

            window.Close();
        }
    }
}