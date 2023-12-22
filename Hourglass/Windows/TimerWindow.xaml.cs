﻿// --------------------------------------------------------------------------------------------------------------------
// <copyright file="TimerWindow.xaml.cs" company="Chris Dziemborowicz">
//   Copyright (c) Chris Dziemborowicz. All rights reserved.
// </copyright>
// --------------------------------------------------------------------------------------------------------------------

namespace Hourglass.Windows;

using System;
using System.ComponentModel;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media.Animation;
using System.Windows.Shell;

using Extensions;
using Managers;
using Properties;
using Timing;

/// <summary>
/// The mode of a <see cref="TimerWindow"/>.
/// </summary>
public enum TimerWindowMode
{
    /// <summary>
    /// Indicates that the <see cref="TimerWindow"/> is accepting user input to start a new timer.
    /// </summary>
    Input,

    /// <summary>
    /// Indicates that the <see cref="TimerWindow"/> is displaying the status of a timer.
    /// </summary>
    Status
}

/// <summary>
/// A timer window.
/// </summary>
public sealed partial class TimerWindow : INotifyPropertyChanged, IRestorableWindow
{
    #region Commands

    /// <summary>
    /// Starts a new timer.
    /// </summary>
    public static readonly RoutedCommand StartCommand = new();

    /// <summary>
    /// Pauses a running timer.
    /// </summary>
    public static readonly RoutedCommand PauseCommand = new();

    /// <summary>
    /// Resumes a paused timer.
    /// </summary>
    public static readonly RoutedCommand ResumeCommand = new();

    /// <summary>
    /// Resumes a paused or resumes timer.
    /// </summary>
    public static readonly RoutedCommand PauseResumeCommand = new();

    /// <summary>
    /// Stops a running timer and enters input mode.
    /// </summary>
    public static readonly RoutedCommand StopCommand = new();

    /// <summary>
    /// Restarts the timer.
    /// </summary>
    public static readonly RoutedCommand RestartCommand = new();

    /// <summary>
    /// Closes the window.
    /// </summary>
    public static readonly RoutedCommand CloseCommand = new();

    /// <summary>
    /// Cancels editing.
    /// </summary>
    public static readonly RoutedCommand CancelCommand = new();

    /// <summary>
    /// Updates the app.
    /// </summary>
    public static readonly RoutedCommand UpdateCommand = new();

    /// <summary>
    /// Exits input mode, enters input mode, or exits full-screen mode depending on the state of the window.
    /// </summary>
    public static readonly RoutedCommand EscapeCommand = new();

    /// <summary>
    /// Toggles full-screen mode.
    /// </summary>
    public static readonly RoutedCommand FullScreenCommand = new();

    #endregion

    #region Private Members

    /// <summary>
    /// The <see cref="InterfaceScaler"/> for the window.
    /// </summary>
    private readonly InterfaceScaler _scaler = new();

    /// <summary>
    /// The <see cref="SoundPlayer"/> used to play notification sounds.
    /// </summary>
    private readonly SoundPlayer _soundPlayer = new();

    /// <summary>
    /// The <see cref="TimerWindowMode"/> of the window.
    /// </summary>
    private TimerWindowMode _mode;

    /// <summary>
    /// The timer backing the window.
    /// </summary>
    private Timer _timer = new(TimerOptionsManager.Instance.MostRecentOptions);

    /// <summary>
    /// The timer to resume when the window loads, or <c>null</c> if no timer is to be resumed.
    /// </summary>
    private Timer _timerToResumeOnLoad;

    /// <summary>
    /// The <see cref="TimerStart"/> to start when the window loads, or <c>null</c> if no <see cref="TimerStart"/>
    /// is to be started.
    /// </summary>
    private TimerStart _timerStartToStartOnLoad;

    /// <summary>
    /// The last <see cref="TimerStart"/> used to start a timer in the window.
    /// </summary>
    private TimerStart _lastTimerStart = TimerStartManager.Instance.LastTimerStart;

    /// <summary>
    /// The currently loaded theme.
    /// </summary>
    private Theme _theme;

    /// <summary>
    /// The number of times the flash expiration storyboard has completed since the timer last expired.
    /// </summary>
    private int _flashExpirationCount;

    /// <summary>
    /// The storyboard that flashes red to notify the user that the timer has expired.
    /// </summary>
    private Storyboard _flashExpirationStoryboard;

    /// <summary>
    /// The storyboard that glows red to notify the user that the timer has expired.
    /// </summary>
    private Storyboard _glowExpirationStoryboard;

    /// <summary>
    /// The storyboard that flashes red to notify the user that the input was invalid.
    /// </summary>
    private Storyboard _validationErrorStoryboard;

    /// <summary>
    /// A value indicating whether the window is in full-screen mode.
    /// </summary>
    private bool _isFullScreen;

    /// <summary>
    /// The <see cref="Window.WindowState"/> before the window was minimized.
    /// </summary>
    private WindowState _restoreWindowState = WindowState.Normal;

    #endregion

    #region Constructors

    /// <summary>
    /// Initializes a new instance of the <see cref="TimerWindow"/> class.
    /// </summary>
    public TimerWindow()
    {
        InitializeComponent();
        InitializeResources();
        InitializeAnimations();
        InitializeSoundPlayer();
        InitializeUpdateButton();

        BindTimer();
        SwitchToInputMode();

        Menu.Bind(this /* window */);
        _scaler.Bind(this /* window */);

        TimerManager.Instance.Add(Timer);
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="TimerWindow"/> class.
    /// </summary>
    /// <param name="timer">The timer to resume when the window loads, or <c>null</c> if no timer is to be resumed.
    /// </param>
    public TimerWindow(Timer timer)
        : this()
    {
        _timerToResumeOnLoad = timer;
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="TimerWindow"/> class.
    /// </summary>
    /// <param name="timerStart">The <see cref="TimerStart"/> to start when the window loads, or <c>null</c> if no
    /// <see cref="TimerStart"/> is to be started.</param>
    public TimerWindow(TimerStart timerStart)
        : this()
    {
        _timerStartToStartOnLoad = timerStart;
    }

    #endregion

    /// <summary>
    /// Raised when a property value changes.
    /// </summary>
    public event PropertyChangedEventHandler PropertyChanged;

    #region Properties

    /// <summary>
    /// Gets the <see cref="WindowSize"/> for the window persisted in the settings.
    /// </summary>
    public WindowSize PersistedSize => Settings.Default.WindowSize;

    /// <summary>
    /// Gets the <see cref="TimerWindowMode"/> of the window.
    /// </summary>
    public TimerWindowMode Mode
    {
        get => _mode;

        private set
        {
            if (_mode == value)
            {
                return;
            }

            _mode = value;
            OnPropertyChanged(nameof(Mode));
        }
    }

    /// <summary>
    /// Gets the <see cref="ContextMenu"/> for the window.
    /// </summary>
    public ContextMenu Menu { get; } = new();

    /// <summary>
    /// Gets the timer backing the window.
    /// </summary>
    public Timer Timer
    {
        get => _timer;

        private set
        {
            if (_timer == value)
            {
                return;
            }

            UnbindTimer();
            _timer = value;
            BindTimer();
            OnPropertyChanged(
                nameof(Timer),
                nameof(Options));
        }
    }

    /// <summary>
    /// Gets the <see cref="TimerOptions"/> for the timer backing the window.
    /// </summary>
    public TimerOptions Options => Timer.Options;

    /// <summary>
    /// Gets the last <see cref="TimerStart"/> used to start a timer in the window.
    /// </summary>
    public TimerStart LastTimerStart
    {
        get => _lastTimerStart;

        private set
        {
            if (_lastTimerStart == value)
            {
                return;
            }

            _lastTimerStart = value;
            OnPropertyChanged(nameof(LastTimerStart));
        }
    }

    /// <summary>
    /// Gets the currently loaded theme.
    /// </summary>
    public Theme Theme => _theme;

    /// <summary>
    /// Gets or sets a value indicating whether the window is in full-screen mode.
    /// </summary>
    public bool IsFullScreen
    {
        get => _isFullScreen;

        set
        {
            if (_isFullScreen == value)
            {
                return;
            }

            _isFullScreen = value;

            if (_isFullScreen)
            {
                WindowStyle = WindowStyle.None;
                WindowState = WindowState.Normal; // Needed to put the window on top of the taskbar
                WindowState = WindowState.Maximized;
                ResizeMode = ResizeMode.NoResize;
            }
            else
            {
                WindowStyle = WindowStyle.SingleBorderWindow;
                WindowState = _restoreWindowState;
                ResizeMode = ResizeMode.CanResize;
            }

            OnPropertyChanged(nameof(IsFullScreen));
        }
    }

    /// <summary>
    /// Gets or sets the <see cref="Window.WindowState"/> before the window was minimized.
    /// </summary>
    public WindowState RestoreWindowState
    {
        get => _restoreWindowState;

        set
        {
            if (_restoreWindowState == value)
            {
                return;
            }

            _restoreWindowState = value;
            OnPropertyChanged(nameof(RestoreWindowState));
        }
    }

    public bool DoNotPromptOnExit { get; set; }

    public bool DoNotActivateNextWindow { get; set; }

    public bool ForceClose { get; set; }

    #endregion

    #region Public Methods

    /// <summary>
    /// Opens the <see cref="TimerWindow"/> if it is not already open and displays a new timer started with the
    /// specified <see cref="TimerStart"/>.
    /// </summary>
    /// <param name="timerStart">A <see cref="TimerStart"/>.</param>
    /// <param name="remember">A value indicating whether to remember the <see cref="TimerStart"/> as a recent
    /// input.</param>
    public void Show(TimerStart timerStart, bool remember = true)
    {
        // Keep track of the input
        LastTimerStart = timerStart;

        // Start a new timer
        Timer newTimer = new(Options);
        if (!newTimer.Start(timerStart))
        {
            // The user has started a timer that expired in the past
            if (Options.LockInterface)
            {
                // If the interface is locked, there is nothing the user can do or should be able to do other than
                // close the window, so pretend that the timer expired immediately
                Show(TimerStart.Zero, false /* remember */);
                return;
            }
            else
            {
                // Otherwise, assume the user made an error and display a validation error animation
                Show();
                SwitchToInputMode();
                BeginValidationErrorAnimation();
                return;
            }
        }

        TimerManager.Instance.Add(newTimer);

        if (remember)
        {
            TimerStartManager.Instance.Add(timerStart);
        }

        // Show the window
        Show(newTimer);
    }

    /// <summary>
    /// Opens the <see cref="TimerWindow"/> if it is not already open and displays the specified timer.
    /// </summary>
    /// <param name="existingTimer">A timer.</param>
    public void Show(Timer existingTimer)
    {
        // Show the status of the existing timer
        Timer = existingTimer;
        SwitchToStatusMode();

        // Show the window if it is not already open
        if (!IsVisible)
        {
            Show();
        }

        // Notify expiration if the existing timer is expired
        if (Timer.State == TimerState.Expired)
        {
            if (Options.LoopSound)
            {
                BeginExpirationAnimationAndSound();
            }
            else
            {
                BeginExpirationAnimation(true /* glowOnly */);
            }
        }
    }

    /// <summary>
    /// Brings the window to the front, activates it, and focuses it.
    /// </summary>
    public void BringToFrontAndActivate()
    {
        try
        {
            Show();

            if (WindowState == WindowState.Minimized)
            {
                WindowState = RestoreWindowState;
            }

            Topmost = false;
            Topmost = true;
            Topmost = Options.AlwaysOnTop;

            Activate();
        }
        catch (InvalidOperationException)
        {
            // This happens if the window is closing (waiting for the user to confirm) when this method is called
        }
    }

    /// <summary>
    /// Minimizes the window to the notification area of the taskbar.
    /// </summary>
    /// <remarks>
    /// This method does nothing if <see cref="Settings.ShowInNotificationArea"/> is <c>false</c>.
    /// </remarks>
    public void MinimizeToNotificationArea()
    {
        if (Settings.Default.ShowInNotificationArea)
        {
            if (WindowState != WindowState.Minimized)
            {
                RestoreWindowState = WindowState;
                WindowState = WindowState.Minimized;
            }

            Hide();
        }
    }

    /// <summary>
    /// Returns a string that represents the current object.
    /// </summary>
    /// <returns>A string that represents the current object.</returns>
    public override string ToString()
    {
        if (Timer.State == TimerState.Stopped && Mode == TimerWindowMode.Input)
        {
            string input = string.IsNullOrWhiteSpace(TimerTextBox.Text)
                ? Properties.Resources.TimerWindowBlankTitlePlaceholder
                : TimerTextBox.Text;

            string title = TitleTextBox.Text;

            string format = string.IsNullOrWhiteSpace(title)
                ? Properties.Resources.TimerWindwoNewTimerFormatString
                : Properties.Resources.TimerWindowNewTimerWithTitleFormatString;

            return string.Format(format, input, title);
        }

        return Timer.ToString();
    }

    #endregion

    /// <summary>
    /// Raises the <see cref="PropertyChanged"/> event.
    /// </summary>
    /// <param name="propertyNames">One or more property names.</param>
    private void OnPropertyChanged(params string[] propertyNames)
    {
        PropertyChangedEventHandler eventHandler = PropertyChanged;

        if (eventHandler is not null)
        {
            foreach (string propertyName in propertyNames)
            {
                eventHandler(this, new(propertyName));
            }
        }
    }

    #region Private Methods (Modes)

    /// <summary>
    /// Sets the window to accept user input to start a new <see cref="Timer"/>.
    /// </summary>
    /// <param name="textBoxToFocus">The <see cref="TextBox"/> to focus. The default is <see cref="TimerTextBox"/>.
    /// </param>
    private void SwitchToInputMode(TextBox textBoxToFocus = null)
    {
        Mode = TimerWindowMode.Input;

        TitleTextBox.Text = Timer.Options.Title;
        TimerTextBox.Text = LastTimerStart?.ToString() ?? string.Empty;

        textBoxToFocus ??= TimerTextBox;
        textBoxToFocus.SelectAll();
        textBoxToFocus.Focus();

        EndAnimationsAndSounds();
        UpdateBoundControls();
    }

    /// <summary>
    /// Sets the window to display the status of a running or paused <see cref="Timer"/>.
    /// </summary>
    private void SwitchToStatusMode()
    {
        Mode = TimerWindowMode.Status;

        UnfocusAll();
        EndAnimationsAndSounds();
        UpdateBoundControls();
    }

    /// <summary>
    /// <para>
    /// When the window is in input mode, this method switches back to status mode if there is a running or paused
    /// timer, or stops the notification sound if it is playing.
    /// </para><para>
    /// When the window is in status mode, this method switches to input mode if the timer is expired or stops the
    /// notification sound if it is playing.
    /// </para>
    /// </summary>
    /// <remarks>
    /// This is invoked when the user presses the Escape key, or performs an equivalent action.
    /// </remarks>
    /// <returns>A value indicating whether any action was performed.</returns>
    private bool CancelOrReset()
    {
        switch (Mode)
        {
            case TimerWindowMode.Input:
                // Switch back to showing the running timer if there is one
                if (Timer.State != TimerState.Stopped && Timer.State != TimerState.Expired)
                {
                    SwitchToStatusMode();
                    return true;
                }

                // Stop playing the notification sound if it is playing
                if (_soundPlayer.IsPlaying)
                {
                    EndAnimationsAndSounds();
                    return true;
                }

                return false;

            case TimerWindowMode.Status:
                // Switch to input mode if the timer is expired and the interface is not locked
                if (Timer.State == TimerState.Expired && !Options.LockInterface)
                {
                    Timer.Stop();
                    SwitchToInputMode();
                    return true;
                }

                // Stop playing the notification sound if it is playing
                if (_soundPlayer.IsPlaying)
                {
                    EndAnimationsAndSounds();
                    return true;
                }

                // Stop editing and un-focuses buttons if focused
                return UnfocusAll();
        }

        return false;
    }

    /// <summary>
    /// Removes focus from all controls.
    /// </summary>
    /// <returns>A value indicating whether the focus was removed from any element.</returns>
    private bool UnfocusAll()
    {
        return TitleTextBox.Unfocus()
               || TimerTextBox.Unfocus()
               || StartButton.Unfocus()
               || PauseButton.Unfocus()
               || ResumeButton.Unfocus()
               || StopButton.Unfocus()
               || RestartButton.Unfocus()
               || CloseButton.Unfocus()
               || CancelButton.Unfocus()
               || UpdateButton.Unfocus();
    }

    #endregion

    /// <summary>
    /// Initializes localized resources.
    /// </summary>
    private void InitializeResources()
    {
        Watermark.SetHint(TitleTextBox, Properties.Resources.TimerWindowTitleTextHint);
        Watermark.SetHint(TimerTextBox, Properties.Resources.TimerWindowTimerTextHint);

        StartButton.Content = Properties.Resources.TimerWindowStartButtonContent;
        PauseButton.Content = Properties.Resources.TimerWindowPauseButtonContent;
        ResumeButton.Content = Properties.Resources.TimerWindowResumeButtonContent;
        StopButton.Content = Properties.Resources.TimerWindowStopButtonContent;
        RestartButton.Content = Properties.Resources.TimerWindowRestartButtonContent;
        CloseButton.Content = Properties.Resources.TimerWindowCloseButtonContent;
        CancelButton.Content = Properties.Resources.TimerWindowCancelButtonContent;
        UpdateButton.Content = Properties.Resources.TimerWindowUpdateButtonContent;
    }

    #region Private Methods (Animations and Sounds)

    /// <summary>
    /// Initializes the animation members.
    /// </summary>
    private void InitializeAnimations()
    {
        // Flash expiration storyboard
        DoubleAnimation outerFlashAnimation = new(1.0, 0.0, new(TimeSpan.FromSeconds(0.2)))
        {
            EasingFunction = new ExponentialEase { EasingMode = EasingMode.EaseOut }
        };
        Storyboard.SetTarget(outerFlashAnimation, OuterNotificationBorder);
        Storyboard.SetTargetProperty(outerFlashAnimation, new(OpacityProperty));

        DoubleAnimation innerFlashAnimation = new(1.0, 0.0, new(TimeSpan.FromSeconds(0.2)))
        {
            EasingFunction = new ExponentialEase { EasingMode = EasingMode.EaseOut }
        };
        Storyboard.SetTarget(innerFlashAnimation, InnerNotificationBorder);
        Storyboard.SetTargetProperty(innerFlashAnimation, new(OpacityProperty));

        _flashExpirationStoryboard = new();
        _flashExpirationStoryboard.Children.Add(outerFlashAnimation);
        _flashExpirationStoryboard.Children.Add(innerFlashAnimation);
        _flashExpirationStoryboard.Completed += FlashExpirationStoryboardCompleted;
        Storyboard.SetTarget(_flashExpirationStoryboard, this);

        // Glow expiration storyboard
        DoubleAnimation outerGlowAnimation = new(1.0, 0.5, new(TimeSpan.FromSeconds(1.5)))
        {
            EasingFunction = new ExponentialEase { EasingMode = EasingMode.EaseInOut }
        };
        Storyboard.SetTarget(outerGlowAnimation, OuterNotificationBorder);
        Storyboard.SetTargetProperty(outerGlowAnimation, new(OpacityProperty));

        DoubleAnimation innerGlowAnimation = new(1.0, 0.5, new(TimeSpan.FromSeconds(1.5)))
        {
            EasingFunction = new ExponentialEase { EasingMode = EasingMode.EaseInOut }
        };
        Storyboard.SetTarget(innerGlowAnimation, InnerNotificationBorder);
        Storyboard.SetTargetProperty(innerGlowAnimation, new(OpacityProperty));

        _glowExpirationStoryboard = new();
        _glowExpirationStoryboard.Children.Add(outerGlowAnimation);
        _glowExpirationStoryboard.Children.Add(innerGlowAnimation);
        _glowExpirationStoryboard.AutoReverse = true;
        _glowExpirationStoryboard.RepeatBehavior = RepeatBehavior.Forever;
        Storyboard.SetTarget(_glowExpirationStoryboard, this);

        // Validation error storyboard
        DoubleAnimation validationErrorAnimation = new(1.0, 0.0, new(TimeSpan.FromSeconds(1)))
        {
            EasingFunction = new ExponentialEase { EasingMode = EasingMode.EaseOut }
        };
        Storyboard.SetTarget(validationErrorAnimation, InnerNotificationBorder);
        Storyboard.SetTargetProperty(validationErrorAnimation, new(OpacityProperty));

        _validationErrorStoryboard = new();
        _validationErrorStoryboard.Children.Add(validationErrorAnimation);
        Storyboard.SetTarget(_validationErrorStoryboard, this);
    }

    /// <summary>
    /// Initializes the sound player.
    /// </summary>
    private void InitializeSoundPlayer()
    {
        _soundPlayer.PlaybackStarted += SoundPlayerPlaybackStarted;
        _soundPlayer.PlaybackStopped += SoundPlayerPlaybackStopped;
        _soundPlayer.PlaybackCompleted += SoundPlayerPlaybackCompleted;
    }

    /// <summary>
    /// Begins the animation used to notify the user that the timer has expired.
    /// </summary>
    /// <param name="glowOnly"><c>true</c> to show the glow animation only, or <c>false</c> to show the flash
    /// animation followed by the glow animation. Default is <c>false</c>.</param>
    private void BeginExpirationAnimation(bool glowOnly = false)
    {
        // Begin animation
        if (glowOnly)
        {
            _glowExpirationStoryboard.Stop();
            _glowExpirationStoryboard.Begin();
        }
        else
        {
            _flashExpirationCount = 0;
            _flashExpirationStoryboard.Stop();
            _flashExpirationStoryboard.Begin();
        }

        // Bring the window to the front if required
        if (Options.PopUpWhenExpired)
        {
            BringToFrontAndActivate();
        }
        else if (Settings.Default.ShowInNotificationArea && !IsVisible)
        {
            NotificationAreaIconManager.Instance.NotifyIcon.ShowBalloonTipForExpiredTimer();
        }
    }

    /// <summary>
    /// Begins the sound used to notify the user that the timer has expired.
    /// </summary>
    private void BeginExpirationSound()
    {
        _soundPlayer.Play(Options.Sound, Options.LoopSound);
    }

    /// <summary>
    /// Begins the animation and sound used to notify the user that the timer has expired.
    /// </summary>
    private void BeginExpirationAnimationAndSound()
    {
        BeginExpirationAnimation();
        BeginExpirationSound();
    }

    /// <summary>
    /// Begins the animation used notify the user that the input was invalid.
    /// </summary>
    private void BeginValidationErrorAnimation()
    {
        _validationErrorStoryboard.Stop();
        _validationErrorStoryboard.Begin();
    }

    /// <summary>
    /// Stops all animations and sounds.
    /// </summary>
    private void EndAnimationsAndSounds()
    {
        _flashExpirationCount = 0;
        _flashExpirationStoryboard.Stop();
        _glowExpirationStoryboard.Stop();
        _validationErrorStoryboard.Stop();

        _soundPlayer.Stop();
    }

    /// <summary>
    /// Invoked when the flash expiration storyboard has completely finished playing.
    /// </summary>
    /// <param name="sender">The originator of the event.</param>
    /// <param name="e">The event data.</param>
    private void FlashExpirationStoryboardCompleted(object sender, EventArgs e)
    {
        _flashExpirationCount++;

        switch (Mode)
        {
            case TimerWindowMode.Input:
                // Flash three times, or flash indefinitely if the sound is looped
                if (_flashExpirationCount < 3 || Options.LoopSound)
                {
                    _flashExpirationStoryboard.Begin();
                }

                break;

            case TimerWindowMode.Status:
                if (Options.LoopTimer && _timer.SupportsLooping)
                {
                    // Flash three times, or flash indefinitely if the sound is looped
                    if (_flashExpirationCount < 3 || Options.LoopSound)
                    {
                        _flashExpirationStoryboard.Begin();
                    }
                }
                else if (Options.Sound is null && Options.ShutDownWhenExpired)
                {
                    // Flash three times and then shut down -- see SoundPlayerPlaybackCompleted for case with sound
                    if (_flashExpirationCount < 3)
                    {
                        _flashExpirationStoryboard.Begin();
                    }
                    else
                    {
                        WindowsExtensions.ShutDown();
                    }
                }
                else if (Options.Sound is null && Options.CloseWhenExpired)
                {
                    // Flash three times and then close -- see SoundPlayerPlaybackCompleted for case with sound
                    if (_flashExpirationCount < 3)
                    {
                        _flashExpirationStoryboard.Begin();
                    }
                    else
                    {
                        Close();
                    }
                }
                else
                {
                    // Flash three times and then glow, or flash indefinitely if the sound is looped
                    if (_flashExpirationCount < 2 || Options.LoopSound)
                    {
                        _flashExpirationStoryboard.Begin();
                    }
                    else
                    {
                        _glowExpirationStoryboard.Begin();
                    }
                }

                break;
        }
    }

    /// <summary>
    /// Invoked when sound playback has started.
    /// </summary>
    /// <param name="sender">A <see cref="SoundPlayer"/>.</param>
    /// <param name="e">The event data.</param>
    private void SoundPlayerPlaybackStarted(object sender, EventArgs e)
    {
        // Do nothing
    }

    /// <summary>
    /// Invoked when sound playback has stopped.
    /// </summary>
    /// <param name="sender">A <see cref="SoundPlayer"/>.</param>
    /// <param name="e">The event data.</param>
    private void SoundPlayerPlaybackStopped(object sender, EventArgs e)
    {
        // Do nothing
    }

    /// <summary>
    /// Invoked when sound playback has completed.
    /// </summary>
    /// <param name="sender">A <see cref="SoundPlayer"/>.</param>
    /// <param name="e">The event data.</param>
    private void SoundPlayerPlaybackCompleted(object sender, EventArgs e)
    {
        if (!Options.LoopTimer && Mode == TimerWindowMode.Status)
        {
            if (Options.ShutDownWhenExpired)
            {
                WindowsExtensions.ShutDown();
            }
            else if (Options.CloseWhenExpired)
            {
                Close();
            }
        }
    }

    #endregion

    #region Private Methods (Update Button)

    /// <summary>
    /// Initializes the update button.
    /// </summary>
    private void InitializeUpdateButton()
    {
        UpdateManager.Instance.PropertyChanged += UpdateManagerPropertyChanged;
        UpdateButton.IsEnabled = UpdateManager.Instance.HasUpdates && (Mode == TimerWindowMode.Input || !Options.LockInterface);
    }

    /// <summary>
    /// Invoked when a <see cref="UpdateManager"/> property value changes.
    /// </summary>
    /// <param name="sender">The <see cref="UpdateManager"/>.</param>
    /// <param name="e">The event data.</param>
    private void UpdateManagerPropertyChanged(object sender, PropertyChangedEventArgs e)
    {
        Dispatcher.BeginInvoke(new Action(() =>
        {
            UpdateButton.IsEnabled = UpdateManager.Instance.HasUpdates && (Mode == TimerWindowMode.Input || !Options.LockInterface);
        }));
    }

    #endregion

    #region Private Methods (Timer Binding)

    /// <summary>
    /// Binds the <see cref="TimerWindow"/> event handlers and controls to a timer.
    /// </summary>
    private void BindTimer()
    {
        Timer.Started += TimerStarted;
        Timer.Paused += TimerPaused;
        Timer.Resumed += TimerResumed;
        Timer.Stopped += TimerStopped;
        Timer.Expired += TimerExpired;
        Timer.Tick += TimerTick;
        Timer.PropertyChanged += TimerPropertyChanged;
        Options.PropertyChanged += TimerOptionsPropertyChanged;

        _theme = Options.Theme;
        _theme.PropertyChanged += ThemePropertyChanged;

        UpdateBoundControls();
    }

    /// <summary>
    /// Returns the progress bar value for the current timer.
    /// </summary>
    /// <returns>The progress bar value for the current timer.</returns>
    private double GetProgressBarValue()
    {
        if (Options.ReverseProgressBar)
        {
            return Timer.TimeElapsedAsPercentage ?? 100.0;
        }
        else
        {
            return Timer.TimeLeftAsPercentage ?? 0.0;
        }
    }

    /// <summary>
    /// Updates the controls bound to timer properties.
    /// </summary>
    private void UpdateBoundControls()
    {
        switch (Mode)
        {
            case TimerWindowMode.Input:
                ProgressBar.Value = GetProgressBarValue();
                UpdateTaskbarProgress();

                // Enable and disable command buttons as required
                StartButton.IsEnabled = true;
                PauseButton.IsEnabled = false;
                ResumeButton.IsEnabled = false;
                StopButton.IsEnabled = false;
                RestartButton.IsEnabled = false;
                CloseButton.IsEnabled = false;
                CancelButton.IsEnabled = Timer.State != TimerState.Stopped && Timer.State != TimerState.Expired;
                UpdateButton.IsEnabled = UpdateManager.Instance.HasUpdates;

                // Restore the border, context menu, and watermark text that appear for the text boxes
                TitleTextBox.BorderThickness = new(1);
                TimerTextBox.BorderThickness = new(1);
                TitleTextBox.IsReadOnly = false;
                TimerTextBox.IsReadOnly = false;
                Watermark.SetHint(TitleTextBox, Properties.Resources.TimerWindowTitleTextHint);
                Watermark.SetHint(TimerTextBox, Properties.Resources.TimerWindowTimerTextHint);

                Topmost = Options.AlwaysOnTop;

                UpdateBoundTheme();
                UpdateKeepAwake();
                UpdateWindowTitle();
                UpdateWindowChrome();
                return;

            case TimerWindowMode.Status:
                if (Timer.State == TimerState.Expired && !string.IsNullOrWhiteSpace(Timer.Options.Title) && !TitleTextBox.IsFocused)
                {
                    TitleTextBox.TextChanged -= TitleTextBoxTextChanged;
                    TitleTextBox.Text = Timer.Options.ShowTimeElapsed
                        ? Timer.TimeElapsedAsString
                        : Timer.TimeLeftAsString;
                    TitleTextBox.TextChanged += TitleTextBoxTextChanged;

                    TimerTextBox.Text = Timer.Options.Title;
                }
                else
                {
                    TitleTextBox.TextChanged -= TitleTextBoxTextChanged;
                    TitleTextBox.Text = Timer.Options.Title;
                    TitleTextBox.TextChanged += TitleTextBoxTextChanged;

                    TimerTextBox.Text = Timer.Options.ShowTimeElapsed
                        ? Timer.TimeElapsedAsString
                        : Timer.TimeLeftAsString;
                }

                ProgressBar.Value = GetProgressBarValue();
                UpdateTaskbarProgress();

                if (Options.LockInterface)
                {
                    // Disable command buttons except for close when stopped or expired
                    StartButton.IsEnabled = false;
                    PauseButton.IsEnabled = false;
                    ResumeButton.IsEnabled = false;
                    StopButton.IsEnabled = false;
                    RestartButton.IsEnabled = false;
                    CloseButton.IsEnabled = Timer.State == TimerState.Stopped || Timer.State == TimerState.Expired;
                    CancelButton.IsEnabled = false;
                    UpdateButton.IsEnabled = false;

                    // Hide the border, context menu, and watermark text that appear for the text boxes
                    TitleTextBox.BorderThickness = new(0);
                    TimerTextBox.BorderThickness = new(0);
                    TitleTextBox.IsReadOnly = true;
                    TimerTextBox.IsReadOnly = true;
                    Watermark.SetHint(TitleTextBox, null);
                    Watermark.SetHint(TimerTextBox, null);
                }
                else
                {
                    // Enable and disable command buttons as required
                    StartButton.IsEnabled = false;
                    PauseButton.IsEnabled = Timer.State == TimerState.Running && Timer.SupportsPause;
                    ResumeButton.IsEnabled = Timer.State == TimerState.Paused;
                    StopButton.IsEnabled = Timer.State != TimerState.Stopped && Timer.State != TimerState.Expired;
                    RestartButton.IsEnabled = Timer.SupportsRestart;
                    CloseButton.IsEnabled = Timer.State == TimerState.Stopped || Timer.State == TimerState.Expired;
                    CancelButton.IsEnabled = false;
                    UpdateButton.IsEnabled = UpdateManager.Instance.HasUpdates;

                    // Restore the border, context menu, and watermark text that appear for the text boxes
                    TitleTextBox.BorderThickness = new(1);
                    TimerTextBox.BorderThickness = new(1);
                    TitleTextBox.IsReadOnly = false;
                    TimerTextBox.IsReadOnly = false;
                    Watermark.SetHint(TitleTextBox, Properties.Resources.TimerWindowTitleTextHint);
                    Watermark.SetHint(TimerTextBox, Properties.Resources.TimerWindowTimerTextHint);
                }

                Topmost = Options.AlwaysOnTop;

                UpdateBoundTheme();
                UpdateKeepAwake();
                UpdateWindowTitle();
                UpdateWindowChrome();
                return;
        }
    }

    /// <summary>
    /// Updates the control properties set by the bound theme.
    /// </summary>
    private void UpdateBoundTheme()
    {
        InnerGrid.Background = Theme.BackgroundBrush;
        ProgressBar.Foreground = Theme.ProgressBarBrush;
        ProgressBar.Background = Theme.ProgressBackgroundBrush;
        InnerNotificationBorder.BorderBrush = Theme.ExpirationFlashBrush;
        OuterNotificationBorder.Background = Theme.ExpirationFlashBrush;
        TimerTextBox.Foreground = Theme.PrimaryTextBrush;
        TimerTextBox.CaretBrush = Theme.PrimaryTextBrush;
        Watermark.SetHintBrush(TimerTextBox, Theme.PrimaryHintBrush);
        TitleTextBox.Foreground = Theme.SecondaryTextBrush;
        TitleTextBox.CaretBrush = Theme.SecondaryTextBrush;
        Watermark.SetHintBrush(TitleTextBox, Theme.SecondaryHintBrush);
        TimeExpiredLabel.Foreground = Theme.SecondaryTextBrush;

        foreach (Button button in ButtonPanel.GetAllVisualChildren().OfType<Button>())
        {
            button.Foreground = button.IsMouseOver ? Theme.ButtonHoverBrush : Theme.ButtonBrush;
        }
    }

    /// <summary>
    /// Updates the progress shown in the taskbar.
    /// </summary>
    private void UpdateTaskbarProgress()
    {
        if (!Options.ShowProgressInTaskbar)
        {
            TaskbarItemInfo.ProgressState = TaskbarItemProgressState.None;
            return;
        }

        switch (Timer.State)
        {
            case TimerState.Stopped:
                TaskbarItemInfo.ProgressState = TaskbarItemProgressState.None;
                TaskbarItemInfo.ProgressValue = 0.0;
                break;

            case TimerState.Running:
                if (Timer.SupportsProgress)
                {
                    TaskbarItemInfo.ProgressState = TaskbarItemProgressState.Normal;
                    TaskbarItemInfo.ProgressValue = GetProgressBarValue() / 100.0;
                }
                else
                {
                    TaskbarItemInfo.ProgressState = TaskbarItemProgressState.None;
                    TaskbarItemInfo.ProgressValue = 0.0;
                }

                break;

            case TimerState.Paused:
                if (Timer.SupportsProgress)
                {
                    TaskbarItemInfo.ProgressState = TaskbarItemProgressState.Paused;
                    TaskbarItemInfo.ProgressValue = GetProgressBarValue() / 100.0;
                }
                else
                {
                    TaskbarItemInfo.ProgressState = TaskbarItemProgressState.Paused;
                    TaskbarItemInfo.ProgressValue = 0.0;
                }

                break;

            case TimerState.Expired:
                TaskbarItemInfo.ProgressState = TaskbarItemProgressState.Error;
                TaskbarItemInfo.ProgressValue = 1.0;
                break;
        }
    }

    /// <summary>
    /// Updates the registration of this window in the <see cref="KeepAwakeManager"/> based on the state of the
    /// timer.
    /// </summary>
    private void UpdateKeepAwake()
    {
        if (Timer.State == TimerState.Running && !Options.DoNotKeepComputerAwake)
        {
            KeepAwakeManager.Instance.StartKeepAwakeFor(this);
        }
        else
        {
            KeepAwakeManager.Instance.StopKeepAwakeFor(this);
        }
    }

    /// <summary>
    /// Updates the window title.
    /// </summary>
    private void UpdateWindowTitle()
    {
        switch (Options.WindowTitleMode)
        {
            case WindowTitleMode.None:
                // Although the title bar is hidden in this mode, the window title is still used for the Taskbar.
                Title = Properties.Resources.TimerWindowTitle;
                break;

            case WindowTitleMode.ApplicationName:
                Title = Properties.Resources.TimerWindowTitle;
                break;

            case WindowTitleMode.TimeLeft:
                Title = Timer.State != TimerState.Stopped
                    ? Timer.TimeLeftAsString
                    : Properties.Resources.TimerWindowTitle;
                break;

            case WindowTitleMode.TimeElapsed:
                Title = Timer.State != TimerState.Stopped
                    ? Timer.TimeElapsedAsString
                    : Properties.Resources.TimerWindowTitle;
                break;

            case WindowTitleMode.TimerTitle:
                Title = !string.IsNullOrWhiteSpace(Options.Title)
                    ? Options.Title
                    : Properties.Resources.TimerWindowTitle;
                break;

            case WindowTitleMode.TimeLeftPlusTimerTitle:
                if (Timer.State != TimerState.Stopped && !string.IsNullOrWhiteSpace(Options.Title))
                {
                    Title = string.Join(
                        Properties.Resources.TimerWindowTitleSeparator,
                        Timer.TimeLeftAsString,
                        Options.Title);
                }
                else if (Timer.State != TimerState.Stopped)
                {
                    Title = Timer.TimeLeftAsString;
                }
                else if (!string.IsNullOrWhiteSpace(Options.Title))
                {
                    Title = Options.Title;
                }
                else
                {
                    Title = Properties.Resources.TimerWindowTitle;
                }

                break;

            case WindowTitleMode.TimeElapsedPlusTimerTitle:
                if (Timer.State != TimerState.Stopped && !string.IsNullOrWhiteSpace(Options.Title))
                {
                    Title = string.Join(
                        Properties.Resources.TimerWindowTitleSeparator,
                        Timer.TimeElapsedAsString,
                        Options.Title);
                }
                else if (Timer.State != TimerState.Stopped)
                {
                    Title = Timer.TimeElapsedAsString;
                }
                else if (!string.IsNullOrWhiteSpace(Options.Title))
                {
                    Title = Options.Title;
                }
                else
                {
                    Title = Properties.Resources.TimerWindowTitle;
                }

                break;

            case WindowTitleMode.TimerTitlePlusTimeLeft:
                if (Timer.State != TimerState.Stopped && !string.IsNullOrWhiteSpace(Options.Title))
                {
                    Title = string.Join(
                        Properties.Resources.TimerWindowTitleSeparator,
                        Options.Title,
                        Timer.TimeLeftAsString);
                }
                else if (Timer.State != TimerState.Stopped)
                {
                    Title = Timer.TimeLeftAsString;
                }
                else if (!string.IsNullOrWhiteSpace(Options.Title))
                {
                    Title = Options.Title;
                }
                else
                {
                    Title = Properties.Resources.TimerWindowTitle;
                }

                break;

            case WindowTitleMode.TimerTitlePlusTimeElapsed:
                if (Timer.State != TimerState.Stopped && !string.IsNullOrWhiteSpace(Options.Title))
                {
                    Title = string.Join(
                        Properties.Resources.TimerWindowTitleSeparator,
                        Options.Title,
                        Timer.TimeElapsedAsString);
                }
                else if (Timer.State != TimerState.Stopped)
                {
                    Title = Timer.TimeElapsedAsString;
                }
                else if (!string.IsNullOrWhiteSpace(Options.Title))
                {
                    Title = Options.Title;
                }
                else
                {
                    Title = Properties.Resources.TimerWindowTitle;
                }

                break;
        }
    }

    /// <summary>
    /// Updates the window chrome.
    /// </summary>
    private void UpdateWindowChrome()
    {
        if (!IsLoaded)
        {
            return;
        }

        if (Options.WindowTitleMode == WindowTitleMode.None)
        {
            if (WindowChrome.GetWindowChrome(this)?.CaptionHeight != 0 || WindowChrome.GetWindowChrome(this)?.UseAeroCaptionButtons != false)
            {
                WindowChrome.SetWindowChrome(
                    this,
                    new()
                    {
                        CaptionHeight = 0,
                        UseAeroCaptionButtons = false
                    });
            }
        }
        else
        {
            if (WindowChrome.GetWindowChrome(this) is not null)
            {
                WindowChrome.SetWindowChrome(this, null);
            }
        }

        this.SetImmersiveDarkMode(Options.Theme.Type == ThemeType.BuiltInDark);
    }

    /// <summary>
    /// Unbinds the <see cref="TimerWindow"/> event handlers and controls from a timer.
    /// </summary>
    private void UnbindTimer()
    {
        Timer.Started -= TimerStarted;
        Timer.Paused -= TimerPaused;
        Timer.Resumed -= TimerResumed;
        Timer.Stopped -= TimerStopped;
        Timer.Expired -= TimerExpired;
        Timer.Tick -= TimerTick;
        Timer.PropertyChanged -= TimerPropertyChanged;
        Options.PropertyChanged -= TimerOptionsPropertyChanged;

        Timer.Interval = TimerBase.DefaultInterval;
        Options.WindowSize = WindowSize.FromWindow(this /* window */);

        if (Timer.State == TimerState.Stopped || Timer.State == TimerState.Expired)
        {
            TimerManager.Instance.Remove(Timer);
        }
    }

    #endregion

    #region Private Methods (Timer Events)

    /// <summary>
    /// Invoked when the timer is started.
    /// </summary>
    /// <param name="sender">The timer.</param>
    /// <param name="e">The event data.</param>
    private void TimerStarted(object sender, EventArgs e)
    {
        // Do nothing
    }

    /// <summary>
    /// Invoked when the timer is paused.
    /// </summary>
    /// <param name="sender">The timer.</param>
    /// <param name="e">The event data.</param>
    private void TimerPaused(object sender, EventArgs e)
    {
        // Do nothing
    }

    /// <summary>
    /// Invoked when the timer is resumed from a paused state.
    /// </summary>
    /// <param name="sender">The timer.</param>
    /// <param name="e">The event data.</param>
    private void TimerResumed(object sender, EventArgs e)
    {
        // Do nothing
    }

    /// <summary>
    /// Invoked when the timer is stopped.
    /// </summary>
    /// <param name="sender">The timer.</param>
    /// <param name="e">The event data.</param>
    private void TimerStopped(object sender, EventArgs e)
    {
        // Do nothing
    }

    /// <summary>
    /// Invoked when the timer expires.
    /// </summary>
    /// <param name="sender">The timer.</param>
    /// <param name="e">The event data.</param>
    private void TimerExpired(object sender, EventArgs e)
    {
        BeginExpirationAnimationAndSound();
    }

    /// <summary>
    /// Invoked when the timer ticks.
    /// </summary>
    /// <param name="sender">The timer.</param>
    /// <param name="e">The event data.</param>
    private void TimerTick(object sender, EventArgs e)
    {
        // Do nothing
    }

    /// <summary>
    /// Invoked when a timer property value changes.
    /// </summary>
    /// <param name="sender">The timer.</param>
    /// <param name="e">The event data.</param>
    private void TimerPropertyChanged(object sender, PropertyChangedEventArgs e)
    {
        UpdateBoundControls();
    }

    /// <summary>
    /// Invoked when a <see cref="TimerOptions"/> property value changes.
    /// </summary>
    /// <param name="sender">The <see cref="TimerOptions"/>.</param>
    /// <param name="e">The event data.</param>
    private void TimerOptionsPropertyChanged(object sender, PropertyChangedEventArgs e)
    {
        if (e.PropertyName == nameof(Theme))
        {
            _theme.PropertyChanged -= ThemePropertyChanged;
            _theme = Options.Theme;
            _theme.PropertyChanged += ThemePropertyChanged;
        }

        UpdateBoundControls();
    }

    /// <summary>
    /// Invoked when a <see cref="Theme"/> property value changes.
    /// </summary>
    /// <param name="sender">The <see cref="Theme"/>.</param>
    /// <param name="e">The event data.</param>
    private void ThemePropertyChanged(object sender, PropertyChangedEventArgs e)
    {
        UpdateBoundControls();
    }

    #endregion

    #region Private Methods (Commands)

    /// <summary>
    /// Invoked when the <see cref="StartCommand"/> is executed.
    /// </summary>
    /// <param name="sender">The <see cref="TimerWindow"/>.</param>
    /// <param name="e">The event data.</param>
    private void StartCommandExecuted(object sender, ExecutedRoutedEventArgs e)
    {
        TimerStart timerStart = TimerStart.FromString(TimerTextBox.Text);
        if (timerStart is null)
        {
            BeginValidationErrorAnimation();
            return;
        }

        // If the interface was previously locked, unlock it when a new timer is started
        Options.LockInterface = false;

        Show(timerStart);
        StartButton.Unfocus();
    }

    /// <summary>
    /// Invoked when the <see cref="PauseCommand"/> is executed.
    /// </summary>
    /// <param name="sender">The <see cref="TimerWindow"/>.</param>
    /// <param name="e">The event data.</param>
    private void PauseCommandExecuted(object sender, ExecutedRoutedEventArgs e)
    {
        if (Options.LockInterface)
        {
            return;
        }

        Timer.Pause();
        PauseButton.Unfocus();
    }

    /// <summary>
    /// Invoked when the <see cref="ResumeCommand"/> is executed.
    /// </summary>
    /// <param name="sender">The <see cref="TimerWindow"/>.</param>
    /// <param name="e">The event data.</param>
    private void ResumeCommandExecuted(object sender, ExecutedRoutedEventArgs e)
    {
        if (Options.LockInterface)
        {
            return;
        }

        Timer.Resume();
        ResumeButton.Unfocus();
    }

    /// <summary>
    /// Invoked when the <see cref="PauseResumeCommand"/> is executed.
    /// </summary>
    /// <param name="sender">The <see cref="TimerWindow"/>.</param>
    /// <param name="e">The event data.</param>
    private void PauseResumeCommandExecuted(object sender, ExecutedRoutedEventArgs e)
    {
        if (Options.LockInterface)
        {
            return;
        }

        if (Timer.State == TimerState.Running)
        {
            Timer.Pause();
            PauseButton.Unfocus();
        }
        else if (Timer.State == TimerState.Paused)
        {
            Timer.Resume();
            ResumeButton.Unfocus();
        }
    }

    /// <summary>
    /// Invoked when the <see cref="StopCommand"/> is executed.
    /// </summary>
    /// <param name="sender">The <see cref="TimerWindow"/>.</param>
    /// <param name="e">The event data.</param>
    private void StopCommandExecuted(object sender, ExecutedRoutedEventArgs e)
    {
        if (Options.LockInterface)
        {
            return;
        }

        Timer = new(Options);
        TimerManager.Instance.Add(Timer);

        SwitchToInputMode();
        StopButton.Unfocus();
    }

    /// <summary>
    /// Invoked when the <see cref="RestartCommand"/> is executed.
    /// </summary>
    /// <param name="sender">The <see cref="TimerWindow"/>.</param>
    /// <param name="e">The event data.</param>
    private void RestartCommandExecuted(object sender, ExecutedRoutedEventArgs e)
    {
        if (Options.LockInterface || !Timer.SupportsRestart)
        {
            return;
        }

        Timer.Restart();
        SwitchToStatusMode();
        RestartButton.Unfocus();
    }

    /// <summary>
    /// Invoked when the <see cref="CloseCommand"/> is executed.
    /// </summary>
    /// <param name="sender">The <see cref="TimerWindow"/>.</param>
    /// <param name="e">The event data.</param>
    private void CloseCommandExecuted(object sender, ExecutedRoutedEventArgs e)
    {
        if (Options.LockInterface && Timer.State != TimerState.Stopped && Timer.State != TimerState.Expired)
        {
            return;
        }

        Close();
        CloseButton.Unfocus();
    }

    /// <summary>
    /// Invoked when the <see cref="CancelCommand"/> is executed.
    /// </summary>
    /// <param name="sender">The <see cref="TimerWindow"/>.</param>
    /// <param name="e">The event data.</param>
    private void CancelCommandExecuted(object sender, ExecutedRoutedEventArgs e)
    {
        // Switch back to showing the running timer if there is one
        if (Timer.State != TimerState.Stopped && Timer.State != TimerState.Expired)
        {
            SwitchToStatusMode();
            CancelButton.Unfocus();
        }
    }

    /// <summary>
    /// Invoked when the <see cref="UpdateCommand"/> is executed.
    /// </summary>
    /// <param name="sender">The <see cref="TimerWindow"/>.</param>
    /// <param name="e">The event data.</param>
    private void UpdateCommandExecuted(object sender, ExecutedRoutedEventArgs e)
    {
        Uri updateUri = UpdateManager.Instance.UpdateUri;
        if (updateUri?.Scheme == Uri.UriSchemeHttp || updateUri?.Scheme == Uri.UriSchemeHttps)
        {
            try
            {
                updateUri.Navigate();
            }
            catch (Exception ex)
            {
                string message = string.Format(
                    Properties.Resources.TimerWindowCouldNotLaunchWebBrowserErrorMessage,
                    updateUri);

                ErrorDialog dialog = new();
                dialog.ShowDialog(
                    title: Properties.Resources.TimerWindowCouldNotLaunchWebBrowserErrorTitle,
                    message: message,
                    details: ex.ToString());
            }
        }
    }

    /// <summary>
    /// Invoked when the <see cref="EscapeCommand"/> is executed.
    /// </summary>
    /// <param name="sender">The <see cref="TimerWindow"/>.</param>
    /// <param name="e">The event data.</param>
    private void EscapeCommandExecuted(object sender, ExecutedRoutedEventArgs e)
    {
        if (CancelOrReset())
        {
            return;
        }

        if (IsFullScreen)
        {
            IsFullScreen = false;
            return;
        }

        if (Equals(FocusManager.GetFocusedElement(this), this))
        {
            WindowState = WindowState.Minimized;
        }
    }

    /// <summary>
    /// Invoked when the <see cref="FullScreenCommand"/> is executed.
    /// </summary>
    /// <param name="sender">The <see cref="TimerWindow"/>.</param>
    /// <param name="e">The event data.</param>
    private void FullScreenCommandExecuted(object sender, ExecutedRoutedEventArgs e)
    {
        IsFullScreen = !IsFullScreen;
    }

    #endregion

    #region Private Methods (Window Events)

    /// <summary>
    /// Invoked when the mouse pointer enters the bounds of a <see cref="Button"/>.
    /// </summary>
    /// <param name="sender">A <see cref="Button"/>.</param>
    /// <param name="e">The event data.</param>
    private void ButtonMouseEnter(object sender, MouseEventArgs e)
    {
        Button button = (Button)sender;
        button.Foreground = Theme.ButtonHoverBrush;
    }

    /// <summary>
    /// Invoked when the mouse pointer leaves the bounds of a <see cref="Button"/>.
    /// </summary>
    /// <param name="sender">A <see cref="Button"/>.</param>
    /// <param name="e">The event data.</param>
    private void ButtonMouseLeave(object sender, MouseEventArgs e)
    {
        Button button = (Button)sender;
        button.Foreground = Theme.ButtonBrush;
    }

    /// <summary>
    /// Invoked when a key on the keyboard is pressed in the <see cref="TitleTextBox"/>.
    /// </summary>
    /// <param name="sender">The <see cref="TitleTextBox"/>.</param>
    /// <param name="e">The event data.</param>
    private void TitleTextBoxKeyDown(object sender, KeyEventArgs e)
    {
        if (e.Key == Key.Enter && Mode == TimerWindowMode.Status)
        {
            if (Timer.State == TimerState.Expired)
            {
                SwitchToInputMode(TimerTextBox /* textBoxToFocus */);
            }
            else
            {
                TitleTextBox.Unfocus();
            }

            e.Handled = true;
        }
    }

    /// <summary>
    /// Invoked when any mouse button is pressed while the pointer is over the <see cref="TitleTextBox"/>.
    /// </summary>
    /// <param name="sender">The <see cref="TitleTextBox"/>.</param>
    /// <param name="e">The event data.</param>
    private void TitleTextBoxPreviewMouseDown(object sender, MouseButtonEventArgs e)
    {
        if (Options.LockInterface && Mode != TimerWindowMode.Input)
        {
            e.Handled = true;
        }
        else if (Mode != TimerWindowMode.Input && (Timer.State == TimerState.Stopped || Timer.State == TimerState.Expired))
        {
            SwitchToInputMode(TitleTextBox /* textBoxToFocus */);
            e.Handled = true;
        }
        else if (!TitleTextBox.IsFocused)
        {
            TitleTextBox.SelectAll();
            TitleTextBox.Focus();
            e.Handled = true;
        }
    }

    /// <summary>
    /// Invoked when the <see cref="TitleTextBox"/> is in the process of acquiring keyboard focus.
    /// </summary>
    /// <param name="sender">The <see cref="TitleTextBox"/>.</param>
    /// <param name="e">The event data.</param>
    private void TitleTextBoxPreviewGotKeyboardFocus(object sender, KeyboardFocusChangedEventArgs e)
    {
        if (Options.LockInterface && Mode != TimerWindowMode.Input)
        {
            e.Handled = true;
        }
        else if (Mode != TimerWindowMode.Input && (Timer.State == TimerState.Stopped || Timer.State == TimerState.Expired))
        {
            SwitchToInputMode(TitleTextBox /* textBoxToFocus */);
            e.Handled = true;
        }
        else
        {
            TitleTextBox.SelectAll();
        }
    }

    /// <summary>
    /// Invoked when the <see cref="TitleTextBox"/> content changes.
    /// </summary>
    /// <param name="sender">The <see cref="TitleTextBox"/>.</param>
    /// <param name="e">The event data.</param>
    private void TitleTextBoxTextChanged(object sender, TextChangedEventArgs e)
    {
        Timer.Options.Title = string.IsNullOrWhiteSpace(TitleTextBox.Text)
            ? string.Empty
            : TitleTextBox.Text;
    }

    /// <summary>
    /// Invoked when any mouse button is pressed while the pointer is over the <see cref="TimerTextBox"/>.
    /// </summary>
    /// <param name="sender">The <see cref="TimerTextBox"/>.</param>
    /// <param name="e">The event data.</param>
    private void TimerTextBoxPreviewMouseDown(object sender, MouseButtonEventArgs e)
    {
        if (Options.LockInterface && Mode != TimerWindowMode.Input)
        {
            e.Handled = true;
        }
        else if (Mode != TimerWindowMode.Input)
        {
            SwitchToInputMode();
            e.Handled = true;
        }
        else if (!TimerTextBox.IsFocused)
        {
            TimerTextBox.SelectAll();
            TimerTextBox.Focus();
            e.Handled = true;
        }
    }

    /// <summary>
    /// Invoked when the <see cref="TimerTextBox"/> is in the process of acquiring keyboard focus.
    /// </summary>
    /// <param name="sender">The <see cref="TimerTextBox"/>.</param>
    /// <param name="e">The event data.</param>
    private void TimerTextBoxPreviewGotKeyboardFocus(object sender, KeyboardFocusChangedEventArgs e)
    {
        if (Options.LockInterface && Mode != TimerWindowMode.Input)
        {
            e.Handled = true;
        }
        else if (Mode != TimerWindowMode.Input)
        {
            SwitchToInputMode();
            e.Handled = true;
        }
        else
        {
            TimerTextBox.SelectAll();
        }
    }

    /// <summary>
    /// Invoked when the <see cref="TimerWindow"/> is laid out, rendered, and ready for interaction.
    /// </summary>
    /// <param name="sender">The <see cref="TimerWindow"/>.</param>
    /// <param name="e">The event data.</param>
    private void WindowLoaded(object sender, RoutedEventArgs e)
    {
        // Deal with any input or timer set in the constructor
        if (_timerStartToStartOnLoad is not null)
        {
            Show(_timerStartToStartOnLoad);
        }
        else if (Options.LockInterface)
        {
            // If the interface is locked but no timer input was specified, there is nothing the user can do or
            // should be able to do other than close the window, so pretend that the timer expired immediately
            Show(TimerStart.Zero, false /* remember */);
        }
        else if (_timerToResumeOnLoad is not null)
        {
            Show(_timerToResumeOnLoad);
        }

        _timerStartToStartOnLoad = null;
        _timerToResumeOnLoad = null;

        // Minimize to notification area if required
        if (WindowState == WindowState.Minimized && Settings.Default.ShowInNotificationArea)
        {
            MinimizeToNotificationArea();
        }
    }

    /// <summary>
    /// Invoked when any mouse button is depressed on the <see cref="TimerWindow"/>.
    /// </summary>
    /// <param name="sender">The <see cref="TimerWindow"/>.</param>
    /// <param name="e">The event data.</param>
    private void WindowMouseDown(object sender, MouseButtonEventArgs e)
    {
        if (e.ChangedButton == MouseButton.Left)
        {
            DragMove();
        }

        if (e.OriginalSource is Panel)
        {
            CancelOrReset();
            e.Handled = true;
        }
    }

    /// <summary>
    /// Invoked when a mouse button is clicked two or more times on the <see cref="TimerWindow"/>.
    /// </summary>
    /// <param name="sender">The <see cref="TimerWindow"/>.</param>
    /// <param name="e">The event data.</param>
    private void WindowMouseDoubleClick(object sender, MouseButtonEventArgs e)
    {
        if (!e.OriginalSource.IsTextBoxView())
        {
            IsFullScreen = !IsFullScreen;
            e.Handled = true;
        }
    }

    /// <summary>
    /// Invoked when the window's WindowState property changes.
    /// </summary>
    /// <param name="sender">The <see cref="TimerWindow"/>.</param>
    /// <param name="e">The event data.</param>
    private void WindowStateChanged(object sender, EventArgs e)
    {
        if (WindowState != WindowState.Minimized && !IsFullScreen)
        {
            RestoreWindowState = WindowState;
        }

        bool isMinimized = WindowState == WindowState.Minimized;

        if (isMinimized && Settings.Default.ShowInNotificationArea)
        {
            MinimizeToNotificationArea();
        }

        UpdateBoundControls();

        if (isMinimized)
        {
            this.BringNextToFrontAndActivate();
        }
    }

    /// <summary>
    /// Invoked directly after <see cref="Window.Close"/> is called, and can be handled to cancel window closure.
    /// </summary>
    /// <param name="sender">The <see cref="TimerWindow"/>.</param>
    /// <param name="e">The event data.</param>
    private void WindowClosing(object sender, CancelEventArgs e)
    {
        // Do not allow the window to be closed if the interface is locked and the timer is running
        e.Cancel = !ForceClose &&
                   Options.LockInterface &&
                   Timer.State != TimerState.Stopped &&
                   Timer.State != TimerState.Expired;
        if (e.Cancel)
        {
            return;
        }

        if (ForceClose ||
            DoNotPromptOnExit ||
            !Options.PromptOnExit ||
            Timer.State == TimerState.Stopped ||
            Timer.State == TimerState.Expired)
        {
            // Clean up
            UnbindTimer();
            _soundPlayer.Dispose();

            Settings.Default.WindowSize = WindowSize.FromWindow(this /* window */);

            UpdateManager.Instance.PropertyChanged -= UpdateManagerPropertyChanged;
            KeepAwakeManager.Instance.StopKeepAwakeFor(this);
            AppManager.Instance.Persist();

            return;
        }

        e.Cancel = true;

        Dispatcher.BeginInvoke(ConfirmClose);
    }

    private void ConfirmClose()
    {
        BringToFrontAndActivate();

        MessageBoxResult result = this.ShowTaskDialog(
            Properties.Resources.TimerWindowCloseTaskDialogInstruction,
            Properties.Resources.StopAndCloseWindowCloseTaskDialogCommand,
            Properties.Resources.MinimizeWindowCloseTaskDialogCommand);

        switch (result)
        {
            case MessageBoxResult.Yes:
                ForceClose = true;
                Close();
                return;
            case MessageBoxResult.No:
                WindowState = WindowState.Minimized;
                return;
        }
    }

    private void WindowClosed(object sender, EventArgs e)
    {
        this.BringNextToFrontAndActivate();
    }

    #endregion
}