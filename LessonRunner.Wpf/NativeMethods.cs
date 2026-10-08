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
/// Thin wrappers around Win32 APIs used by the WPF shell (monitor detection, etc.).
/// </summary>
internal static class NativeMethods
{
    #region Monitor API
    /// <summary>
    /// Returns a handle to the monitor nearest to the given window handle.
    /// Pass <see cref="MONITOR_DEFAULTTONEAREST"/> to fall back to the nearest
    /// monitor when the window does not intersect any monitor.
    /// </summary>
    [DllImport("user32.dll")]
    internal static extern nint MonitorFromWindow(nint hWnd, uint dwFlags);

    /// <summary>Fallback: return the nearest monitor even if the window is off-screen.</summary>
    internal const uint MONITOR_DEFAULTTONEAREST = 2;

    /// <summary>Populates a <see cref="MONITORINFO"/> for the given monitor handle.</summary>
    [DllImport("user32.dll", CharSet = CharSet.Auto)]
    [return: MarshalAs(UnmanagedType.Bool)]
    internal static extern bool GetMonitorInfo(nint hMonitor, ref MONITORINFO lpmi);

    [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Auto)]
    internal struct MONITORINFO
    {
        public uint  cbSize;
        public RECT  rcMonitor;   // full monitor bounds (pixels)
        public RECT  rcWork;      // work area - excludes taskbar (pixels)
        public uint  dwFlags;
    }

    [StructLayout(LayoutKind.Sequential)]
    internal struct RECT
    {
        public int left;
        public int top;
        public int right;
        public int bottom;
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
