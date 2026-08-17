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

using System;
using System.Drawing;

namespace CSharp.Ch05.TextbookCode.Ch05RealWorldScenario02
{
    class Shape : IDisposable, IComparable<Shape>
    {
        // The FillBrush and OutlinePen properties.
        public Brush FillBrush { get; set; }
        public Pen OutlinePen { get; set; }

        // Remember whether we've already run Dispose.
        private bool IsDisposed = false;

        // Clean up managed resources.
        public void Dispose()
        {
            // If we've already run Dispose, do nothing.
            if (IsDisposed) return;

            // Dispose of FillBrush and OutlinePen.
            FillBrush.Dispose();
            OutlinePen.Dispose();

            // Remember that we ran Dispose.
            IsDisposed = true;
        }

        // Stubbed out to satisfy the IComparable<Shape> interface, the original download
        //     declares this interface without ever implementing a body for it, which
        //     doesn't compile, a class implementing an interface member must provide one.
        public int CompareTo(Shape other)
        {
            throw new NotImplementedException();
        }
    }
}

#region Source Code Information
/* ******************************************************************** *
 *                   Copyright (C) 2026, DataBank IMX                   *
 *                                                                      *
 * Source code provided for reference only! Reuse not permitted!        *
 * ******************************************************************** */
#endregion
