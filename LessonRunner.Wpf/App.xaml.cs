#region Copyright
/* ******************************************************************** *
 *                    Copyright (C) 2026, DataBank IMX                  *
 *                                                                      *
 * All rights reserved                                                  *
 *                                                                      *
 * For further information consult:                                     *
 *  - The DataBank IMX End User License Agreement (EULA)                *
 *    or                                                                *
 *  - DataBank IMX Intellectual Property Statement                      *
 *                                                                      *
 * Above referenced documents available upon request from:              *
 *     development@databankimx.com                                      *
 *                                                                      *
 * ******************************************************************** */
#endregion

#region Using Directives
using System.Windows;
using System.Windows.Input;
#endregion

namespace LessonRunner.Wpf;

/// <summary>
/// Represents the WPF application for the process.
/// </summary>
/// <remarks>Registers a dispatcher unhandled exception handler during construction to release stuck modifier keys
/// and clear keyboard focus after unexpected UI thread exceptions.</remarks>
public partial class App : Application
{
    #region Constructor
    public App()
    {
        // Recover keyboard state after any unhandled WPF dispatcher exception.
        // Without this, modifier keys (Shift, Ctrl, Alt) can get stuck in a
        // pressed state because WPF never receives the corresponding KeyUp
        // event when an exception unwinds the message loop mid-keystroke.
        DispatcherUnhandledException += (_, e) =>
        {
            KeyboardRecovery.ReleaseStuckModifiers();
            Keyboard.ClearFocus();
        };
    }
    #endregion
}

#region Source Code Information
/* ******************************************************************** *
 *                    Copyright (C) 2026, DataBank IMX                  *
 *                                                                      *
 * Source code provided for reference only! Reuse not permitted!        *
 * ******************************************************************** */
#endregion
