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
using System.Drawing;
#endregion

namespace CSharp.Ch05.ImplementingClassHierarchies.Models.Objects
{
    /// <summary>
    /// Defines a Circle as a special case of the Ellipse shape
    /// </summary>
    public class Circle : Ellipse
    {
        #region Constructors
        /// <summary>
        /// Constructor that takes a RectangleF as a parameter.
        /// </summary>
        /// <param name="rect">The rectangle upon which the circle will be based</param>
        public Circle(RectangleF rect) : base(rect)
        {
            // Validate width and height.
            if (rect.Width != rect.Height)
                throw new ArgumentOutOfRangeException(
                    "width and height",
                    "Circle width and height must be the same.");
        }

        /// <inheritdoc />
        /// <summary>
        /// Constructor that takes x, y, width, and height as parameters.
        /// </summary>
        /// <param name="x">Location of left edge of the rectangle upon which the circle will be based</param>
        /// <param name="y">Location of top edge of the rectangle upon which the circle will be based</param>
        /// <param name="width">Width of the rectangle upon which the circle will be based</param>
        /// <param name="height">Height of the rectangle upon which the circle will be based</param>
        public Circle(float x, float y, float width, float height) : this(new RectangleF(x, y, width, height))
        {
        }
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
