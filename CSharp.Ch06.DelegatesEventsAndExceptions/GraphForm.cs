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
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Windows.Forms;
#endregion

namespace CSharp.Ch06.DelegatesEventsAndExceptions
{
    /// <summary>
    /// Creates a windows form to draw equation graphs
    /// </summary>
    public partial class GraphForm : Form
    {
        #region Private Members
        // Delegate function for graphing
        private Func<float, float> theFunction;
        #endregion

        #region Constructors
        /// <summary>
        /// Create a new instance of GraphForm
        /// </summary>
        public GraphForm()
        {
            InitializeComponent();

            // The form's event handlers are inherently delegates (remember, these can be added "chained" together, hence +=)

            // Here, we're adding an event handler as an anonymous method
            Load += delegate
            {
                // Select the first equation when the form loads
                EquationComboBox.SelectedIndex = 0;
            };
            // This is equivalent to the following using a named method:
            // `Load += GraphForm_Load;`

            // Here, we are adding named methods to event handlers
            EquationComboBox.SelectedIndexChanged += EquationComboBox_SelectedIndexChanged;
            GraphPictureBox.Paint += GraphPictureBox_Paint;
        }
        #endregion

        #region Event Handlers
        // Select the first equation when the form loads
        private void GraphForm_Load(object sender, EventArgs e)
        {
            EquationComboBox.SelectedIndex = 0;
        }

        // Draw the currently selected function
        private void GraphPictureBox_Paint(object sender, PaintEventArgs e)
        {
            e.Graphics.Clear(Color.White);
            e.Graphics.SmoothingMode = SmoothingMode.AntiAlias;
            DrawGraph(e.Graphics);
        }

        // Select delegate and redraw when the user selects an equation
        private void EquationComboBox_SelectedIndexChanged(object sender, EventArgs ev)
        {
            switch (EquationComboBox.SelectedIndex)
            {
                case 0: // y = 12 * Sin(3 * x) / (1 + |x|) : Using statement lambda syntax
                    theFunction = x => (float)(12 * Math.Sin(3 * x) / (1 + Math.Abs(x)));
                    break;
                case 1: // y = |20 * Cos(|x|) / (|x| + 1)| : Using anonymous method delegate syntax
                    theFunction = delegate (float x)
                    {
                        x = Math.Abs(x);
                        if (x < 0.001) return 20;
                        return (float)Math.Abs(20 * Math.Cos(x) / (x + 1));
                    };
                    break;
                case 2: // y = Ax^6 + Bx^5 + Cx^4 + Dx^3 + Ex^2 + Fx + G : Using statement lambda syntax
                    theFunction = x =>
                    {
                        const float a = -0.0003f;
                        const float b = -0.0024f;
                        const float c = 0.02f;
                        const float d = 0.09f;
                        const float e = -0.5f;
                        const float f = 0.3f;
                        const float g = 3f;
                        return (((((a * x + b) * x + c) * x + d) * x + e) * x + f) * x + g;
                    };
                    break;
                default:
                    EquationComboBox.SelectedIndex = 0;
                    break;
            }
            GraphPictureBox.Refresh();
        }
        #endregion

        #region Helper Functions
        // Draw the graph
        private void DrawGraph(Graphics graphics)
        {
            // Map to turn right-side up and center at the origin.
            const float xMin = -10;
            const float yMin = -10;
            const float xMax = 10;
            const float yMax = 10;
            var rect = new RectangleF(xMin, yMin, xMax - xMin, yMax - yMin);
            #pragma warning disable IDE0300 // Simplify collection initialization
            #pragma warning disable IDE0090 // Use 'new(...)'
            PointF[] pts =
            {
                new PointF(0, GraphPictureBox.ClientSize.Height),
                new PointF(GraphPictureBox.ClientSize.Width, GraphPictureBox.ClientSize.Height),
                new PointF(0, 0),
            };
            #pragma warning restore IDE0090
            #pragma warning restore IDE0300
            var transform = new Matrix(rect, pts);
            graphics.Transform = transform;

            // See how far it is between horizontal pixels in world coordinates.
            pts = [new PointF(1, 0)];
            transform.Invert();
            transform.TransformVectors(pts);
            float dx = pts[0].X;

            // Generate points on the curve.
            var points = new List<PointF>();
            for (float x = xMin; x <= xMax; x += dx)
                points.Add(new PointF(x, theFunction(x)));

            // Use a thin pen.
            using (var thinPen = new Pen(Color.Gray, 0))
            {
                // Draw the coordinate axes.
                graphics.DrawLine(thinPen, xMin, 0, xMax, 0);
                graphics.DrawLine(thinPen, 0, yMin, 0, yMax);
                for (float x = xMin; x <= xMax; x++)
                    graphics.DrawLine(thinPen, x, -0.5f, x, 0.5f);
                for (float y = yMin; y <= yMax; y++)
                    graphics.DrawLine(thinPen, -0.5f, y, 0.5f, y);

                // Draw the graph.
                thinPen.Color = Color.Red;
                // `thin_pen.Color = Color.Black;`
                graphics.DrawLines(thinPen, points.ToArray());
            }
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
