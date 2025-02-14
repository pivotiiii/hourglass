﻿// --------------------------------------------------------------------------------------------------------------------
// <copyright file="EnvironmentExtensions.cs" company="Chris Dziemborowicz">
//   Copyright (c) Chris Dziemborowicz. All rights reserved.
// </copyright>
// --------------------------------------------------------------------------------------------------------------------

namespace Hourglass.Extensions;

using System;

/// <summary>
/// Provides information about the current environment and platform.
/// </summary>
public static class EnvironmentExtensions
{
    /// <summary>
    /// Gets a value indicating whether the current environment and platform is Windows 10 or newer.
    /// </summary>
    public static bool IsWindows10OrNewer =>
        Environment.OSVersion.Platform == PlatformID.Win32NT
        && Environment.OSVersion.Version.Major >= 10;

    /// <summary>
    /// Gets a value indicating whether the current environment and platform is Windows 10 with a specified build
    /// or newer.
    /// </summary>
    /// <param name="build">A minimum Windows 10 build to use as a threshold.</param>
    /// <returns>A value indicating whether the current environment and platform is Windows 10 with a specified
    /// build or newer.</returns>
    public static bool IsWindows10BuildOrNewer(int build)
    {
        return Environment.OSVersion.Platform == PlatformID.Win32NT
               && Environment.OSVersion.Version.Major >= 10
               && Environment.OSVersion.Version.Build >= build;
    }
}