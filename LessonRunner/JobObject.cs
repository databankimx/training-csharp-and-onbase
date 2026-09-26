#region Copyright
/* ******************************************************************** *
 *                   Copyright (C) 2026, DataBank IMX                   *
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
using System;
using System.Diagnostics;
using System.Runtime.InteropServices;
#endregion

namespace LessonRunner
{
    /// <summary>
    /// Wraps a Windows Job Object with JOB_OBJECT_LIMIT_KILL_ON_JOB_CLOSE set, so every
    /// process assigned to it - and any child/grandchild processes those spawn in turn - is
    /// automatically terminated the moment this process's own handle to the job closes. That
    /// happens whenever this process exits, for any reason: a clean return from Main(), the
    /// console window's own close button, a crash, or Task Manager's "End Task" - none of
    /// which reliably run ordinary C# cleanup code (a try/finally, ProcessExit, or
    /// CancelKeyPress can all be skipped depending on how the process actually goes down), but
    /// all of which the OS itself enforces once the handle closes.
    ///
    /// Needed here specifically because "dotnet run" spawns the actual lesson .exe as its own
    /// child process rather than replacing itself - killing just the "dotnet run" process
    /// LessonRunner directly started would leave that grandchild running and still holding a
    /// lock on its own build output, exactly the DLL-lock problem this exists to prevent.
    /// </summary>
    internal sealed class JobObject : IDisposable
    {
        private readonly IntPtr handle;

        internal JobObject()
        {
            handle = CreateJobObject(IntPtr.Zero, null);
            if (handle == IntPtr.Zero)
            {
                throw new InvalidOperationException($"CreateJobObject failed (Win32 error {Marshal.GetLastWin32Error()})");
            }

            var extendedInfo = new JOBOBJECT_EXTENDED_LIMIT_INFORMATION
            {
                BasicLimitInformation = new JOBOBJECT_BASIC_LIMIT_INFORMATION
                {
                    LimitFlags = JOB_OBJECT_LIMIT_KILL_ON_JOB_CLOSE
                }
            };

            int length = Marshal.SizeOf(typeof(JOBOBJECT_EXTENDED_LIMIT_INFORMATION));
            IntPtr infoPtr = Marshal.AllocHGlobal(length);
            try
            {
                Marshal.StructureToPtr(extendedInfo, infoPtr, false);
                if (!SetInformationJobObject(handle, JobObjectInfoType.ExtendedLimitInformation, infoPtr, (uint)length))
                {
                    throw new InvalidOperationException($"SetInformationJobObject failed (Win32 error {Marshal.GetLastWin32Error()})");
                }
            }
            finally
            {
                Marshal.FreeHGlobal(infoPtr);
            }
        }

        /// <summary>
        /// Assigns a process to this job - from then on, that process (and anything it spawns)
        /// is torn down alongside every other process in the job when this JobObject's handle
        /// closes. Safe to call more than once per process lifetime is not required here; each
        /// lesson gets a freshly-started process assigned exactly once.
        /// </summary>
        internal void Assign(Process process)
        {
            if (!AssignProcessToJobObject(handle, process.Handle))
            {
                // A process that has already exited by the time this runs (e.g. it failed to
                // start at all and Process.Start still returned a Process object briefly) will
                // fail here - not worth treating as fatal, since there's nothing left to track.
                int error = Marshal.GetLastWin32Error();
                Console.WriteLine($"Warning: could not add process to cleanup job (Win32 error {error}) - it may not be automatically closed if this program exits abnormally.");
            }
        }

        public void Dispose()
        {
            if (handle != IntPtr.Zero)
            {
                CloseHandle(handle);
            }
        }

        #region P/Invoke
        private const uint JOB_OBJECT_LIMIT_KILL_ON_JOB_CLOSE = 0x2000;

        private enum JobObjectInfoType
        {
            ExtendedLimitInformation = 9
        }

        [StructLayout(LayoutKind.Sequential)]
        private struct JOBOBJECT_BASIC_LIMIT_INFORMATION
        {
            public long PerProcessUserTimeLimit;
            public long PerJobUserTimeLimit;
            public uint LimitFlags;
            public UIntPtr MinimumWorkingSetSize;
            public UIntPtr MaximumWorkingSetSize;
            public uint ActiveProcessLimit;
            public UIntPtr Affinity;
            public uint PriorityClass;
            public uint SchedulingClass;
        }

        [StructLayout(LayoutKind.Sequential)]
        private struct IO_COUNTERS
        {
            public ulong ReadOperationCount;
            public ulong WriteOperationCount;
            public ulong OtherOperationCount;
            public ulong ReadTransferCount;
            public ulong WriteTransferCount;
            public ulong OtherTransferCount;
        }

        [StructLayout(LayoutKind.Sequential)]
        private struct JOBOBJECT_EXTENDED_LIMIT_INFORMATION
        {
            public JOBOBJECT_BASIC_LIMIT_INFORMATION BasicLimitInformation;
            public IO_COUNTERS IoInfo;
            public UIntPtr ProcessMemoryLimit;
            public UIntPtr JobMemoryLimit;
            public UIntPtr PeakProcessMemoryUsed;
            public UIntPtr PeakJobMemoryUsed;
        }

        [DllImport("kernel32.dll", CharSet = CharSet.Unicode, SetLastError = true)]
        private static extern IntPtr CreateJobObject(IntPtr lpJobAttributes, string lpName);

        [DllImport("kernel32.dll", SetLastError = true)]
        private static extern bool SetInformationJobObject(IntPtr hJob, JobObjectInfoType infoType, IntPtr lpJobObjectInfo, uint cbJobObjectInfoLength);

        [DllImport("kernel32.dll", SetLastError = true)]
        private static extern bool AssignProcessToJobObject(IntPtr hJob, IntPtr hProcess);

        [DllImport("kernel32.dll", SetLastError = true)]
        private static extern bool CloseHandle(IntPtr hObject);
        #endregion
    }
}

#region Source Code Information
/* ******************************************************************** *
 *                   Copyright (C) 2026, DataBank IMX                   *
 *                                                                      *
 * Source code provided for reference only! Reuse not permitted!        *
 * ******************************************************************** */
#endregion
