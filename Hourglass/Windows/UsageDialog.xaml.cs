﻿// --------------------------------------------------------------------------------------------------------------------
// <copyright file="UsageDialog.xaml.cs" company="Chris Dziemborowicz">
//   Copyright (c) Chris Dziemborowicz. All rights reserved.
// </copyright>
// --------------------------------------------------------------------------------------------------------------------

namespace Hourglass.Windows;

using System;
using System.Windows;
using System.Windows.Media;
using System.Windows.Navigation;

using Extensions;

/// <summary>
/// A window that displays command-line usage.
/// </summary>
public sealed partial class UsageDialog
{
    private static UsageDialog _instance;

    /// <summary>
    /// Initializes a new instance of the <see cref="UsageDialog"/> class.
    /// </summary>
    public UsageDialog()
    {
        InitializeComponent();
        InitializeMaxSize();
    }

    /// <summary>
    /// Gets or sets an optional error message to be displayed.
    /// </summary>
    public string ErrorMessage { get; set; }

    /// <summary>
    /// Initializes the <see cref="Window.MaxWidth"/> and <see cref="Window.MaxHeight"/> properties.
    /// </summary>
    private void InitializeMaxSize()
    {
        MaxWidth = 0.75 * SystemParameters.WorkArea.Width;
        MaxHeight = 0.75 * SystemParameters.WorkArea.Height;
    }

    public static void ShowOrActivate(string errorMessage = null)
    {
        if (_instance is not null)
        {
            _instance.Activate();
            return;
        }

        _instance = new()
        {
            ErrorMessage = errorMessage
        };

        if (Application.Current?.Dispatcher is not null)
        {
            _instance.Show();
        }
        else
        {
            _instance.ShowDialog();
        }
    }

    private void UsageDialogClosed(object sender, EventArgs e)
    {
#pragma warning disable S2696
        _instance = null;
#pragma warning restore S2696
    }

    /// <summary>
    /// Invoked when the window is laid out, rendered, and ready for interaction.
    /// </summary>
    /// <param name="sender">The window.</param>
    /// <param name="e">The event data.</param>
    private void WindowLoaded(object sender, RoutedEventArgs e)
    {
        if (!string.IsNullOrWhiteSpace(ErrorMessage))
        {
            MessageTextBlock.Background = new SolidColorBrush(Color.FromRgb(199, 80, 80));
            MessageTextBlock.Text = ErrorMessage;
        }
        else
        {
            MessageTextBlock.Background = Brushes.Gray;
            MessageTextBlock.Text = Properties.Resources.UsageDialogDefaultMessageText;
        }

        Activate();
    }

    /// <summary>
    /// Invoked when the "About Hourglass" hyperlink is clicked.
    /// </summary>
    /// <param name="sender">The "About Hourglass" hyperlink.</param>
    /// <param name="e">The event data.</param>
    private void AboutHourglassHyperlinkClick(object sender, RoutedEventArgs e)
    {
        AboutDialog.ShowOrActivate();
    }

    /// <summary>
    /// Invoked when the close button is clicked.
    /// </summary>
    /// <param name="sender">The close button.</param>
    /// <param name="e">The event data.</param>
    private void CloseButtonClick(object sender, RoutedEventArgs e)
    {
        Close();
    }

    private void HyperlinkRequestNavigate(object sender, RequestNavigateEventArgs e)
    {
        e.Uri.Navigate();
    }
}