﻿// --------------------------------------------------------------------------------------------------------------------
// <copyright file="Manager.cs" company="Chris Dziemborowicz">
//   Copyright (c) Chris Dziemborowicz. All rights reserved.
// </copyright>
// --------------------------------------------------------------------------------------------------------------------

namespace Hourglass.Managers;

using System;

/// <summary>
/// A base class for singleton manager classes.
/// </summary>
public abstract class Manager : IDisposable
{
    /// <summary>
    /// Indicates whether this object has been disposed.
    /// </summary>
    private bool _disposed;

    /// <summary>
    /// Gets a value indicating whether this object has been disposed.
    /// </summary>
    protected bool Disposed => _disposed;

    /// <summary>
    /// Initializes the class.
    /// </summary>
    public virtual void Initialize()
    {
    }

    /// <summary>
    /// Persists the state of the class.
    /// </summary>
    public virtual void Persist()
    {
    }

    /// <summary>
    /// Disposes the manager.
    /// </summary>
    public void Dispose()
    {
        Dispose(true /* disposing */);
        GC.SuppressFinalize(this);
    }

    /// <summary>
    /// Disposes the manager.
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
            // Do nothing
        }
    }
}