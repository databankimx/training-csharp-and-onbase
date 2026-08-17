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

namespace CSharp.Ch05.TextbookCode.EllipsesAndCircles
{
    class Circle : Ellipse
    {
        // Constructor that takes a RectangleF as a parameter.
        public Circle(RectangleF rect)
            : base(rect)
        {
            // Validate width and height.
            if (rect.Width != rect.Height)
                throw new ArgumentOutOfRangeException(
                    "width and height",
                    "Circle width and height must be the same.");
        }

        // Constructor that takes x, y, width, and height as parameters.
        public Circle(float x, float y, float width, float height)
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
