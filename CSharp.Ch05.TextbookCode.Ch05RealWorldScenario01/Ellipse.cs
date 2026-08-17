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

namespace CSharp.Ch05.TextbookCode.Ch05RealWorldScenario01
{
    class Ellipse
    {
        public RectangleF Location { get; set; }

        // Constructor that takes a RectangleF as a parameter.
        public Ellipse(RectangleF rect)
        {
            // Validate width and height.
            if (rect.Width <= 0)
                throw new ArgumentOutOfRangeException(
                    "width",
                    "Ellipse width must be greater than 0.");
            if (rect.Height <= 0)
                throw new ArgumentOutOfRangeException(
                    "height",
                    "Ellipse height must be greater than 0.");

            // Save the location.
            Location = rect;
        }

        // Constructor that takes x, y, width, and height as parameters.
        public Ellipse(float x, float y, float width, float height)
            : this(new RectangleF(x, y, width, height))
        {
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
