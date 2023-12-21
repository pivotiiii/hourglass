// --------------------------------------------------------------------------------------------------------------------
// <copyright file="AboutDialog.xaml.cs" company="Chris Dziemborowicz">
//   Copyright (c) Chris Dziemborowicz. All rights reserved.
// </copyright>
// --------------------------------------------------------------------------------------------------------------------

namespace Hourglass.Windows;

using System;
using System.Windows;
using System.Windows.Navigation;

using Extensions;
using Managers;

/// <summary>
/// A window that displays information about the app.
/// </summary>
public sealed partial class AboutDialog
{
    /// <summary>
    /// The instance of the <see cref="AboutDialog"/> that is showing, or null if there is no instance showing.
    /// </summary>
    private static AboutDialog _instance;

    /// <summary>
    /// Initializes a new instance of the <see cref="AboutDialog"/> class.
    /// </summary>
    public AboutDialog()
    {
        InitializeComponent();
        InitializeMaxSize();
    }

    /// <summary>
    /// A string describing the app's copyright.
    /// </summary>
    public static string Copyright
    {
        get
        {
            // Finds the copyright line in the license string. Perhaps overkill, but doing it this way means there's
            // one less place to update the copyright notice.
            return Array.Find(License.Split(Environment.NewLine.ToCharArray()), static line => line.StartsWith("Copyright")) ??
                   throw new NotSupportedException("Could not find copyright line in license.");
        }
    }

    /// <summary>
    /// A string containing the app's license.
    /// </summary>
    public static string License => Properties.Resources.License;

    /// <summary>
    /// A string describing the app's version.
    /// </summary>
    public static string Version
    {
        get
        {
            Version version = UpdateManager.Instance.CurrentVersion;

            if (version.Revision != 0)
            {
                return version.ToString();
            }

            if (version.Build != 0)
            {
                return version.ToString(3 /* fieldCount */);
            }

            return version.ToString(2 /* fieldCount */);
        }
    }

    /// <summary>
    /// Shows or activates the <see cref="AboutDialog"/>. Call this method instead of the constructor to prevent
    /// multiple instances of the dialog.
    /// </summary>
    public static void ShowOrActivate()
    {
        if (_instance is null)
        {
            _instance = new();
            _instance.Show();
        }
        else
        {
            _instance.Activate();
        }
    }

    /// <summary>
    /// Initializes the <see cref="Window.MaxWidth"/> and <see cref="Window.MaxHeight"/> properties.
    /// </summary>
    private void InitializeMaxSize()
    {
        MaxWidth = 0.75 * SystemParameters.WorkArea.Width;
        MaxHeight = 0.75 * SystemParameters.WorkArea.Height;
    }

    private void AboutDialogClosed(object sender, EventArgs e)
    {
#pragma warning disable S2696
        _instance = null;
#pragma warning restore S2696
    }

    /// <summary>
    /// Invoked when navigation events are requested.
    /// </summary>
    /// <param name="sender">The hyperlink requesting navigation.</param>
    /// <param name="e">The event data.</param>
    private void HyperlinkRequestNavigate(object sender, RequestNavigateEventArgs e)
    {
        e.Uri.Navigate();
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
}