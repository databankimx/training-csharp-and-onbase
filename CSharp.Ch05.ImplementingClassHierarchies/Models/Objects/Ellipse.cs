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
    /// Defines an Ellipse Shape
    /// </summary>
    public class Ellipse
    {
        #region Properties
        /// <summary>
        /// The rectangle upon which the ellipse will be based
        /// </summary>
        public RectangleF Location { get; set; }
        #endregion

        #region Constructors
        /// <summary>
        /// Constructor that takes a RectangleF as a parameter.
        /// </summary>
        /// <param name="rect">The rectangle upon which the ellipse will be based</param>
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
        
        /// <summary>
        /// Constructor that takes x, y, width, and height as parameters.
        /// </summary>
        /// <param name="x">Location of left edge of the rectangle upon which the ellipse will be based</param>
        /// <param name="y">Location of top edge of the rectangle upon which the ellipse will be based</param>
        /// <param name="width">Width of the rectangle upon which the ellipse will be based</param>
        /// <param name="height">Height of the rectangle upon which the ellipse will be based</param>
        public Ellipse(float x, float y, float width, float height) : this(new RectangleF(x, y, width, height))
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
