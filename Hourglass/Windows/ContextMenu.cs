// --------------------------------------------------------------------------------------------------------------------
// <copyright file="ContextMenu.cs" company="Chris Dziemborowicz">
//   Copyright (c) Chris Dziemborowicz. All rights reserved.
// </copyright>
// --------------------------------------------------------------------------------------------------------------------

namespace Hourglass.Windows;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Threading;

using Extensions;
using Managers;
using Properties;
using Timing;

/// <summary>
/// A <see cref="System.Windows.Controls.ContextMenu"/> for the <see cref="TimerWindow"/>.
/// </summary>
public sealed class ContextMenu : System.Windows.Controls.ContextMenu
{
    #region Private Members

    /// <summary>
    /// The <see cref="TimerWindow"/> that uses this context menu.
    /// </summary>
    private TimerWindow _timerWindow;

    /// <summary>
    /// A <see cref="DispatcherTimer"/> used to raise events.
    /// </summary>
    private DispatcherTimer _dispatcherTimer;

    /// <summary>
    /// The "Always on top" <see cref="MenuItem"/>.
    /// </summary>
    private MenuItem _alwaysOnTopMenuItem;

    /// <summary>
    /// The "Full screen" <see cref="MenuItem"/>.
    /// </summary>
    private MenuItem _fullScreenMenuItem;

    /// <summary>
    /// The "Prompt on exit" <see cref="MenuItem"/>.
    /// </summary>
    private MenuItem _promptOnExitMenuItem;

    /// <summary>
    /// The "Show progress in taskbar" <see cref="MenuItem"/>.
    /// </summary>
    private MenuItem _showProgressInTaskbarMenuItem;

    /// <summary>
    /// The "Show in notification area" <see cref="MenuItem"/>.
    /// </summary>
    private MenuItem _showInNotificationAreaMenuItem;

    /// <summary>
    /// The "Loop timer" <see cref="MenuItem"/>.
    /// </summary>
    private MenuItem _loopTimerMenuItem;

    /// <summary>
    /// The "Pop up when expired" <see cref="MenuItem"/>.
    /// </summary>
    private MenuItem _popUpWhenExpiredMenuItem;

    /// <summary>
    /// The "Close when expired" <see cref="MenuItem"/>.
    /// </summary>
    private MenuItem _closeWhenExpiredMenuItem;

    /// <summary>
    /// The "Recent inputs" <see cref="MenuItem"/>.
    /// </summary>
    private MenuItem _recentInputsMenuItem;

    /// <summary>
    /// The "Clear recent inputs" <see cref="MenuItem"/>.
    /// </summary>
    private MenuItem _clearRecentInputsMenuItem;

    /// <summary>
    /// The "Saved timers" <see cref="MenuItem"/>.
    /// </summary>
    private MenuItem _savedTimersMenuItem;

    /// <summary>
    /// The "Open all saved timers" <see cref="MenuItem"/>.
    /// </summary>
    private MenuItem _openAllSavedTimersMenuItem;

    /// <summary>
    /// The "Clear saved timers" <see cref="MenuItem"/>.
    /// </summary>
    private MenuItem _clearSavedTimersMenuItem;

    /// <summary>
    /// The "Theme" <see cref="MenuItem"/>.
    /// </summary>
    private MenuItem _themeMenuItem;

    /// <summary>
    /// The "Light theme" <see cref="MenuItem"/>.
    /// </summary>
    private MenuItem _lightThemeMenuItem;

    /// <summary>
    /// The "Dark theme" <see cref="MenuItem"/>.
    /// </summary>
    private MenuItem _darkThemeMenuItem;

    /// <summary>
    /// The "Manage themes" <see cref="MenuItem"/>.
    /// </summary>
    private MenuItem _manageThemesMenuItem;

    /// <summary>
    /// The "Theme" <see cref="MenuItem"/>s associated with <see cref="Theme"/>s.
    /// </summary>
    private IList<MenuItem> _selectableThemeMenuItems;

    /// <summary>
    /// The "Sound" <see cref="MenuItem"/>.
    /// </summary>
    private MenuItem _soundMenuItem;

    /// <summary>
    /// The "Sound" <see cref="MenuItem"/>s associated with <see cref="Sound"/>s.
    /// </summary>
    private IList<MenuItem> _selectableSoundMenuItems;

    /// <summary>
    /// The "Loop sound" <see cref="MenuItem"/>.
    /// </summary>
    private MenuItem _loopSoundMenuItem;

    /// <summary>
    /// The "Do not keep computer awake" <see cref="MenuItem"/>.
    /// </summary>
    private MenuItem _doNotKeepComputerAwakeMenuItem;

    /// <summary>
    /// The "Open saved timers on startup" <see cref="MenuItem"/>.
    /// </summary>
    private MenuItem _openSavedTimersOnStartupMenuItem;

    /// <summary>
    /// The "Prefer 24-hour time when parsing" <see cref="MenuItem"/>.
    /// </summary>
    private MenuItem _prefer24HourTimeMenuItem;

    /// <summary>
    /// The "Reverse progress bar" <see cref="MenuItem"/>.
    /// </summary>
    private MenuItem _reverseProgressBarMenuItem;

    /// <summary>
    /// The "Show time elapsed" <see cref="MenuItem"/>.
    /// </summary>
    private MenuItem _showTimeElapsedMenuItem;

    /// <summary>
    /// The "Shut down when expired" <see cref="MenuItem"/>.
    /// </summary>
    private MenuItem _shutDownWhenExpiredMenuItem;

    /// <summary>
    /// The "Window title" <see cref="MenuItem"/>s associated with <see cref="WindowTitleMode"/>s.
    /// </summary>
    private IList<MenuItem> _selectableWindowTitleMenuItems;

    /// <summary>
    /// The "Restore" <see cref="MenuItem"/>.
    /// </summary>
    private MenuItem _restoreMenuItem;

    /// <summary>
    /// The "Minimize" <see cref="MenuItem"/>.
    /// </summary>
    private MenuItem _minimizeMenuItem;

    /// <summary>
    /// The "Maximize" <see cref="MenuItem"/>.
    /// </summary>
    private MenuItem _maximizeMenuItem;

    /// <summary>
    /// Separates the "Restore", "Minimize", and "Maximize" menu items from the "Close" menu item.
    /// </summary>
    private Separator _windowStateItemsSeparator;

    #endregion

    /// <summary>
    /// Gets the date and time the menu was last visible.
    /// </summary>
    public DateTime LastShown { get; private set; } = DateTime.MinValue;

    /// <summary>
    /// Binds the <see cref="ContextMenu"/> to a <see cref="TimerWindow"/>.
    /// </summary>
    /// <param name="window">A <see cref="TimerWindow"/>.</param>
    public void Bind(TimerWindow window)
    {
        // Validate state
        if (_timerWindow is not null)
        {
            throw new InvalidOperationException(@"Timer window is already created.");
        }

        SetValue(TextOptions.TextFormattingModeProperty, TextFormattingMode.Display);

        // Initialize members
        _timerWindow = window ?? throw new ArgumentNullException(nameof(window));

        _timerWindow.ContextMenuOpening += WindowContextMenuOpening;
        _timerWindow.ContextMenuClosing += WindowContextMenuClosing;
        _timerWindow.ContextMenu = this;

        _dispatcherTimer = new(DispatcherPriority.Normal, Dispatcher)
        {
            Interval = TimeSpan.FromSeconds(1)
        };
        _dispatcherTimer.Tick += DispatcherTimerTick;

        _selectableThemeMenuItems = new List<MenuItem>();
        _selectableSoundMenuItems = new List<MenuItem>();
        _selectableWindowTitleMenuItems = new List<MenuItem>();

        // Build the menu
        BuildMenu();
    }

    #region Private Methods (Lifecycle)

    /// <summary>
    /// Invoked when the context menu is opened.
    /// </summary>
    /// <param name="sender">The bound <see cref="TimerWindow"/>.</param>
    /// <param name="e">The event data.</param>
    private void WindowContextMenuOpening(object sender, ContextMenuEventArgs e)
    {
        // Do not show the context menu if the user interface is locked
        if (_timerWindow.Options.LockInterface)
        {
            e.Handled = true;
            return;
        }

        // Update dynamic items
        UpdateRecentInputsMenuItem();
        UpdateSavedTimersMenuItem();
        UpdateThemeMenuItem();
        UpdateSoundMenuItem();
        UpdateWindowStateMenuItems();

        // Update binding
        UpdateMenuFromOptions();

        LastShown = DateTime.Now;
        _dispatcherTimer.Start();
    }

    /// <summary>
    /// Invoked when the <see cref="_dispatcherTimer"/> interval has elapsed.
    /// </summary>
    /// <param name="sender">The <see cref="DispatcherTimer"/>.</param>
    /// <param name="e">The event data.</param>
    private void DispatcherTimerTick(object sender, EventArgs e)
    {
        LastShown = DateTime.Now;
        UpdateSavedTimersHeaders();
    }

    /// <summary>
    /// Invoked just before the context menu is closed.
    /// </summary>
    /// <param name="sender">The bound <see cref="TimerWindow"/>.</param>
    /// <param name="e">The event data.</param>
    private void WindowContextMenuClosing(object sender, ContextMenuEventArgs e)
    {
        UpdateOptionsFromMenu();

        LastShown = DateTime.Now;
        _dispatcherTimer.Stop();

        AppManager.Instance.Persist();
    }

    #endregion

    #region Private Methods (Binding)

    /// <summary>
    /// Reads the options from the <see cref="TimerOptions"/> and applies them to this menu.
    /// </summary>
    private void UpdateMenuFromOptions()
    {
        // Always on top
        _alwaysOnTopMenuItem.IsChecked = _timerWindow.Options.AlwaysOnTop;

        // Full screen
        _fullScreenMenuItem.IsChecked = _timerWindow.IsFullScreen;

        // Prompt on exit
        _promptOnExitMenuItem.IsChecked = _timerWindow.Options.PromptOnExit;

        // Show progress in taskbar
        _showProgressInTaskbarMenuItem.IsChecked = _timerWindow.Options.ShowProgressInTaskbar;

        // Show in notification area
        _showInNotificationAreaMenuItem.IsChecked = Settings.Default.ShowInNotificationArea;

        // Loop timer
        if (_timerWindow.Timer.SupportsLooping)
        {
            _loopTimerMenuItem.IsEnabled = true;
            _loopTimerMenuItem.IsChecked = _timerWindow.Options.LoopTimer;
        }
        else
        {
            _loopTimerMenuItem.IsEnabled = false;
            _loopTimerMenuItem.IsChecked = false;
        }

        // Pop up when expired
        _popUpWhenExpiredMenuItem.IsChecked = _timerWindow.Options.PopUpWhenExpired;

        // Close when expired
        if ((!_timerWindow.Options.LoopTimer || !_timerWindow.Timer.SupportsLooping) && !_timerWindow.Options.LoopSound)
        {
            _closeWhenExpiredMenuItem.IsChecked = _timerWindow.Options.CloseWhenExpired;
            _closeWhenExpiredMenuItem.IsEnabled = true;
        }
        else
        {
            _closeWhenExpiredMenuItem.IsChecked = false;
            _closeWhenExpiredMenuItem.IsEnabled = false;
        }

        // Theme
        foreach (MenuItem menuItem in _selectableThemeMenuItems)
        {
            Theme menuItemTheme = (Theme)menuItem.Tag;
            menuItem.IsChecked = menuItemTheme == _timerWindow.Options.Theme;
            if (_timerWindow.Options.Theme.Type == ThemeType.UserProvided)
            {
                menuItem.Visibility = menuItemTheme.Type == ThemeType.BuiltInLight || menuItemTheme.Type == ThemeType.UserProvided
                    ? Visibility.Visible
                    : Visibility.Collapsed;
            }
            else
            {
                menuItem.Visibility = menuItemTheme.Type == _timerWindow.Options.Theme.Type || menuItemTheme.Type == ThemeType.UserProvided
                    ? Visibility.Visible
                    : Visibility.Collapsed;
            }
        }

        _lightThemeMenuItem.IsChecked = _timerWindow.Options.Theme.Type == ThemeType.BuiltInLight;
        _darkThemeMenuItem.IsChecked = _timerWindow.Options.Theme.Type == ThemeType.BuiltInDark;

        // Sound
        foreach (MenuItem menuItem in _selectableSoundMenuItems)
        {
            menuItem.IsChecked = menuItem.Tag == _timerWindow.Options.Sound;
        }

        // Loop sound
        _loopSoundMenuItem.IsChecked = _timerWindow.Options.LoopSound;

        // Do not keep computer awake
        _doNotKeepComputerAwakeMenuItem.IsChecked = _timerWindow.Options.DoNotKeepComputerAwake;

        // Open saved timers on startup
        _openSavedTimersOnStartupMenuItem.IsChecked = Settings.Default.OpenSavedTimersOnStartup;

        // Prefer 24-hour time when parsing
        _prefer24HourTimeMenuItem.IsChecked = Settings.Default.Prefer24HourTime;

        // Reverse progress bar
        _reverseProgressBarMenuItem.IsChecked = _timerWindow.Options.ReverseProgressBar;

        // Show time elapsed
        _showTimeElapsedMenuItem.IsChecked = _timerWindow.Options.ShowTimeElapsed;

        // Shut down when expired
        if ((!_timerWindow.Options.LoopTimer || !_timerWindow.Timer.SupportsLooping) && !_timerWindow.Options.LoopSound)
        {
            _shutDownWhenExpiredMenuItem.IsChecked = _timerWindow.Options.ShutDownWhenExpired;
            _shutDownWhenExpiredMenuItem.IsEnabled = true;
        }
        else
        {
            _shutDownWhenExpiredMenuItem.IsChecked = false;
            _shutDownWhenExpiredMenuItem.IsEnabled = false;
        }

        // Window title
        foreach (MenuItem menuItem in _selectableWindowTitleMenuItems)
        {
            WindowTitleMode windowTitleMode = (WindowTitleMode)menuItem.Tag;
            menuItem.IsChecked = windowTitleMode == _timerWindow.Options.WindowTitleMode;
        }
    }

    /// <summary>
    /// Reads the options from this menu and applies them to the <see cref="TimerOptions"/>.
    /// </summary>
    private void UpdateOptionsFromMenu()
    {
        // Always on top
        _timerWindow.Options.AlwaysOnTop = _alwaysOnTopMenuItem.IsChecked;

        // Full screen
        _timerWindow.IsFullScreen = _fullScreenMenuItem.IsChecked;

        // Prompt on exit
        _timerWindow.Options.PromptOnExit = _promptOnExitMenuItem.IsChecked;

        // Show progress in taskbar
        _timerWindow.Options.ShowProgressInTaskbar = _showProgressInTaskbarMenuItem.IsChecked;

        // Show in notification area
        Settings.Default.ShowInNotificationArea = _showInNotificationAreaMenuItem.IsChecked;

        // Loop timer
        if (_loopTimerMenuItem.IsEnabled)
        {
            _timerWindow.Options.LoopTimer = _loopTimerMenuItem.IsChecked;
        }

        // Pop up when expired
        _timerWindow.Options.PopUpWhenExpired = _popUpWhenExpiredMenuItem.IsChecked;

        // Close when expired
        if (_closeWhenExpiredMenuItem.IsEnabled)
        {
            _timerWindow.Options.CloseWhenExpired = _closeWhenExpiredMenuItem.IsChecked;
        }

        // Sound
        MenuItem selectedSoundMenuItem = _selectableSoundMenuItems.FirstOrDefault(mi => mi.IsChecked);
        _timerWindow.Options.Sound = selectedSoundMenuItem?.Tag as Sound;

        // Loop sound
        _timerWindow.Options.LoopSound = _loopSoundMenuItem.IsChecked;

        // Do not keep computer awake
        _timerWindow.Options.DoNotKeepComputerAwake = _doNotKeepComputerAwakeMenuItem.IsChecked;

        // Open saved timers on startup
        Settings.Default.OpenSavedTimersOnStartup = _openSavedTimersOnStartupMenuItem.IsChecked;

        // Prefer 24-hour time when parsing
        Settings.Default.Prefer24HourTime = _prefer24HourTimeMenuItem.IsChecked;

        // Reverse progress bar
        _timerWindow.Options.ReverseProgressBar = _reverseProgressBarMenuItem.IsChecked;

        // Show time elapsed
        _timerWindow.Options.ShowTimeElapsed = _showTimeElapsedMenuItem.IsChecked;

        // Shut down when expired
        if (_shutDownWhenExpiredMenuItem.IsEnabled)
        {
            _timerWindow.Options.ShutDownWhenExpired = _shutDownWhenExpiredMenuItem.IsChecked;
        }

        // Window title
        MenuItem selectedWindowTitleMenuItem = _selectableWindowTitleMenuItems.FirstOrDefault(mi => mi.IsChecked);
        _timerWindow.Options.WindowTitleMode = selectedWindowTitleMenuItem is not null
            ? (WindowTitleMode)selectedWindowTitleMenuItem.Tag
            : WindowTitleMode.ApplicationName;
    }

    /// <summary>
    /// Invoked when a checkable <see cref="MenuItem"/> is clicked.
    /// </summary>
    /// <param name="sender">The <see cref="MenuItem"/> where the event handler is attached.</param>
    /// <param name="e">The event data.</param>
    private void CheckableMenuItemClick(object sender, RoutedEventArgs e)
    {
        UpdateOptionsFromMenu();
        UpdateMenuFromOptions();
    }

    #endregion

    /// <summary>
    /// Builds or rebuilds the context menu.
    /// </summary>
    private void BuildMenu()
    {
        Items.Clear();

        // New timer
        MenuItem newTimerMenuItem = new()
        {
            Header = Properties.Resources.ContextMenuNewTimerMenuItem
        };
        newTimerMenuItem.Click += NewTimerMenuItemClick;
        Items.Add(newTimerMenuItem);

        Items.Add(new Separator());

        // Always on top
        _alwaysOnTopMenuItem = new()
        {
            Header = Properties.Resources.ContextMenuAlwaysOnTopMenuItem,
            IsCheckable = true
        };
        _alwaysOnTopMenuItem.Click += CheckableMenuItemClick;
        Items.Add(_alwaysOnTopMenuItem);

        // Full screen
        _fullScreenMenuItem = new()
        {
            Header = Properties.Resources.ContextMenuFullScreenMenuItem,
            IsCheckable = true
        };
        _fullScreenMenuItem.Click += CheckableMenuItemClick;
        Items.Add(_fullScreenMenuItem);

        // Window title
        MenuItem windowTitleMenuItem = new()
        {
            Header = Properties.Resources.ContextMenuWindowTitleMenuItem
        };

        // No window title
        MenuItem noWindowTitleMenuItem = new()
        {
            Header = Properties.Resources.ContextMenuNoWindowTitleMenuItem,
            IsCheckable = true,
            Tag = WindowTitleMode.None
        };
        noWindowTitleMenuItem.Click += WindowTitleMenuItemClick;
        noWindowTitleMenuItem.Click += CheckableMenuItemClick;
        windowTitleMenuItem.Items.Add(noWindowTitleMenuItem);
        _selectableWindowTitleMenuItems.Add(noWindowTitleMenuItem);

        windowTitleMenuItem.Items.Add(new Separator());

        // Application name (window title)
        MenuItem applicationNameWindowTitleMenuItem = new()
        {
            Header = Properties.Resources.ContextMenuApplicationNameWindowTitleMenuItem,
            IsCheckable = true,
            Tag = WindowTitleMode.ApplicationName
        };
        applicationNameWindowTitleMenuItem.Click += WindowTitleMenuItemClick;
        applicationNameWindowTitleMenuItem.Click += CheckableMenuItemClick;
        windowTitleMenuItem.Items.Add(applicationNameWindowTitleMenuItem);
        _selectableWindowTitleMenuItems.Add(applicationNameWindowTitleMenuItem);

        windowTitleMenuItem.Items.Add(new Separator());

        // Time left (window title)
        MenuItem timeLeftWindowTitleMenuItem = new()
        {
            Header = Properties.Resources.ContextMenuTimeLeftWindowTitleMenuItem,
            IsCheckable = true,
            Tag = WindowTitleMode.TimeLeft
        };
        timeLeftWindowTitleMenuItem.Click += WindowTitleMenuItemClick;
        timeLeftWindowTitleMenuItem.Click += CheckableMenuItemClick;
        windowTitleMenuItem.Items.Add(timeLeftWindowTitleMenuItem);
        _selectableWindowTitleMenuItems.Add(timeLeftWindowTitleMenuItem);

        // Time elapsed (window title)
        MenuItem timeElapsedWindowTitleMenuItem = new()
        {
            Header = Properties.Resources.ContextMenuTimeElapsedWindowTitleMenuItem,
            IsCheckable = true,
            Tag = WindowTitleMode.TimeElapsed
        };
        timeElapsedWindowTitleMenuItem.Click += WindowTitleMenuItemClick;
        timeElapsedWindowTitleMenuItem.Click += CheckableMenuItemClick;
        windowTitleMenuItem.Items.Add(timeElapsedWindowTitleMenuItem);
        _selectableWindowTitleMenuItems.Add(timeElapsedWindowTitleMenuItem);

        // Timer title (window title)
        MenuItem timerTitleWindowTitleMenuItem = new()
        {
            Header = Properties.Resources.ContextMenuTimerTitleWindowTitleMenuItem,
            IsCheckable = true,
            Tag = WindowTitleMode.TimerTitle
        };
        timerTitleWindowTitleMenuItem.Click += WindowTitleMenuItemClick;
        timerTitleWindowTitleMenuItem.Click += CheckableMenuItemClick;
        windowTitleMenuItem.Items.Add(timerTitleWindowTitleMenuItem);
        _selectableWindowTitleMenuItems.Add(timerTitleWindowTitleMenuItem);

        windowTitleMenuItem.Items.Add(new Separator());

        // Time left + timer title (window title)
        MenuItem timeLeftPlusTimerTitleWindowTitleMenuItem = new()
        {
            Header = Properties.Resources.ContextMenuTimeLeftPlusTimerTitleWindowTitleMenuItem,
            IsCheckable = true,
            Tag = WindowTitleMode.TimeLeftPlusTimerTitle
        };
        timeLeftPlusTimerTitleWindowTitleMenuItem.Click += WindowTitleMenuItemClick;
        timeLeftPlusTimerTitleWindowTitleMenuItem.Click += CheckableMenuItemClick;
        windowTitleMenuItem.Items.Add(timeLeftPlusTimerTitleWindowTitleMenuItem);
        _selectableWindowTitleMenuItems.Add(timeLeftPlusTimerTitleWindowTitleMenuItem);

        // Time elapsed + timer title (window title)
        MenuItem timeElapsedPlusTimerTitleWindowTitleMenuItem = new()
        {
            Header = Properties.Resources.ContextMenuTimeElapsedPlusTimerTitleWindowTitleMenuItem,
            IsCheckable = true,
            Tag = WindowTitleMode.TimeElapsedPlusTimerTitle
        };
        timeElapsedPlusTimerTitleWindowTitleMenuItem.Click += WindowTitleMenuItemClick;
        timeElapsedPlusTimerTitleWindowTitleMenuItem.Click += CheckableMenuItemClick;
        windowTitleMenuItem.Items.Add(timeElapsedPlusTimerTitleWindowTitleMenuItem);
        _selectableWindowTitleMenuItems.Add(timeElapsedPlusTimerTitleWindowTitleMenuItem);

        windowTitleMenuItem.Items.Add(new Separator());

        // Timer title + time left (window title)
        MenuItem timerTitlePlusTimeLeftWindowTitleMenuItem = new()
        {
            Header = Properties.Resources.ContextMenuTimerTitlePlusTimeLeftWindowTitleMenuItem,
            IsCheckable = true,
            Tag = WindowTitleMode.TimerTitlePlusTimeLeft
        };
        timerTitlePlusTimeLeftWindowTitleMenuItem.Click += WindowTitleMenuItemClick;
        timerTitlePlusTimeLeftWindowTitleMenuItem.Click += CheckableMenuItemClick;
        windowTitleMenuItem.Items.Add(timerTitlePlusTimeLeftWindowTitleMenuItem);
        _selectableWindowTitleMenuItems.Add(timerTitlePlusTimeLeftWindowTitleMenuItem);

        // Timer title + time elapsed (window title)
        MenuItem timerTitlePlusTimeElapsedWindowTitleMenuItem = new()
        {
            Header = Properties.Resources.ContextMenuTimerTitlePlusTimeElapsedWindowTitleMenuItem,
            IsCheckable = true,
            Tag = WindowTitleMode.TimerTitlePlusTimeElapsed
        };
        timerTitlePlusTimeElapsedWindowTitleMenuItem.Click += WindowTitleMenuItemClick;
        timerTitlePlusTimeElapsedWindowTitleMenuItem.Click += CheckableMenuItemClick;
        windowTitleMenuItem.Items.Add(timerTitlePlusTimeElapsedWindowTitleMenuItem);
        _selectableWindowTitleMenuItems.Add(timerTitlePlusTimeElapsedWindowTitleMenuItem);

        Items.Add(windowTitleMenuItem);

        Items.Add(new Separator());

        // Prompt on exit
        _promptOnExitMenuItem = new()
        {
            Header = Properties.Resources.ContextMenuPromptOnExitMenuItem,
            IsCheckable = true
        };
        _promptOnExitMenuItem.Click += CheckableMenuItemClick;
        Items.Add(_promptOnExitMenuItem);

        // Show progress in taskbar
        _showProgressInTaskbarMenuItem = new()
        {
            Header = Properties.Resources.ContextMenuShowProgressInTaskbarMenuItem,
            IsCheckable = true
        };
        _showProgressInTaskbarMenuItem.Click += CheckableMenuItemClick;
        Items.Add(_showProgressInTaskbarMenuItem);

        // Show in notification area
        _showInNotificationAreaMenuItem = new()
        {
            Header = Properties.Resources.ContextMenuShowInNotificationAreaMenuItem,
            IsCheckable = true
        };
        _showInNotificationAreaMenuItem.Click += CheckableMenuItemClick;
        Items.Add(_showInNotificationAreaMenuItem);

        Items.Add(new Separator());

        // Loop timer
        _loopTimerMenuItem = new()
        {
            Header = Properties.Resources.ContextMenuLoopTimerMenuItem,
            IsCheckable = true
        };
        _loopTimerMenuItem.Click += CheckableMenuItemClick;
        Items.Add(_loopTimerMenuItem);

        // Pop up when expired
        _popUpWhenExpiredMenuItem = new()
        {
            Header = Properties.Resources.ContextMenuPopUpWhenExpiredMenuItem,
            IsCheckable = true
        };
        _popUpWhenExpiredMenuItem.Click += CheckableMenuItemClick;
        Items.Add(_popUpWhenExpiredMenuItem);

        // Close when expired
        _closeWhenExpiredMenuItem = new()
        {
            Header = Properties.Resources.ContextMenuCloseWhenExpiredMenuItem,
            IsCheckable = true
        };
        _closeWhenExpiredMenuItem.Click += CheckableMenuItemClick;
        Items.Add(_closeWhenExpiredMenuItem);

        Items.Add(new Separator());

        // Recent inputs
        _recentInputsMenuItem = new()
        {
            Header = Properties.Resources.ContextMenuRecentInputsMenuItem
        };
        Items.Add(_recentInputsMenuItem);

        // Saved timers
        _savedTimersMenuItem = new()
        {
            Header = Properties.Resources.ContextMenuSavedTimersMenuItem
        };
        Items.Add(_savedTimersMenuItem);

        Items.Add(new Separator());

        // Theme
        _themeMenuItem = new()
        {
            Header = Properties.Resources.ContextMenuThemeMenuItem
        };
        Items.Add(_themeMenuItem);

        // Sound
        _soundMenuItem = new()
        {
            Header = Properties.Resources.ContextMenuSoundMenuItem
        };
        Items.Add(_soundMenuItem);

        Items.Add(new Separator());

        // Advanced options
        MenuItem advancedOptionsMenuItem = new()
        {
            Header = Properties.Resources.ContextMenuAdvancedOptionsMenuItem
        };
        Items.Add(advancedOptionsMenuItem);

        // Do not keep computer awake
        _doNotKeepComputerAwakeMenuItem = new()
        {
            Header = Properties.Resources.ContextMenuDoNotKeepComputerAwakeMenuItem,
            IsCheckable = true
        };
        _doNotKeepComputerAwakeMenuItem.Click += CheckableMenuItemClick;
        advancedOptionsMenuItem.Items.Add(_doNotKeepComputerAwakeMenuItem);

        // Open saved timers on startup
        _openSavedTimersOnStartupMenuItem = new()
        {
            Header = Properties.Resources.ContextMenuOpenSavedTimersOnStartupMenuItem,
            IsCheckable = true
        };
        _openSavedTimersOnStartupMenuItem.Click += CheckableMenuItemClick;
        advancedOptionsMenuItem.Items.Add(_openSavedTimersOnStartupMenuItem);

        // Prefer 24-hour time when parsing
        _prefer24HourTimeMenuItem = new()
        {
            Header = Properties.Resources.ContextMenuPrefer24HourTimeMenuItem,
            IsCheckable = true
        };
        _prefer24HourTimeMenuItem.Click += CheckableMenuItemClick;
        advancedOptionsMenuItem.Items.Add(_prefer24HourTimeMenuItem);

        // Reverse progress bar
        _reverseProgressBarMenuItem = new()
        {
            Header = Properties.Resources.ContextMenuReverseProgressBarMenuItem,
            IsCheckable = true
        };
        _reverseProgressBarMenuItem.Click += CheckableMenuItemClick;
        advancedOptionsMenuItem.Items.Add(_reverseProgressBarMenuItem);

        // Show time elapsed
        _showTimeElapsedMenuItem = new()
        {
            Header = Properties.Resources.ContextMenuShowTimeElapsedMenuItem,
            IsCheckable = true
        };
        _showTimeElapsedMenuItem.Click += CheckableMenuItemClick;
        advancedOptionsMenuItem.Items.Add(_showTimeElapsedMenuItem);

        // Shut down when expired
        _shutDownWhenExpiredMenuItem = new()
        {
            Header = Properties.Resources.ContextMenuShutDownWhenExpiredMenuItem,
            IsCheckable = true
        };
        _shutDownWhenExpiredMenuItem.Click += CheckableMenuItemClick;
        advancedOptionsMenuItem.Items.Add(_shutDownWhenExpiredMenuItem);

        Items.Add(new Separator());

        // FAQ
        MenuItem faqMenuItem = new()
        {
            Header = Properties.Resources.ContextMenuFAQMenuItem
        };
        faqMenuItem.Click += FAQMenuItemClick;
        Items.Add(faqMenuItem);

        // Usage
        MenuItem usageMenuItem = new()
        {
            Header = Properties.Resources.ContextMenuUsageMenuItem
        };
        usageMenuItem.Click += UsageMenuItemClick;
        Items.Add(usageMenuItem);

        Items.Add(new Separator());

        // About
        MenuItem aboutMenuItem = new()
        {
            Header = Properties.Resources.ContextMenuAboutMenuItem
        };
        aboutMenuItem.Click += AboutMenuItemClick;
        Items.Add(aboutMenuItem);

        Items.Add(new Separator());

        // Restore
        _restoreMenuItem = new()
        {
            Header = Properties.Resources.ContextMenuRestoreMenuItem
        };
        _restoreMenuItem.Click += RestoreMenuItemClick;
        Items.Add(_restoreMenuItem);

        // Minimize
        _minimizeMenuItem = new()
        {
            Header = Properties.Resources.ContextMenuMinimizeMenuItem
        };
        _minimizeMenuItem.Click += MinimizeMenuItemClick;
        Items.Add(_minimizeMenuItem);

        // Maximize
        _maximizeMenuItem = new()
        {
            Header = Properties.Resources.ContextMenuMaximizeMenuItem
        };
        _maximizeMenuItem.Click += MaximizeMenuItemClick;
        Items.Add(_maximizeMenuItem);

        _windowStateItemsSeparator = new();
        Items.Add(_windowStateItemsSeparator);

        // Close
        MenuItem closeMenuItem = new()
        {
            Header = Properties.Resources.ContextMenuCloseMenuItem
        };
        closeMenuItem.Click += CloseMenuItemClick;
        Items.Add(closeMenuItem);
    }


    /// <summary>
    /// Invoked when the "New timer" <see cref="MenuItem"/> is clicked.
    /// </summary>
    /// <param name="sender">The <see cref="MenuItem"/> where the event handler is attached.</param>
    /// <param name="e">The event data.</param>
    private void NewTimerMenuItemClick(object sender, RoutedEventArgs e)
    {
        TimerWindow window = new();
        window.RestoreFromWindow(_timerWindow);
        window.Show();
    }

    #region Private Methods (Recent Inputs)

    /// <summary>
    /// Updates the <see cref="_recentInputsMenuItem"/>.
    /// </summary>
    private void UpdateRecentInputsMenuItem()
    {
        _recentInputsMenuItem.Items.Clear();

        if (TimerStartManager.Instance.TimerStarts.Count == 0)
        {
            MenuItem noRecentInputsMenuItem = new()
            {
                Header = Properties.Resources.ContextMenuNoRecentInputsMenuItem,
                Foreground = Brushes.DarkGray
            };

            _recentInputsMenuItem.Items.Add(noRecentInputsMenuItem);
        }
        else
        {
            foreach (TimerStart timerStart in TimerStartManager.Instance.TimerStarts)
            {
                MenuItem timerMenuItem = new()
                {
                    Header = timerStart.ToString(),
                    Tag = timerStart
                };
                timerMenuItem.Click += RecentInputMenuItemClick;

                _recentInputsMenuItem.Items.Add(timerMenuItem);
            }
        }

        _recentInputsMenuItem.Items.Add(new Separator());

        if (_clearRecentInputsMenuItem is null)
        {
            _clearRecentInputsMenuItem = new()
            {
                Header = Properties.Resources.ContextMenuClearRecentInputsMenuItem
            };
            _clearRecentInputsMenuItem.Click += ClearRecentInputsMenuItemClick;
        }

        _recentInputsMenuItem.Items.Add(_clearRecentInputsMenuItem);
    }

    /// <summary>
    /// Invoked when a recent <see cref="TimerStart"/> <see cref="MenuItem"/> is clicked.
    /// </summary>
    /// <param name="sender">The <see cref="MenuItem"/> where the event handler is attached.</param>
    /// <param name="e">The event data.</param>
    private void RecentInputMenuItemClick(object sender, RoutedEventArgs e)
    {
        MenuItem menuItem = (MenuItem)sender;
        TimerStart timerStart = (TimerStart)menuItem.Tag;

        TimerWindow window;
        if (_timerWindow.Timer.State == TimerState.Stopped || _timerWindow.Timer.State == TimerState.Expired)
        {
            window = _timerWindow;
        }
        else
        {
            window = new();
            window.Options.Set(_timerWindow.Options);
            window.RestoreFromWindow(_timerWindow);
        }

        window.Show(timerStart);
    }

    /// <summary>
    /// Invoked when the "Clear recent inputs" <see cref="MenuItem"/> is clicked.
    /// </summary>
    /// <param name="sender">The <see cref="MenuItem"/> where the event handler is attached.</param>
    /// <param name="e">The event data.</param>
    private void ClearRecentInputsMenuItemClick(object sender, RoutedEventArgs e)
    {
        TimerStartManager.Instance.Clear();
    }

    #endregion

    #region Private Methods (Saved Timers)

    /// <summary>
    /// Updates the <see cref="_savedTimersMenuItem"/>.
    /// </summary>
    private void UpdateSavedTimersMenuItem()
    {
        _savedTimersMenuItem.Items.Clear();

        IList<Timer> savedTimers = TimerManager.Instance.ResumableTimers;

        if (savedTimers.Count == 0)
        {
            MenuItem noRunningTimersMenuItem = new()
            {
                Header = Properties.Resources.ContextMenuNoSavedTimersMenuItem,
                Foreground = Brushes.DarkGray
            };

            _savedTimersMenuItem.Items.Add(noRunningTimersMenuItem);
        }
        else
        {
            foreach (Timer savedTimer in savedTimers)
            {
                savedTimer.Update();

                MenuItem timerMenuItem = new()
                {
                    Header = GetHeaderForTimer(savedTimer),
                    Icon = GetIconForTimer(savedTimer),
                    Tag = savedTimer
                };
                timerMenuItem.Click += SavedTimerMenuItemClick;

                _savedTimersMenuItem.Items.Add(timerMenuItem);
            }
        }

        _savedTimersMenuItem.Items.Add(new Separator());

        if (_openAllSavedTimersMenuItem is null)
        {
            _openAllSavedTimersMenuItem = new()
            {
                Header = Properties.Resources.ContextMenuOpenAllSavedTimersMenuItem
            };
            _openAllSavedTimersMenuItem.Click += OpenAllSavedTimersMenuItemClick;
        }

        _savedTimersMenuItem.Items.Add(_openAllSavedTimersMenuItem);

        if (_clearSavedTimersMenuItem is null)
        {
            _clearSavedTimersMenuItem = new()
            {
                Header = Properties.Resources.ContextMenuClearSavedTimersMenuItem
            };
            _clearSavedTimersMenuItem.Click += ClearSavedTimersMenuItemClick;
        }

        _savedTimersMenuItem.Items.Add(_clearSavedTimersMenuItem);
    }

    /// <summary>
    /// Updates the <see cref="MenuItem.Header"/> of the items in the <see cref="_savedTimersMenuItem"/>.
    /// </summary>
    private void UpdateSavedTimersHeaders()
    {
        foreach (MenuItem menuItem in _savedTimersMenuItem.Items.OfType<MenuItem>())
        {
            if (menuItem.Tag is Timer timer)
            {
                menuItem.Header = GetHeaderForTimer(timer);
                menuItem.Icon = GetIconForTimer(timer);
            }
        }
    }

    /// <summary>
    /// Returns an object that can be set for the <see cref="MenuItem.Header"/> of a <see cref="MenuItem"/> that
    /// displays a <see cref="Timer"/>.
    /// </summary>
    /// <param name="timer">A <see cref="Timer"/>.</param>
    /// <returns>An object that can be set for the <see cref="MenuItem.Header"/>.</returns>
    private object GetHeaderForTimer(Timer timer)
    {
        return timer.ToString();
    }

    /// <summary>
    /// Returns an object that can be set for the <see cref="MenuItem.Icon"/> of a <see cref="MenuItem"/> that
    /// displays a <see cref="Timer"/>.
    /// </summary>
    /// <param name="timer">A <see cref="Timer"/>.</param>
    /// <returns>An object that can be set for the <see cref="MenuItem.Icon"/>.</returns>
    private object GetIconForTimer(Timer timer)
    {
        Border outerBorder = new()
        {
            BorderBrush = new SolidColorBrush(Colors.LightGray),
            BorderThickness = new(1),
            CornerRadius = new(2),
            Width = 16,
            Height = 6
        };

        if (timer.State == TimerState.Expired)
        {
            Border progress = new()
            {
                Background = new SolidColorBrush(Color.FromRgb(199, 80, 80)),
                Width = 16,
                Height = 6
            };

            outerBorder.Child = progress;
        }
        else if (!timer.Options.ReverseProgressBar && timer.TimeLeftAsPercentage.HasValue)
        {
            Border progress = new()
            {
                Background = timer.Options.Theme.ProgressBarBrush,
                HorizontalAlignment = HorizontalAlignment.Left,
                Width = MathExtensions.LimitToRange(timer.TimeLeftAsPercentage.Value / 100.0 * 16.0, 0.0, 16.0),
                Height = 6
            };

            outerBorder.Child = progress;
        }
        else if (timer.Options.ReverseProgressBar && timer.TimeElapsedAsPercentage.HasValue)
        {
            Border progress = new()
            {
                Background = timer.Options.Theme.ProgressBarBrush,
                HorizontalAlignment = HorizontalAlignment.Left,
                Width = MathExtensions.LimitToRange(timer.TimeElapsedAsPercentage.Value / 100.0 * 16.0, 0.0, 16.0),
                Height = 6
            };

            outerBorder.Child = progress;
        }

        return outerBorder;
    }

    /// <summary>
    /// Invoked when a saved timer <see cref="MenuItem"/> is clicked.
    /// </summary>
    /// <param name="sender">The <see cref="MenuItem"/> where the event handler is attached.</param>
    /// <param name="e">The event data.</param>
    private void SavedTimerMenuItemClick(object sender, RoutedEventArgs e)
    {
        MenuItem menuItem = (MenuItem)sender;
        Timer savedTimer = (Timer)menuItem.Tag;
        ShowSavedTimer(savedTimer);
    }

    /// <summary>
    /// Invoked when the "Open all saved timers" <see cref="MenuItem"/> is clicked.
    /// </summary>
    /// <param name="sender">The <see cref="MenuItem"/> where the event handler is attached.</param>
    /// <param name="e">The event data.</param>
    private void OpenAllSavedTimersMenuItemClick(object sender, RoutedEventArgs e)
    {
        foreach (Timer savedTimer in TimerManager.Instance.ResumableTimers)
        {
            ShowSavedTimer(savedTimer);
        }
    }

    /// <summary>
    /// Shows an existing <see cref="Timer"/>.
    /// </summary>
    /// <param name="savedTimer">An existing <see cref="Timer"/>.</param>
    private void ShowSavedTimer(Timer savedTimer)
    {
        if (_timerWindow.Timer.State == TimerState.Stopped || _timerWindow.Timer.State == TimerState.Expired)
        {
            ShowSavedTimerInCurrentWindow(savedTimer);
        }
        else
        {
            ShowSavedTimerInNewWindow(savedTimer);
        }
    }

    /// <summary>
    /// Shows an existing <see cref="Timer"/> in the current <see cref="TimerWindow"/>.
    /// </summary>
    /// <param name="savedTimer">An existing <see cref="Timer"/>.</param>
    private void ShowSavedTimerInCurrentWindow(Timer savedTimer)
    {
        if (savedTimer.Options.WindowSize is not null)
        {
            _timerWindow.Restore(savedTimer.Options.WindowSize);
        }

        _timerWindow.Show(savedTimer);
        UpdateMenuFromOptions();
    }

    /// <summary>
    /// Shows an existing <see cref="Timer"/> in a new <see cref="TimerWindow"/>.
    /// </summary>
    /// <param name="savedTimer">An existing <see cref="Timer"/>.</param>
    private void ShowSavedTimerInNewWindow(Timer savedTimer)
    {
        TimerWindow newTimerWindow = new();

        if (savedTimer.Options.WindowSize is not null)
        {
            newTimerWindow.Restore(savedTimer.Options.WindowSize);
        }
        else
        {
            newTimerWindow.RestoreFromWindow(_timerWindow);
        }

        newTimerWindow.Show(savedTimer);
    }

    /// <summary>
    /// Invoked when the "Clear saved timers" <see cref="MenuItem"/> is clicked.
    /// </summary>
    /// <param name="sender">The <see cref="MenuItem"/> where the event handler is attached.</param>
    /// <param name="e">The event data.</param>
    private void ClearSavedTimersMenuItemClick(object sender, RoutedEventArgs e)
    {
        TimerManager.Instance.ClearResumableTimers();
    }

    #endregion

    #region Private Methods (Theme)

    /// <summary>
    /// Updates the <see cref="_themeMenuItem"/>.
    /// </summary>
    private void UpdateThemeMenuItem()
    {
        _themeMenuItem.Items.Clear();
        _selectableThemeMenuItems.Clear();

        // Switch between light and dark themes
        if (_lightThemeMenuItem is null)
        {
            _lightThemeMenuItem = new()
            {
                Header = Properties.Resources.ContextMenuLightThemeMenuItem,
                Tag = ThemeType.BuiltInLight
            };
            _lightThemeMenuItem.Click += ThemeTypeMenuItemClick;
        }

        _themeMenuItem.Items.Add(_lightThemeMenuItem);

        if (_darkThemeMenuItem is null)
        {
            _darkThemeMenuItem = new()
            {
                Header = Properties.Resources.ContextMenuDarkThemeMenuItem,
                Tag = ThemeType.BuiltInDark
            };
            _darkThemeMenuItem.Click += ThemeTypeMenuItemClick;
        }

        _themeMenuItem.Items.Add(_darkThemeMenuItem);

        // Built-in themes
        CreateThemeMenuItemsFromList(ThemeManager.Instance.BuiltInThemes);

        // User-provided themes
        if (ThemeManager.Instance.UserProvidedThemes.Count > 0)
        {
            CreateThemeMenuItemsFromList(ThemeManager.Instance.UserProvidedThemes);
        }

        // Manage themes
        _themeMenuItem.Items.Add(new Separator());

        if (_manageThemesMenuItem is null)
        {
            _manageThemesMenuItem = new()
            {
                Header = Properties.Resources.ContextMenuManageThemesMenuItem
            };
            _manageThemesMenuItem.Click += ManageThemesMenuItemClick;
        }

        _themeMenuItem.Items.Add(_manageThemesMenuItem);
    }

    /// <summary>
    /// Creates a <see cref="MenuItem"/> for each <see cref="Theme"/> in the collection.
    /// </summary>
    /// <param name="themes">A collection of <see cref="Theme"/>s.</param>
    private void CreateThemeMenuItemsFromList(IList<Theme> themes)
    {
        _themeMenuItem.Items.Add(new Separator());

        foreach (Theme theme in themes)
        {
            CreateThemeMenuItem(theme);
        }
    }

    /// <summary>
    /// Creates a <see cref="MenuItem"/> for a <see cref="Theme"/>.
    /// </summary>
    /// <param name="theme">A <see cref="Theme"/>.</param>
    private void CreateThemeMenuItem(Theme theme)
    {
        MenuItem menuItem = new()
        {
            Header = GetHeaderForTheme(theme),
            Tag = theme,
            IsCheckable = true
        };
        menuItem.Click += ThemeMenuItemClick;
        menuItem.Click += CheckableMenuItemClick;

        _themeMenuItem.Items.Add(menuItem);
        _selectableThemeMenuItems.Add(menuItem);
    }

    /// <summary>
    /// Returns an object that can be set for the <see cref="MenuItem.Header"/> of a <see cref="MenuItem"/> that
    /// displays a <see cref="Theme"/>.
    /// </summary>
    /// <param name="theme">A <see cref="Theme"/>.</param>
    /// <returns>An object that can be set for the <see cref="MenuItem.Header"/>.</returns>
    private object GetHeaderForTheme(Theme theme)
    {
        Border border = new()
        {
            Background = theme.ProgressBarBrush,
            CornerRadius = new(2),
            Width = 8,
            Height = 8
        };

        TextBlock textBlock = new()
        {
            Text = theme.Name ?? Properties.Resources.ContextMenuUnnamedTheme,
            Margin = new(5, 0, 0, 0)
        };

        StackPanel stackPanel = new()
        {
            Orientation = Orientation.Horizontal
        };
        stackPanel.Children.Add(border);
        stackPanel.Children.Add(textBlock);
        return stackPanel;
    }

    /// <summary>
    /// Invoked when a theme type <see cref="MenuItem"/> is clicked.
    /// </summary>
    /// <param name="sender">The <see cref="MenuItem"/> where the event handler is attached.</param>
    /// <param name="e">The event data.</param>
    private void ThemeTypeMenuItemClick(object sender, RoutedEventArgs e)
    {
        MenuItem clickedMenuItem = (MenuItem)sender;
        ThemeType type = (ThemeType)clickedMenuItem.Tag;

        _timerWindow.Options.Theme = type == ThemeType.BuiltInDark
            ? _timerWindow.Options.Theme.DarkVariant
            : _timerWindow.Options.Theme.LightVariant;
    }

    /// <summary>
    /// Invoked when a theme <see cref="MenuItem"/> is clicked.
    /// </summary>
    /// <param name="sender">The <see cref="MenuItem"/> where the event handler is attached.</param>
    /// <param name="e">The event data.</param>
    private void ThemeMenuItemClick(object sender, RoutedEventArgs e)
    {
        foreach (MenuItem menuItem in _selectableThemeMenuItems)
        {
            menuItem.IsChecked = ReferenceEquals(menuItem, sender);
        }

        MenuItem selectedMenuItem = (MenuItem)sender;
        _timerWindow.Options.Theme = (Theme)selectedMenuItem.Tag;
    }

    /// <summary>
    /// Invoked when the "Manage themes" <see cref="MenuItem"/> is clicked.
    /// </summary>
    /// <param name="sender">The <see cref="MenuItem"/> where the event handler is attached.</param>
    /// <param name="e">The event data.</param>
    private void ManageThemesMenuItemClick(object sender, RoutedEventArgs e)
    {
        ThemeManagerWindow window = Application.Current.Windows.OfType<ThemeManagerWindow>().FirstOrDefault();
        if (window is not null)
        {
            window.SetTimerWindow(_timerWindow);
            window.BringToFrontAndActivate();
        }
        else
        {
            window = new(_timerWindow);
            window.Show();
        }
    }

    #endregion

    #region Private Methods (Sound)

    /// <summary>
    /// Updates the <see cref="_soundMenuItem"/>.
    /// </summary>
    private void UpdateSoundMenuItem()
    {
        _soundMenuItem.Items.Clear();
        _selectableSoundMenuItems.Clear();

        // Sounds
        CreateSoundMenuItem(Sound.NoSound);
        CreateSoundMenuItemsFromList(SoundManager.Instance.BuiltInSounds);
        CreateSoundMenuItemsFromList(SoundManager.Instance.UserProvidedSounds);

        // Options
        _soundMenuItem.Items.Add(new Separator());

        if (_loopSoundMenuItem is null)
        {
            _loopSoundMenuItem = new()
            {
                Header = Properties.Resources.ContextMenuLoopSoundMenuItem,
                IsCheckable = true
            };
            _loopSoundMenuItem.Click += CheckableMenuItemClick;
        }

        _soundMenuItem.Items.Add(_loopSoundMenuItem);
    }

    /// <summary>
    /// Creates a <see cref="MenuItem"/> for a <see cref="Sound"/>.
    /// </summary>
    /// <param name="sound">A <see cref="Sound"/>.</param>
    private void CreateSoundMenuItem(Sound sound)
    {
        MenuItem menuItem = new()
        {
            Header = sound is not null ? sound.Name : Properties.Resources.ContextMenuNoSoundMenuItem,
            Tag = sound,
            IsCheckable = true
        };
        menuItem.Click += SoundMenuItemClick;
        menuItem.Click += CheckableMenuItemClick;

        _soundMenuItem.Items.Add(menuItem);
        _selectableSoundMenuItems.Add(menuItem);
    }

    /// <summary>
    /// Creates a <see cref="MenuItem"/> for each <see cref="Sound"/> in the collection.
    /// </summary>
    /// <param name="sounds">A collection of <see cref="Sound"/>s.</param>
    private void CreateSoundMenuItemsFromList(IList<Sound> sounds)
    {
        if (sounds.Count > 0)
        {
            _soundMenuItem.Items.Add(new Separator());
            foreach (Sound sound in sounds)
            {
                CreateSoundMenuItem(sound);
            }
        }
    }

    /// <summary>
    /// Invoked when a sound <see cref="MenuItem"/> is clicked.
    /// </summary>
    /// <param name="sender">The <see cref="MenuItem"/> where the event handler is attached.</param>
    /// <param name="e">The event data.</param>
    private void SoundMenuItemClick(object sender, RoutedEventArgs e)
    {
        foreach (MenuItem menuItem in _selectableSoundMenuItems)
        {
            menuItem.IsChecked = ReferenceEquals(menuItem, sender);
        }
    }

    #endregion

    /// <summary>
    /// Invoked when a window title <see cref="MenuItem"/> is clicked.
    /// </summary>
    /// <param name="sender">The <see cref="MenuItem"/> where the event handler is attached.</param>
    /// <param name="e">The event data.</param>
    private void WindowTitleMenuItemClick(object sender, RoutedEventArgs e)
    {
        foreach (MenuItem menuItem in _selectableWindowTitleMenuItems)
        {
            menuItem.IsChecked = ReferenceEquals(menuItem, sender);
        }
    }

    #region Private Methods (Window State)

    /// <summary>
    /// Updates the <see cref="_restoreMenuItem"/>, <see cref="_minimizeMenuItem"/>, and
    /// <see cref="_maximizeMenuItem"/>.
    /// </summary>
    private void UpdateWindowStateMenuItems()
    {
        _restoreMenuItem.IsEnabled = _timerWindow.WindowState != WindowState.Normal;
        _minimizeMenuItem.IsEnabled = _timerWindow.WindowState != WindowState.Minimized;
        _maximizeMenuItem.IsEnabled = _timerWindow.WindowState != WindowState.Maximized;

        if (_timerWindow.IsFullScreen || _timerWindow.Options.WindowTitleMode == WindowTitleMode.None)
        {
            // "Restore", "Minimize", and "Maximize" are not on the window, so we provide our own.
            _restoreMenuItem.Visibility = Visibility.Visible;
            _minimizeMenuItem.Visibility = Visibility.Visible;
            _maximizeMenuItem.Visibility = Visibility.Visible;
            _windowStateItemsSeparator.Visibility = Visibility.Visible;
        }
        else
        {
            // "Restore", "Minimize", and "Maximize" are on the window, so no need for them here.
            _restoreMenuItem.Visibility = Visibility.Collapsed;
            _minimizeMenuItem.Visibility = Visibility.Collapsed;
            _maximizeMenuItem.Visibility = Visibility.Collapsed;
            _windowStateItemsSeparator.Visibility = Visibility.Collapsed;
        }
    }

    private void FAQMenuItemClick(object sender, RoutedEventArgs e)
    {
        Consts.FAQUri.Navigate();
    }

    private void UsageMenuItemClick(object sender, RoutedEventArgs e)
    {
        CommandLineArguments.ShowUsage();
    }

    /// <summary>
    /// Invoked when the "About" <see cref="MenuItem"/> is clicked.
    /// </summary>
    /// <param name="sender">The <see cref="MenuItem"/> where the event handler is attached.</param>
    /// <param name="e">The event data.</param>
    private void AboutMenuItemClick(object sender, RoutedEventArgs e)
    {
        AboutDialog.ShowOrActivate();
    }

    /// <summary>
    /// Invoked when the "Restore" <see cref="MenuItem"/> is clicked.
    /// </summary>
    /// <param name="sender">The <see cref="MenuItem"/> where the event handler is attached.</param>
    /// <param name="e">The event data.</param>
    private void RestoreMenuItemClick(object sender, RoutedEventArgs e)
    {
        if (_timerWindow.IsFullScreen)
        {
            // Must set the menu item value here, since it will sync to the TimerWindow on menu close.
            _fullScreenMenuItem.IsChecked = false;
            _timerWindow.IsFullScreen = false;
        }
        else
        {
            _timerWindow.WindowState = WindowState.Normal;
        }
    }

    /// <summary>
    /// Invoked when the "Minimize" <see cref="MenuItem"/> is clicked.
    /// </summary>
    /// <param name="sender">The <see cref="MenuItem"/> where the event handler is attached.</param>
    /// <param name="e">The event data.</param>
    private void MinimizeMenuItemClick(object sender, RoutedEventArgs e)
    {
        _timerWindow.WindowState = WindowState.Minimized;
    }

    /// <summary>
    /// Invoked when the "Maximize" <see cref="MenuItem"/> is clicked.
    /// </summary>
    /// <param name="sender">The <see cref="MenuItem"/> where the event handler is attached.</param>
    /// <param name="e">The event data.</param>
    private void MaximizeMenuItemClick(object sender, RoutedEventArgs e)
    {
        _timerWindow.WindowState = WindowState.Maximized;
    }

    #endregion

    /// <summary>
    /// Invoked when the "Close" <see cref="MenuItem"/> is clicked.
    /// </summary>
    /// <param name="sender">The <see cref="MenuItem"/> where the event handler is attached.</param>
    /// <param name="e">The event data.</param>
    private void CloseMenuItemClick(object sender, RoutedEventArgs e)
    {
        _timerWindow.Close();
    }
}