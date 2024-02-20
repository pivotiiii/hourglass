// --------------------------------------------------------------------------------------------------------------------
// <copyright file="NotificationAreaIcon.cs" company="Chris Dziemborowicz">
//   Copyright (c) Chris Dziemborowicz. All rights reserved.
// </copyright>
// --------------------------------------------------------------------------------------------------------------------

namespace Hourglass.Windows;

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Drawing.Drawing2D;
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
    /// Normal notification area icon.
    /// </summary>
    private readonly Icon _normalIcon;

    /// <summary>
    /// Paused notification area icon.
    /// </summary>
    private readonly Lazy<Icon> _pausedIcon;

    /// <summary>
    /// Indicates whether this object has been disposed.
    /// </summary>
    private bool _disposed;

    /// <summary>
    /// Last mouse click time.
    /// </summary>
    private DateTime _lastClickTime;

    /// <summary>
    /// Initializes a new instance of the <see cref="NotificationAreaIcon"/> class.
    /// </summary>
    public NotificationAreaIcon()
    {
        _normalIcon = new(Resources.TrayIcon, SystemInformation.SmallIconSize);
        _pausedIcon = new(CreatePausedIcon);

        _notifyIcon = new()
        {
            Icon = _normalIcon,
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

    private Icon CreatePausedIcon()
    {
        const int diameter          = 8;
        const int circleBorderWidth = 1;

        const int pauseWidth        = 1;
        const int pause1LeftOffset  = 2;
        const int pause2LeftOffset  = 4;
        const int pauseTopOffset    = 2;
        const int pauseBottomOffset = 4;

        Color circlePenColor = Color.FromArgb(unchecked((int)0xFF787878));
        Color pauseLineColor = Color.FromArgb(unchecked((int)0xFF303030));

        int width  = _normalIcon.Width;
        int height = _normalIcon.Height;

        using Bitmap bitmap = _normalIcon.ToBitmap();
        using Graphics graphics = Graphics.FromImage(bitmap);

        graphics.SmoothingMode = SmoothingMode.HighQuality;

        int circleX = width  - diameter - circleBorderWidth;
        int circleY = height - diameter - circleBorderWidth;

        graphics.FillEllipse(Brushes.White, circleX, circleY, diameter, diameter);

        using Pen circlePen = new(circlePenColor, circleBorderWidth);
        graphics.DrawEllipse(circlePen, circleX, circleY, diameter, diameter);

        graphics.SmoothingMode = SmoothingMode.Default;

        using SolidBrush pauseLineBrush = new(pauseLineColor);
        DrawPauseLine(pause1LeftOffset);
        DrawPauseLine(pause2LeftOffset);

        return Icon.FromHandle(bitmap.GetHicon());

        void DrawPauseLine(int leftOffset) =>
            graphics.FillRectangle(
                pauseLineBrush,
                width  - diameter + leftOffset,
                height - diameter + pauseTopOffset,
                pauseWidth,
                diameter - pauseBottomOffset);
    }

    /// <summary>
    /// Gets or sets a value indicating whether the icon is visible in the notification area of the taskbar.
    /// </summary>
    public bool IsVisible
    {
        get => _notifyIcon.Visible;
        set
        {
            _notifyIcon.Visible = value;

            if (value)
            {
                RefreshIcon();
            }
        }
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
        Dispose(true);
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

        if (!disposing)
        {
            return;
        }

        _dispatcherTimer.Stop();

        _notifyIcon.Dispose();
        if (_pausedIcon.IsValueCreated)
        {
            _pausedIcon.Value.Dispose();
        }
        _normalIcon.Dispose();

        Settings.Default.PropertyChanged -= SettingsPropertyChanged;
    }

    /// <summary>
    /// Restores all <see cref="TimerWindow"/>s.
    /// </summary>
    private void RestoreAllTimerWindows()
    {
        if (Application.Current is null)
        {
            return;
        }

        foreach (TimerWindow window in Application.Current.Windows.OfType<TimerWindow>().ArrangeDescending())
        {
            window.BringToFrontAndActivate();
        }
    }

    /// <summary>
    /// Restores all <see cref="TimerWindow"/>s that show expired timers.
    /// </summary>
    private void RestoreAllExpiredTimerWindows()
    {
        if (Application.Current is null)
        {
            return;
        }

        foreach (TimerWindow window in Application.Current.Windows.OfType<TimerWindow>().Where(static w => w.Timer.State == TimerState.Expired))
        {
            window.BringToFrontAndActivate();
        }
    }

    /// <summary>
    /// Invoked after the value of an application settings property is changed.
    /// </summary>
    /// <param name="sender">The settings object.</param>
    /// <param name="e">The event data.</param>
    private void SettingsPropertyChanged(object sender, PropertyChangedEventArgs e)
    {
        if (IsVisible == Settings.Default.ShowInNotificationArea)
        {
            return;
        }

        IsVisible = Settings.Default.ShowInNotificationArea;

        if (!IsVisible)
        {
            RestoreAllTimerWindows();
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
            .Where(static window => window.WindowState != WindowState.Minimized)
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

        string[] windowStrings = Application.Current.Windows.OfType<TimerWindow>()
            .Arrange()
            .Select(static window => window.ToString())
            .Where(static windowString => !string.IsNullOrWhiteSpace(windowString))
            .ToArray();

        if (!windowStrings.Any())
        {
            _notifyIcon.Text = Resources.NoTimersNotificationAreaText;
            return;
        }

        const int maxSize = 63;

        StringBuilder builder = new(maxSize);

        foreach (string windowString in windowStrings)
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
            _notifyIcon.ContextMenu.MenuItems.AddRange(GetApplicationMenuItems().ToArray());
        }

        MenuItem exitMenuItem = new(Resources.NotificationAreaIconExitMenuItem);
        exitMenuItem.Click += ExitMenuItemClick;
        _notifyIcon.ContextMenu.MenuItems.Add(exitMenuItem);

        if (hasApplication)
        {
            _dispatcherTimer.Start();
        }

        IEnumerable<MenuItem> GetApplicationMenuItems()
        {
            MenuItem menuItem = new(Resources.NotificationAreaIconNewTimerMenuItem);
            menuItem.Click += NewTimerMenuItemClick;
            yield return menuItem;

            yield return NewSeparatorMenuItem();

            bool shouldAddSeparator = false;
            foreach (TimerWindow window in Application.Current.Windows.OfType<TimerWindow>().Arrange())
            {
                shouldAddSeparator = true;

                menuItem = new(window.ToString())
                {
                    Tag = window
                };
                menuItem.Click += WindowMenuItemClick;
                yield return menuItem;
            }

            if (shouldAddSeparator)
            {
                shouldAddSeparator = false;

                yield return NewSeparatorMenuItem();

                if (TimerManager.CanPauseAll())
                {
                    shouldAddSeparator = true;
                    menuItem = new(Resources.NotificationAreaIconPauseAllMenuItem);
                    menuItem.Click += delegate { TimerManager.PauseAll(); };
                    yield return menuItem;
                }

                if (TimerManager.CanResumeAll())
                {
                    shouldAddSeparator = true;
                    menuItem = new(Resources.NotificationAreaIconResumeAllMenuItem);
                    menuItem.Click += delegate { TimerManager.ResumeAll(); };
                    yield return menuItem;
                }
            }

            if (shouldAddSeparator)
            {
                yield return NewSeparatorMenuItem();
            }

            menuItem = new(Resources.NotificationAreaIconAboutMenuItem);
            menuItem.Click += delegate { AboutDialog.ShowOrActivate(); };
            yield return menuItem;

            yield return NewSeparatorMenuItem();

            static MenuItem NewSeparatorMenuItem() =>
                new("-");
        }
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
            if (menuItem.Tag is not TimerWindow window)
            {
                continue;
            }

            if (!window.Timer.Disposed)
            {
                window.Timer.Update();
            }

            menuItem.Text = window.ToString();
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
            .Any(static window => window.Options.LockInterface && IsTimerRunningFor(window)))
        {
            return;
        }

        TimerWindow? firstTimerWindow = Application.Current.Windows.OfType<TimerWindow>()
            .Arrange()
            .FirstOrDefault(static window => window.Options.PromptOnExit && IsTimerRunningFor(window));

        if (firstTimerWindow is not null)
        {
            WindowState windowState = firstTimerWindow.WindowState;

            firstTimerWindow.BringToFrontAndActivate();

            MessageBoxResult result = firstTimerWindow.ShowTaskDialog(
                Resources.ExitMenuTaskDialogInstruction,
                Resources.StopAndExitMenuTaskDialogCommand);

            firstTimerWindow.WindowState = windowState;

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

        static bool IsTimerRunningFor(TimerWindow window) =>
            window.Timer.State != TimerState.Stopped &&
            window.Timer.State != TimerState.Expired;
    }

    /// <summary>
    /// Refreshes notification area icon.
    /// </summary>
    public void RefreshIcon() =>
        _notifyIcon.Icon =
            TimerManager.GetPausableTimers(TimerState.Paused).Any()
                ? _pausedIcon.Value
                : _normalIcon;
}