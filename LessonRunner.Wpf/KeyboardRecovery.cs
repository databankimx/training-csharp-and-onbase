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
using System.Runtime.InteropServices;
#endregion

namespace LessonRunner.Wpf;

/// <summary>
/// One-shot utility to unstick modifier keys that got latched at the Win32
/// level after an unhandled WPF exception interrupted keyboard message processing.
/// </summary>
internal static class KeyboardRecovery
{
    #region Win32 Interop
    [DllImport("user32.dll")]
    private static extern void keybd_event(byte bVk, byte bScan, uint dwFlags, nuint dwExtraInfo);

    private const uint KEYEVENTF_KEYUP = 0x0002;

    // Virtual key codes for the modifier keys that most commonly get stuck
    private static readonly byte[] ModifierKeys =
    [
        0x10, // VK_SHIFT
        0x11, // VK_CONTROL
        0x12, // VK_MENU (Alt)
        0xA0, // VK_LSHIFT
        0xA1, // VK_RSHIFT
        0xA2, // VK_LCONTROL
        0xA3, // VK_RCONTROL
        0xA4, // VK_LMENU
        0xA5, // VK_RMENU
    ];
    #endregion

    #region Methods
    /// <summary>
    /// Sends a synthetic KeyUp event for every common modifier key,
    /// clearing any stuck state left by an interrupted exception.
    /// </summary>
    internal static void ReleaseStuckModifiers()
    {
        foreach (var vk in ModifierKeys)
            keybd_event(vk, 0, KEYEVENTF_KEYUP, 0);
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
