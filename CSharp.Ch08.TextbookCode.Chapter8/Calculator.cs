/*
 * Warning!
 *
 * This is the unedited code downloaded from the Textbook publisher
 * This code does not follow coding standards or best practices and may have bugs/errors
 *
 * Downloaded From:
 * https://media.wiley.com/product_ancillary/94/11186120/DOWNLOAD/MCSD%20Certification%20Code%20and%20Test%20Questions.zip
 */

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Chapter8
{
    class Calculator
    {
        private double _x;
        private double _y;

        public double X
        {
            get { return this._x; }
            set { this._x = value; }
        }

        public double Y
        {
            get { return this._y; }
            set { this._y = value; }
        }

        public double Divide()
        {
            if (this.Y == 0)
            {
                return 0;
            }
            else
            {
                return this.X / this.Y;
            }
        }

        public double Exponent(double power)
        {
            return Math.Pow(this.X, power);
        }
    }
}
